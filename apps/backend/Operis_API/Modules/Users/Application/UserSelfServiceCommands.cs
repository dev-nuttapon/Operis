using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public sealed class UserSelfServiceCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IKeycloakAdminClient keycloakAdminClient) : IUserSelfServiceCommands
{
    public async Task<UserPasswordChangeResult> ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new UserPasswordChangeResult(UserPasswordChangeStatus.NotFound);
        }

        var currentPassword = request.CurrentPassword?.Trim();
        if (string.IsNullOrWhiteSpace(currentPassword))
        {
            return new UserPasswordChangeResult(UserPasswordChangeStatus.ValidationError, "Current password is required.", ApiErrorCodes.PasswordRequired);
        }

        var newPassword = request.NewPassword?.Trim();
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return new UserPasswordChangeResult(UserPasswordChangeStatus.ValidationError, "Password is required.", ApiErrorCodes.PasswordRequired);
        }

        if (newPassword.Length < 8)
        {
            return new UserPasswordChangeResult(UserPasswordChangeStatus.ValidationError, "Password must be at least 8 characters.", ApiErrorCodes.PasswordMinLength);
        }

        if (!string.Equals(newPassword, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return new UserPasswordChangeResult(UserPasswordChangeStatus.ValidationError, "Password and confirmation do not match.", ApiErrorCodes.PasswordMismatch);
        }

        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId && x.DeletedAt == null, cancellationToken);
        if (user is null)
        {
            return new UserPasswordChangeResult(UserPasswordChangeStatus.NotFound);
        }

        var profile = await keycloakAdminClient.GetUserByIdAsync(user.Id, cancellationToken);
        if (profile is null)
        {
            return new UserPasswordChangeResult(
                UserPasswordChangeStatus.ExternalFailure,
                "Unable to resolve user profile in Keycloak.",
                ApiErrorCodes.ExternalDependencyFailure,
                "Unable to verify current password.",
                StatusCodes.Status502BadGateway);
        }

        var username = string.IsNullOrWhiteSpace(profile.Email) ? profile.Username : profile.Email;
        if (string.IsNullOrWhiteSpace(username))
        {
            return new UserPasswordChangeResult(
                UserPasswordChangeStatus.ExternalFailure,
                "Keycloak user is missing a username.",
                ApiErrorCodes.ExternalDependencyFailure,
                "Unable to verify current password.",
                StatusCodes.Status502BadGateway);
        }

        var validation = await keycloakAdminClient.ValidateUserPasswordAsync(username, currentPassword, cancellationToken);
        if (!validation.Success)
        {
            if (validation.InvalidCredentials)
            {
                return new UserPasswordChangeResult(
                    UserPasswordChangeStatus.ValidationError,
                    "Current password is invalid.",
                    ApiErrorCodes.CurrentPasswordInvalid);
            }

            return new UserPasswordChangeResult(
                UserPasswordChangeStatus.ExternalFailure,
                validation.ErrorMessage ?? "Unable to verify current password in Keycloak.",
                ApiErrorCodes.ExternalDependencyFailure,
                "Unable to verify current password.",
                StatusCodes.Status502BadGateway);
        }

        var updateResult = await keycloakAdminClient.UpdatePasswordAsync(user.Id, newPassword, false, cancellationToken);
        if (!updateResult.Success)
        {
            return new UserPasswordChangeResult(
                UserPasswordChangeStatus.ExternalFailure,
                updateResult.ErrorMessage,
                ApiErrorCodes.ExternalDependencyFailure,
                "Unable to update password in Keycloak.",
                StatusCodes.Status502BadGateway);
        }

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "change_password",
            EntityType: "user",
            EntityId: user.Id,
            StatusCode: StatusCodes.Status204NoContent,
            ActorUserId: user.Id,
            DepartmentId: user.DepartmentId,
            IsSensitive: true));

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UserPasswordChangeResult(UserPasswordChangeStatus.Success);
    }
}
