namespace Operis_API.Modules.Documents.Application;

public sealed record DocumentTemplateHistoryListQuery(
    Guid TemplateId,
    string? Search,
    int Page = 1,
    int PageSize = 10);
