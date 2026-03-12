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

        var payload = JsonSerializer.Serialize(new KeycloakCreateUserDto
        {
            Username = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Enabled = true,
            EmailVerified = true
        }, JsonOptions);

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

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/roles");
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
            .Select(x => new KeycloakRole(x.Id!, x.Name!, x.Description))
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

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}/role-mappings/realm");
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
            .Select(x => new KeycloakRole(x.Id!, x.Name!, x.Description))
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

        var allRoles = await ListRealmRolesAsync(cancellationToken);
        var payloadRoles = allRoles
            .Where(role => requestedRoles.Contains(role.Name, StringComparer.OrdinalIgnoreCase))
            .Select(role => new KeycloakRoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description
            })
            .ToList();

        if (payloadRoles.Count != requestedRoles.Length)
        {
            return false;
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var payload = JsonSerializer.Serialize(payloadRoles, JsonOptions);
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{TrimTrailingSlash(_options.BaseUrl)}/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}/role-mappings/realm");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
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

    private sealed class KeycloakCreateUserDto
    {
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public bool Enabled { get; init; }
        public bool EmailVerified { get; init; }
    }
}
