namespace Operis_API.Modules.Workflows;

public interface IWorkflowCommands
{
    Task<WorkflowCommandResult> CreateDefinitionAsync(CreateWorkflowDefinitionRequest request, CancellationToken cancellationToken);
    Task<WorkflowCommandResult> UpdateDefinitionAsync(Guid workflowDefinitionId, UpdateWorkflowDefinitionRequest request, CancellationToken cancellationToken);
    Task<WorkflowCommandResult> ActivateDefinitionAsync(Guid workflowDefinitionId, CancellationToken cancellationToken);
    Task<WorkflowCommandResult> ArchiveDefinitionAsync(Guid workflowDefinitionId, CancellationToken cancellationToken);
}
