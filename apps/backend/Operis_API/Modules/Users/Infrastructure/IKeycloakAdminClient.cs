namespace Operis_API.Modules.Users.Infrastructure;

public interface IKeycloakAdminClient
{
    Task<KeycloakUserProfile?> FindUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<KeycloakUserProfile?> GetUserByIdAsync(string keycloakUserId, CancellationToken cancellationToken);
    Task<KeycloakCreateUserResult> CreateUserAsync(string email, string firstName, string lastName, CancellationToken cancellationToken);
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
