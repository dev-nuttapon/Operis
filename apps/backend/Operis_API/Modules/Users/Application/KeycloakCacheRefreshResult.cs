using Operis_API.Modules.Users.Contracts;

namespace Operis_API.Modules.Users.Application;

public sealed record KeycloakCacheRefreshResult(
    UserCommandStatus Status,
    int Refreshed,
    int Missing,
    string? ErrorMessage = null,
    string? ErrorCode = null,
    string? ProblemTitle = null,
    int? ProblemStatusCode = null);
