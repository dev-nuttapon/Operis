using Operis_API.Modules.Users.Infrastructure;

namespace Operis_API.Shared.Configuration;

public static class Phase0ConfigurationValidator
{
    public static Phase0SecurityOptions Validate(IConfiguration configuration)
    {
        var keycloakOptions = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>()
                              ?? throw new InvalidOperationException("Keycloak configuration is missing.");

        if (string.IsNullOrWhiteSpace(keycloakOptions.BaseUrl)
            || string.IsNullOrWhiteSpace(keycloakOptions.Realm)
            || string.IsNullOrWhiteSpace(keycloakOptions.ClientId))
        {
            throw new InvalidOperationException("Keycloak BaseUrl, Realm, and ClientId must be configured.");
        }

        var redisConnectionString = configuration["Redis:ConnectionString"];
        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new InvalidOperationException("Redis connection string must be configured for Phase 0.");
        }

        var options = configuration.GetSection(Phase0SecurityOptions.SectionName).Get<Phase0SecurityOptions>()
                      ?? new Phase0SecurityOptions();

        if (options.SessionIdleTimeoutMinutes <= 0)
        {
            throw new InvalidOperationException("SecurityAccess:SessionIdleTimeoutMinutes must be greater than zero.");
        }

        if (options.SessionWarningMinutes <= 0 || options.SessionWarningMinutes >= options.SessionIdleTimeoutMinutes)
        {
            throw new InvalidOperationException("SecurityAccess:SessionWarningMinutes must be greater than zero and less than SessionIdleTimeoutMinutes.");
        }

        if (options.RedisSessionTtlMinutes < options.SessionIdleTimeoutMinutes)
        {
            throw new InvalidOperationException("SecurityAccess:RedisSessionTtlMinutes must be at least the session idle timeout.");
        }

        if (options.RedisUserCacheTtlMinutes <= 0 || options.PermissionMatrixCacheTtlMinutes <= 0)
        {
            throw new InvalidOperationException("SecurityAccess cache TTL values must be greater than zero.");
        }

        return options;
    }
}
