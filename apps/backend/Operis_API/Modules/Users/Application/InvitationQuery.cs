using Operis_API.Modules.Users.Domain;

namespace Operis_API.Modules.Users.Application;

public sealed record InvitationQuery(
    InvitationStatus? Status,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? Search,
    string? SortBy,
    string? SortOrder,
    int Page = 1,
    int PageSize = 10);
