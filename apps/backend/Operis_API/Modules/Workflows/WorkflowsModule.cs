using Microsoft.AspNetCore.Mvc;
using Operis_API.Shared.Modules;

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

        return endpoints;
    }

    private static async Task<IResult> ListWorkflowDefinitionsAsync(
        IWorkflowQueries queries,
        CancellationToken cancellationToken)
    {
        var definitions = await queries.ListDefinitionsAsync(cancellationToken);
        return Results.Ok(definitions);
    }

    private static async Task<IResult> CreateWorkflowDefinitionAsync(
        CreateWorkflowDefinitionRequest request,
        IWorkflowCommands commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.CreateDefinitionAsync(request, cancellationToken);

        return result.Status switch
        {
            WorkflowCommandStatus.Success => Results.Created($"/api/v1/workflows/definitions/{result.Response!.Id}", result.Response),
            WorkflowCommandStatus.Conflict => Results.Conflict(new ProblemDetails { Title = result.ErrorMessage }),
            WorkflowCommandStatus.ValidationError => Results.BadRequest(new ProblemDetails { Title = result.ErrorMessage }),
            _ => Results.BadRequest()
        };
    }
}
