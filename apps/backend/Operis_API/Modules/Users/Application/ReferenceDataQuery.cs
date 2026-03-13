namespace Operis_API.Modules.Users.Application;

public sealed record ReferenceDataQuery(
    string? Search,
    string? SortBy,
    string? SortOrder,
    Guid? DivisionId = null,
    Guid? DepartmentId = null,
    int Page = 1,
    int PageSize = 10);
