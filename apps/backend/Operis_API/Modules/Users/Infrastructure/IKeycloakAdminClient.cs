namespace Operis_API.Modules.Users.Infrastructure;

public interface IKeycloakAdminClient
{
    Task<KeycloakUserProfile?> FindUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<KeycloakUserProfile?> GetUserByIdAsync(string keycloakUserId, CancellationToken cancellationToken);
    Task<KeycloakCreateUserResult> CreateUserAsync(string email, string firstName, string lastName, CancellationToken cancellationToken);
    Task<IReadOnlyList<KeycloakRole>> ListRealmRolesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<KeycloakRole>> GetUserRealmRolesAsync(string keycloakUserId, CancellationToken cancellationToken);
    Task<bool> AssignRealmRolesAsync(string keycloakUserId, IEnumerable<string> roleNames, CancellationToken cancellationToken);
}

public sealed record KeycloakCreateUserResult(bool Success, bool AlreadyExists, string? UserId, string? ErrorMessage);

public sealed record KeycloakUserProfile(
    string Id,
    string Email,
    string Username,
    string? FirstName,
    string? LastName,
    bool Enabled,
    bool EmailVerified);

public sealed record KeycloakRole(string Id, string Name, string? Description);
