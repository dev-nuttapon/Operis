namespace Operis_API.Shared.Configuration;

public sealed class Phase0SecurityOptions
{
    public const string SectionName = "SecurityAccess";

    public int SessionIdleTimeoutMinutes { get; set; } = 30;
    public int SessionWarningMinutes { get; set; } = 5;
    public int RedisSessionTtlMinutes { get; set; } = 60;
    public int RedisUserCacheTtlMinutes { get; set; } = 30;
    public int PermissionMatrixCacheTtlMinutes { get; set; } = 15;
    public bool KeycloakRoleMappingRequired { get; set; } = true;
}
