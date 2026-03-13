namespace Operis_API.Modules.Workflows;

public sealed record WorkflowDefinitionContract(
    Guid Id,
    string Code,
    string Name,
    string Status);

public sealed record CreateWorkflowDefinitionRequest(string Name);
