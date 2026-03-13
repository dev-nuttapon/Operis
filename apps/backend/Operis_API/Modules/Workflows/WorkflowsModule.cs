using Microsoft.AspNetCore.Mvc;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;
using System.Security.Claims;

namespace Operis_API.Modules.Workflows;

public sealed class WorkflowsModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IWorkflowQueries, WorkflowQueries>();
        services.AddScoped<IWorkflowCommands, WorkflowCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/workflows")
            .WithTags("Workflows")
            .RequireAuthorization();

        group.MapGet("/definitions", ListWorkflowDefinitionsAsync)
            .WithName("Workflows_ListDefinitions");
        group.MapPost("/definitions", CreateWorkflowDefinitionAsync)
            .WithName("Workflows_CreateDefinition");
        group.MapPut("/definitions/{workflowDefinitionId:guid}", UpdateWorkflowDefinitionAsync)
            .WithName("Workflows_UpdateDefinition");
        group.MapPost("/definitions/{workflowDefinitionId:guid}/activate", ActivateWorkflowDefinitionAsync)
            .WithName("Workflows_ActivateDefinition");
        group.MapPost("/definitions/{workflowDefinitionId:guid}/archive", ArchiveWorkflowDefinitionAsync)
            .WithName("Workflows_ArchiveDefinition");

        return endpoints;
    }

    private static async Task<IResult> ListWorkflowDefinitionsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IWorkflowQueries queries,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Workflows.Read))
        {
            return Results.Forbid();
        }

        var definitions = await queries.ListDefinitionsAsync(cancellationToken);
        return Results.Ok(definitions);
    }

    private static async Task<IResult> CreateWorkflowDefinitionAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        CreateWorkflowDefinitionRequest request,
        IWorkflowCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Workflows.ManageDefinitions))
        {
            return Results.Forbid();
        }

        var result = await commands.CreateDefinitionAsync(request, cancellationToken);

        return result.Status switch
        {
            WorkflowCommandStatus.Success => Results.Created($"/api/v1/workflows/definitions/{result.Response!.Id}", result.Response),
            WorkflowCommandStatus.Conflict => Results.Conflict(new ProblemDetails { Title = result.ErrorMessage }),
            WorkflowCommandStatus.ValidationError => Results.BadRequest(new ProblemDetails { Title = result.ErrorMessage }),
            _ => Results.BadRequest()
        };
    }

    private static async Task<IResult> UpdateWorkflowDefinitionAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid workflowDefinitionId,
        UpdateWorkflowDefinitionRequest request,
        IWorkflowCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Workflows.ManageDefinitions))
        {
            return Results.Forbid();
        }

        var result = await commands.UpdateDefinitionAsync(workflowDefinitionId, request, cancellationToken);
        return ToCommandResult(result);
    }

    private static async Task<IResult> ActivateWorkflowDefinitionAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid workflowDefinitionId,
        IWorkflowCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Workflows.ManageDefinitions))
        {
            return Results.Forbid();
        }

        var result = await commands.ActivateDefinitionAsync(workflowDefinitionId, cancellationToken);
        return ToCommandResult(result);
    }

    private static async Task<IResult> ArchiveWorkflowDefinitionAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        Guid workflowDefinitionId,
        IWorkflowCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Workflows.ManageDefinitions))
        {
            return Results.Forbid();
        }

        var result = await commands.ArchiveDefinitionAsync(workflowDefinitionId, cancellationToken);
        return ToCommandResult(result);
    }

    private static IResult ToCommandResult(WorkflowCommandResult result)
    {
        return result.Status switch
        {
            WorkflowCommandStatus.Success => Results.Ok(result.Response),
            WorkflowCommandStatus.Conflict => Results.Conflict(new ProblemDetails { Title = result.ErrorMessage }),
            WorkflowCommandStatus.ValidationError => Results.BadRequest(new ProblemDetails { Title = result.ErrorMessage }),
            _ => Results.BadRequest()
        };
    }
}
