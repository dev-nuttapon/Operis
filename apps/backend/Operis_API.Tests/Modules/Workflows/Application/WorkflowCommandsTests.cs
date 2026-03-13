using Microsoft.EntityFrameworkCore;
using Operis_API.Modules.Workflows;
using Operis_API.Modules.Workflows.Infrastructure;
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
        dbContext.WorkflowDefinitions.Add(new WorkflowDefinitionEntity
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

    [Fact]
    public async Task ActivateDefinitionAsync_WhenSuccessful_UpdatesStatusToActive()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.WorkflowDefinitions.Add(new WorkflowDefinitionEntity
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
        var workflowDefinitionId = await dbContext.WorkflowDefinitions.Select(x => x.Id).SingleAsync();

        var result = await sut.ActivateDefinitionAsync(workflowDefinitionId, CancellationToken.None);

        Assert.Equal(WorkflowCommandStatus.Success, result.Status);
        Assert.Equal("active", (await dbContext.WorkflowDefinitions.SingleAsync()).Status);
        Assert.Single(auditLogWriter.Entries);
    }

    [Fact]
    public async Task ArchiveDefinitionAsync_WhenAlreadyArchived_ReturnsConflictWithoutNewAudit()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.WorkflowDefinitions.Add(new WorkflowDefinitionEntity
        {
            Id = Guid.NewGuid(),
            Code = "document-review",
            Name = "Document Review",
            Status = "archived",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var sut = new WorkflowCommands(dbContext, auditLogWriter);
        var workflowDefinitionId = await dbContext.WorkflowDefinitions.Select(x => x.Id).SingleAsync();

        var result = await sut.ArchiveDefinitionAsync(workflowDefinitionId, CancellationToken.None);

        Assert.Equal(WorkflowCommandStatus.Conflict, result.Status);
        Assert.Empty(auditLogWriter.Entries);
    }

    [Fact]
    public async Task UpdateDefinitionAsync_WhenSuccessful_UpdatesNameAndCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.WorkflowDefinitions.Add(new WorkflowDefinitionEntity
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
        var workflowDefinitionId = await dbContext.WorkflowDefinitions.Select(x => x.Id).SingleAsync();

        var result = await sut.UpdateDefinitionAsync(
            workflowDefinitionId,
            new UpdateWorkflowDefinitionRequest("Policy Approval"),
            CancellationToken.None);

        var entity = await dbContext.WorkflowDefinitions.SingleAsync();

        Assert.Equal(WorkflowCommandStatus.Success, result.Status);
        Assert.Equal("Policy Approval", entity.Name);
        Assert.Equal("policy-approval", entity.Code);
        Assert.Single(auditLogWriter.Entries);
    }

    [Fact]
    public async Task UpdateDefinitionAsync_WhenRenamedToExistingCode_ReturnsConflict()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.WorkflowDefinitions.AddRange(
            new WorkflowDefinitionEntity
            {
                Id = Guid.NewGuid(),
                Code = "document-review",
                Name = "Document Review",
                Status = "draft",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new WorkflowDefinitionEntity
            {
                Id = Guid.NewGuid(),
                Code = "policy-approval",
                Name = "Policy Approval",
                Status = "draft",
                CreatedAt = DateTimeOffset.UtcNow
            });
        await dbContext.SaveChangesAsync();

        var targetId = await dbContext.WorkflowDefinitions
            .Where(x => x.Code == "document-review")
            .Select(x => x.Id)
            .SingleAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var sut = new WorkflowCommands(dbContext, auditLogWriter);

        var result = await sut.UpdateDefinitionAsync(
            targetId,
            new UpdateWorkflowDefinitionRequest("Policy Approval"),
            CancellationToken.None);

        Assert.Equal(WorkflowCommandStatus.Conflict, result.Status);
        Assert.Empty(auditLogWriter.Entries);
    }
}
