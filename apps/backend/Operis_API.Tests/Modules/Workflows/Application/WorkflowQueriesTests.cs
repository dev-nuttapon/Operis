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
        var sut = new WorkflowQueries(dbContext, auditLogWriter);

        var result = await sut.ListDefinitionsAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("document-review", result[0].Code);
        Assert.Equal("active", result[0].Status);
        Assert.Single(auditLogWriter.Entries);
    }
}
