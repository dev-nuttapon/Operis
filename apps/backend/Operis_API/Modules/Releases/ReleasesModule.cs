using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Releases.Application;
using Operis_API.Modules.Releases.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Releases;

public sealed class ReleasesModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IReleaseQueries, ReleaseQueries>();
        services.AddScoped<IReleaseCommands, ReleaseCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var releases = endpoints.MapGroup("/api/v1/releases").WithTags("Releases").RequireAuthorization();
        releases.MapGet("/", ListReleasesAsync);
        releases.MapPost("/", CreateReleaseAsync);
        releases.MapGet("/{id:guid}", GetReleaseAsync);
        releases.MapPut("/{id:guid}", UpdateReleaseAsync);
        releases.MapPut("/{id:guid}/approve", ApproveReleaseAsync);
        releases.MapPut("/{id:guid}/release", ExecuteReleaseAsync);

        var checklists = endpoints.MapGroup("/api/v1/deployment-checklists").WithTags("Releases").RequireAuthorization();
        checklists.MapGet("/", ListDeploymentChecklistsAsync);
        checklists.MapPost("/", CreateDeploymentChecklistAsync);
        checklists.MapPut("/{id:guid}", UpdateDeploymentChecklistAsync);

        var releaseNotes = endpoints.MapGroup("/api/v1/release-notes").WithTags("Releases").RequireAuthorization();
        releaseNotes.MapGet("/", ListReleaseNotesAsync);
        releaseNotes.MapPost("/", CreateReleaseNoteAsync);
        releaseNotes.MapPut("/{id:guid}/publish", PublishReleaseNoteAsync);

        return endpoints;
    }

    private static async Task<IResult> ListReleasesAsync(ClaimsPrincipal principal, [AsParameters] ReleaseListQuery query, IReleaseQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Releases.Read))
        {
            return Forbidden("You do not have permission to read releases.");
        }

        return Results.Ok(await queries.ListReleasesAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetReleaseAsync(ClaimsPrincipal principal, Guid id, IReleaseQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Releases.Read))
        {
            return Forbidden("You do not have permission to read releases.");
        }

        var detail = await queries.GetReleaseAsync(id, cancellationToken);
        return detail is null
            ? Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.ReleaseNotFound, "Release not found.", "Release not found."))
            : Results.Ok(detail);
    }

    private static async Task<IResult> CreateReleaseAsync(ClaimsPrincipal principal, CreateReleaseRequest request, IReleaseCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Releases.Manage, "You do not have permission to manage releases.", () => commands.CreateReleaseAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateReleaseAsync(ClaimsPrincipal principal, Guid id, UpdateReleaseRequest request, IReleaseCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Releases.Manage, "You do not have permission to manage releases.", () => commands.UpdateReleaseAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ApproveReleaseAsync(ClaimsPrincipal principal, Guid id, ApproveReleaseRequest request, IReleaseCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Releases.Approve, "You do not have permission to approve releases.", () => commands.ApproveReleaseAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ExecuteReleaseAsync(ClaimsPrincipal principal, Guid id, ExecuteReleaseRequest request, IReleaseCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Releases.Approve, "You do not have permission to release deployments.", () => commands.ExecuteReleaseAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListDeploymentChecklistsAsync(ClaimsPrincipal principal, [AsParameters] DeploymentChecklistListQuery query, IReleaseQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Releases.Read))
        {
            return Forbidden("You do not have permission to read deployment checklists.");
        }

        return Results.Ok(await queries.ListDeploymentChecklistsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateDeploymentChecklistAsync(ClaimsPrincipal principal, CreateDeploymentChecklistRequest request, IReleaseCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Releases.Manage, "You do not have permission to manage deployment checklists.", () => commands.CreateDeploymentChecklistAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateDeploymentChecklistAsync(ClaimsPrincipal principal, Guid id, UpdateDeploymentChecklistRequest request, IReleaseCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Releases.Manage, "You do not have permission to manage deployment checklists.", () => commands.UpdateDeploymentChecklistAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListReleaseNotesAsync(ClaimsPrincipal principal, [AsParameters] ReleaseNoteListQuery query, IReleaseQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Releases.Read))
        {
            return Forbidden("You do not have permission to read release notes.");
        }

        return Results.Ok(await queries.ListReleaseNotesAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateReleaseNoteAsync(ClaimsPrincipal principal, CreateReleaseNoteRequest request, IReleaseCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Releases.Manage, "You do not have permission to manage release notes.", () => commands.CreateReleaseNoteAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> PublishReleaseNoteAsync(ClaimsPrincipal principal, Guid id, IReleaseCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Releases.Approve, "You do not have permission to publish release notes.", () => commands.PublishReleaseNoteAsync(id, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<ReleaseCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (!permissionMatrix.HasPermission(principal, permission))
        {
            return Forbidden(forbiddenDetail);
        }

        var result = await action();
        return result.Status switch
        {
            ReleaseCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            ReleaseCommandStatus.Success => Results.Ok(result.Value),
            ReleaseCommandStatus.NotFound => Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, result.ErrorCode ?? ApiErrorCodes.ResourceNotFound, "Resource not found.", result.ErrorMessage)),
            ReleaseCommandStatus.ValidationError => Results.BadRequest(ApiProblemDetailsFactory.Create(StatusCodes.Status400BadRequest, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Validation failed.", result.ErrorMessage)),
            ReleaseCommandStatus.Conflict => Results.Conflict(ApiProblemDetailsFactory.Create(StatusCodes.Status409Conflict, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Request conflict.", result.ErrorMessage)),
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
