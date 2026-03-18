namespace Operis_API.Modules.Workflows;

public interface IWorkflowQueries
{
    Task<WorkflowDefinitionListResponse> ListDefinitionsAsync(WorkflowDefinitionListQuery query, CancellationToken cancellationToken);
}
