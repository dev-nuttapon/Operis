using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Configuration;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Users.Application;

public sealed class AdminSecurityCommands(
    OperisDbContext dbContext,
    IReferenceDataCache referenceDataCache,
    IAuditLogWriter auditLogWriter,
    IPermissionMatrix permissionMatrix,
    IOptions<Phase0SecurityOptions> options) : IAdminSecurityCommands
{
    public async Task<AdminSecurityCommandResult<PermissionMatrixResponse>> ApplyPermissionMatrixAsync(
        string actor,
        ApplyPermissionMatrixRequest request,
        CancellationToken cancellationToken)
    {
        var reason = request.Reason?.Trim();
        if (string.IsNullOrWhiteSpace(reason))
        {
            return new AdminSecurityCommandResult<PermissionMatrixResponse>(
                AdminSecurityCommandStatus.ValidationError,
                ErrorMessage: "Reason is required.",
                ErrorCode: ApiErrorCodes.ReasonRequired);
        }

        var roles = await dbContext.AppRoles
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var roleMap = roles.ToDictionary(x => x.Id);
        var requestedRoles = request.Roles ?? [];
        if (requestedRoles.Any(item => !roleMap.ContainsKey(item.RoleId)))
        {
            return new AdminSecurityCommandResult<PermissionMatrixResponse>(
                AdminSecurityCommandStatus.ValidationError,
                ErrorMessage: "One or more selected roles do not exist.",
                ErrorCode: ApiErrorCodes.RolesNotFound);
        }

        var invalidPermission = requestedRoles
            .SelectMany(x => x.PermissionKeys ?? [])
            .FirstOrDefault(key => !Permissions.All.Contains(key, StringComparer.Ordinal));
        if (!string.IsNullOrWhiteSpace(invalidPermission))
        {
            return new AdminSecurityCommandResult<PermissionMatrixResponse>(
                AdminSecurityCommandStatus.ValidationError,
                ErrorMessage: "One or more permission keys are invalid.",
                ErrorCode: ApiErrorCodes.InvalidPermissionKey);
        }

        var now = DateTimeOffset.UtcNow;
        var existingEntries = await dbContext.Set<PermissionMatrixEntryEntity>().ToListAsync(cancellationToken);
        dbContext.Set<PermissionMatrixEntryEntity>().RemoveRange(existingEntries);

        var entries = requestedRoles
            .SelectMany(role =>
            {
                var roleEntity = roleMap[role.RoleId];
                var granted = new HashSet<string>(role.PermissionKeys ?? [], StringComparer.Ordinal);
                return Permissions.All.Select(permission => new PermissionMatrixEntryEntity
                {
                    Id = Guid.NewGuid(),
                    RoleKeycloakName = roleEntity.KeycloakRoleName,
                    PermissionKey = permission,
                    IsGranted = granted.Contains(permission),
                    AppliedAt = now,
                    AppliedBy = actor,
                    Reason = reason
                });
            })
            .ToArray();

        dbContext.Set<PermissionMatrixEntryEntity>().AddRange(entries);
        await dbContext.SaveChangesAsync(cancellationToken);
        await referenceDataCache.InvalidateAppRolesAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "apply",
            EntityType: "permission_matrix",
            StatusCode: StatusCodes.Status200OK,
            Reason: reason,
            Metadata: new
            {
                roleCount = requestedRoles.Count,
                entryCount = entries.Length
            },
            After: new
            {
                appliedAt = now,
                appliedBy = actor
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        var responseRoles = roles
            .Select(role => new PermissionRoleGrantResponse(
                role.Id,
                role.Name,
                role.KeycloakRoleName,
                permissionMatrix.GetPermissions([role.KeycloakRoleName]).OrderBy(x => x, StringComparer.Ordinal).ToArray()))
            .ToArray();

        return new AdminSecurityCommandResult<PermissionMatrixResponse>(
            AdminSecurityCommandStatus.Success,
            new PermissionMatrixResponse(
                "applied",
                now,
                actor,
                reason,
                Permissions.All.Select(key => new PermissionCatalogItemResponse(key, Permissions.GetDisplayName(key))).ToArray(),
                responseRoles));
    }

    public async Task<AdminSecurityCommandResult<SystemSettingsResponse>> UpdateSystemSettingsAsync(
        string actor,
        UpdateSystemSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var reason = request.Reason?.Trim();
        if (string.IsNullOrWhiteSpace(reason))
        {
            return new AdminSecurityCommandResult<SystemSettingsResponse>(
                AdminSecurityCommandStatus.ValidationError,
                ErrorMessage: "Reason is required.",
                ErrorCode: ApiErrorCodes.ReasonRequired);
        }

        if (request.SessionIdleTimeoutMinutes <= 0
            || request.SessionWarningMinutes <= 0
            || request.SessionWarningMinutes >= request.SessionIdleTimeoutMinutes
            || request.RedisSessionTtlMinutes < request.SessionIdleTimeoutMinutes
            || request.RedisUserCacheTtlMinutes <= 0
            || request.PermissionMatrixCacheTtlMinutes <= 0)
        {
            return new AdminSecurityCommandResult<SystemSettingsResponse>(
                AdminSecurityCommandStatus.ValidationError,
                ErrorMessage: "One or more system setting values are invalid.",
                ErrorCode: ApiErrorCodes.InvalidSystemSettingValue);
        }

        var before = await dbContext.Set<SystemSettingEntity>()
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [SystemSettingKeys.SessionIdleTimeoutMinutes] = request.SessionIdleTimeoutMinutes.ToString(),
            [SystemSettingKeys.SessionWarningMinutes] = request.SessionWarningMinutes.ToString(),
            [SystemSettingKeys.RedisSessionTtlMinutes] = request.RedisSessionTtlMinutes.ToString(),
            [SystemSettingKeys.RedisUserCacheTtlMinutes] = request.RedisUserCacheTtlMinutes.ToString(),
            [SystemSettingKeys.PermissionMatrixCacheTtlMinutes] = request.PermissionMatrixCacheTtlMinutes.ToString(),
            [SystemSettingKeys.KeycloakRoleMappingRequired] = request.KeycloakRoleMappingRequired.ToString()
        };

        var now = DateTimeOffset.UtcNow;
        foreach (var (key, value) in values)
        {
            var entity = await dbContext.Set<SystemSettingEntity>().FindAsync([key], cancellationToken);
            if (entity is null)
            {
                entity = new SystemSettingEntity
                {
                    Key = key,
                    Value = value,
                    UpdatedAt = now,
                    UpdatedBy = actor,
                    Reason = reason
                };
                dbContext.Set<SystemSettingEntity>().Add(entity);
            }
            else
            {
                entity.Value = value;
                entity.UpdatedAt = now;
                entity.UpdatedBy = actor;
                entity.Reason = reason;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var afterEntities = await dbContext.Set<SystemSettingEntity>()
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Key, x => x, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var response = AdminSecurityQueries.MapSettings(afterEntities, options.Value);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "update",
            EntityType: "system_settings",
            StatusCode: StatusCodes.Status200OK,
            Reason: reason,
            Before: before,
            After: values));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AdminSecurityCommandResult<SystemSettingsResponse>(AdminSecurityCommandStatus.Success, response);
    }
}

internal static class SystemSettingKeys
{
    public const string SessionIdleTimeoutMinutes = "session_idle_timeout_minutes";
    public const string SessionWarningMinutes = "session_warning_minutes";
    public const string RedisSessionTtlMinutes = "redis_session_ttl_minutes";
    public const string RedisUserCacheTtlMinutes = "redis_user_cache_ttl_minutes";
    public const string PermissionMatrixCacheTtlMinutes = "permission_matrix_cache_ttl_minutes";
    public const string KeycloakRoleMappingRequired = "keycloak_role_mapping_required";
}
