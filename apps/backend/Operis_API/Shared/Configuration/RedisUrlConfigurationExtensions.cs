namespace Operis_API.Shared.Configuration;

public static class RedisUrlConfigurationExtensions
{
    public static void ApplyRedisUrlOverride(this ConfigurationManager configuration)
    {
        var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL");
        if (string.IsNullOrWhiteSpace(redisUrl))
        {
            return;
        }

        var connectionString = BuildRedisConnectionString(redisUrl);
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Redis:ConnectionString"] = connectionString
        });
    }

    private static string BuildRedisConnectionString(string redisUrl)
    {
        if (!Uri.TryCreate(redisUrl, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException("Invalid REDIS_URL format.");
        }

        if (!string.Equals(uri.Scheme, "redis", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, "rediss", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("REDIS_URL must use redis scheme.");
        }

        var userInfoParts = uri.UserInfo.Split(':', 2, StringSplitOptions.None);
        var password = userInfoParts.Length > 1
            ? Uri.UnescapeDataString(userInfoParts[1])
            : Uri.UnescapeDataString(userInfoParts.ElementAtOrDefault(0) ?? string.Empty);
        var database = uri.AbsolutePath.Trim('/').Length == 0 ? "0" : uri.AbsolutePath.Trim('/');

        var parts = new List<string>
        {
            $"{uri.Host}:{uri.Port}"
        };

        if (!string.IsNullOrWhiteSpace(password))
        {
            parts.Add($"password={password}");
        }

        parts.Add($"defaultDatabase={database}");
        parts.Add("abortConnect=false");

        if (string.Equals(uri.Scheme, "rediss", StringComparison.OrdinalIgnoreCase))
        {
            parts.Add("ssl=true");
        }

        return string.Join(',', parts);
    }
}
