namespace Operis_API.Modules.Documents.Application;

public sealed record DocumentTemplateListQuery(
    string? Search,
    int Page,
    int PageSize);
