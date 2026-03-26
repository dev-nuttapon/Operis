using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Operis_API.Modules.Workflows;
using Operis_API.Modules.Workflows.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Workflows;

public sealed class WorkflowsModuleHandlerTests
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
    public async Task ListWorkflowDefinitionsAsync_ReturnsOkResult()
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

        var queries = new WorkflowQueries(dbContext, new FakeWorkflowDefinitionCache(), new FakeAuditLogWriter());

        var result = await InvokeListWorkflowDefinitionsAsync(queries);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task ListWorkflowDefinitionsAsync_WithoutPermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var queries = new WorkflowQueries(dbContext, new FakeWorkflowDefinitionCache(), new FakeAuditLogWriter());

        var result = await InvokeListWorkflowDefinitionsAsync(queries, CreateUnprivilegedPrincipal());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkflowDefinitionAsync_WhenSuccessful_ReturnsCreatedResult()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var (roleId, docId) = await SeedDependenciesAsync(dbContext);
        var commands = new WorkflowCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new FakeWorkflowDefinitionCache());

        var steps = new[] { new WorkflowStepRequest("Submit", "submit", 1, true, docId, 1, [roleId], []) };
        var result = await InvokeCreateWorkflowDefinitionAsync(
            new CreateWorkflowDefinitionRequest("Document Review", null, steps),
            commands);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status201Created, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkflowDefinitionAsync_WithoutPermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var (roleId, docId) = await SeedDependenciesAsync(dbContext);
        var commands = new WorkflowCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new FakeWorkflowDefinitionCache());

        var steps = new[] { new WorkflowStepRequest("Submit", "submit", 1, true, docId, 1, [roleId], []) };
        var result = await InvokeCreateWorkflowDefinitionAsync(
            new CreateWorkflowDefinitionRequest("Document Review", null, steps),
            commands,
            CreateUnprivilegedPrincipal());

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkflowDefinitionAsync_WhenNameMissing_ReturnsValidationCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var (roleId, docId) = await SeedDependenciesAsync(dbContext);
        var commands = new WorkflowCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new FakeWorkflowDefinitionCache());

        var steps = new[] { new WorkflowStepRequest("Submit", "submit", 1, true, docId, 1, [roleId], []) };
        var result = await InvokeCreateWorkflowDefinitionAsync(
            new CreateWorkflowDefinitionRequest(string.Empty, null, steps),
            commands);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        // For WorkflowCommands, empty name returns ValidationError result, which module maps to 400 with code
        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task ActivateWorkflowDefinitionAsync_WhenSuccessful_ReturnsOkResult()
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

        var commands = new WorkflowCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new FakeWorkflowDefinitionCache());

        var result = await InvokeActivateWorkflowDefinitionAsync(entityId, commands);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task UpdateWorkflowDefinitionAsync_WhenSuccessful_ReturnsOkResult()
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

        var commands = new WorkflowCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new FakeWorkflowDefinitionCache());

        var steps = new[] { new WorkflowStepRequest("Submit", "submit", 1, true, docId, 1, [roleId], []) };
        var result = await InvokeUpdateWorkflowDefinitionAsync(
            entityId,
            new UpdateWorkflowDefinitionRequest("Policy Approval", null, steps),
            commands);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeListWorkflowDefinitionsAsync(IWorkflowQueries queries, ClaimsPrincipal? principal = null)
    {
        var method = typeof(WorkflowsModule).GetMethod(
            "ListWorkflowDefinitionsAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("WorkflowsModule.ListWorkflowDefinitionsAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal ?? CreateAdminPrincipal(), new PermissionMatrix(), queries, CancellationToken.None, null, 1, 10])!;
        return await task;
    }

    private static async Task<IResult> InvokeCreateWorkflowDefinitionAsync(
        CreateWorkflowDefinitionRequest request,
        IWorkflowCommands commands,
        ClaimsPrincipal? principal = null)
    {
        var method = typeof(WorkflowsModule).GetMethod(
            "CreateWorkflowDefinitionAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("WorkflowsModule.CreateWorkflowDefinitionAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal ?? CreateAdminPrincipal(), new PermissionMatrix(), request, commands, CancellationToken.None])!;
        return await task;
    }

    private static async Task<IResult> InvokeUpdateWorkflowDefinitionAsync(
        Guid workflowDefinitionId,
        UpdateWorkflowDefinitionRequest request,
        IWorkflowCommands commands)
    {
        var method = typeof(WorkflowsModule).GetMethod(
            "UpdateWorkflowDefinitionAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("WorkflowsModule.UpdateWorkflowDefinitionAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [CreateAdminPrincipal(), new PermissionMatrix(), workflowDefinitionId, request, commands, CancellationToken.None])!;
        return await task;
    }

    private static async Task<IResult> InvokeActivateWorkflowDefinitionAsync(
        Guid workflowDefinitionId,
        IWorkflowCommands commands)
    {
        var method = typeof(WorkflowsModule).GetMethod(
            "ActivateWorkflowDefinitionAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("WorkflowsModule.ActivateWorkflowDefinitionAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [CreateAdminPrincipal(), new PermissionMatrix(), workflowDefinitionId, commands, CancellationToken.None])!;
        return await task;
    }

    private static ClaimsPrincipal CreateAdminPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:super_admin")], "TestAuth"));

    private static ClaimsPrincipal CreateUnprivilegedPrincipal() =>
        new(new ClaimsIdentity([], "TestAuth"));
}
