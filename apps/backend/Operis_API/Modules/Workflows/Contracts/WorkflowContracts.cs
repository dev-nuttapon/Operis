namespace Operis_API.Modules.Workflows;

public sealed record WorkflowDefinitionContract(
    Guid Id,
    string Code,
    string Name,
    string Status);

public sealed record WorkflowDefinitionStatusSummary(
    int All,
    int Draft,
    int Active,
    int Archived);

public sealed record WorkflowDefinitionListResponse(
    IReadOnlyList<WorkflowDefinitionContract> Items,
    int Total,
    int Page,
    int PageSize,
    WorkflowDefinitionStatusSummary StatusSummary);

public sealed record CreateWorkflowDefinitionRequest(string Name);
public sealed record UpdateWorkflowDefinitionRequest(string Name);
