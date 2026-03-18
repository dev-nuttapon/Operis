namespace Operis_API.Modules.Users.Infrastructure;

public interface IKeycloakAdminClient
{
    Task<KeycloakUserProfile?> FindUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<KeycloakUserProfile?> GetUserByIdAsync(string keycloakUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<KeycloakUserProfile>> SearchUsersAsync(string search, int first, int max, CancellationToken cancellationToken);
    Task<KeycloakCreateUserResult> CreateUserAsync(string email, string firstName, string lastName, string? password, CancellationToken cancellationToken);
    Task<KeycloakUpdateUserResult> UpdateUserAsync(string keycloakUserId, string email, string firstName, string lastName, CancellationToken cancellationToken);
    Task<KeycloakUpdateUserResult> UpdatePasswordAsync(string keycloakUserId, string password, bool temporary, CancellationToken cancellationToken);
    Task<KeycloakUpdateUserResult> DisableUserAsync(string keycloakUserId, CancellationToken cancellationToken);
    Task<KeycloakUpdateUserResult> DeleteUserAsync(string keycloakUserId, CancellationToken cancellationToken);
    Task<KeycloakUpdateUserResult> ExecuteActionsEmailAsync(string keycloakUserId, IEnumerable<string> actions, CancellationToken cancellationToken);
    Task<IReadOnlyList<KeycloakRole>> ListRealmRolesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<KeycloakRole>> GetUserRealmRolesAsync(string keycloakUserId, CancellationToken cancellationToken);
    Task<bool> AssignRealmRolesAsync(string keycloakUserId, IEnumerable<string> roleNames, CancellationToken cancellationToken);
    Task<bool> SetManagedRolesAsync(string keycloakUserId, IEnumerable<string> managedRoleNames, IEnumerable<string> desiredRoleNames, CancellationToken cancellationToken);
}

public sealed record KeycloakCreateUserResult(bool Success, bool AlreadyExists, string? UserId, string? ErrorMessage);
public sealed record KeycloakUpdateUserResult(bool Success, bool Conflict, string? ErrorMessage);

public sealed record KeycloakUserProfile(
    string Id,
    string Email,
    string Username,
    string? FirstName,
    string? LastName,
    bool Enabled,
    bool EmailVerified);

public sealed record KeycloakRole(string Id, string Name, string? Description, bool ClientRole);
