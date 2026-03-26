using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Risks.Application;
using Operis_API.Modules.Risks.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Risks;

public sealed class RisksModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRiskQueries, RiskQueries>();
        services.AddScoped<IRiskCommands, RiskCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var risks = endpoints.MapGroup("/api/v1/risks")
            .WithTags("Risks")
            .RequireAuthorization();

        risks.MapGet("/", ListRisksAsync);
        risks.MapPost("/", CreateRiskAsync);
        risks.MapGet("/{riskId:guid}", GetRiskAsync);
        risks.MapPut("/{riskId:guid}", UpdateRiskAsync);
        risks.MapPut("/{riskId:guid}/assess", AssessRiskAsync);
        risks.MapPut("/{riskId:guid}/mitigate", MitigateRiskAsync);
        risks.MapPut("/{riskId:guid}/close", CloseRiskAsync);

        var issues = endpoints.MapGroup("/api/v1/issues")
            .WithTags("Issues")
            .RequireAuthorization();

        issues.MapGet("/", ListIssuesAsync);
        issues.MapPost("/", CreateIssueAsync);
        issues.MapGet("/{issueId:guid}", GetIssueAsync);
        issues.MapPut("/{issueId:guid}", UpdateIssueAsync);
        issues.MapPost("/{issueId:guid}/actions", CreateIssueActionAsync);
        issues.MapPut("/{issueId:guid}/actions/{actionId:guid}", UpdateIssueActionAsync);
        issues.MapPut("/{issueId:guid}/resolve", ResolveIssueAsync);
        issues.MapPut("/{issueId:guid}/close", CloseIssueAsync);

        return endpoints;
    }

    private static async Task<IResult> ListRisksAsync(
        ClaimsPrincipal principal,
        [AsParameters] RiskListQuery query,
        IRiskQueries queries,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Risks.Read))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read risks.");
        }

        return Results.Ok(await queries.ListRisksAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetRiskAsync(
        ClaimsPrincipal principal,
        Guid riskId,
        IRiskQueries queries,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ReadSingleAsync(principal, permissionMatrix, Permissions.Risks.Read, "You do not have permission to read risks.", () => queries.GetRiskAsync(riskId, cancellationToken));

    private static async Task<IResult> CreateRiskAsync(
        ClaimsPrincipal principal,
        CreateRiskRequest request,
        IRiskCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Risks.Manage, "You do not have permission to manage risks.", () => commands.CreateRiskAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateRiskAsync(
        ClaimsPrincipal principal,
        Guid riskId,
        UpdateRiskRequest request,
        IRiskCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Risks.Manage, "You do not have permission to manage risks.", () => commands.UpdateRiskAsync(riskId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> AssessRiskAsync(
        ClaimsPrincipal principal,
        Guid riskId,
        RiskTransitionRequest request,
        IRiskCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Risks.Manage, "You do not have permission to manage risks.", () => commands.AssessRiskAsync(riskId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> MitigateRiskAsync(
        ClaimsPrincipal principal,
        Guid riskId,
        RiskTransitionRequest request,
        IRiskCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Risks.Manage, "You do not have permission to manage risks.", () => commands.MitigateRiskAsync(riskId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> CloseRiskAsync(
        ClaimsPrincipal principal,
        Guid riskId,
        RiskTransitionRequest request,
        IRiskCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Risks.Manage, "You do not have permission to manage risks.", () => commands.CloseRiskAsync(riskId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListIssuesAsync(
        ClaimsPrincipal principal,
        [AsParameters] IssueListQuery query,
        IRiskQueries queries,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Risks.Read))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read issues.");
        }

        var canReadSensitive = permissionMatrix.HasPermission(principal, Permissions.Risks.ReadSensitive);
        return Results.Ok(await queries.ListIssuesAsync(query, canReadSensitive, cancellationToken));
    }

    private static async Task<IResult> GetIssueAsync(
        ClaimsPrincipal principal,
        Guid issueId,
        IRiskQueries queries,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Risks.Read))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read issues.");
        }

        var item = await queries.GetIssueAsync(issueId, permissionMatrix.HasPermission(principal, Permissions.Risks.ReadSensitive), cancellationToken);
        return item is null ? NotFoundWithCode() : Results.Ok(item);
    }

    private static async Task<IResult> CreateIssueAsync(
        ClaimsPrincipal principal,
        CreateIssueRequest request,
        IRiskCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken)
    {
        if (request.IsSensitive && LacksPermission(principal, permissionMatrix, Permissions.Risks.ReadSensitive))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to create sensitive issues.");
        }

        return await ExecuteAsync(principal, permissionMatrix, Permissions.Risks.Manage, "You do not have permission to manage issues.", () => commands.CreateIssueAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);
    }

    private static async Task<IResult> UpdateIssueAsync(
        ClaimsPrincipal principal,
        Guid issueId,
        UpdateIssueRequest request,
        IRiskCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken)
    {
        if (request.IsSensitive && LacksPermission(principal, permissionMatrix, Permissions.Risks.ReadSensitive))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to manage sensitive issues.");
        }

        return await ExecuteAsync(principal, permissionMatrix, Permissions.Risks.Manage, "You do not have permission to manage issues.", () => commands.UpdateIssueAsync(issueId, request, ResolveActor(principal), cancellationToken));
    }

    private static async Task<IResult> CreateIssueActionAsync(
        ClaimsPrincipal principal,
        Guid issueId,
        CreateIssueActionRequest request,
        IRiskCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Risks.Manage, "You do not have permission to manage issues.", () => commands.CreateIssueActionAsync(issueId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> UpdateIssueActionAsync(
        ClaimsPrincipal principal,
        Guid issueId,
        Guid actionId,
        UpdateIssueActionRequest request,
        IRiskCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Risks.Manage, "You do not have permission to manage issues.", () => commands.UpdateIssueActionAsync(issueId, actionId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ResolveIssueAsync(
        ClaimsPrincipal principal,
        Guid issueId,
        IssueResolutionRequest request,
        IRiskCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Risks.Manage, "You do not have permission to manage issues.", () => commands.ResolveIssueAsync(issueId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> CloseIssueAsync(
        ClaimsPrincipal principal,
        Guid issueId,
        IssueResolutionRequest request,
        IRiskCommands commands,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Risks.Manage, "You do not have permission to manage issues.", () => commands.CloseIssueAsync(issueId, request, ResolveActor(principal), cancellationToken));

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

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<RiskCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (LacksPermission(principal, permissionMatrix, permission))
        {
            return ForbiddenWithCode("Forbidden.", forbiddenDetail);
        }

        var result = await action();
        return result.Status switch
        {
            RiskCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            RiskCommandStatus.Success => Results.Ok(result.Value),
            RiskCommandStatus.NotFound => NotFoundWithCode(result.ErrorMessage, result.ErrorCode),
            RiskCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            RiskCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
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
