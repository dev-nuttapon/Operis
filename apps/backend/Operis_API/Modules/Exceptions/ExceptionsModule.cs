using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Exceptions.Application;
using Operis_API.Modules.Exceptions.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Exceptions;

public sealed class ExceptionsModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IExceptionQueries, ExceptionQueries>();
        services.AddScoped<IExceptionCommands, ExceptionCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/exceptions/waivers").WithTags("Exceptions").RequireAuthorization();
        group.MapGet("/", ListWaiversAsync);
        group.MapGet("/{waiverId:guid}", GetWaiverAsync);
        group.MapPost("/", CreateWaiverAsync);
        group.MapPut("/{waiverId:guid}", UpdateWaiverAsync);
        group.MapPost("/{waiverId:guid}/transition", TransitionWaiverAsync);
        return endpoints;
    }

    private static async Task<IResult> ListWaiversAsync(ClaimsPrincipal principal, [AsParameters] WaiverListQuery query, IExceptionQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanRead(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read process waivers.");
        }

        return Results.Ok(await queries.ListWaiversAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetWaiverAsync(ClaimsPrincipal principal, Guid waiverId, IExceptionQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanRead(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read process waivers.");
        }

        var detail = await queries.GetWaiverAsync(waiverId, cancellationToken);
        return detail is null
            ? Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.ResourceNotFound, "Waiver not found.", "Waiver not found."))
            : Results.Ok(detail);
    }

    private static Task<IResult> CreateWaiverAsync(ClaimsPrincipal principal, CreateWaiverRequest request, IExceptionCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Exceptions.Manage, "You do not have permission to manage process waivers.", () => commands.CreateWaiverAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static Task<IResult> UpdateWaiverAsync(ClaimsPrincipal principal, Guid waiverId, UpdateWaiverRequest request, IExceptionCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Exceptions.Manage, "You do not have permission to manage process waivers.", () => commands.UpdateWaiverAsync(waiverId, request, ResolveActor(principal), cancellationToken));

    private static Task<IResult> TransitionWaiverAsync(ClaimsPrincipal principal, Guid waiverId, TransitionWaiverRequest request, IExceptionCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        var permission = request.TargetStatus.Trim().Equals("approved", StringComparison.OrdinalIgnoreCase)
            ? Permissions.Exceptions.Approve
            : Permissions.Exceptions.Manage;
        var message = request.TargetStatus.Trim().Equals("approved", StringComparison.OrdinalIgnoreCase)
            ? "You do not have permission to approve process waivers."
            : "You do not have permission to transition process waivers.";

        return ExecuteAsync(principal, permissionMatrix, permission, message, () => commands.TransitionWaiverAsync(waiverId, request, ResolveActor(principal), cancellationToken));
    }

    private static bool CanRead(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix) =>
        permissionMatrix.HasAnyPermission(principal, Permissions.Exceptions.Read, Permissions.Exceptions.Manage, Permissions.Exceptions.Approve);

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<ExceptionCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (!permissionMatrix.HasPermission(principal, permission))
        {
            return Forbidden(forbiddenDetail);
        }

        var result = await action();
        return result.Status switch
        {
            ExceptionCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            ExceptionCommandStatus.Success => Results.Ok(result.Value),
            ExceptionCommandStatus.NotFound => Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, result.ErrorCode ?? ApiErrorCodes.ResourceNotFound, "Resource not found.", result.ErrorMessage)),
            ExceptionCommandStatus.ValidationError => Results.BadRequest(ApiProblemDetailsFactory.Create(StatusCodes.Status400BadRequest, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Validation failed.", result.ErrorMessage)),
            ExceptionCommandStatus.Conflict => Results.Conflict(ApiProblemDetailsFactory.Create(StatusCodes.Status409Conflict, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Request conflict.", result.ErrorMessage)),
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
