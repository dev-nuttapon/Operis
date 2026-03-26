using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Workflows.Infrastructure;

namespace Operis_API.Modules.Workflows;

public sealed class WorkflowCacheCommands(OperisDbContext dbContext, IWorkflowDefinitionCache cache) : IWorkflowCacheCommands
{
    public Task<int> RefreshDefinitionsAsync(CancellationToken cancellationToken) => cache.RefreshAsync(dbContext, cancellationToken);
}
