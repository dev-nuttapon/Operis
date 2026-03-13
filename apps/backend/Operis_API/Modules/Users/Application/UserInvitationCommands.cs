using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Domain;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public sealed class UserInvitationCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IKeycloakAdminClient keycloakAdminClient) : IUserInvitationCommands
{
    public async Task<InvitationCommandResult> CreateInvitationAsync(CreateInvitationRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return new InvitationCommandResult(InvitationCommandStatus.ValidationError, "Email is required.", ApiErrorCodes.EmailRequired);
        }

        var invitedBy = request.InvitedBy?.Trim();
        if (string.IsNullOrWhiteSpace(invitedBy))
        {
            return new InvitationCommandResult(InvitationCommandStatus.ValidationError, "Invited by is required.", ApiErrorCodes.InvitedByRequired);
        }

        var emailConflict = await ValidateEmailAvailabilityAsync(email, cancellationToken: cancellationToken);
        if (emailConflict is not null)
        {
            return new InvitationCommandResult(InvitationCommandStatus.Conflict, emailConflict, ApiErrorCodeResolver.Resolve(emailConflict, ApiErrorCodes.RequestValidationFailed));
        }

        if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            return new InvitationCommandResult(InvitationCommandStatus.ValidationError, "Expiration date must be in the future.", ApiErrorCodes.ExpirationFuture);
        }

        var departmentValidation = await ValidateDepartmentSelectionAsync(request.DivisionId, request.DepartmentId, cancellationToken);
        if (!departmentValidation.Success)
        {
            return new InvitationCommandResult(
                InvitationCommandStatus.ValidationError,
                departmentValidation.ErrorMessage,
                ApiErrorCodeResolver.Resolve(departmentValidation.ErrorMessage, ApiErrorCodes.RequestValidationFailed));
        }

        var jobTitleValidation = await ValidateJobTitleSelectionAsync(request.DepartmentId, request.JobTitleId, cancellationToken);
        if (!jobTitleValidation.Success)
        {
            return new InvitationCommandResult(
                InvitationCommandStatus.ValidationError,
                jobTitleValidation.ErrorMessage,
                ApiErrorCodeResolver.Resolve(jobTitleValidation.ErrorMessage, ApiErrorCodes.RequestValidationFailed));
        }

        var invitation = new UserInvitationEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            InvitationToken = GenerateInvitationToken(),
            InvitedBy = invitedBy,
            DepartmentId = request.DepartmentId,
            JobTitleId = request.JobTitleId,
            Status = InvitationStatus.Pending,
            InvitedAt = DateTimeOffset.UtcNow,
            ExpiresAt = request.ExpiresAt
        };

        dbContext.UserInvitations.Add(invitation);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "invite",
            EntityType: "invitation",
            EntityId: invitation.Id.ToString(),
            StatusCode: StatusCodes.Status201Created,
            ActorEmail: invitedBy,
            DepartmentId: invitation.DepartmentId,
            After: ToInvitationAuditState(invitation),
            Metadata: new
            {
                setupPath = $"/invite/{invitation.InvitationToken}"
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        var (divisions, departments, jobTitles) = await LoadReferenceMapsAsync(cancellationToken);
        return new InvitationCommandResult(
            InvitationCommandStatus.Success,
            Response: ToResponse(invitation, divisions, departments, jobTitles));
    }

    public async Task<InvitationCommandResult> UpdateInvitationAsync(Guid invitationId, UpdateInvitationRequest request, CancellationToken cancellationToken)
    {
        var invitation = await dbContext.UserInvitations.FirstOrDefaultAsync(x => x.Id == invitationId, cancellationToken);
        if (invitation is null)
        {
            return new InvitationCommandResult(InvitationCommandStatus.NotFound);
        }

        var status = GetInvitationStatus(invitation);
        if (status == InvitationStatus.Accepted)
        {
            return new InvitationCommandResult(InvitationCommandStatus.ValidationError, "Accepted invitation cannot be updated.", ApiErrorCodes.InvitationUpdateAccepted);
        }

        if (status == InvitationStatus.Cancelled)
        {
            return new InvitationCommandResult(InvitationCommandStatus.ValidationError, "Cancelled invitation cannot be updated.", ApiErrorCodes.InvitationUpdateCancelled);
        }

        if (status == InvitationStatus.Rejected)
        {
            return new InvitationCommandResult(InvitationCommandStatus.ValidationError, "Rejected invitation cannot be updated.", ApiErrorCodes.InvitationUpdateRejected);
        }

        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return new InvitationCommandResult(InvitationCommandStatus.ValidationError, "Email is required.", ApiErrorCodes.EmailRequired);
        }

        if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            return new InvitationCommandResult(InvitationCommandStatus.ValidationError, "Expiration date must be in the future.", ApiErrorCodes.ExpirationFuture);
        }

        var departmentValidation = await ValidateDepartmentSelectionAsync(request.DivisionId, request.DepartmentId, cancellationToken);
        if (!departmentValidation.Success)
        {
            return new InvitationCommandResult(
                InvitationCommandStatus.ValidationError,
                departmentValidation.ErrorMessage,
                ApiErrorCodeResolver.Resolve(departmentValidation.ErrorMessage, ApiErrorCodes.RequestValidationFailed));
        }

        var jobTitleValidation = await ValidateJobTitleSelectionAsync(request.DepartmentId, request.JobTitleId, cancellationToken);
        if (!jobTitleValidation.Success)
        {
            return new InvitationCommandResult(
                InvitationCommandStatus.ValidationError,
                jobTitleValidation.ErrorMessage,
                ApiErrorCodeResolver.Resolve(jobTitleValidation.ErrorMessage, ApiErrorCodes.RequestValidationFailed));
        }

        var before = ToInvitationAuditState(invitation);
        var emailChanged = !string.Equals(invitation.Email, email, StringComparison.OrdinalIgnoreCase);
        if (emailChanged)
        {
            var emailConflict = await ValidateEmailAvailabilityAsync(email, cancellationToken, invitation.Id);
            if (emailConflict is not null)
            {
                return new InvitationCommandResult(InvitationCommandStatus.Conflict, emailConflict, ApiErrorCodeResolver.Resolve(emailConflict, ApiErrorCodes.RequestValidationFailed));
            }
        }

        invitation.Email = email;
        invitation.DepartmentId = request.DepartmentId;
        invitation.JobTitleId = request.JobTitleId;
        invitation.ExpiresAt = request.ExpiresAt;

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "update",
            EntityType: "invitation",
            EntityId: invitation.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            ActorEmail: invitation.InvitedBy,
            DepartmentId: invitation.DepartmentId,
            Before: before,
            After: ToInvitationAuditState(invitation),
            Changes: new
            {
                invitation.Email,
                invitation.DepartmentId,
                invitation.JobTitleId,
                invitation.ExpiresAt
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        var (divisions, departments, jobTitles) = await LoadReferenceMapsAsync(cancellationToken);
        return new InvitationCommandResult(
            InvitationCommandStatus.Success,
            Response: ToResponse(invitation, divisions, departments, jobTitles));
    }

    public async Task<InvitationCommandResult> CancelInvitationAsync(Guid invitationId, CancellationToken cancellationToken)
    {
        var invitation = await dbContext.UserInvitations.FirstOrDefaultAsync(x => x.Id == invitationId, cancellationToken);
        if (invitation is null)
        {
            return new InvitationCommandResult(InvitationCommandStatus.NotFound);
        }

        var before = ToInvitationAuditState(invitation);
        var status = GetInvitationStatus(invitation);
        if (status == InvitationStatus.Accepted)
        {
            return new InvitationCommandResult(InvitationCommandStatus.ValidationError, "Accepted invitation cannot be cancelled.", ApiErrorCodes.InvitationCancelAccepted);
        }

        invitation.Status = InvitationStatus.Cancelled;
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "cancel_invitation",
            EntityType: "invitation",
            EntityId: invitation.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            ActorEmail: invitation.InvitedBy,
            DepartmentId: invitation.DepartmentId,
            Before: before,
            After: ToInvitationAuditState(invitation),
            Changes: new
            {
                invitation.Status
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        var (divisions, departments, jobTitles) = await LoadReferenceMapsAsync(cancellationToken);
        return new InvitationCommandResult(
            InvitationCommandStatus.Success,
            Response: ToResponse(invitation, divisions, departments, jobTitles));
    }

    public async Task<InvitationCommandResult> AcceptInvitationAsync(string token, AcceptInvitationRequest request, CancellationToken cancellationToken)
    {
        var invitation = await dbContext.UserInvitations
            .FirstOrDefaultAsync(x => x.InvitationToken == token, cancellationToken);
        if (invitation is null)
        {
            return new InvitationCommandResult(InvitationCommandStatus.NotFound);
        }

        var before = ToInvitationAuditState(invitation);
        var status = GetInvitationStatus(invitation);
        if (status == InvitationStatus.Accepted)
        {
            return new InvitationCommandResult(InvitationCommandStatus.Conflict, "Invitation has already been accepted.", ApiErrorCodes.InvitationAccepted);
        }

        if (status == InvitationStatus.Rejected)
        {
            return new InvitationCommandResult(InvitationCommandStatus.Conflict, "Invitation has already been rejected.", ApiErrorCodes.InvitationRejected);
        }

        if (status == InvitationStatus.Cancelled)
        {
            return new InvitationCommandResult(InvitationCommandStatus.ValidationError, "Invitation has been cancelled.", ApiErrorCodes.InvitationCancelled);
        }

        if (status == InvitationStatus.Expired)
        {
            return new InvitationCommandResult(InvitationCommandStatus.ValidationError, "Invitation has expired.", ApiErrorCodes.InvitationExpired);
        }

        var password = request.Password?.Trim();
        if (string.IsNullOrWhiteSpace(password))
        {
            return new InvitationCommandResult(InvitationCommandStatus.ValidationError, "Password is required.", ApiErrorCodes.PasswordRequired);
        }

        if (password.Length < 8)
        {
            return new InvitationCommandResult(InvitationCommandStatus.ValidationError, "Password must be at least 8 characters.", ApiErrorCodes.PasswordMinLength);
        }

        if (!string.Equals(password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return new InvitationCommandResult(InvitationCommandStatus.ValidationError, "Password and confirmation do not match.", ApiErrorCodes.PasswordMismatch);
        }

        var emailConflict = await ValidateEmailAvailabilityAsync(invitation.Email, cancellationToken, invitation.Id);
        if (emailConflict is not null)
        {
            return new InvitationCommandResult(InvitationCommandStatus.Conflict, emailConflict, ApiErrorCodeResolver.Resolve(emailConflict, ApiErrorCodes.RequestValidationFailed));
        }

        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var keycloakResult = await keycloakAdminClient.CreateUserAsync(
            invitation.Email,
            firstName,
            lastName,
            password,
            cancellationToken);
        if (!keycloakResult.Success)
        {
            return new InvitationCommandResult(
                InvitationCommandStatus.ExternalFailure,
                keycloakResult.ErrorMessage,
                ApiErrorCodes.ExternalDependencyFailure,
                "Unable to provision user in Keycloak.",
                StatusCodes.Status502BadGateway);
        }

        var user = new UserEntity
        {
            Id = keycloakResult.UserId ?? throw new InvalidOperationException("Keycloak user id is required."),
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = invitation.InvitedBy,
            DepartmentId = invitation.DepartmentId,
            JobTitleId = invitation.JobTitleId
        };

        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTimeOffset.UtcNow;

        dbContext.Users.Add(user);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "accept_invitation",
            EntityType: "invitation",
            EntityId: invitation.Id.ToString(),
            StatusCode: StatusCodes.Status200OK,
            ActorType: "anonymous",
            ActorUserId: user.Id,
            ActorEmail: invitation.Email,
            DepartmentId: invitation.DepartmentId,
            Before: before,
            After: ToInvitationAuditState(invitation),
            Changes: new
            {
                invitation.Status,
                invitation.AcceptedAt
            },
            Metadata: new
            {
                userId = user.Id,
                firstName,
                lastName
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        var (divisions, departments, jobTitles) = await LoadReferenceMapsAsync(cancellationToken);
        return new InvitationCommandResult(
            InvitationCommandStatus.Success,
            Response: ToResponse(invitation, divisions, departments, jobTitles));
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
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        return (divisions, departments, jobTitles);
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

    private async Task<string?> ValidateEmailAvailabilityAsync(string email, CancellationToken cancellationToken, Guid? ignoredInvitationId = null)
    {
        var userExists = await LocalUserExistsForEmailAsync(email, cancellationToken);
        if (userExists)
        {
            return "User already exists.";
        }

        var pendingInvitationExists = await dbContext.UserInvitations
            .AnyAsync(
                x => x.Email == email
                    && x.Status == InvitationStatus.Pending
                    && (!x.ExpiresAt.HasValue || x.ExpiresAt > DateTimeOffset.UtcNow)
                    && (!ignoredInvitationId.HasValue || x.Id != ignoredInvitationId.Value),
                cancellationToken);
        if (pendingInvitationExists)
        {
            return "Pending invitation already exists.";
        }

        var pendingRequestExists = await dbContext.UserRegistrationRequests
            .AnyAsync(x => x.Email == email && x.Status == RegistrationRequestStatus.Pending, cancellationToken);
        if (pendingRequestExists)
        {
            return "Pending registration request already exists.";
        }

        return null;
    }

    private async Task<bool> LocalUserExistsForEmailAsync(string email, CancellationToken cancellationToken)
    {
        var keycloakUser = await keycloakAdminClient.FindUserByEmailAsync(email, cancellationToken);
        return keycloakUser is not null;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string GenerateInvitationToken()
    {
        Span<byte> bytes = stackalloc byte[24];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static InvitationStatus GetInvitationStatus(UserInvitationEntity entity)
    {
        if (entity.Status == InvitationStatus.Pending && entity.ExpiresAt.HasValue && entity.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            return InvitationStatus.Expired;
        }

        return entity.Status;
    }

    private static object ToInvitationAuditState(UserInvitationEntity entity) => new
    {
        entity.Id,
        entity.Email,
        entity.InvitationToken,
        entity.InvitedBy,
        entity.DepartmentId,
        entity.JobTitleId,
        Status = GetInvitationStatus(entity),
        entity.InvitedAt,
        entity.ExpiresAt,
        entity.AcceptedAt,
        entity.RejectedAt
    };

    private static InvitationResponse ToResponse(
        UserInvitationEntity entity,
        IReadOnlyDictionary<Guid, string> divisions,
        IReadOnlyDictionary<Guid, CachedDepartmentItem> departments,
        IReadOnlyDictionary<Guid, string> jobTitles)
    {
        Guid? divisionId = null;
        string? divisionName = null;
        string? departmentName = null;

        if (entity.DepartmentId.HasValue && departments.TryGetValue(entity.DepartmentId.Value, out var department))
        {
            divisionId = department.DivisionId;
            departmentName = department.Name;

            if (divisionId.HasValue && divisions.TryGetValue(divisionId.Value, out var resolvedDivisionName))
            {
                divisionName = resolvedDivisionName;
            }
        }

        return new InvitationResponse(
            entity.Id,
            entity.Email,
            entity.InvitationToken,
            entity.InvitedBy,
            divisionId,
            divisionName,
            entity.DepartmentId,
            departmentName,
            entity.JobTitleId,
            entity.JobTitleId.HasValue && jobTitles.TryGetValue(entity.JobTitleId.Value, out var jobTitleName) ? jobTitleName : null,
            GetInvitationStatus(entity),
            entity.InvitedAt,
            entity.ExpiresAt,
            entity.AcceptedAt,
            entity.RejectedAt,
            $"/invite/{entity.InvitationToken}");
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
