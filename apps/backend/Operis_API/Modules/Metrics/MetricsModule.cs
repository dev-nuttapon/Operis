using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Metrics.Application;
using Operis_API.Modules.Metrics.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Metrics;

public sealed class MetricsModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMetricsQueries, MetricsQueries>();
        services.AddScoped<IMetricsCommands, MetricsCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var definitions = endpoints.MapGroup("/api/v1/metric-definitions").WithTags("Metrics").RequireAuthorization();
        definitions.MapGet("/", ListMetricDefinitionsAsync);
        definitions.MapPost("/", CreateMetricDefinitionAsync);
        definitions.MapPut("/{metricDefinitionId:guid}", UpdateMetricDefinitionAsync);

        var schedules = endpoints.MapGroup("/api/v1/metric-collection-schedules").WithTags("Metrics").RequireAuthorization();
        schedules.MapGet("/", ListMetricCollectionSchedulesAsync);
        schedules.MapPost("/", CreateMetricCollectionScheduleAsync);

        var results = endpoints.MapGroup("/api/v1/metric-results").WithTags("Metrics").RequireAuthorization();
        results.MapGet("/", ListMetricResultsAsync);

        var gates = endpoints.MapGroup("/api/v1/quality-gates").WithTags("Metrics").RequireAuthorization();
        gates.MapGet("/", ListQualityGatesAsync);
        gates.MapPost("/evaluate", EvaluateQualityGateAsync);
        gates.MapPut("/{qualityGateResultId:guid}/override", OverrideQualityGateAsync);

        return endpoints;
    }

    private static async Task<IResult> ListMetricDefinitionsAsync(ClaimsPrincipal principal, [AsParameters] MetricDefinitionListQuery query, IMetricsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Metrics.Read))
        {
            return Forbidden("You do not have permission to read metric definitions.");
        }

        return Results.Ok(await queries.ListMetricDefinitionsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateMetricDefinitionAsync(ClaimsPrincipal principal, CreateMetricDefinitionRequest request, IMetricsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Metrics.Manage, "You do not have permission to manage metric definitions.", () => commands.CreateMetricDefinitionAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateMetricDefinitionAsync(ClaimsPrincipal principal, Guid metricDefinitionId, UpdateMetricDefinitionRequest request, IMetricsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Metrics.Manage, "You do not have permission to manage metric definitions.", () => commands.UpdateMetricDefinitionAsync(metricDefinitionId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListMetricCollectionSchedulesAsync(ClaimsPrincipal principal, [AsParameters] MetricCollectionScheduleListQuery query, IMetricsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Metrics.Read))
        {
            return Forbidden("You do not have permission to read metric schedules.");
        }

        return Results.Ok(await queries.ListMetricCollectionSchedulesAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateMetricCollectionScheduleAsync(ClaimsPrincipal principal, CreateMetricCollectionScheduleRequest request, IMetricsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Metrics.Manage, "You do not have permission to manage metric schedules.", () => commands.CreateMetricCollectionScheduleAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> ListMetricResultsAsync(ClaimsPrincipal principal, [AsParameters] MetricResultListQuery query, IMetricsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Metrics.Read))
        {
            return Forbidden("You do not have permission to read metric results.");
        }

        return Results.Ok(await queries.ListMetricResultsAsync(query, cancellationToken));
    }

    private static async Task<IResult> ListQualityGatesAsync(ClaimsPrincipal principal, [AsParameters] QualityGateListQuery query, IMetricsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Metrics.Read))
        {
            return Forbidden("You do not have permission to read quality gates.");
        }

        return Results.Ok(await queries.ListQualityGatesAsync(query, cancellationToken));
    }

    private static async Task<IResult> EvaluateQualityGateAsync(ClaimsPrincipal principal, EvaluateQualityGateRequest request, IMetricsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Metrics.Manage, "You do not have permission to evaluate quality gates.", () => commands.EvaluateQualityGateAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> OverrideQualityGateAsync(ClaimsPrincipal principal, Guid qualityGateResultId, OverrideQualityGateRequest request, IMetricsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Metrics.OverrideQualityGates, "You do not have permission to override quality gates.", () => commands.OverrideQualityGateAsync(qualityGateResultId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<MetricsCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (!permissionMatrix.HasPermission(principal, permission))
        {
            return Forbidden(forbiddenDetail);
        }

        var result = await action();
        return result.Status switch
        {
            MetricsCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            MetricsCommandStatus.Success => Results.Ok(result.Value),
            MetricsCommandStatus.NotFound => Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, result.ErrorCode ?? ApiErrorCodes.ResourceNotFound, "Resource not found.", result.ErrorMessage)),
            MetricsCommandStatus.ValidationError => Results.BadRequest(ApiProblemDetailsFactory.Create(StatusCodes.Status400BadRequest, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Validation failed.", result.ErrorMessage)),
            MetricsCommandStatus.Conflict => Results.Conflict(ApiProblemDetailsFactory.Create(StatusCodes.Status409Conflict, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Request conflict.", result.ErrorMessage)),
            _ => Results.Problem(ApiProblemDetailsFactory.Create(StatusCodes.Status500InternalServerError, ApiErrorCodes.InternalFailure, "Request failed.", result.ErrorMessage))
        };
    }

    private static string? ResolveActor(ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Email)
        ?? principal.FindFirstValue("preferred_username")
        ?? principal.FindFirstValue("sub")
        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

    private static IResult Forbidden(string detail) =>
        Results.Json(ApiProblemDetailsFactory.Create(StatusCodes.Status403Forbidden, "forbidden", "Forbidden.", detail), statusCode: StatusCodes.Status403Forbidden);
}
