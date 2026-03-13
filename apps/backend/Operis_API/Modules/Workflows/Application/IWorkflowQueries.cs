namespace Operis_API.Modules.Workflows;

public interface IWorkflowQueries
{
    Task<IReadOnlyList<WorkflowDefinitionContract>> ListDefinitionsAsync(CancellationToken cancellationToken);
}
