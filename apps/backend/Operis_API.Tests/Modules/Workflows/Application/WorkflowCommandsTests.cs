using Microsoft.EntityFrameworkCore;
using Operis_API.Modules.Workflows;
using Operis_API.Modules.Workflows.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Workflows.Application;

public sealed class WorkflowCommandsTests
{
    private async Task<(Guid RoleId, Guid DocumentId)> SeedDependenciesAsync(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext)
    {
        var roleId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        dbContext.ProjectRoles.Add(new Operis_API.Modules.Users.Infrastructure.ProjectRoleEntity { Id = roleId, Name = "Test Role", CreatedAt = DateTimeOffset.UtcNow });
        dbContext.Documents.Add(new Operis_API.Modules.Documents.Infrastructure.DocumentEntity
        {
            Id = docId,
            Title = "Test Doc",
            PhaseCode = "DEV",
            OwnerUserId = "user-1",
            Classification = "internal",
            RetentionClass = "standard",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();
        return (roleId, docId);
    }

    [Fact]
    public async Task CreateDefinitionAsync_WhenSuccessful_PersistsDraftWorkflowDefinition()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var (roleId, docId) = await SeedDependenciesAsync(dbContext);
        
        var auditLogWriter = new FakeAuditLogWriter();
        var sut = new WorkflowCommands(dbContext, auditLogWriter, new FakeBusinessAuditEventWriter(), new FakeWorkflowDefinitionCache());

        var steps = new[] { new WorkflowStepRequest("Submit", "submit", 1, true, docId, 1, [roleId], []) };
        var result = await sut.CreateDefinitionAsync(
            new CreateWorkflowDefinitionRequest("Document Review", null, steps),
            CancellationToken.None);

        Assert.Equal(WorkflowCommandStatus.Success, result.Status);
        var entity = await dbContext.WorkflowDefinitions.OrderByDescending(x => x.CreatedAt).FirstAsync();
        Assert.Equal("document-review", entity.Code);
        Assert.Equal("draft", entity.Status);
        Assert.Single(auditLogWriter.Entries);
    }

    [Fact]
    public async Task CreateDefinitionAsync_WhenNameConflicts_ReturnsConflictWithoutAudit()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var (roleId, docId) = await SeedDependenciesAsync(dbContext);
        
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
        var sut = new WorkflowCommands(dbContext, auditLogWriter, new FakeBusinessAuditEventWriter(), new FakeWorkflowDefinitionCache());

        var steps = new[] { new WorkflowStepRequest("Submit", "submit", 1, true, docId, 1, [roleId], []) };
        var result = await sut.CreateDefinitionAsync(
            new CreateWorkflowDefinitionRequest("Document Review", null, steps),
            CancellationToken.None);

        Assert.Equal(WorkflowCommandStatus.Conflict, result.Status);
        Assert.Empty(auditLogWriter.Entries);
    }

    [Fact]
    public async Task ActivateDefinitionAsync_WhenSuccessful_UpdatesStatusToActive()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var entityId = Guid.NewGuid();
        dbContext.WorkflowDefinitions.Add(new WorkflowDefinitionEntity
        {
            Id = entityId,
            Code = "document-review",
            Name = "Document Review",
            Status = "draft",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var sut = new WorkflowCommands(dbContext, auditLogWriter, new FakeBusinessAuditEventWriter(), new FakeWorkflowDefinitionCache());

        var result = await sut.ActivateDefinitionAsync(entityId, CancellationToken.None);

        Assert.Equal(WorkflowCommandStatus.Success, result.Status);
        Assert.Equal("active", (await dbContext.WorkflowDefinitions.SingleAsync(x => x.Id == entityId)).Status);
        Assert.Single(auditLogWriter.Entries);
    }

    [Fact]
    public async Task ArchiveDefinitionAsync_WhenAlreadyArchived_ReturnsConflictWithoutNewAudit()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var entityId = Guid.NewGuid();
        dbContext.WorkflowDefinitions.Add(new WorkflowDefinitionEntity
        {
            Id = entityId,
            Code = "document-review",
            Name = "Document Review",
            Status = "archived",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var sut = new WorkflowCommands(dbContext, auditLogWriter, new FakeBusinessAuditEventWriter(), new FakeWorkflowDefinitionCache());

        var result = await sut.ArchiveDefinitionAsync(entityId, CancellationToken.None);

        Assert.Equal(WorkflowCommandStatus.Conflict, result.Status);
        Assert.Empty(auditLogWriter.Entries);
    }

    [Fact]
    public async Task UpdateDefinitionAsync_WhenSuccessful_UpdatesNameAndCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var (roleId, docId) = await SeedDependenciesAsync(dbContext);
        
        var entityId = Guid.NewGuid();
        dbContext.WorkflowDefinitions.Add(new WorkflowDefinitionEntity
        {
            Id = entityId,
            Code = "document-review",
            Name = "Document Review",
            Status = "draft",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var sut = new WorkflowCommands(dbContext, auditLogWriter, new FakeBusinessAuditEventWriter(), new FakeWorkflowDefinitionCache());

        var steps = new[] { new WorkflowStepRequest("Submit", "submit", 1, true, docId, 1, [roleId], []) };
        var result = await sut.UpdateDefinitionAsync(
            entityId,
            new UpdateWorkflowDefinitionRequest("Policy Approval", null, steps),
            CancellationToken.None);

        var entity = await dbContext.WorkflowDefinitions.SingleAsync(x => x.Id == entityId);

        Assert.Equal(WorkflowCommandStatus.Success, result.Status);
        Assert.Equal("Policy Approval", entity.Name);
        Assert.Equal("policy-approval", entity.Code);
        Assert.Single(auditLogWriter.Entries);
    }

    [Fact]
    public async Task UpdateDefinitionAsync_WhenRenamedToExistingCode_ReturnsConflict()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var (roleId, docId) = await SeedDependenciesAsync(dbContext);
        
        var targetId = Guid.NewGuid();
        dbContext.WorkflowDefinitions.AddRange(
            new WorkflowDefinitionEntity
            {
                Id = targetId,
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

        var auditLogWriter = new FakeAuditLogWriter();
        var sut = new WorkflowCommands(dbContext, auditLogWriter, new FakeBusinessAuditEventWriter(), new FakeWorkflowDefinitionCache());

        var steps = new[] { new WorkflowStepRequest("Submit", "submit", 1, true, docId, 1, [roleId], []) };
        var result = await sut.UpdateDefinitionAsync(
            targetId,
            new UpdateWorkflowDefinitionRequest("Policy Approval", null, steps),
            CancellationToken.None);

        Assert.Equal(WorkflowCommandStatus.Conflict, result.Status);
        Assert.Empty(auditLogWriter.Entries);
    }
}
