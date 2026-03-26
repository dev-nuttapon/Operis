namespace Operis_API.Modules.Users.Contracts;

public sealed record PermissionCatalogItemResponse(string Key, string Label);

public sealed record PermissionRoleGrantResponse(
    Guid RoleId,
    string RoleName,
    string RoleKeycloakName,
    IReadOnlyList<string> GrantedPermissions);

public sealed record PermissionMatrixResponse(
    string State,
    DateTimeOffset? AppliedAt,
    string? AppliedBy,
    string? AppliedReason,
    IReadOnlyList<PermissionCatalogItemResponse> Permissions,
    IReadOnlyList<PermissionRoleGrantResponse> Roles);

public sealed record ApplyPermissionMatrixRoleRequest(Guid RoleId, IReadOnlyList<string> PermissionKeys);

public sealed record ApplyPermissionMatrixRequest(string Reason, IReadOnlyList<ApplyPermissionMatrixRoleRequest> Roles);

public sealed record SystemSettingsResponse(
    int SessionIdleTimeoutMinutes,
    int SessionWarningMinutes,
    int RedisSessionTtlMinutes,
    int RedisUserCacheTtlMinutes,
    int PermissionMatrixCacheTtlMinutes,
    bool KeycloakRoleMappingRequired,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy,
    string? Reason);

public sealed record UpdateSystemSettingsRequest(
    string Reason,
    int SessionIdleTimeoutMinutes,
    int SessionWarningMinutes,
    int RedisSessionTtlMinutes,
    int RedisUserCacheTtlMinutes,
    int PermissionMatrixCacheTtlMinutes,
    bool KeycloakRoleMappingRequired);
