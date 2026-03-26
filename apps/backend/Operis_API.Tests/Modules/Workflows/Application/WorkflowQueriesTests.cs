using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Workflows;
using Operis_API.Modules.Workflows.Infrastructure;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Workflows.Application;

public sealed class WorkflowQueriesTests
{
    [Fact]
    public async Task ListDefinitionsAsync_ReturnsPersistedWorkflowDefinitionsInDescendingCreatedOrder()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.WorkflowDefinitions.AddRange(
            new WorkflowDefinitionEntity
            {
                Id = Guid.NewGuid(),
                Code = "policy-approval",
                Name = "Policy Approval",
                Status = "draft",
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
            },
            new WorkflowDefinitionEntity
            {
                Id = Guid.NewGuid(),
                Code = "document-review",
                Name = "Document Review",
                Status = "active",
                CreatedAt = DateTimeOffset.UtcNow
            });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var sut = new WorkflowQueries(dbContext, new FakeWorkflowDefinitionCacheWithItems(), auditLogWriter);

        var result = await sut.ListDefinitionsAsync(new WorkflowDefinitionListQuery(null, 1, 10), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal("document-review", result.Items[0].Code);
        Assert.Equal("active", result.Items[0].Status);
        Assert.Single(auditLogWriter.Entries);
    }

    private sealed class FakeWorkflowDefinitionCacheWithItems : IWorkflowDefinitionCache
    {
        public Task<IReadOnlyList<WorkflowDefinitionContract>> GetDefinitionsAsync(OperisDbContext db, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<WorkflowDefinitionContract>>(
                db.WorkflowDefinitions
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new WorkflowDefinitionContract(x.Id, x.Code, x.Name, x.Status, null))
                    .ToList());

        public Task<int> RefreshAsync(OperisDbContext db, CancellationToken cancellationToken) => Task.FromResult(0);
        public Task InvalidateAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
