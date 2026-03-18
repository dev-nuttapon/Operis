namespace Operis_API.Modules.Documents.Application;

public sealed record DocumentListQuery(
    string? Search,
    int Page = 1,
    int PageSize = 10);

public sealed record DocumentVersionListQuery(
    Guid DocumentId,
    string? Search,
    int Page = 1,
    int PageSize = 10);

public sealed record DocumentHistoryListQuery(
    Guid DocumentId,
    string? Search,
    int Page = 1,
    int PageSize = 10);
