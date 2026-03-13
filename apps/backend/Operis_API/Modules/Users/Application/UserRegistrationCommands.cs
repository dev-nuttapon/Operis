using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Domain;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public sealed class UserRegistrationCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IKeycloakAdminClient keycloakAdminClient) : IUserRegistrationCommands
{
    public async Task<RegistrationCommandResult> CreateRegistrationRequestAsync(CreateRegistrationRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Email is required.", ApiErrorCodes.EmailRequired);
        }

        var emailConflict = await ValidateEmailAvailabilityAsync(email, cancellationToken: cancellationToken);
        if (emailConflict is not null)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.Conflict, emailConflict, ApiErrorCodeResolver.Resolve(emailConflict, ApiErrorCodes.RequestValidationFailed));
        }

        var departmentValidation = await ValidateDepartmentSelectionAsync(request.DivisionId, request.DepartmentId, cancellationToken);
        if (!departmentValidation.Success)
        {
            return new RegistrationCommandResult(
                RegistrationCommandStatus.ValidationError,
                departmentValidation.ErrorMessage,
                ApiErrorCodeResolver.Resolve(departmentValidation.ErrorMessage, ApiErrorCodes.RequestValidationFailed));
        }

        var jobTitleValidation = await ValidateJobTitleSelectionAsync(request.DepartmentId, request.JobTitleId, cancellationToken);
        if (!jobTitleValidation.Success)
        {
            return new RegistrationCommandResult(
                RegistrationCommandStatus.ValidationError,
                jobTitleValidation.ErrorMessage,
                ApiErrorCodeResolver.Resolve(jobTitleValidation.ErrorMessage, ApiErrorCodes.RequestValidationFailed));
        }

        var registrationRequest = new UserRegistrationRequestEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DepartmentId = request.DepartmentId,
            JobTitleId = request.JobTitleId,
            Status = RegistrationRequestStatus.Pending,
            RequestedAt = DateTimeOffset.UtcNow
        };

        dbContext.UserRegistrationRequests.Add(registrationRequest);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "register",
            EntityType: "registration_request",
            EntityId: registrationRequest.Id.ToString(),
            StatusCode: StatusCodes.Status201Created,
            ActorType: "anonymous",
            ActorEmail: registrationRequest.Email,
            DepartmentId: registrationRequest.DepartmentId,
            After: ToRegistrationRequestAuditState(registrationRequest)));
        await dbContext.SaveChangesAsync(cancellationToken);

        var (divisions, departments, jobTitles) = await LoadReferenceMapsAsync(cancellationToken);
        return new RegistrationCommandResult(
            RegistrationCommandStatus.Success,
            Response: ToResponse(registrationRequest, divisions, departments, jobTitles));
    }

    public async Task<RegistrationCommandResult> ApproveRegistrationRequestAsync(Guid requestId, ReviewRegistrationRequest request, CancellationToken cancellationToken)
    {
        var registrationRequest = await dbContext.UserRegistrationRequests
            .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);

        if (registrationRequest is null)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.NotFound);
        }

        if (registrationRequest.Status != RegistrationRequestStatus.Pending)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Registration request has already been reviewed.", ApiErrorCodes.RegistrationReviewed);
        }

        var before = ToRegistrationRequestAuditState(registrationRequest);
        var existingKeycloakUser = await keycloakAdminClient.FindUserByEmailAsync(registrationRequest.Email, cancellationToken);
        var userExists = existingKeycloakUser is not null
            && await dbContext.Users.AnyAsync(x => x.Id == existingKeycloakUser.Id, cancellationToken);
        if (userExists)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.Conflict, "User already exists.", ApiErrorCodes.UserExists);
        }

        var keycloakResult = await keycloakAdminClient.CreateUserAsync(
            registrationRequest.Email,
            registrationRequest.FirstName,
            registrationRequest.LastName,
            null,
            cancellationToken);
        if (!keycloakResult.Success)
        {
            return new RegistrationCommandResult(
                RegistrationCommandStatus.ExternalFailure,
                keycloakResult.ErrorMessage,
                ApiErrorCodes.ExternalDependencyFailure,
                "Unable to provision user in Keycloak.",
                StatusCodes.Status502BadGateway);
        }

        var now = DateTimeOffset.UtcNow;
        var passwordSetupToken = GenerateRegistrationPasswordSetupToken();
        var passwordSetupExpiresAt = now.AddDays(7);
        registrationRequest.Status = RegistrationRequestStatus.Approved;
        registrationRequest.ReviewedAt = now;
        registrationRequest.ReviewedBy = request.ReviewedBy.Trim();
        registrationRequest.ProvisionedUserId = keycloakResult.UserId;
        registrationRequest.PasswordSetupToken = passwordSetupToken;
        registrationRequest.PasswordSetupExpiresAt = passwordSetupExpiresAt;
        registrationRequest.PasswordSetupCompletedAt = null;

        var user = new UserEntity
        {
            Id = keycloakResult.UserId ?? throw new InvalidOperationException("Keycloak user id is required."),
            Status = UserStatus.Active,
            CreatedAt = now,
            CreatedBy = registrationRequest.ReviewedBy,
            DepartmentId = registrationRequest.DepartmentId,
            JobTitleId = registrationRequest.JobTitleId
        };

        dbContext.Users.Add(user);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "approve",
            EntityType: "registration_request",
            EntityId: registrationRequest.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            ActorEmail: request.ReviewedBy.Trim(),
            DepartmentId: registrationRequest.DepartmentId,
            Before: before,
            After: ToRegistrationRequestAuditState(registrationRequest),
            Changes: new
            {
                status = registrationRequest.Status,
                registrationRequest.ReviewedBy,
                registrationRequest.ReviewedAt,
                registrationRequest.ProvisionedUserId,
                registrationRequest.PasswordSetupToken,
                registrationRequest.PasswordSetupExpiresAt
            },
            Metadata: new
            {
                userId = user.Id,
                setupPath = $"/register/setup-password/{passwordSetupToken}"
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        var (divisions, departments, jobTitles) = await LoadReferenceMapsAsync(cancellationToken);
        return new RegistrationCommandResult(
            RegistrationCommandStatus.Success,
            Response: ToResponse(registrationRequest, divisions, departments, jobTitles));
    }

    public async Task<RegistrationCommandResult> RejectRegistrationRequestAsync(Guid requestId, RejectRegistrationRequest request, CancellationToken cancellationToken)
    {
        var registrationRequest = await dbContext.UserRegistrationRequests
            .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);

        if (registrationRequest is null)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.NotFound);
        }

        if (registrationRequest.Status != RegistrationRequestStatus.Pending)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Registration request has already been reviewed.", ApiErrorCodes.RegistrationReviewed);
        }

        var before = ToRegistrationRequestAuditState(registrationRequest);
        registrationRequest.Status = RegistrationRequestStatus.Rejected;
        registrationRequest.ReviewedBy = request.ReviewedBy.Trim();
        registrationRequest.ReviewedAt = DateTimeOffset.UtcNow;
        registrationRequest.RejectionReason = request.Reason.Trim();

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "reject",
            EntityType: "registration_request",
            EntityId: registrationRequest.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            ActorEmail: registrationRequest.ReviewedBy,
            DepartmentId: registrationRequest.DepartmentId,
            Reason: registrationRequest.RejectionReason,
            Before: before,
            After: ToRegistrationRequestAuditState(registrationRequest),
            Changes: new
            {
                status = registrationRequest.Status,
                registrationRequest.ReviewedBy,
                registrationRequest.ReviewedAt,
                registrationRequest.RejectionReason
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        var (divisions, departments, jobTitles) = await LoadReferenceMapsAsync(cancellationToken);
        return new RegistrationCommandResult(
            RegistrationCommandStatus.Success,
            Response: ToResponse(registrationRequest, divisions, departments, jobTitles));
    }

    public async Task<RegistrationCommandResult> CompleteRegistrationPasswordSetupAsync(string token, CompleteRegistrationPasswordSetupRequest request, CancellationToken cancellationToken)
    {
        var registrationRequest = await dbContext.UserRegistrationRequests
            .FirstOrDefaultAsync(x => x.PasswordSetupToken == token, cancellationToken);

        if (registrationRequest is null)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.NotFound);
        }

        if (registrationRequest.Status != RegistrationRequestStatus.Approved)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Registration request is not approved.", ApiErrorCodes.RegistrationNotApproved);
        }

        if (registrationRequest.PasswordSetupCompletedAt.HasValue)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.Conflict, "Password setup has already been completed.", ApiErrorCodes.PasswordSetupCompleted);
        }

        if (registrationRequest.PasswordSetupExpiresAt.HasValue &&
            registrationRequest.PasswordSetupExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Password setup link has expired.", ApiErrorCodes.PasswordSetupExpired);
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Password is required.", ApiErrorCodes.PasswordRequired);
        }

        if (request.Password.Length < 8)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Password must be at least 8 characters.", ApiErrorCodes.PasswordMinLength);
        }

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Password and confirmation do not match.", ApiErrorCodes.PasswordMismatch);
        }

        if (string.IsNullOrWhiteSpace(registrationRequest.ProvisionedUserId))
        {
            return new RegistrationCommandResult(
                RegistrationCommandStatus.InternalFailure,
                "The approved registration is missing a provisioned Keycloak user reference.",
                ApiErrorCodes.InternalFailure,
                "Unable to resolve provisioned user.",
                StatusCodes.Status500InternalServerError);
        }

        var before = ToRegistrationRequestAuditState(registrationRequest);
        var passwordUpdated = await keycloakAdminClient.UpdatePasswordAsync(
            registrationRequest.ProvisionedUserId,
            request.Password,
            temporary: false,
            cancellationToken);
        if (!passwordUpdated.Success)
        {
            return new RegistrationCommandResult(
                RegistrationCommandStatus.ExternalFailure,
                passwordUpdated.ErrorMessage,
                ApiErrorCodes.ExternalDependencyFailure,
                "Unable to update password in Keycloak.",
                StatusCodes.Status502BadGateway);
        }

        registrationRequest.PasswordSetupCompletedAt = DateTimeOffset.UtcNow;
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "complete_password_setup",
            EntityType: "registration_request",
            EntityId: registrationRequest.Id.ToString(),
            StatusCode: StatusCodes.Status204NoContent,
            ActorType: "anonymous",
            ActorEmail: registrationRequest.Email,
            DepartmentId: registrationRequest.DepartmentId,
            Before: before,
            After: ToRegistrationRequestAuditState(registrationRequest),
            Changes: new
            {
                registrationRequest.PasswordSetupCompletedAt
            },
            Metadata: new
            {
                provisionedUserId = registrationRequest.ProvisionedUserId
            },
            IsSensitive: true));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RegistrationCommandResult(RegistrationCommandStatus.Success);
    }

    private async Task<(IReadOnlyDictionary<Guid, string> Divisions, IReadOnlyDictionary<Guid, CachedDepartmentItem> Departments, IReadOnlyDictionary<Guid, string> JobTitles)> LoadReferenceMapsAsync(CancellationToken cancellationToken)
    {
        var divisions = await dbContext.Divisions
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var departments = await dbContext.Departments
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(
                x => x.Id,
                x => new CachedDepartmentItem(x.Id, x.Name, x.DisplayOrder, x.DivisionId, null, x.CreatedAt, x.UpdatedAt, x.DeletedReason, x.DeletedBy, x.DeletedAt),
                cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        return (divisions, departments, jobTitles);
    }

    private static RegistrationRequestResponse ToResponse(
        UserRegistrationRequestEntity entity,
        IReadOnlyDictionary<Guid, string> divisions,
        IReadOnlyDictionary<Guid, CachedDepartmentItem> departments,
        IReadOnlyDictionary<Guid, string> jobTitles) =>
        new(
            entity.Id,
            entity.Email,
            entity.FirstName,
            entity.LastName,
            entity.DepartmentId.HasValue && departments.TryGetValue(entity.DepartmentId.Value, out var department) ? department.DivisionId : null,
            entity.DepartmentId.HasValue && departments.TryGetValue(entity.DepartmentId.Value, out department) && department.DivisionId.HasValue && divisions.TryGetValue(department.DivisionId.Value, out var divisionName) ? divisionName : null,
            entity.DepartmentId,
            entity.DepartmentId.HasValue && departments.TryGetValue(entity.DepartmentId.Value, out department) ? department.Name : null,
            entity.JobTitleId,
            entity.JobTitleId.HasValue && jobTitles.TryGetValue(entity.JobTitleId.Value, out var jobTitleName) ? jobTitleName : null,
            entity.Status,
            entity.RequestedAt,
            entity.ReviewedAt,
            entity.ReviewedBy,
            entity.RejectionReason,
            !string.IsNullOrWhiteSpace(entity.PasswordSetupToken) ? $"/register/setup-password/{entity.PasswordSetupToken}" : null,
            entity.PasswordSetupExpiresAt,
            entity.PasswordSetupCompletedAt);

    private static object ToRegistrationRequestAuditState(UserRegistrationRequestEntity entity) => new
    {
        entity.Id,
        entity.Email,
        entity.FirstName,
        entity.LastName,
        entity.DepartmentId,
        entity.JobTitleId,
        entity.ProvisionedUserId,
        entity.PasswordSetupToken,
        entity.PasswordSetupExpiresAt,
        entity.PasswordSetupCompletedAt,
        entity.Status,
        entity.RequestedAt,
        entity.ReviewedAt,
        entity.ReviewedBy,
        entity.RejectionReason
    };

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string GenerateRegistrationPasswordSetupToken()
    {
        Span<byte> bytes = stackalloc byte[24];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private async Task<DepartmentValidationResult> ValidateDepartmentSelectionAsync(Guid? divisionId, Guid? departmentId, CancellationToken cancellationToken)
    {
        if (!departmentId.HasValue)
        {
            return !divisionId.HasValue
                ? DepartmentValidationResult.Valid()
                : DepartmentValidationResult.Invalid("Department is required when division is selected.");
        }

        var department = await dbContext.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == departmentId.Value && x.DeletedAt == null, cancellationToken);
        if (department is null)
        {
            return DepartmentValidationResult.Invalid("Department does not exist.");
        }

        if (divisionId.HasValue && department.DivisionId != divisionId)
        {
            return DepartmentValidationResult.Invalid("Department does not belong to the selected division.");
        }

        return DepartmentValidationResult.Valid();
    }

    private async Task<JobTitleValidationResult> ValidateJobTitleSelectionAsync(Guid? departmentId, Guid? jobTitleId, CancellationToken cancellationToken)
    {
        if (!jobTitleId.HasValue)
        {
            return JobTitleValidationResult.Valid();
        }

        if (!departmentId.HasValue)
        {
            return JobTitleValidationResult.Invalid("Department is required when job title is selected.");
        }

        var jobTitle = await dbContext.JobTitles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == jobTitleId.Value && x.DeletedAt == null, cancellationToken);
        if (jobTitle is null)
        {
            return JobTitleValidationResult.Invalid("Job title does not exist.");
        }

        if (jobTitle.DepartmentId != departmentId)
        {
            return JobTitleValidationResult.Invalid("Job title does not belong to the selected department.");
        }

        return JobTitleValidationResult.Valid();
    }

    private async Task<string?> ValidateEmailAvailabilityAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var userExists = await LocalUserExistsForEmailAsync(email, cancellationToken);
        if (userExists)
        {
            return "User already exists.";
        }

        var pendingRegistrationExists = await dbContext.UserRegistrationRequests
            .AnyAsync(x => x.Email == email && x.Status == RegistrationRequestStatus.Pending, cancellationToken);
        if (pendingRegistrationExists)
        {
            return "Pending registration request already exists.";
        }

        var pendingInvitationExists = await dbContext.UserInvitations
            .AnyAsync(x => x.Email == email && x.Status == InvitationStatus.Pending, cancellationToken);
        if (pendingInvitationExists)
        {
            return "Pending invitation already exists.";
        }

        return null;
    }

    private async Task<bool> LocalUserExistsForEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var keycloakUser = await keycloakAdminClient.FindUserByEmailAsync(email, cancellationToken);
        if (keycloakUser is not null)
        {
            return await dbContext.Users.AnyAsync(x => x.Id == keycloakUser.Id && x.DeletedAt == null, cancellationToken);
        }

        return false;
    }

    private sealed record DepartmentValidationResult(bool Success, string? ErrorMessage)
    {
        public static DepartmentValidationResult Valid() => new(true, null);
        public static DepartmentValidationResult Invalid(string errorMessage) => new(false, errorMessage);
    }

    private sealed record JobTitleValidationResult(bool Success, string? ErrorMessage)
    {
        public static JobTitleValidationResult Valid() => new(true, null);
        public static JobTitleValidationResult Invalid(string errorMessage) => new(false, errorMessage);
    }
}
