using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Domain;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;

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
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Email is required.");
        }

        var emailConflict = await ValidateEmailAvailabilityAsync(email, cancellationToken: cancellationToken);
        if (emailConflict is not null)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.Conflict, emailConflict);
        }

        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await dbContext.Departments
                .AnyAsync(x => x.Id == request.DepartmentId.Value && x.DeletedAt == null, cancellationToken);
            if (!departmentExists)
            {
                return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Department does not exist.");
            }
        }

        if (request.JobTitleId.HasValue)
        {
            var jobTitleExists = await dbContext.JobTitles
                .AnyAsync(x => x.Id == request.JobTitleId.Value && x.DeletedAt == null, cancellationToken);
            if (!jobTitleExists)
            {
                return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Job title does not exist.");
            }
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

        var (departments, jobTitles) = await LoadReferenceMapsAsync(cancellationToken);
        return new RegistrationCommandResult(
            RegistrationCommandStatus.Success,
            Response: ToResponse(registrationRequest, departments, jobTitles));
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
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Registration request has already been reviewed.");
        }

        var before = ToRegistrationRequestAuditState(registrationRequest);
        var existingKeycloakUser = await keycloakAdminClient.FindUserByEmailAsync(registrationRequest.Email, cancellationToken);
        var userExists = existingKeycloakUser is not null
            && await dbContext.Users.AnyAsync(x => x.Id == existingKeycloakUser.Id, cancellationToken);
        if (userExists)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.Conflict, "User already exists.");
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

        var (departments, jobTitles) = await LoadReferenceMapsAsync(cancellationToken);
        return new RegistrationCommandResult(
            RegistrationCommandStatus.Success,
            Response: ToResponse(registrationRequest, departments, jobTitles));
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
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Registration request has already been reviewed.");
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

        var (departments, jobTitles) = await LoadReferenceMapsAsync(cancellationToken);
        return new RegistrationCommandResult(
            RegistrationCommandStatus.Success,
            Response: ToResponse(registrationRequest, departments, jobTitles));
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
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Registration request is not approved.");
        }

        if (registrationRequest.PasswordSetupCompletedAt.HasValue)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.Conflict, "Password setup has already been completed.");
        }

        if (registrationRequest.PasswordSetupExpiresAt.HasValue &&
            registrationRequest.PasswordSetupExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Password setup link has expired.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Password is required.");
        }

        if (request.Password.Length < 8)
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Password must be at least 8 characters.");
        }

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return new RegistrationCommandResult(RegistrationCommandStatus.ValidationError, "Password and confirmation do not match.");
        }

        if (string.IsNullOrWhiteSpace(registrationRequest.ProvisionedUserId))
        {
            return new RegistrationCommandResult(
                RegistrationCommandStatus.InternalFailure,
                "The approved registration is missing a provisioned Keycloak user reference.",
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

    private async Task<(IReadOnlyDictionary<Guid, string> Departments, IReadOnlyDictionary<Guid, string> JobTitles)> LoadReferenceMapsAsync(CancellationToken cancellationToken)
    {
        var departments = await dbContext.Departments
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var jobTitles = await dbContext.JobTitles
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        return (departments, jobTitles);
    }

    private static RegistrationRequestResponse ToResponse(
        UserRegistrationRequestEntity entity,
        IReadOnlyDictionary<Guid, string> departments,
        IReadOnlyDictionary<Guid, string> jobTitles) =>
        new(
            entity.Id,
            entity.Email,
            entity.FirstName,
            entity.LastName,
            entity.DepartmentId,
            entity.DepartmentId.HasValue && departments.TryGetValue(entity.DepartmentId.Value, out var departmentName) ? departmentName : null,
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
}
