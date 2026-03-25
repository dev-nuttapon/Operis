using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Operis_API.Modules.Users.Infrastructure;

public sealed class KeycloakUserCache(IDistributedCache cache) : IKeycloakUserCache
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12)
    };

    private const string CachePrefix = "keycloak:user:";

    public async Task<KeycloakUserProfile?> GetAsync(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var cached = await cache.GetStringAsync(BuildKey(userId), cancellationToken);
        return string.IsNullOrWhiteSpace(cached)
            ? null
            : JsonSerializer.Deserialize<KeycloakUserProfile>(cached, JsonOptions);
    }

    public Task SetAsync(string userId, KeycloakUserProfile profile, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.CompletedTask;
        }

        var payload = JsonSerializer.Serialize(profile);
        return cache.SetStringAsync(BuildKey(userId), payload, CacheOptions, cancellationToken);
    }

    public Task InvalidateAsync(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.CompletedTask;
        }

        return cache.RemoveAsync(BuildKey(userId), cancellationToken);
    }

    private static string BuildKey(string userId) => $"{CachePrefix}{userId}";
}
