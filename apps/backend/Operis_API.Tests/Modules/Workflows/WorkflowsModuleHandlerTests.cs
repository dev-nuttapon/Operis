using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Operis_API.Modules.Workflows;
using Operis_API.Modules.Workflows.Infrastructure;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Workflows;

public sealed class WorkflowsModuleHandlerTests
{
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

        var queries = new WorkflowQueries(dbContext, new FakeAuditLogWriter());

        var result = await InvokeListWorkflowDefinitionsAsync(queries);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkflowDefinitionAsync_WhenSuccessful_ReturnsCreatedResult()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var commands = new WorkflowCommands(dbContext, new FakeAuditLogWriter());

        var result = await InvokeCreateWorkflowDefinitionAsync(
            new CreateWorkflowDefinitionRequest("Document Review"),
            commands);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status201Created, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task ActivateWorkflowDefinitionAsync_WhenSuccessful_ReturnsOkResult()
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

        var commands = new WorkflowCommands(dbContext, new FakeAuditLogWriter());
        var workflowDefinitionId = await dbContext.WorkflowDefinitions.Select(x => x.Id).SingleAsync();

        var result = await InvokeActivateWorkflowDefinitionAsync(workflowDefinitionId, commands);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task UpdateWorkflowDefinitionAsync_WhenSuccessful_ReturnsOkResult()
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

        var commands = new WorkflowCommands(dbContext, new FakeAuditLogWriter());
        var workflowDefinitionId = await dbContext.WorkflowDefinitions.Select(x => x.Id).SingleAsync();

        var result = await InvokeUpdateWorkflowDefinitionAsync(
            workflowDefinitionId,
            new UpdateWorkflowDefinitionRequest("Policy Approval"),
            commands);

        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeListWorkflowDefinitionsAsync(IWorkflowQueries queries)
    {
        var method = typeof(WorkflowsModule).GetMethod(
            "ListWorkflowDefinitionsAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("WorkflowsModule.ListWorkflowDefinitionsAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [queries, CancellationToken.None])!;
        return await task;
    }

    private static async Task<IResult> InvokeCreateWorkflowDefinitionAsync(
        CreateWorkflowDefinitionRequest request,
        IWorkflowCommands commands)
    {
        var method = typeof(WorkflowsModule).GetMethod(
            "CreateWorkflowDefinitionAsync",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("WorkflowsModule.CreateWorkflowDefinitionAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [request, commands, CancellationToken.None])!;
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

        var task = (Task<IResult>)method.Invoke(null, [workflowDefinitionId, request, commands, CancellationToken.None])!;
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

        var task = (Task<IResult>)method.Invoke(null, [workflowDefinitionId, commands, CancellationToken.None])!;
        return await task;
    }
}
