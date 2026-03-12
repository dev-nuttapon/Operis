namespace Operis_API.Shared.Configuration;

public static class DatabaseUrlConfigurationExtensions
{
    public static void ApplyDatabaseUrlOverride(this ConfigurationManager configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrWhiteSpace(databaseUrl))
        {
            return;
        }

        var connectionString = BuildPostgresConnectionString(databaseUrl);
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = connectionString
        });
    }

    private static string BuildPostgresConnectionString(string databaseUrl)
    {
        if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException("Invalid DATABASE_URL format.");
        }

        if (!string.Equals(uri.Scheme, "postgresql", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, "postgres", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("DATABASE_URL must use postgres scheme.");
        }

        var userInfoParts = uri.UserInfo.Split(':', 2, StringSplitOptions.None);
        var username = Uri.UnescapeDataString(userInfoParts.ElementAtOrDefault(0) ?? string.Empty);
        var password = Uri.UnescapeDataString(userInfoParts.ElementAtOrDefault(1) ?? string.Empty);
        var database = uri.AbsolutePath.TrimStart('/');
        var schema = ReadQueryValue(uri.Query, "schema");

        var parts = new List<string>
        {
            $"Host={uri.Host}",
            $"Port={uri.Port}",
            $"Database={database}",
            $"Username={username}",
            $"Password={password}",
            "SSL Mode=Disable",
            "Trust Server Certificate=true"
        };

        if (!string.IsNullOrWhiteSpace(schema))
        {
            parts.Add($"Search Path={schema}");
        }

        return string.Join(';', parts);
    }

    private static string? ReadQueryValue(string queryString, string key)
    {
        if (string.IsNullOrWhiteSpace(queryString))
        {
            return null;
        }

        var trimmed = queryString.TrimStart('?');
        var pairs = trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            var split = pair.Split('=', 2, StringSplitOptions.None);
            if (split.Length == 0)
            {
                continue;
            }

            var queryKey = Uri.UnescapeDataString(split[0]);
            if (!string.Equals(queryKey, key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return split.Length == 2 ? Uri.UnescapeDataString(split[1]) : string.Empty;
        }

        return null;
    }
}
