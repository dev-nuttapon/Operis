using Operis_API.Infrastructure.Persistence;

namespace Operis_API.Modules.Workflows.Infrastructure;

public interface IWorkflowDefinitionCache
{
    Task<IReadOnlyList<WorkflowDefinitionContract>> GetDefinitionsAsync(OperisDbContext dbContext, CancellationToken cancellationToken);
    Task<int> RefreshAsync(OperisDbContext dbContext, CancellationToken cancellationToken);
    Task InvalidateAsync(CancellationToken cancellationToken);
}
