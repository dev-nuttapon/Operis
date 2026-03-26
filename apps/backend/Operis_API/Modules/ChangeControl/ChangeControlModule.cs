using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.ChangeControl.Application;
using Operis_API.Modules.ChangeControl.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.ChangeControl;

public sealed class ChangeControlModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IChangeControlQueries, ChangeControlQueries>();
        services.AddScoped<IChangeControlCommands, ChangeControlCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/change-control")
            .WithTags("ChangeControl")
            .RequireAuthorization();

        group.MapGet("/change-requests", ListChangeRequestsAsync);
        group.MapGet("/change-requests/{changeRequestId:guid}", GetChangeRequestAsync);
        group.MapPost("/change-requests", CreateChangeRequestAsync);
        group.MapPut("/change-requests/{changeRequestId:guid}", UpdateChangeRequestAsync);
        group.MapPut("/change-requests/{changeRequestId:guid}/submit", SubmitChangeRequestAsync);
        group.MapPut("/change-requests/{changeRequestId:guid}/approve", ApproveChangeRequestAsync);
        group.MapPut("/change-requests/{changeRequestId:guid}/reject", RejectChangeRequestAsync);
        group.MapPut("/change-requests/{changeRequestId:guid}/implement", ImplementChangeRequestAsync);
        group.MapPut("/change-requests/{changeRequestId:guid}/close", CloseChangeRequestAsync);

        group.MapGet("/configuration-items", ListConfigurationItemsAsync);
        group.MapGet("/configuration-items/{configurationItemId:guid}", GetConfigurationItemAsync);
        group.MapPost("/configuration-items", CreateConfigurationItemAsync);
        group.MapPut("/configuration-items/{configurationItemId:guid}", UpdateConfigurationItemAsync);
        group.MapPut("/configuration-items/{configurationItemId:guid}/approve", ApproveConfigurationItemAsync);

        group.MapGet("/baseline-registry", ListBaselineRegistryAsync);
        group.MapGet("/baseline-registry/{baselineRegistryId:guid}", GetBaselineRegistryAsync);
        group.MapPost("/baseline-registry", CreateBaselineRegistryAsync);
        group.MapPut("/baseline-registry/{baselineRegistryId:guid}/approve", ApproveBaselineRegistryAsync);
        group.MapPut("/baseline-registry/{baselineRegistryId:guid}/supersede", SupersedeBaselineRegistryAsync);

        return endpoints;
    }

    private static async Task<IResult> ListChangeRequestsAsync(ClaimsPrincipal principal, [AsParameters] ChangeControlListQuery query, IChangeControlQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.ChangeControl.Read))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read change requests.");
        }

        return Results.Ok(await queries.ListChangeRequestsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetChangeRequestAsync(ClaimsPrincipal principal, Guid changeRequestId, IChangeControlQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ReadSingleAsync(principal, permissionMatrix, Permissions.ChangeControl.Read, "You do not have permission to read change requests.", () => queries.GetChangeRequestAsync(changeRequestId, cancellationToken));

    private static async Task<IResult> CreateChangeRequestAsync(ClaimsPrincipal principal, CreateChangeRequestRequest request, IChangeControlCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.ChangeControl.Manage, "You do not have permission to manage change requests.", () => commands.CreateChangeRequestAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateChangeRequestAsync(ClaimsPrincipal principal, Guid changeRequestId, UpdateChangeRequestRequest request, IChangeControlCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.ChangeControl.Manage, "You do not have permission to manage change requests.", () => commands.UpdateChangeRequestAsync(changeRequestId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> SubmitChangeRequestAsync(ClaimsPrincipal principal, Guid changeRequestId, IChangeControlCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.ChangeControl.Manage, "You do not have permission to manage change requests.", () => commands.SubmitChangeRequestAsync(changeRequestId, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ApproveChangeRequestAsync(ClaimsPrincipal principal, Guid changeRequestId, ChangeDecisionRequest request, IChangeControlCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.ChangeControl.Approve, "You do not have permission to approve change requests.", () => commands.ApproveChangeRequestAsync(changeRequestId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> RejectChangeRequestAsync(ClaimsPrincipal principal, Guid changeRequestId, ChangeDecisionRequest request, IChangeControlCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.ChangeControl.Approve, "You do not have permission to approve change requests.", () => commands.RejectChangeRequestAsync(changeRequestId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ImplementChangeRequestAsync(ClaimsPrincipal principal, Guid changeRequestId, ChangeImplementationRequest request, IChangeControlCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.ChangeControl.Manage, "You do not have permission to manage change requests.", () => commands.ImplementChangeRequestAsync(changeRequestId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> CloseChangeRequestAsync(ClaimsPrincipal principal, Guid changeRequestId, ChangeImplementationRequest request, IChangeControlCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.ChangeControl.Manage, "You do not have permission to manage change requests.", () => commands.CloseChangeRequestAsync(changeRequestId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListConfigurationItemsAsync(ClaimsPrincipal principal, [AsParameters] ChangeControlListQuery query, IChangeControlQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.ChangeControl.ReadConfiguration))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read configuration items.");
        }

        return Results.Ok(await queries.ListConfigurationItemsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetConfigurationItemAsync(ClaimsPrincipal principal, Guid configurationItemId, IChangeControlQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ReadSingleAsync(principal, permissionMatrix, Permissions.ChangeControl.ReadConfiguration, "You do not have permission to read configuration items.", () => queries.GetConfigurationItemAsync(configurationItemId, cancellationToken));

    private static async Task<IResult> CreateConfigurationItemAsync(ClaimsPrincipal principal, CreateConfigurationItemRequest request, IChangeControlCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.ChangeControl.ManageConfiguration, "You do not have permission to manage configuration items.", () => commands.CreateConfigurationItemAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateConfigurationItemAsync(ClaimsPrincipal principal, Guid configurationItemId, UpdateConfigurationItemRequest request, IChangeControlCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.ChangeControl.ManageConfiguration, "You do not have permission to manage configuration items.", () => commands.UpdateConfigurationItemAsync(configurationItemId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ApproveConfigurationItemAsync(ClaimsPrincipal principal, Guid configurationItemId, IChangeControlCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.ChangeControl.ManageConfiguration, "You do not have permission to manage configuration items.", () => commands.ApproveConfigurationItemAsync(configurationItemId, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListBaselineRegistryAsync(ClaimsPrincipal principal, [AsParameters] ChangeControlListQuery query, IChangeControlQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.ChangeControl.ManageBaselines, Permissions.ChangeControl.ApproveBaselines, Permissions.ChangeControl.ReadConfiguration))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read baseline registry.");
        }

        return Results.Ok(await queries.ListBaselineRegistryAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetBaselineRegistryAsync(ClaimsPrincipal principal, Guid baselineRegistryId, IChangeControlQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ReadSingleAsync(principal, permissionMatrix, Permissions.ChangeControl.ManageBaselines, "You do not have permission to read baseline registry.", () => queries.GetBaselineRegistryAsync(baselineRegistryId, cancellationToken), Permissions.ChangeControl.ApproveBaselines, Permissions.ChangeControl.ReadConfiguration);

    private static async Task<IResult> CreateBaselineRegistryAsync(ClaimsPrincipal principal, CreateBaselineRegistryRequest request, IChangeControlCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.ChangeControl.ManageBaselines, "You do not have permission to manage baselines.", () => commands.CreateBaselineRegistryAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> ApproveBaselineRegistryAsync(ClaimsPrincipal principal, Guid baselineRegistryId, IChangeControlCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.ChangeControl.ApproveBaselines, "You do not have permission to approve baselines.", () => commands.ApproveBaselineRegistryAsync(baselineRegistryId, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> SupersedeBaselineRegistryAsync(ClaimsPrincipal principal, Guid baselineRegistryId, BaselineOverrideRequest request, IChangeControlCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.ChangeControl.ApproveBaselines, "You do not have permission to supersede baselines.", () => commands.SupersedeBaselineRegistryAsync(baselineRegistryId, request, ResolveActor(principal), permissionMatrix.HasPermission(principal, Permissions.ChangeControl.EmergencyOverride), cancellationToken));

    private static async Task<IResult> ReadSingleAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<T?>> loader, params string[] alternatePermissions)
        where T : class
    {
        if (LacksPermission(principal, permissionMatrix, permission, alternatePermissions))
        {
            return ForbiddenWithCode("Forbidden.", forbiddenDetail);
        }

        var item = await loader();
        return item is null ? NotFoundWithCode() : Results.Ok(item);
    }

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<ChangeControlCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (LacksPermission(principal, permissionMatrix, permission))
        {
            return ForbiddenWithCode("Forbidden.", forbiddenDetail);
        }

        var result = await action();
        return result.Status switch
        {
            ChangeControlCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            ChangeControlCommandStatus.Success => Results.Ok(result.Value),
            ChangeControlCommandStatus.NotFound => NotFoundWithCode(result.ErrorMessage, result.ErrorCode),
            ChangeControlCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            ChangeControlCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            _ => ProblemWithCode("Request failed.", result.ErrorMessage, result.ErrorCode, StatusCodes.Status500InternalServerError, ApiErrorCodes.InternalFailure)
        };
    }

    private static string? ResolveActor(ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Email)
        ?? principal.FindFirstValue("preferred_username")
        ?? principal.FindFirstValue("sub")
        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

    private static bool LacksPermission(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, params string[] alternatePermissions)
    {
        if (permissionMatrix.HasPermission(principal, permission))
        {
            return false;
        }

        return !alternatePermissions.Any(permissionName => permissionMatrix.HasPermission(principal, permissionName));
    }

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
