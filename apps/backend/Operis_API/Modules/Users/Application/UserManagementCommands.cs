using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Domain;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public sealed class UserManagementCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IKeycloakAdminClient keycloakAdminClient) : IUserManagementCommands
{
    public async Task<UserCommandResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return new UserCommandResult(UserCommandStatus.ValidationError, "Email is required.", ApiErrorCodes.EmailRequired);
        }

        var password = request.Password?.Trim();
        if (string.IsNullOrWhiteSpace(password))
        {
            return new UserCommandResult(UserCommandStatus.ValidationError, "Password is required.", ApiErrorCodes.PasswordRequired);
        }

        if (password.Length < 8)
        {
            return new UserCommandResult(UserCommandStatus.ValidationError, "Password must be at least 8 characters.", ApiErrorCodes.PasswordMinLength);
        }

        if (!string.Equals(password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return new UserCommandResult(UserCommandStatus.ValidationError, "Password and confirmation do not match.", ApiErrorCodes.PasswordMismatch);
        }

        var departmentValidation = await ValidateDepartmentSelectionAsync(request.DivisionId, request.DepartmentId, cancellationToken);
        if (!departmentValidation.Success)
        {
            return new UserCommandResult(
                UserCommandStatus.ValidationError,
                departmentValidation.ErrorMessage,
                ApiErrorCodeResolver.Resolve(departmentValidation.ErrorMessage, ApiErrorCodes.RequestValidationFailed));
        }

        var jobTitleValidation = await ValidateJobTitleSelectionAsync(request.DepartmentId, request.JobTitleId, cancellationToken);
        if (!jobTitleValidation.Success)
        {
            return new UserCommandResult(
                UserCommandStatus.ValidationError,
                jobTitleValidation.ErrorMessage,
                ApiErrorCodeResolver.Resolve(jobTitleValidation.ErrorMessage, ApiErrorCodes.RequestValidationFailed));
        }

        var roleIds = request.RoleIds ?? [];
        var selectedRoles = roleIds.Count == 0
            ? []
            : await dbContext.AppRoles
                .Where(x => roleIds.Contains(x.Id) && x.DeletedAt == null)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);

        if (selectedRoles.Count != roleIds.Count)
        {
            return new UserCommandResult(UserCommandStatus.ValidationError, "One or more selected roles do not exist.", ApiErrorCodes.RolesNotFound);
        }

        var existingKeycloakUser = await keycloakAdminClient.FindUserByEmailAsync(email, cancellationToken);
        var userExists = existingKeycloakUser is not null
            && await dbContext.Users.AnyAsync(x => x.Id == existingKeycloakUser.Id, cancellationToken);
        if (userExists)
        {
            return new UserCommandResult(UserCommandStatus.Conflict, "User already exists.", ApiErrorCodes.UserExists);
        }

        var keycloakResult = await keycloakAdminClient.CreateUserAsync(
            email,
            request.FirstName.Trim(),
            request.LastName.Trim(),
            password,
            cancellationToken);
        if (!keycloakResult.Success)
        {
            return new UserCommandResult(
                UserCommandStatus.ExternalFailure,
                keycloakResult.ErrorMessage,
                ApiErrorCodes.ExternalDependencyFailure,
                "Unable to provision user in Keycloak.",
                StatusCodes.Status502BadGateway);
        }

        var now = DateTimeOffset.UtcNow;
        var user = new UserEntity
        {
            Id = keycloakResult.UserId ?? throw new InvalidOperationException("Keycloak user id is required."),
            Status = UserStatus.Active,
            CreatedAt = now,
            CreatedBy = request.CreatedBy.Trim(),
            DepartmentId = request.DepartmentId,
            JobTitleId = request.JobTitleId
        };

        var keycloakRoleNames = selectedRoles.Select(x => x.KeycloakRoleName).ToArray();
        if (keycloakRoleNames.Length > 0)
        {
            var roleAssigned = await keycloakAdminClient.AssignRealmRolesAsync(user.Id, keycloakRoleNames, cancellationToken);
            if (!roleAssigned)
            {
                return new UserCommandResult(
                UserCommandStatus.ExternalFailure,
                "The selected roles could not be mapped in Keycloak.",
                ApiErrorCodes.ExternalDependencyFailure,
                "Unable to assign roles in Keycloak.",
                StatusCodes.Status502BadGateway);
            }
        }

        dbContext.Users.Add(user);
        var selectedRoleNames = selectedRoles.Select(x => x.Name).ToArray();
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "create",
            EntityType: "user",
            EntityId: user.Id,
            StatusCode: StatusCodes.Status201Created,
            DepartmentId: user.DepartmentId,
            After: ToUserAuditState(
                user,
                new KeycloakUserProfile(user.Id, email, email, request.FirstName.Trim(), request.LastName.Trim(), true, true),
                selectedRoleNames),
            Metadata: new
            {
                roleNames = selectedRoleNames
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UserCommandResult(
            UserCommandStatus.Success,
            Response: ToResponse(user, selectedRoleNames, departmentValidation.Department));
    }

    public async Task<UserCommandResult> UpdateUserAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && x.DeletedAt == null, cancellationToken);
        if (user is null)
        {
            return new UserCommandResult(UserCommandStatus.NotFound);
        }

        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return new UserCommandResult(UserCommandStatus.ValidationError, "Email is required.", ApiErrorCodes.EmailRequired);
        }

        var departmentValidation = await ValidateDepartmentSelectionAsync(request.DivisionId, request.DepartmentId, cancellationToken);
        if (!departmentValidation.Success)
        {
            return new UserCommandResult(
                UserCommandStatus.ValidationError,
                departmentValidation.ErrorMessage,
                ApiErrorCodeResolver.Resolve(departmentValidation.ErrorMessage, ApiErrorCodes.RequestValidationFailed));
        }

        var jobTitleValidation = await ValidateJobTitleSelectionAsync(request.DepartmentId, request.JobTitleId, cancellationToken);
        if (!jobTitleValidation.Success)
        {
            return new UserCommandResult(
                UserCommandStatus.ValidationError,
                jobTitleValidation.ErrorMessage,
                ApiErrorCodeResolver.Resolve(jobTitleValidation.ErrorMessage, ApiErrorCodes.RequestValidationFailed));
        }

        var roleIds = request.RoleIds ?? [];
        var selectedRoles = roleIds.Count == 0
            ? []
            : await dbContext.AppRoles
                .Where(x => roleIds.Contains(x.Id) && x.DeletedAt == null)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);
        if (selectedRoles.Count != roleIds.Count)
        {
            return new UserCommandResult(UserCommandStatus.ValidationError, "One or more selected roles do not exist.", ApiErrorCodes.RolesNotFound);
        }

        var existingProfile = await ResolveKeycloakProfileAsync(user, cancellationToken);
        var before = ToUserAuditState(user, existingProfile, null);
        var existingKeycloakUser = await keycloakAdminClient.FindUserByEmailAsync(email, cancellationToken);
        if (existingKeycloakUser is not null && !string.Equals(existingKeycloakUser.Id, user.Id, StringComparison.Ordinal))
        {
            return new UserCommandResult(UserCommandStatus.Conflict, "User already exists.", ApiErrorCodes.UserExists);
        }

        var keycloakResult = await keycloakAdminClient.UpdateUserAsync(
            user.Id,
            email,
            request.FirstName.Trim(),
            request.LastName.Trim(),
            cancellationToken);
        if (!keycloakResult.Success)
        {
            return keycloakResult.Conflict
                ? new UserCommandResult(UserCommandStatus.Conflict, "User already exists.", ApiErrorCodes.UserExists)
                : new UserCommandResult(
                    UserCommandStatus.ExternalFailure,
                    keycloakResult.ErrorMessage,
                    ApiErrorCodes.ExternalDependencyFailure,
                    "Unable to update user in Keycloak.",
                    StatusCodes.Status502BadGateway);
        }

        var managedRoleNames = await dbContext.AppRoles
            .Where(x => x.DeletedAt == null)
            .Select(x => x.KeycloakRoleName)
            .ToListAsync(cancellationToken);
        var desiredRoleNames = selectedRoles.Select(x => x.KeycloakRoleName).ToArray();
        var rolesUpdated = await keycloakAdminClient.SetManagedRolesAsync(user.Id, managedRoleNames, desiredRoleNames, cancellationToken);
        if (!rolesUpdated)
        {
            return new UserCommandResult(
                UserCommandStatus.ExternalFailure,
                "The selected roles could not be synchronized in Keycloak.",
                ApiErrorCodes.ExternalDependencyFailure,
                "Unable to update roles in Keycloak.",
                StatusCodes.Status502BadGateway);
        }

        user.DepartmentId = request.DepartmentId;
        user.JobTitleId = request.JobTitleId;
        var selectedRoleNames = selectedRoles.Select(x => x.Name).ToArray();
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "update",
            EntityType: "user",
            EntityId: user.Id,
            StatusCode: StatusCodes.Status200OK,
            DepartmentId: user.DepartmentId,
            Before: before,
            After: ToUserAuditState(
                user,
                new KeycloakUserProfile(user.Id, email, email, request.FirstName.Trim(), request.LastName.Trim(), true, true),
                selectedRoleNames),
            Changes: new
            {
                email,
                firstName = request.FirstName.Trim(),
                lastName = request.LastName.Trim(),
                departmentId = user.DepartmentId,
                jobTitleId = user.JobTitleId,
                roleNames = selectedRoleNames
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UserCommandResult(
            UserCommandStatus.Success,
            Response: ToResponse(user, selectedRoleNames, departmentValidation.Department));
    }

    public async Task<UserCommandResult> DeleteUserAsync(string userId, SoftDeleteRequest request, string actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && x.DeletedAt == null, cancellationToken);
        if (entity is null)
        {
            return new UserCommandResult(UserCommandStatus.NotFound);
        }

        var before = ToUserAuditState(entity);
        var keycloakResult = await keycloakAdminClient.DisableUserAsync(entity.Id, cancellationToken);
        if (!keycloakResult.Success)
        {
            return new UserCommandResult(
                UserCommandStatus.ExternalFailure,
                keycloakResult.ErrorMessage,
                ApiErrorCodes.ExternalDependencyFailure,
                "Unable to disable user in Keycloak.",
                StatusCodes.Status502BadGateway);
        }

        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = actor;
        entity.DeletedReason = NormalizeDeleteReason(request.Reason);
        entity.Status = UserStatus.Deleted;

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "soft_delete",
            EntityType: "user",
            EntityId: entity.Id,
            StatusCode: StatusCodes.Status204NoContent,
            Reason: entity.DeletedReason,
            DepartmentId: entity.DepartmentId,
            Before: before,
            After: ToUserAuditState(entity),
            Changes: new
            {
                status = entity.Status,
                entity.DeletedAt,
                entity.DeletedBy,
                entity.DeletedReason
            }));
        await dbContext.SaveChangesAsync(cancellationToken);
        return new UserCommandResult(UserCommandStatus.Success);
    }

    private async Task<KeycloakUserProfile?> ResolveKeycloakProfileAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(user.Id)
            ? null
            : await keycloakAdminClient.GetUserByIdAsync(user.Id, cancellationToken);
    }

    private async Task<DepartmentValidationResult> ValidateDepartmentSelectionAsync(Guid? divisionId, Guid? departmentId, CancellationToken cancellationToken)
    {
        if (!departmentId.HasValue)
        {
            if (!divisionId.HasValue)
            {
                return DepartmentValidationResult.Valid(null);
            }
            return DepartmentValidationResult.Invalid("Department is required when division is selected.");
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

        return DepartmentValidationResult.Valid(department);
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

    private static UserResponse ToResponse(UserEntity entity, IReadOnlyList<string> roles, DepartmentEntity? department) =>
        new(
            entity.Id,
            entity.Status,
            entity.CreatedAt,
            entity.CreatedBy,
            department?.DivisionId,
            null,
            entity.DepartmentId,
            department?.Name,
            entity.JobTitleId,
            null,
            roles,
            entity.PreferredLanguage,
            entity.PreferredTheme,
            entity.DeletedReason,
            entity.DeletedBy,
            entity.DeletedAt,
            null);

    private static object ToUserAuditState(
        UserEntity entity,
        KeycloakUserProfile? keycloakProfile = null,
        IReadOnlyList<string>? roleNames = null) => new
    {
        entity.Id,
        entity.Status,
        entity.CreatedAt,
        entity.CreatedBy,
        entity.DepartmentId,
        entity.JobTitleId,
        entity.PreferredLanguage,
        entity.PreferredTheme,
        entity.DeletedReason,
        entity.DeletedBy,
        entity.DeletedAt,
        keycloak = keycloakProfile is null
            ? null
            : new
            {
                keycloakProfile.Email,
                keycloakProfile.Username,
                keycloakProfile.FirstName,
                keycloakProfile.LastName,
                keycloakProfile.Enabled,
                keycloakProfile.EmailVerified
            },
        roles = roleNames
    };

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string NormalizeDeleteReason(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "No reason provided";
        }

        var normalized = value.Trim();
        return normalized.Length > 500 ? normalized[..500] : normalized;
    }

    private sealed record DepartmentValidationResult(bool Success, string? ErrorMessage, DepartmentEntity? Department)
    {
        public static DepartmentValidationResult Valid(DepartmentEntity? department) => new(true, null, department);
        public static DepartmentValidationResult Invalid(string errorMessage) => new(false, errorMessage, null);
    }

    private sealed record JobTitleValidationResult(bool Success, string? ErrorMessage)
    {
        public static JobTitleValidationResult Valid() => new(true, null);
        public static JobTitleValidationResult Invalid(string errorMessage) => new(false, errorMessage);
    }
}
