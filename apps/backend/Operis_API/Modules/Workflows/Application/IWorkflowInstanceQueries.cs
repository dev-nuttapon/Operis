namespace Operis_API.Modules.Workflows;

public interface IWorkflowInstanceQueries
{
    Task<WorkflowInstanceDetailContract?> GetInstanceAsync(Guid workflowInstanceId, CancellationToken cancellationToken);
    Task<WorkflowInstanceDetailContract?> GetInstanceByDocumentAsync(Guid documentId, CancellationToken cancellationToken);
}
