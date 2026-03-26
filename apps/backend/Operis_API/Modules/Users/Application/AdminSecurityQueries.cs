using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Configuration;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Users.Application;

public sealed class AdminSecurityQueries(
    OperisDbContext dbContext,
    IReferenceDataCache referenceDataCache,
    IAuditLogWriter auditLogWriter,
    IPermissionMatrix permissionMatrix,
    IOptions<Phase0SecurityOptions> options) : IAdminSecurityQueries
{
    public async Task<PermissionMatrixResponse> GetPermissionMatrixAsync(CancellationToken cancellationToken)
    {
        var roles = await referenceDataCache.GetAppRolesAsync(dbContext, cancellationToken);
        var appliedEntries = await dbContext.Set<PermissionMatrixEntryEntity>()
            .AsNoTracking()
            .OrderByDescending(x => x.AppliedAt)
            .ToListAsync(cancellationToken);

        var latestEntry = appliedEntries.FirstOrDefault();
        var roleGrants = roles
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(role => new PermissionRoleGrantResponse(
                role.Id,
                role.Name,
                role.KeycloakRoleName,
                permissionMatrix.GetPermissions([role.KeycloakRoleName]).OrderBy(x => x, StringComparer.Ordinal).ToArray()))
            .ToArray();

        var response = new PermissionMatrixResponse(
            latestEntry is null ? "draft" : "applied",
            latestEntry?.AppliedAt,
            latestEntry?.AppliedBy,
            latestEntry?.Reason,
            Permissions.All
                .Select(key => new PermissionCatalogItemResponse(key, Permissions.GetDisplayName(key)))
                .ToArray(),
            roleGrants);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "read",
            EntityType: "permission_matrix",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new { roleCount = roleGrants.Length, appliedEntries = appliedEntries.Count }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task<SystemSettingsResponse> GetSystemSettingsAsync(CancellationToken cancellationToken)
    {
        var stored = await dbContext.Set<SystemSettingEntity>()
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Key, x => x, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var current = MapSettings(stored, options.Value);
        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "read",
            EntityType: "system_settings",
            StatusCode: StatusCodes.Status200OK));
        await dbContext.SaveChangesAsync(cancellationToken);

        return current;
    }

    internal static SystemSettingsResponse MapSettings(
        IReadOnlyDictionary<string, SystemSettingEntity> stored,
        Phase0SecurityOptions defaults)
    {
        static int ReadInt(IReadOnlyDictionary<string, SystemSettingEntity> values, string key, int fallback) =>
            values.TryGetValue(key, out var value) && int.TryParse(value.Value, out var parsed) ? parsed : fallback;

        static bool ReadBool(IReadOnlyDictionary<string, SystemSettingEntity> values, string key, bool fallback) =>
            values.TryGetValue(key, out var value) && bool.TryParse(value.Value, out var parsed) ? parsed : fallback;

        var latest = stored.Values.OrderByDescending(x => x.UpdatedAt).FirstOrDefault();
        return new SystemSettingsResponse(
            ReadInt(stored, SystemSettingKeys.SessionIdleTimeoutMinutes, defaults.SessionIdleTimeoutMinutes),
            ReadInt(stored, SystemSettingKeys.SessionWarningMinutes, defaults.SessionWarningMinutes),
            ReadInt(stored, SystemSettingKeys.RedisSessionTtlMinutes, defaults.RedisSessionTtlMinutes),
            ReadInt(stored, SystemSettingKeys.RedisUserCacheTtlMinutes, defaults.RedisUserCacheTtlMinutes),
            ReadInt(stored, SystemSettingKeys.PermissionMatrixCacheTtlMinutes, defaults.PermissionMatrixCacheTtlMinutes),
            ReadBool(stored, SystemSettingKeys.KeycloakRoleMappingRequired, defaults.KeycloakRoleMappingRequired),
            latest?.UpdatedAt,
            latest?.UpdatedBy,
            latest?.Reason);
    }
}
