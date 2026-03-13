using Microsoft.EntityFrameworkCore;
using Operis_API.Modules.Workflows;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Workflows.Application;

public sealed class WorkflowCommandsTests
{
    [Fact]
    public async Task CreateDefinitionAsync_WhenSuccessful_PersistsDraftWorkflowDefinition()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var auditLogWriter = new FakeAuditLogWriter();
        var sut = new WorkflowCommands(dbContext, auditLogWriter);

        var result = await sut.CreateDefinitionAsync(
            new CreateWorkflowDefinitionRequest("Document Review"),
            CancellationToken.None);

        var entity = await dbContext.WorkflowDefinitions.SingleAsync();

        Assert.Equal(WorkflowCommandStatus.Success, result.Status);
        Assert.Equal("document-review", entity.Code);
        Assert.Equal("draft", entity.Status);
        Assert.Single(auditLogWriter.Entries);
    }

    [Fact]
    public async Task CreateDefinitionAsync_WhenNameConflicts_ReturnsConflictWithoutAudit()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.WorkflowDefinitions.Add(new Modules.Workflows.Infrastructure.WorkflowDefinitionEntity
        {
            Id = Guid.NewGuid(),
            Code = "document-review",
            Name = "Document Review",
            Status = "draft",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var sut = new WorkflowCommands(dbContext, auditLogWriter);

        var result = await sut.CreateDefinitionAsync(
            new CreateWorkflowDefinitionRequest("Document Review"),
            CancellationToken.None);

        Assert.Equal(WorkflowCommandStatus.Conflict, result.Status);
        Assert.Single(await dbContext.WorkflowDefinitions.ToListAsync());
        Assert.Empty(auditLogWriter.Entries);
    }
}
