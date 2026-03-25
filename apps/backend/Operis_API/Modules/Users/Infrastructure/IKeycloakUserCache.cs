namespace Operis_API.Modules.Users.Infrastructure;

public interface IKeycloakUserCache
{
    Task<KeycloakUserProfile?> GetAsync(string userId, CancellationToken cancellationToken);
    Task SetAsync(string userId, KeycloakUserProfile profile, CancellationToken cancellationToken);
    Task InvalidateAsync(string userId, CancellationToken cancellationToken);
}
