using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Operis_API.Modules.Users.Infrastructure;

public sealed class KeycloakAdminClient(HttpClient httpClient, IOptions<KeycloakOptions> options) : IKeycloakAdminClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly KeycloakOptions _options = options.Value;
    private string? _accessToken;
    private DateTimeOffset _accessTokenExpiresAt;

    public async Task<KeycloakUserProfile?> FindUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        if (!IsConfigured())
        {
            return null;
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users?email={Uri.EscapeDataString(email)}&exact=true");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var users = JsonSerializer.Deserialize<List<KeycloakUserDto>>(json, JsonOptions) ?? [];
        var user = users.FirstOrDefault();
        return user is null ? null : ToProfile(user);
    }

    public async Task<KeycloakUserProfile?> GetUserByIdAsync(string keycloakUserId, CancellationToken cancellationToken)
    {
        if (!IsConfigured() || string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return null;
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var user = JsonSerializer.Deserialize<KeycloakUserDto>(json, JsonOptions);
        return user is null ? null : ToProfile(user);
    }

    public async Task<KeycloakCreateUserResult> CreateUserAsync(
        string email,
        string firstName,
        string lastName,
        string? password,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured())
        {
            return new KeycloakCreateUserResult(false, false, null, "Keycloak is not configured.");
        }

        var existing = await FindUserByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            return new KeycloakCreateUserResult(true, true, existing.Id, null);
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return new KeycloakCreateUserResult(false, false, null, "Unable to acquire Keycloak access token.");
        }

        var createUserPayload = new KeycloakCreateUserDto
        {
            Username = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Enabled = true,
            EmailVerified = true
        };
        if (!string.IsNullOrWhiteSpace(password))
        {
            createUserPayload.Credentials =
            [
                new KeycloakCredentialDto
                {
                    Type = "password",
                    Value = password,
                    Temporary = false
                }
            ];
        }

        var payload = JsonSerializer.Serialize(createUserPayload, JsonOptions);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var conflictedUser = await FindUserByEmailAsync(email, cancellationToken);
            return new KeycloakCreateUserResult(
                conflictedUser is not null,
                true,
                conflictedUser?.Id,
                conflictedUser is null ? "Keycloak user already exists but cannot resolve id." : null);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return new KeycloakCreateUserResult(false, false, null, error);
        }

        var userId = ExtractUserIdFromLocation(response.Headers.Location);
        if (string.IsNullOrWhiteSpace(userId))
        {
            var created = await FindUserByEmailAsync(email, cancellationToken);
            userId = created?.Id;
        }

        return new KeycloakCreateUserResult(true, false, userId, null);
    }

    public async Task<KeycloakUpdateUserResult> UpdateUserAsync(
        string keycloakUserId,
        string email,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured() || string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return new KeycloakUpdateUserResult(false, false, "Keycloak is not configured.");
        }

        var existing = await GetUserByIdAsync(keycloakUserId, cancellationToken);
        if (existing is null)
        {
            return new KeycloakUpdateUserResult(false, false, "Keycloak user was not found.");
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return new KeycloakUpdateUserResult(false, false, "Unable to acquire Keycloak access token.");
        }

        var payload = JsonSerializer.Serialize(new KeycloakCreateUserDto
        {
            Username = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Enabled = existing.Enabled,
            EmailVerified = existing.EmailVerified
        }, JsonOptions);

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            return new KeycloakUpdateUserResult(false, true, "Keycloak user already exists.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return new KeycloakUpdateUserResult(false, false, error);
        }

        return new KeycloakUpdateUserResult(true, false, null);
    }

    public async Task<KeycloakUpdateUserResult> DisableUserAsync(string keycloakUserId, CancellationToken cancellationToken)
    {
        if (!IsConfigured() || string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return new KeycloakUpdateUserResult(false, false, "Keycloak is not configured.");
        }

        var existing = await GetUserByIdAsync(keycloakUserId, cancellationToken);
        if (existing is null)
        {
            return new KeycloakUpdateUserResult(false, false, "Keycloak user was not found.");
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return new KeycloakUpdateUserResult(false, false, "Unable to acquire Keycloak access token.");
        }

        var payload = JsonSerializer.Serialize(new KeycloakCreateUserDto
        {
            Username = existing.Username,
            Email = existing.Email,
            FirstName = existing.FirstName ?? string.Empty,
            LastName = existing.LastName ?? string.Empty,
            Enabled = false,
            EmailVerified = existing.EmailVerified
        }, JsonOptions);

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return new KeycloakUpdateUserResult(false, false, error);
        }

        return new KeycloakUpdateUserResult(true, false, null);
    }

    public async Task<KeycloakUpdateUserResult> DeleteUserAsync(string keycloakUserId, CancellationToken cancellationToken)
    {
        if (!IsConfigured() || string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return new KeycloakUpdateUserResult(false, false, "Keycloak is not configured.");
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return new KeycloakUpdateUserResult(false, false, "Unable to acquire Keycloak access token.");
        }

        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return new KeycloakUpdateUserResult(false, false, error);
        }

        return new KeycloakUpdateUserResult(true, false, null);
    }

    public async Task<KeycloakUpdateUserResult> ExecuteActionsEmailAsync(
        string keycloakUserId,
        IEnumerable<string> actions,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured() || string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return new KeycloakUpdateUserResult(false, false, "Keycloak is not configured.");
        }

        var actionList = actions
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (actionList.Length == 0)
        {
            return new KeycloakUpdateUserResult(false, false, "No required actions were provided.");
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return new KeycloakUpdateUserResult(false, false, "Unable to acquire Keycloak access token.");
        }

        var payload = JsonSerializer.Serialize(actionList, JsonOptions);
        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}/execute-actions-email");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return new KeycloakUpdateUserResult(false, false, error);
        }

        return new KeycloakUpdateUserResult(true, false, null);
    }

    public async Task<KeycloakUpdateUserResult> UpdatePasswordAsync(
        string keycloakUserId,
        string password,
        bool temporary,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured() || string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return new KeycloakUpdateUserResult(false, false, "Keycloak is not configured.");
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return new KeycloakUpdateUserResult(false, false, "Unable to acquire Keycloak access token.");
        }

        var payload = JsonSerializer.Serialize(new KeycloakCredentialDto
        {
            Type = "password",
            Value = password,
            Temporary = temporary
        }, JsonOptions);

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}/reset-password");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return new KeycloakUpdateUserResult(false, false, error);
        }

        return new KeycloakUpdateUserResult(true, false, null);
    }

    public async Task<IReadOnlyList<KeycloakRole>> ListRealmRolesAsync(CancellationToken cancellationToken)
    {
        if (!IsConfigured())
        {
            return [];
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        var roles = new List<KeycloakRole>();
        roles.AddRange(await ListRolesAsync(
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/roles",
            token,
            cancellationToken));

        var clientRoles = await ListConfiguredClientRolesAsync(token, cancellationToken);
        if (clientRoles.Count > 0)
        {
            roles.AddRange(clientRoles);
        }

        return roles
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<KeycloakRole>> GetUserRealmRolesAsync(string keycloakUserId, CancellationToken cancellationToken)
    {
        if (!IsConfigured() || string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return [];
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        var roles = new List<KeycloakRole>();
        roles.AddRange(await ListRolesAsync(
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}/role-mappings/realm",
            token,
            cancellationToken));

        var clientUuid = await ResolveRoleClientUuidAsync(token, cancellationToken);
        if (!string.IsNullOrWhiteSpace(clientUuid))
        {
            roles.AddRange(await ListRolesAsync(
                $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}/role-mappings/clients/{Uri.EscapeDataString(clientUuid)}",
                token,
                cancellationToken));
        }

        return roles
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<bool> AssignRealmRolesAsync(string keycloakUserId, IEnumerable<string> roleNames, CancellationToken cancellationToken)
    {
        if (!IsConfigured() || string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return false;
        }

        var requestedRoles = roleNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (requestedRoles.Length == 0)
        {
            return true;
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var realmRoles = await ListRolesAsync(
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/roles",
            token,
            cancellationToken);
        var matchedRoleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var realmPayload = realmRoles
            .Where(role => requestedRoles.Contains(role.Name, StringComparer.OrdinalIgnoreCase))
            .Select(role =>
            {
                matchedRoleNames.Add(role.Name);
                return ToRoleDto(role);
            })
            .ToList();

        var clientUuid = await ResolveRoleClientUuidAsync(token, cancellationToken);
        var clientPayload = new List<KeycloakRoleDto>();
        if (!string.IsNullOrWhiteSpace(clientUuid))
        {
            var clientRoles = await ListRolesAsync(
                $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/clients/{Uri.EscapeDataString(clientUuid)}/roles",
                token,
                cancellationToken);
            clientPayload = clientRoles
                .Where(role => requestedRoles.Contains(role.Name, StringComparer.OrdinalIgnoreCase) && !matchedRoleNames.Contains(role.Name))
                .Select(role =>
                {
                    matchedRoleNames.Add(role.Name);
                    return ToRoleDto(role);
                })
                .ToList();
        }

        if (matchedRoleNames.Count != requestedRoles.Length)
        {
            return false;
        }

        if (realmPayload.Count > 0)
        {
            var assignedRealm = await AssignRolesAsync(
                $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}/role-mappings/realm",
                realmPayload,
                token,
                cancellationToken);
            if (!assignedRealm)
            {
                return false;
            }
        }

        if (clientPayload.Count > 0 && !string.IsNullOrWhiteSpace(clientUuid))
        {
            var assignedClient = await AssignRolesAsync(
                $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}/role-mappings/clients/{Uri.EscapeDataString(clientUuid)}",
                clientPayload,
                token,
                cancellationToken);
            if (!assignedClient)
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> SetManagedRolesAsync(
        string keycloakUserId,
        IEnumerable<string> managedRoleNames,
        IEnumerable<string> desiredRoleNames,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured() || string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return false;
        }

        var managedNames = managedRoleNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (managedNames.Length == 0)
        {
            return true;
        }

        var desiredNames = desiredRoleNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var allRoles = await ListRealmRolesAsync(cancellationToken);
        var managedRoles = allRoles
            .Where(role => managedNames.Contains(role.Name, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(role => role.Name, StringComparer.OrdinalIgnoreCase);
        if (managedRoles.Count != managedNames.Length)
        {
            return false;
        }

        var desiredRoles = desiredNames
            .Select(name => managedRoles.TryGetValue(name, out var role) ? role : null)
            .ToArray();
        if (desiredRoles.Any(role => role is null))
        {
            return false;
        }

        var currentRoles = await GetUserRealmRolesAsync(keycloakUserId, cancellationToken);
        var currentManagedRoles = currentRoles
            .Where(role => managedNames.Contains(role.Name, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        var currentRoleSet = new HashSet<string>(currentManagedRoles.Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
        var desiredRoleSet = new HashSet<string>(desiredNames, StringComparer.OrdinalIgnoreCase);

        var rolesToAdd = desiredRoles
            .Where(role => role is not null && !currentRoleSet.Contains(role.Name))
            .Cast<KeycloakRole>()
            .ToArray();
        var rolesToRemove = currentManagedRoles
            .Where(role => !desiredRoleSet.Contains(role.Name))
            .ToArray();

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        if (rolesToRemove.Length > 0 && !await DeleteRolesAsync(keycloakUserId, rolesToRemove, token, cancellationToken))
        {
            return false;
        }

        if (rolesToAdd.Length > 0 && !await AddRolesAsync(keycloakUserId, rolesToAdd, token, cancellationToken))
        {
            return false;
        }

        return true;
    }

    private bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(_options.BaseUrl) &&
        !string.IsNullOrWhiteSpace(_options.Realm) &&
        !string.IsNullOrWhiteSpace(_options.ClientId) &&
        !string.IsNullOrWhiteSpace(_options.ClientSecret);

    private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_accessToken) && DateTimeOffset.UtcNow < _accessTokenExpiresAt)
        {
            return _accessToken;
        }

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{TrimTrailingSlash(_options.BaseUrl)}/realms/{_options.Realm}/protocol/openid-connect/token");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var token = JsonSerializer.Deserialize<KeycloakTokenDto>(json, JsonOptions);
        if (string.IsNullOrWhiteSpace(token?.AccessToken))
        {
            return null;
        }

        _accessToken = token.AccessToken;
        var expiresIn = token.ExpiresIn <= 0 ? 30 : token.ExpiresIn;
        _accessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(15, expiresIn - 15));
        return _accessToken;
    }

    private static KeycloakUserProfile ToProfile(KeycloakUserDto user) =>
        new(
            user.Id ?? string.Empty,
            user.Email ?? string.Empty,
            user.Username ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.Enabled ?? false,
            user.EmailVerified ?? false);

    private static string? ExtractUserIdFromLocation(Uri? location)
    {
        if (location is null)
        {
            return null;
        }

        var segments = location.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length == 0 ? null : segments[^1];
    }

    private static string TrimTrailingSlash(string value) => value.TrimEnd('/');

    private async Task<IReadOnlyList<KeycloakRole>> ListConfiguredClientRolesAsync(string token, CancellationToken cancellationToken)
    {
        var clientUuid = await ResolveRoleClientUuidAsync(token, cancellationToken);
        if (string.IsNullOrWhiteSpace(clientUuid))
        {
            return [];
        }

        return await ListRolesAsync(
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/clients/{Uri.EscapeDataString(clientUuid)}/roles",
            token,
            cancellationToken,
            true);
    }

    private async Task<string?> ResolveRoleClientUuidAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.RoleClientId))
        {
            return null;
        }

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/clients?clientId={Uri.EscapeDataString(_options.RoleClientId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var clients = JsonSerializer.Deserialize<List<KeycloakClientDto>>(json, JsonOptions) ?? [];
        return clients.FirstOrDefault()?.Id;
    }

    private async Task<List<KeycloakRole>> ListRolesAsync(string url, string token, CancellationToken cancellationToken, bool clientRole = false)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var roles = JsonSerializer.Deserialize<List<KeycloakRoleDto>>(json, JsonOptions) ?? [];
        return roles
            .Where(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => new KeycloakRole(x.Id!, x.Name!, x.Description, clientRole))
            .ToList();
    }

    private async Task<bool> AssignRolesAsync(string url, IReadOnlyList<KeycloakRoleDto> payloadRoles, string token, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(payloadRoles, JsonOptions);
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private async Task<bool> AddRolesAsync(string keycloakUserId, IReadOnlyList<KeycloakRole> roles, string token, CancellationToken cancellationToken)
    {
        var realmRoles = roles.Where(role => !role.ClientRole).Select(ToRoleDto).ToArray();
        var clientRoles = roles.Where(role => role.ClientRole).Select(ToRoleDto).ToArray();

        if (realmRoles.Length > 0)
        {
            var assignedRealm = await AssignRolesAsync(
                $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}/role-mappings/realm",
                realmRoles,
                token,
                cancellationToken);
            if (!assignedRealm)
            {
                return false;
            }
        }

        if (clientRoles.Length > 0)
        {
            var clientUuid = await ResolveRoleClientUuidAsync(token, cancellationToken);
            if (string.IsNullOrWhiteSpace(clientUuid))
            {
                return false;
            }

            var assignedClient = await AssignRolesAsync(
                $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}/role-mappings/clients/{Uri.EscapeDataString(clientUuid)}",
                clientRoles,
                token,
                cancellationToken);
            if (!assignedClient)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> DeleteRolesAsync(string keycloakUserId, IReadOnlyList<KeycloakRole> roles, string token, CancellationToken cancellationToken)
    {
        var realmRoles = roles.Where(role => !role.ClientRole).Select(ToRoleDto).ToArray();
        var clientRoles = roles.Where(role => role.ClientRole).Select(ToRoleDto).ToArray();

        if (realmRoles.Length > 0)
        {
            var removedRealm = await DeleteRoleMappingsAsync(
                $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}/role-mappings/realm",
                realmRoles,
                token,
                cancellationToken);
            if (!removedRealm)
            {
                return false;
            }
        }

        if (clientRoles.Length > 0)
        {
            var clientUuid = await ResolveRoleClientUuidAsync(token, cancellationToken);
            if (string.IsNullOrWhiteSpace(clientUuid))
            {
                return false;
            }

            var removedClient = await DeleteRoleMappingsAsync(
                $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}/role-mappings/clients/{Uri.EscapeDataString(clientUuid)}",
                clientRoles,
                token,
                cancellationToken);
            if (!removedClient)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> DeleteRoleMappingsAsync(string url, IReadOnlyList<KeycloakRoleDto> payloadRoles, string token, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(payloadRoles, JsonOptions);
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private static KeycloakRoleDto ToRoleDto(KeycloakRole role) => new()
    {
        Id = role.Id,
        Name = role.Name,
        Description = role.Description
    };

    private sealed class KeycloakTokenDto
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; init; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }
    }

    private sealed class KeycloakUserDto
    {
        public string? Id { get; init; }
        public string? Username { get; init; }
        public string? Email { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public bool? Enabled { get; init; }
        public bool? EmailVerified { get; init; }
    }

    private sealed class KeycloakRoleDto
    {
        public string? Id { get; init; }
        public string? Name { get; init; }
        public string? Description { get; init; }
    }

    private sealed class KeycloakClientDto
    {
        public string? Id { get; init; }
        public string? ClientId { get; init; }
    }

    private sealed class KeycloakCreateUserDto
    {
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public bool Enabled { get; init; }
        public bool EmailVerified { get; init; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<KeycloakCredentialDto>? Credentials { get; set; }
    }

    private sealed class KeycloakCredentialDto
    {
        public string Type { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public bool Temporary { get; init; }
    }
}
