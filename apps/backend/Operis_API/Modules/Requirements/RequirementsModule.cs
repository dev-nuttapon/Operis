using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Requirements.Application;
using Operis_API.Modules.Requirements.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Requirements;

public sealed class RequirementsModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRequirementQueries, RequirementQueries>();
        services.AddScoped<IRequirementCommands, RequirementCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/requirements")
            .WithTags("Requirements")
            .RequireAuthorization();

        group.MapGet("/", ListRequirementsAsync);
        group.MapGet("/baselines", ListBaselinesAsync);
        group.MapPost("/baselines", CreateBaselineAsync);
        group.MapGet("/traceability", ListTraceabilityAsync);
        group.MapPost("/traceability-links", CreateTraceabilityLinkAsync);
        group.MapDelete("/traceability-links/{linkId:guid}", DeleteTraceabilityLinkAsync);
        group.MapGet("/{requirementId:guid}", GetRequirementAsync);
        group.MapPost("/", CreateRequirementAsync);
        group.MapPut("/{requirementId:guid}", UpdateRequirementAsync);
        group.MapPut("/{requirementId:guid}/submit", SubmitRequirementAsync);
        group.MapPut("/{requirementId:guid}/approve", ApproveRequirementAsync);
        group.MapPut("/{requirementId:guid}/baseline", BaselineRequirementAsync);
        group.MapPut("/{requirementId:guid}/supersede", SupersedeRequirementAsync);

        return endpoints;
    }

    private static async Task<IResult> ListRequirementsAsync(
        ClaimsPrincipal principal,
        [AsParameters] RequirementListQuery query,
        IRequirementQueries queries,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Requirements.Read))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read requirements.");
        }

        return Results.Ok(await queries.ListRequirementsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetRequirementAsync(
        ClaimsPrincipal principal,
        Guid requirementId,
        IRequirementQueries queries,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ReadSingleAsync(principal, permissionMatrix, Permissions.Requirements.Read, "You do not have permission to read requirements.", () => queries.GetRequirementAsync(requirementId, cancellationToken));

    private static async Task<IResult> CreateRequirementAsync(
        ClaimsPrincipal principal,
        CreateRequirementRequest request,
        IRequirementCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Requirements.Manage, "You do not have permission to manage requirements.", () => commands.CreateRequirementAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateRequirementAsync(
        ClaimsPrincipal principal,
        Guid requirementId,
        UpdateRequirementRequest request,
        IRequirementCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Requirements.Manage, "You do not have permission to manage requirements.", () => commands.UpdateRequirementAsync(requirementId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> SubmitRequirementAsync(
        ClaimsPrincipal principal,
        Guid requirementId,
        IRequirementCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Requirements.Manage, "You do not have permission to manage requirements.", () => commands.SubmitRequirementAsync(requirementId, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ApproveRequirementAsync(
        ClaimsPrincipal principal,
        Guid requirementId,
        RequirementDecisionRequest request,
        IRequirementCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Requirements.Approve, "You do not have permission to approve requirements.", () => commands.ApproveRequirementAsync(requirementId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> BaselineRequirementAsync(
        ClaimsPrincipal principal,
        Guid requirementId,
        IRequirementCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Requirements.Baseline, "You do not have permission to baseline requirements.", () => commands.BaselineRequirementAsync(requirementId, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> SupersedeRequirementAsync(
        ClaimsPrincipal principal,
        Guid requirementId,
        RequirementDecisionRequest request,
        IRequirementCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Requirements.Baseline, "You do not have permission to supersede requirements.", () => commands.SupersedeRequirementAsync(requirementId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListBaselinesAsync(
        ClaimsPrincipal principal,
        [AsParameters] RequirementBaselineListQuery query,
        IRequirementQueries queries,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Requirements.Read))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read requirement baselines.");
        }

        return Results.Ok(await queries.ListBaselinesAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateBaselineAsync(
        ClaimsPrincipal principal,
        CreateRequirementBaselineRequest request,
        IRequirementCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Requirements.Baseline, "You do not have permission to baseline requirements.", () => commands.CreateBaselineAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> ListTraceabilityAsync(
        ClaimsPrincipal principal,
        [AsParameters] TraceabilityListQuery query,
        IRequirementQueries queries,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Requirements.Read))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read traceability.");
        }

        return Results.Ok(await queries.ListTraceabilityAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateTraceabilityLinkAsync(
        ClaimsPrincipal principal,
        CreateTraceabilityLinkRequest request,
        IRequirementCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Requirements.ManageTraceability, "You do not have permission to manage traceability.", () => commands.CreateTraceabilityLinkAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> DeleteTraceabilityLinkAsync(
        ClaimsPrincipal principal,
        Guid linkId,
        IRequirementCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Requirements.ManageTraceability, "You do not have permission to manage traceability.", () => commands.DeleteTraceabilityLinkAsync(linkId, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ReadSingleAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<T?>> loader)
        where T : class
    {
        if (LacksPermission(principal, permissionMatrix, permission))
        {
            return ForbiddenWithCode("Forbidden.", forbiddenDetail);
        }

        var item = await loader();
        return item is null ? NotFoundWithCode() : Results.Ok(item);
    }

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<RequirementCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (LacksPermission(principal, permissionMatrix, permission))
        {
            return ForbiddenWithCode("Forbidden.", forbiddenDetail);
        }

        var result = await action();
        return result.Status switch
        {
            RequirementCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            RequirementCommandStatus.Success => Results.Ok(result.Value),
            RequirementCommandStatus.NotFound => NotFoundWithCode(result.ErrorMessage, result.ErrorCode),
            RequirementCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            RequirementCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            _ => ProblemWithCode("Request failed.", result.ErrorMessage, result.ErrorCode, StatusCodes.Status500InternalServerError, ApiErrorCodes.InternalFailure)
        };
    }

    private static string? ResolveActor(ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Email)
        ?? principal.FindFirstValue("preferred_username")
        ?? principal.FindFirstValue("sub")
        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

    private static bool LacksPermission(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission) =>
        !permissionMatrix.HasPermission(principal, permission);

    private static IResult ForbiddenWithCode(string title, string detail) =>
        Results.Json(ApiProblemDetailsFactory.Create(StatusCodes.Status403Forbidden, "forbidden", title, detail), statusCode: StatusCodes.Status403Forbidden);

    private static IResult BadRequestWithCode(string? detail, string? code = null) =>
        Results.BadRequest(ApiProblemDetailsFactory.Create(StatusCodes.Status400BadRequest, code ?? ApiErrorCodes.RequestValidationFailed, "Validation failed.", detail));

    private static IResult ConflictWithCode(string? detail, string? code = null) =>
        Results.Conflict(ApiProblemDetailsFactory.Create(StatusCodes.Status409Conflict, code ?? ApiErrorCodes.RequestValidationFailed, "Request conflict.", detail));

    private static IResult NotFoundWithCode(string? detail = null, string? code = null) =>
        Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, code ?? ApiErrorCodes.ResourceNotFound, "Resource not found.", detail));

    private static IResult ProblemWithCode(string? title, string? detail, string? code, int? statusCode, string fallbackCode) =>
        Results.Problem(ApiProblemDetailsFactory.Create(statusCode ?? StatusCodes.Status500InternalServerError, code ?? fallbackCode, title ?? "Request failed.", detail));
}
