using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Meetings.Application;
using Operis_API.Modules.Meetings.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Meetings;

public sealed class MeetingsModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMeetingQueries, MeetingQueries>();
        services.AddScoped<IMeetingCommands, MeetingCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var meetings = endpoints.MapGroup("/api/v1/meetings")
            .WithTags("Meetings")
            .RequireAuthorization();

        meetings.MapGet("/", ListMeetingsAsync);
        meetings.MapPost("/", CreateMeetingAsync);
        meetings.MapGet("/{meetingId:guid}", GetMeetingAsync);
        meetings.MapPut("/{meetingId:guid}", UpdateMeetingAsync);
        meetings.MapPut("/{meetingId:guid}/approve", ApproveMeetingAsync);
        meetings.MapGet("/{meetingId:guid}/minutes", GetMeetingMinutesAsync);
        meetings.MapPut("/{meetingId:guid}/minutes", UpdateMeetingMinutesAsync);

        var decisions = endpoints.MapGroup("/api/v1/decisions")
            .WithTags("Decisions")
            .RequireAuthorization();

        decisions.MapGet("/", ListDecisionsAsync);
        decisions.MapPost("/", CreateDecisionAsync);
        decisions.MapGet("/{decisionId:guid}", GetDecisionAsync);
        decisions.MapPut("/{decisionId:guid}", UpdateDecisionAsync);
        decisions.MapPut("/{decisionId:guid}/approve", ApproveDecisionAsync);
        decisions.MapPut("/{decisionId:guid}/apply", ApplyDecisionAsync);

        return endpoints;
    }

    private static async Task<IResult> ListMeetingsAsync(ClaimsPrincipal principal, [AsParameters] MeetingListQuery query, IMeetingQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Meetings.Read))
        {
            return Forbidden("You do not have permission to read meetings.");
        }

        var canReadRestricted = permissionMatrix.HasPermission(principal, Permissions.Meetings.ReadRestricted);
        return Results.Ok(await queries.ListMeetingsAsync(query, canReadRestricted, cancellationToken));
    }

    private static async Task<IResult> GetMeetingAsync(ClaimsPrincipal principal, Guid meetingId, IMeetingQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Meetings.Read))
        {
            return Forbidden("You do not have permission to read meetings.");
        }

        var item = await queries.GetMeetingAsync(meetingId, permissionMatrix.HasPermission(principal, Permissions.Meetings.ReadRestricted), cancellationToken);
        return item is null ? NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> CreateMeetingAsync(ClaimsPrincipal principal, CreateMeetingRequest request, IMeetingCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (request.IsRestricted && LacksPermission(principal, permissionMatrix, Permissions.Meetings.ReadRestricted))
        {
            return Forbidden("You do not have permission to create restricted meetings.");
        }

        return await ExecuteAsync(principal, permissionMatrix, Permissions.Meetings.Manage, "You do not have permission to manage meetings.", () => commands.CreateMeetingAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);
    }

    private static async Task<IResult> UpdateMeetingAsync(ClaimsPrincipal principal, Guid meetingId, UpdateMeetingRequest request, IMeetingCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (request.IsRestricted && LacksPermission(principal, permissionMatrix, Permissions.Meetings.ReadRestricted))
        {
            return Forbidden("You do not have permission to manage restricted meetings.");
        }

        return await ExecuteAsync(principal, permissionMatrix, Permissions.Meetings.Manage, "You do not have permission to manage meetings.", () => commands.UpdateMeetingAsync(meetingId, request, ResolveActor(principal), cancellationToken));
    }

    private static async Task<IResult> ApproveMeetingAsync(ClaimsPrincipal principal, Guid meetingId, MeetingApprovalRequest request, IMeetingCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Meetings.Approve, "You do not have permission to approve meetings.", () => commands.ApproveMeetingAsync(meetingId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> GetMeetingMinutesAsync(ClaimsPrincipal principal, Guid meetingId, IMeetingQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Meetings.Read))
        {
            return Forbidden("You do not have permission to read meeting minutes.");
        }

        var item = await queries.GetMeetingMinutesAsync(meetingId, permissionMatrix.HasPermission(principal, Permissions.Meetings.ReadRestricted), cancellationToken);
        return item is null ? NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> UpdateMeetingMinutesAsync(ClaimsPrincipal principal, Guid meetingId, UpdateMeetingMinutesRequest request, IMeetingCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Meetings.Manage, "You do not have permission to manage meeting minutes.", () => commands.UpdateMeetingMinutesAsync(meetingId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListDecisionsAsync(ClaimsPrincipal principal, [AsParameters] DecisionListQuery query, IMeetingQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Meetings.Read))
        {
            return Forbidden("You do not have permission to read decisions.");
        }

        var canReadRestricted = permissionMatrix.HasPermission(principal, Permissions.Meetings.ReadRestricted);
        return Results.Ok(await queries.ListDecisionsAsync(query, canReadRestricted, cancellationToken));
    }

    private static async Task<IResult> GetDecisionAsync(ClaimsPrincipal principal, Guid decisionId, IMeetingQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Meetings.Read))
        {
            return Forbidden("You do not have permission to read decisions.");
        }

        var item = await queries.GetDecisionAsync(decisionId, permissionMatrix.HasPermission(principal, Permissions.Meetings.ReadRestricted), cancellationToken);
        return item is null ? NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> CreateDecisionAsync(ClaimsPrincipal principal, CreateDecisionRequest request, IMeetingCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (request.IsRestricted && LacksPermission(principal, permissionMatrix, Permissions.Meetings.ReadRestricted))
        {
            return Forbidden("You do not have permission to create restricted decisions.");
        }

        return await ExecuteAsync(principal, permissionMatrix, Permissions.Meetings.Manage, "You do not have permission to manage decisions.", () => commands.CreateDecisionAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);
    }

    private static async Task<IResult> UpdateDecisionAsync(ClaimsPrincipal principal, Guid decisionId, UpdateDecisionRequest request, IMeetingCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (request.IsRestricted && LacksPermission(principal, permissionMatrix, Permissions.Meetings.ReadRestricted))
        {
            return Forbidden("You do not have permission to manage restricted decisions.");
        }

        return await ExecuteAsync(principal, permissionMatrix, Permissions.Meetings.Manage, "You do not have permission to manage decisions.", () => commands.UpdateDecisionAsync(decisionId, request, ResolveActor(principal), cancellationToken));
    }

    private static async Task<IResult> ApproveDecisionAsync(ClaimsPrincipal principal, Guid decisionId, DecisionTransitionRequest request, IMeetingCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Meetings.Approve, "You do not have permission to approve decisions.", () => commands.ApproveDecisionAsync(decisionId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ApplyDecisionAsync(ClaimsPrincipal principal, Guid decisionId, DecisionTransitionRequest request, IMeetingCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Meetings.Manage, "You do not have permission to apply decisions.", () => commands.ApplyDecisionAsync(decisionId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<MeetingCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (LacksPermission(principal, permissionMatrix, permission))
        {
            return Forbidden(forbiddenDetail);
        }

        var result = await action();
        return result.Status switch
        {
            MeetingCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            MeetingCommandStatus.Success => Results.Ok(result.Value),
            MeetingCommandStatus.NotFound => Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, result.ErrorCode ?? ApiErrorCodes.ResourceNotFound, "Resource not found.", result.ErrorMessage)),
            MeetingCommandStatus.ValidationError => Results.BadRequest(ApiProblemDetailsFactory.Create(StatusCodes.Status400BadRequest, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Validation failed.", result.ErrorMessage)),
            MeetingCommandStatus.Conflict => Results.Conflict(ApiProblemDetailsFactory.Create(StatusCodes.Status409Conflict, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Request conflict.", result.ErrorMessage)),
            _ => Results.Problem(ApiProblemDetailsFactory.Create(StatusCodes.Status500InternalServerError, ApiErrorCodes.InternalFailure, "Request failed.", result.ErrorMessage))
        };
    }

    private static string? ResolveActor(ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Email)
        ?? principal.FindFirstValue("preferred_username")
        ?? principal.FindFirstValue("sub")
        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

    private static bool LacksPermission(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission) =>
        !permissionMatrix.HasPermission(principal, permission);

    private static IResult Forbidden(string detail) =>
        Results.Json(ApiProblemDetailsFactory.Create(StatusCodes.Status403Forbidden, "forbidden", "Forbidden.", detail), statusCode: StatusCodes.Status403Forbidden);

    private static IResult NotFound() =>
        Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.ResourceNotFound, "Resource not found.", null));
}
