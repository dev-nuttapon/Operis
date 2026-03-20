namespace Operis_API.Modules.Workflows;

public sealed record WorkflowDefinitionContract(
    Guid Id,
    string Code,
    string Name,
    string Status);

public sealed record WorkflowStepContract(
    Guid Id,
    string Name,
    string StepType,
    int DisplayOrder,
    bool IsRequired,
    IReadOnlyList<Guid> RoleIds);

public sealed record WorkflowDefinitionDetailContract(
    Guid Id,
    string Code,
    string Name,
    string Status,
    IReadOnlyList<WorkflowStepContract> Steps);

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

public sealed record WorkflowStepRequest(
    string Name,
    string StepType,
    int DisplayOrder,
    bool IsRequired,
    IReadOnlyList<Guid> RoleIds);

public sealed record CreateWorkflowDefinitionRequest(
    string Name,
    IReadOnlyList<WorkflowStepRequest> Steps);

public sealed record UpdateWorkflowDefinitionRequest(
    string Name,
    IReadOnlyList<WorkflowStepRequest> Steps);
