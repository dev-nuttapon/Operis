namespace Operis_API.Modules.Workflows;

public interface IWorkflowCommands
{
    Task<WorkflowCommandResult> CreateDefinitionAsync(CreateWorkflowDefinitionRequest request, CancellationToken cancellationToken);
}
