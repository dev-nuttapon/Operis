using Operis_API.Modules.Users.Infrastructure;

namespace Operis_API.Tests.Support;

internal sealed class TestKeycloakUserCache : IKeycloakUserCache
{
    public Task<KeycloakUserProfile?> GetAsync(string keycloakUserId, CancellationToken cancellationToken) =>
        Task.FromResult<KeycloakUserProfile?>(null);

    public Task SetAsync(string userId, KeycloakUserProfile profile, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task InvalidateAsync(string keycloakUserId, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
