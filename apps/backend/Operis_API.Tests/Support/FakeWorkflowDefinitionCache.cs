using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Workflows;
using Operis_API.Modules.Workflows.Infrastructure;

namespace Operis_API.Tests.Support;

internal sealed class FakeWorkflowDefinitionCache : IWorkflowDefinitionCache
{
    public Task<IReadOnlyList<WorkflowDefinitionContract>> GetDefinitionsAsync(OperisDbContext dbContext, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<WorkflowDefinitionContract>>([]);

    public Task<int> RefreshAsync(OperisDbContext dbContext, CancellationToken cancellationToken) =>
        Task.FromResult(0);

    public Task InvalidateAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
