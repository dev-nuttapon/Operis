using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Defects.Application;
using Operis_API.Modules.Defects.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Defects;

public sealed class DefectsModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDefectQueries, DefectQueries>();
        services.AddScoped<IDefectCommands, DefectCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var defects = endpoints.MapGroup("/api/v1/defects").WithTags("Defects").RequireAuthorization();
        defects.MapGet("/", ListDefectsAsync);
        defects.MapPost("/", CreateDefectAsync);
        defects.MapGet("/{id:guid}", GetDefectAsync);
        defects.MapPut("/{id:guid}", UpdateDefectAsync);
        defects.MapPut("/{id:guid}/resolve", ResolveDefectAsync);
        defects.MapPut("/{id:guid}/close", CloseDefectAsync);

        var nonConformances = endpoints.MapGroup("/api/v1/non-conformances").WithTags("Defects").RequireAuthorization();
        nonConformances.MapGet("/", ListNonConformancesAsync);
        nonConformances.MapPost("/", CreateNonConformanceAsync);
        nonConformances.MapGet("/{id:guid}", GetNonConformanceAsync);
        nonConformances.MapPut("/{id:guid}", UpdateNonConformanceAsync);
        nonConformances.MapPut("/{id:guid}/close", CloseNonConformanceAsync);

        return endpoints;
    }

    private static async Task<IResult> ListDefectsAsync(ClaimsPrincipal principal, [AsParameters] DefectListQuery query, IDefectQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Defects.Read))
        {
            return Forbidden("You do not have permission to read defects.");
        }

        return Results.Ok(await queries.ListDefectsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetDefectAsync(ClaimsPrincipal principal, Guid id, IDefectQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Defects.Read))
        {
            return Forbidden("You do not have permission to read defects.");
        }

        var detail = await queries.GetDefectAsync(id, cancellationToken);
        return detail is null
            ? Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.DefectNotFound, "Defect not found.", "Defect not found."))
            : Results.Ok(detail);
    }

    private static async Task<IResult> CreateDefectAsync(ClaimsPrincipal principal, CreateDefectRequest request, IDefectCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Defects.Manage, "You do not have permission to manage defects.", () => commands.CreateDefectAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateDefectAsync(ClaimsPrincipal principal, Guid id, UpdateDefectRequest request, IDefectCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Defects.Manage, "You do not have permission to manage defects.", () => commands.UpdateDefectAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ResolveDefectAsync(ClaimsPrincipal principal, Guid id, ResolveDefectRequest request, IDefectCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Defects.Manage, "You do not have permission to resolve defects.", () => commands.ResolveDefectAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> CloseDefectAsync(ClaimsPrincipal principal, Guid id, CloseDefectRequest request, IDefectCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Defects.Manage, "You do not have permission to close defects.", () => commands.CloseDefectAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListNonConformancesAsync(ClaimsPrincipal principal, [AsParameters] NonConformanceListQuery query, IDefectQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Defects.Read))
        {
            return Forbidden("You do not have permission to read non-conformances.");
        }

        return Results.Ok(await queries.ListNonConformancesAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetNonConformanceAsync(ClaimsPrincipal principal, Guid id, IDefectQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Defects.Read))
        {
            return Forbidden("You do not have permission to read non-conformances.");
        }

        var detail = await queries.GetNonConformanceAsync(id, cancellationToken);
        return detail is null
            ? Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.NonConformanceNotFound, "Non-conformance not found.", "Non-conformance not found."))
            : Results.Ok(detail);
    }

    private static async Task<IResult> CreateNonConformanceAsync(ClaimsPrincipal principal, CreateNonConformanceRequest request, IDefectCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Defects.Manage, "You do not have permission to manage non-conformances.", () => commands.CreateNonConformanceAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateNonConformanceAsync(ClaimsPrincipal principal, Guid id, UpdateNonConformanceRequest request, IDefectCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Defects.Manage, "You do not have permission to manage non-conformances.", () => commands.UpdateNonConformanceAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> CloseNonConformanceAsync(ClaimsPrincipal principal, Guid id, CloseNonConformanceRequest request, IDefectCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Defects.Manage, "You do not have permission to close non-conformances.", () => commands.CloseNonConformanceAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<DefectCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (!permissionMatrix.HasPermission(principal, permission))
        {
            return Forbidden(forbiddenDetail);
        }

        var result = await action();
        return result.Status switch
        {
            DefectCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            DefectCommandStatus.Success => Results.Ok(result.Value),
            DefectCommandStatus.NotFound => Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, result.ErrorCode ?? ApiErrorCodes.ResourceNotFound, "Resource not found.", result.ErrorMessage)),
            DefectCommandStatus.ValidationError => Results.BadRequest(ApiProblemDetailsFactory.Create(StatusCodes.Status400BadRequest, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Validation failed.", result.ErrorMessage)),
            DefectCommandStatus.Conflict => Results.Conflict(ApiProblemDetailsFactory.Create(StatusCodes.Status409Conflict, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Request conflict.", result.ErrorMessage)),
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
