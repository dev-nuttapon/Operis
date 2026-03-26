using Operis_API.Modules.Users.Infrastructure;

namespace Operis_API.Tests.Support;

internal sealed class FakeKeycloakUserCache : IKeycloakUserCache
{
    public Task<KeycloakUserProfile?> GetAsync(string userId, CancellationToken cancellationToken) =>
        Task.FromResult<KeycloakUserProfile?>(null);

    public Task SetAsync(string userId, KeycloakUserProfile profile, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task InvalidateAsync(string userId, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
