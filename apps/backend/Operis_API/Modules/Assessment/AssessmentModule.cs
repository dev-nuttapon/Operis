using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Assessment.Application;
using Operis_API.Modules.Assessment.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Assessment;

public sealed class AssessmentModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAssessmentQueries, AssessmentQueries>();
        services.AddScoped<IAssessmentCommands, AssessmentCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/assessment").WithTags("Assessment").RequireAuthorization();
        group.MapGet("/packages", ListPackagesAsync);
        group.MapGet("/packages/{packageId:guid}", GetPackageAsync);
        group.MapPost("/packages", CreatePackageAsync);
        group.MapPost("/packages/{packageId:guid}/transition", TransitionPackageAsync);
        group.MapPost("/packages/{packageId:guid}/notes", AddPackageNoteAsync);
        group.MapGet("/findings", ListFindingsAsync);
        group.MapGet("/findings/{findingId:guid}", GetFindingAsync);
        group.MapPost("/findings", CreateFindingAsync);
        group.MapPost("/findings/{findingId:guid}/transition", TransitionFindingAsync);
        group.MapGet("/control-catalog", ListControlCatalogAsync);
        group.MapGet("/control-catalog/{controlId:guid}", GetControlCatalogItemAsync);
        group.MapPost("/control-catalog", CreateControlCatalogItemAsync);
        group.MapPut("/control-catalog/{controlId:guid}", UpdateControlCatalogItemAsync);
        group.MapGet("/control-mappings", ListControlMappingsAsync);
        group.MapGet("/control-mappings/{mappingId:guid}", GetControlMappingAsync);
        group.MapPost("/control-mappings", CreateControlMappingAsync);
        group.MapPost("/control-mappings/{mappingId:guid}/transition", TransitionControlMappingAsync);
        group.MapGet("/control-coverage", ListControlCoverageAsync);
        return endpoints;
    }

    private static async Task<IResult> ListPackagesAsync(ClaimsPrincipal principal, [AsParameters] AssessmentPackageListQuery query, IAssessmentQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanRead(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read assessment packages.");
        }

        return Results.Ok(await queries.ListPackagesAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetPackageAsync(ClaimsPrincipal principal, Guid packageId, IAssessmentQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanRead(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read assessment packages.");
        }

        var detail = await queries.GetPackageAsync(packageId, cancellationToken);
        return detail is null
            ? Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.AssessmentPackageNotFound, "Assessment package not found.", "Assessment package not found."))
            : Results.Ok(detail);
    }

    private static async Task<IResult> ListFindingsAsync(ClaimsPrincipal principal, [AsParameters] AssessmentFindingListQuery query, IAssessmentQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanRead(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read assessment findings.");
        }

        return Results.Ok(await queries.ListFindingsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetFindingAsync(ClaimsPrincipal principal, Guid findingId, IAssessmentQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanRead(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read assessment findings.");
        }

        var detail = await queries.GetFindingAsync(findingId, cancellationToken);
        return detail is null
            ? Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.AssessmentFindingNotFound, "Assessment finding not found.", "Assessment finding not found."))
            : Results.Ok(detail);
    }

    private static Task<IResult> CreatePackageAsync(ClaimsPrincipal principal, CreateAssessmentPackageRequest request, IAssessmentCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Assessment.WorkspaceManage, "You do not have permission to manage assessment packages.", () => commands.CreatePackageAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static Task<IResult> TransitionPackageAsync(ClaimsPrincipal principal, Guid packageId, TransitionAssessmentPackageRequest request, IAssessmentCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        var requiresReview = string.Equals(request.TargetStatus?.Trim(), "shared", StringComparison.OrdinalIgnoreCase);
        var permission = requiresReview ? Permissions.Assessment.WorkspaceReview : Permissions.Assessment.WorkspaceManage;
        var message = requiresReview
            ? "You do not have permission to share assessment packages."
            : "You do not have permission to manage assessment packages.";

        return ExecuteAsync(principal, permissionMatrix, permission, message, () => commands.TransitionPackageAsync(packageId, request, ResolveActor(principal), cancellationToken));
    }

    private static Task<IResult> AddPackageNoteAsync(ClaimsPrincipal principal, Guid packageId, CreateAssessmentNoteRequest request, IAssessmentCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Assessment.WorkspaceReview, "You do not have permission to add assessor notes.", () => commands.AddPackageNoteAsync(packageId, request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static Task<IResult> CreateFindingAsync(ClaimsPrincipal principal, CreateAssessmentFindingRequest request, IAssessmentCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Assessment.WorkspaceReview, "You do not have permission to manage assessment findings.", () => commands.CreateFindingAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static Task<IResult> TransitionFindingAsync(ClaimsPrincipal principal, Guid findingId, TransitionAssessmentFindingRequest request, IAssessmentCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Assessment.WorkspaceReview, "You do not have permission to review assessment findings.", () => commands.TransitionFindingAsync(findingId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListControlCatalogAsync(ClaimsPrincipal principal, [AsParameters] ControlCatalogListQuery query, IAssessmentQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanReadControls(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read control mapping.");
        }

        return Results.Ok(await queries.ListControlCatalogAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetControlCatalogItemAsync(ClaimsPrincipal principal, Guid controlId, IAssessmentQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanReadControls(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read control mapping.");
        }

        var detail = await queries.GetControlCatalogItemAsync(controlId, cancellationToken);
        return detail is null
            ? Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.ResourceNotFound, "Control catalog item not found.", "Control catalog item not found."))
            : Results.Ok(detail);
    }

    private static Task<IResult> CreateControlCatalogItemAsync(ClaimsPrincipal principal, CreateControlCatalogItemRequest request, IAssessmentCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Assessment.ControlsManage, "You do not have permission to manage control catalog items.", () => commands.CreateControlCatalogItemAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static Task<IResult> UpdateControlCatalogItemAsync(ClaimsPrincipal principal, Guid controlId, UpdateControlCatalogItemRequest request, IAssessmentCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Assessment.ControlsManage, "You do not have permission to manage control catalog items.", () => commands.UpdateControlCatalogItemAsync(controlId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListControlMappingsAsync(ClaimsPrincipal principal, [AsParameters] ControlMappingListQuery query, IAssessmentQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanReadControls(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read control mappings.");
        }

        return Results.Ok(await queries.ListControlMappingsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetControlMappingAsync(ClaimsPrincipal principal, Guid mappingId, IAssessmentQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanReadControls(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read control mappings.");
        }

        var detail = await queries.GetControlMappingAsync(mappingId, cancellationToken);
        return detail is null
            ? Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.ResourceNotFound, "Control mapping not found.", "Control mapping not found."))
            : Results.Ok(detail);
    }

    private static Task<IResult> CreateControlMappingAsync(ClaimsPrincipal principal, CreateControlMappingRequest request, IAssessmentCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Assessment.ControlsManage, "You do not have permission to manage control mappings.", () => commands.CreateControlMappingAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static Task<IResult> TransitionControlMappingAsync(ClaimsPrincipal principal, Guid mappingId, TransitionControlMappingRequest request, IAssessmentCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        ExecuteAsync(principal, permissionMatrix, Permissions.Assessment.ControlsManage, "You do not have permission to manage control mappings.", () => commands.TransitionControlMappingAsync(mappingId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListControlCoverageAsync(ClaimsPrincipal principal, [AsParameters] ControlCoverageListQuery query, IAssessmentQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!CanReadControls(principal, permissionMatrix))
        {
            return Forbidden("You do not have permission to read control coverage.");
        }

        return Results.Ok(await queries.ListControlCoverageAsync(query, ResolveActor(principal), cancellationToken));
    }

    private static bool CanRead(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix) =>
        permissionMatrix.HasAnyPermission(principal, Permissions.Assessment.WorkspaceRead, Permissions.Assessment.WorkspaceManage, Permissions.Assessment.WorkspaceReview);

    private static bool CanReadControls(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix) =>
        permissionMatrix.HasAnyPermission(principal, Permissions.Assessment.ControlsRead, Permissions.Assessment.ControlsManage);

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<AssessmentCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (!permissionMatrix.HasPermission(principal, permission))
        {
            return Forbidden(forbiddenDetail);
        }

        var result = await action();
        return result.Status switch
        {
            AssessmentCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            AssessmentCommandStatus.Success => Results.Ok(result.Value),
            AssessmentCommandStatus.NotFound => Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, result.ErrorCode ?? ApiErrorCodes.ResourceNotFound, "Resource not found.", result.ErrorMessage)),
            AssessmentCommandStatus.ValidationError => Results.BadRequest(ApiProblemDetailsFactory.Create(StatusCodes.Status400BadRequest, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Validation failed.", result.ErrorMessage)),
            AssessmentCommandStatus.Conflict => Results.Conflict(ApiProblemDetailsFactory.Create(StatusCodes.Status409Conflict, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Request conflict.", result.ErrorMessage)),
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
