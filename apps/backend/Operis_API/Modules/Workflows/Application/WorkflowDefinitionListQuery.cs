namespace Operis_API.Modules.Workflows;

public sealed record WorkflowDefinitionListQuery(
    string? Status,
    int Page,
    int PageSize);
