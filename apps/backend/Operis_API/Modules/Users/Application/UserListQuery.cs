using Operis_API.Modules.Users.Domain;

namespace Operis_API.Modules.Users.Application;

public sealed record UserListQuery(
    bool IncludeIdentity = true,
    UserStatus? Status = null,
    Guid? DivisionId = null,
    Guid? DepartmentId = null,
    Guid? JobTitleId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    string? Search = null,
    string? SortBy = null,
    string? SortOrder = null,
    int Page = 1,
    int PageSize = 10);
