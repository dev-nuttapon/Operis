using Operis_API.Modules.Users.Infrastructure;

namespace Operis_API.Tests.Support;

internal sealed class FakeKeycloakAdminClient : IKeycloakAdminClient
{
    public int FindUserByEmailCalls { get; private set; }
    public int GetUserByIdCalls { get; private set; }
    public int GetUserRealmRolesCalls { get; private set; }
    public int CreateUserCalls { get; private set; }
    public int UpdatePasswordCalls { get; private set; }
    public int AssignRealmRolesCalls { get; private set; }
    public int SetManagedRolesCalls { get; private set; }
    public int DisableUserCalls { get; private set; }

    public KeycloakUserProfile? FindUserByEmailResult { get; set; }
    public KeycloakCreateUserResult CreateUserResult { get; set; } = new(true, false, Guid.NewGuid().ToString("N"), null);
    public KeycloakUpdateUserResult UpdateUserResult { get; set; } = new(true, false, null);
    public KeycloakUpdateUserResult UpdatePasswordResult { get; set; } = new(true, false, null);
    public KeycloakUpdateUserResult DisableUserResult { get; set; } = new(true, false, null);
    public bool AssignRealmRolesResult { get; set; } = true;
    public bool SetManagedRolesResult { get; set; } = true;

    public Task<KeycloakUserProfile?> FindUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        FindUserByEmailCalls++;
        return Task.FromResult(FindUserByEmailResult);
    }

    public Task<KeycloakUserProfile?> GetUserByIdAsync(string keycloakUserId, CancellationToken cancellationToken)
    {
        GetUserByIdCalls++;
        return Task.FromResult<KeycloakUserProfile?>(new KeycloakUserProfile(keycloakUserId, $"{keycloakUserId}@example.com", keycloakUserId, "Test", "User", true, true));
    }

    public Task<IReadOnlyList<KeycloakUserProfile>> SearchUsersAsync(string search, int first, int max, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<KeycloakUserProfile>>([]);

    public Task<KeycloakCreateUserResult> CreateUserAsync(string email, string firstName, string lastName, string? password, CancellationToken cancellationToken)
    {
        CreateUserCalls++;
        return Task.FromResult(CreateUserResult);
    }

    public Task<KeycloakUpdateUserResult> UpdateUserAsync(string keycloakUserId, string email, string firstName, string lastName, CancellationToken cancellationToken) =>
        Task.FromResult(UpdateUserResult);

    public Task<KeycloakUpdateUserResult> UpdatePasswordAsync(string keycloakUserId, string password, bool temporary, CancellationToken cancellationToken)
    {
        UpdatePasswordCalls++;
        return Task.FromResult(UpdatePasswordResult);
    }

    public Task<KeycloakPasswordValidationResult> ValidateUserPasswordAsync(string username, string password, CancellationToken cancellationToken) =>
        Task.FromResult(new KeycloakPasswordValidationResult(true, false, null));

    public Task<KeycloakUpdateUserResult> DisableUserAsync(string keycloakUserId, CancellationToken cancellationToken)
    {
        DisableUserCalls++;
        return Task.FromResult(DisableUserResult);
    }

    public Task<KeycloakUpdateUserResult> DeleteUserAsync(string keycloakUserId, CancellationToken cancellationToken) =>
        Task.FromResult(new KeycloakUpdateUserResult(true, false, null));

    public Task<KeycloakUpdateUserResult> ExecuteActionsEmailAsync(string keycloakUserId, IEnumerable<string> actions, CancellationToken cancellationToken) =>
        Task.FromResult(new KeycloakUpdateUserResult(true, false, null));

    public Task<IReadOnlyList<KeycloakRole>> ListRealmRolesAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<KeycloakRole>>([]);

    public Task<IReadOnlyList<KeycloakRole>> GetUserRealmRolesAsync(string keycloakUserId, CancellationToken cancellationToken)
    {
        GetUserRealmRolesCalls++;
        return Task.FromResult<IReadOnlyList<KeycloakRole>>([]);
    }

    public Task<bool> AssignRealmRolesAsync(string keycloakUserId, IEnumerable<string> roleNames, CancellationToken cancellationToken)
    {
        AssignRealmRolesCalls++;
        return Task.FromResult(AssignRealmRolesResult);
    }

    public Task<bool> SetManagedRolesAsync(string keycloakUserId, IEnumerable<string> managedRoleNames, IEnumerable<string> desiredRoleNames, CancellationToken cancellationToken)
    {
        SetManagedRolesCalls++;
        return Task.FromResult(SetManagedRolesResult);
    }
}
