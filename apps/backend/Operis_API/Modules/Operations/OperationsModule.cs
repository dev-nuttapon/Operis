using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Operations.Application;
using Operis_API.Modules.Operations.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Operations;

public sealed class OperationsModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IOperationsQueries, OperationsQueries>();
        services.AddScoped<IOperationsCommands, OperationsCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var accessReviews = endpoints.MapGroup("/api/v1/access-reviews").WithTags("Operations").RequireAuthorization();
        accessReviews.MapGet("/", ListAccessReviewsAsync);
        accessReviews.MapPost("/", CreateAccessReviewAsync);
        accessReviews.MapPut("/{id:guid}", UpdateAccessReviewAsync);
        accessReviews.MapPut("/{id:guid}/approve", ApproveAccessReviewAsync);

        var securityReviews = endpoints.MapGroup("/api/v1/security-reviews").WithTags("Operations").RequireAuthorization();
        securityReviews.MapGet("/", ListSecurityReviewsAsync);
        securityReviews.MapPost("/", CreateSecurityReviewAsync);
        securityReviews.MapPut("/{id:guid}", UpdateSecurityReviewAsync);

        var dependencies = endpoints.MapGroup("/api/v1/external-dependencies").WithTags("Operations").RequireAuthorization();
        dependencies.MapGet("/", ListExternalDependenciesAsync);
        dependencies.MapPost("/", CreateExternalDependencyAsync);
        dependencies.MapPut("/{id:guid}", UpdateExternalDependencyAsync);

        var suppliers = endpoints.MapGroup("/api/v1/suppliers").WithTags("Operations").RequireAuthorization();
        suppliers.MapGet("/", ListSuppliersAsync);
        suppliers.MapPost("/", CreateSupplierAsync);
        suppliers.MapGet("/{id:guid}", GetSupplierAsync);
        suppliers.MapPut("/{id:guid}", UpdateSupplierAsync);

        var supplierAgreements = endpoints.MapGroup("/api/v1/supplier-agreements").WithTags("Operations").RequireAuthorization();
        supplierAgreements.MapGet("/", ListSupplierAgreementsAsync);
        supplierAgreements.MapPost("/", CreateSupplierAgreementAsync);
        supplierAgreements.MapPut("/{id:guid}", UpdateSupplierAgreementAsync);

        var audits = endpoints.MapGroup("/api/v1/configuration-audits").WithTags("Operations").RequireAuthorization();
        audits.MapGet("/", ListConfigurationAuditsAsync);
        audits.MapPost("/", CreateConfigurationAuditAsync);

        return endpoints;
    }

    private static async Task<IResult> ListAccessReviewsAsync(ClaimsPrincipal principal, [AsParameters] AccessReviewListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read access reviews.");
        }

        return Results.Ok(await queries.ListAccessReviewsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateAccessReviewAsync(ClaimsPrincipal principal, CreateAccessReviewRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage access reviews.", () => commands.CreateAccessReviewAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateAccessReviewAsync(ClaimsPrincipal principal, Guid id, UpdateAccessReviewRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage access reviews.", () => commands.UpdateAccessReviewAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ApproveAccessReviewAsync(ClaimsPrincipal principal, Guid id, ApproveAccessReviewRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Approve, "You do not have permission to approve access reviews.", () => commands.ApproveAccessReviewAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListSecurityReviewsAsync(ClaimsPrincipal principal, [AsParameters] SecurityReviewListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read security reviews.");
        }

        return Results.Ok(await queries.ListSecurityReviewsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateSecurityReviewAsync(ClaimsPrincipal principal, CreateSecurityReviewRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage security reviews.", () => commands.CreateSecurityReviewAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateSecurityReviewAsync(ClaimsPrincipal principal, Guid id, UpdateSecurityReviewRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage security reviews.", () => commands.UpdateSecurityReviewAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListExternalDependenciesAsync(ClaimsPrincipal principal, [AsParameters] ExternalDependencyListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read external dependencies.");
        }

        return Results.Ok(await queries.ListExternalDependenciesAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateExternalDependencyAsync(ClaimsPrincipal principal, CreateExternalDependencyRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage external dependencies.", () => commands.CreateExternalDependencyAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateExternalDependencyAsync(ClaimsPrincipal principal, Guid id, UpdateExternalDependencyRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage external dependencies.", () => commands.UpdateExternalDependencyAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListSuppliersAsync(ClaimsPrincipal principal, [AsParameters] SupplierListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read suppliers.");
        }

        return Results.Ok(await queries.ListSuppliersAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetSupplierAsync(ClaimsPrincipal principal, Guid id, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read suppliers.");
        }

        var detail = await queries.GetSupplierAsync(id, cancellationToken);
        return detail is null
            ? Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.ResourceNotFound, "Supplier not found.", "Supplier not found."))
            : Results.Ok(detail);
    }

    private static async Task<IResult> CreateSupplierAsync(ClaimsPrincipal principal, CreateSupplierRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage suppliers.", () => commands.CreateSupplierAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateSupplierAsync(ClaimsPrincipal principal, Guid id, UpdateSupplierRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage suppliers.", () => commands.UpdateSupplierAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListSupplierAgreementsAsync(ClaimsPrincipal principal, [AsParameters] SupplierAgreementListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read supplier agreements.");
        }

        return Results.Ok(await queries.ListSupplierAgreementsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateSupplierAgreementAsync(ClaimsPrincipal principal, CreateSupplierAgreementRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage supplier agreements.", () => commands.CreateSupplierAgreementAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateSupplierAgreementAsync(ClaimsPrincipal principal, Guid id, UpdateSupplierAgreementRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage supplier agreements.", () => commands.UpdateSupplierAgreementAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListConfigurationAuditsAsync(ClaimsPrincipal principal, [AsParameters] ConfigurationAuditListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read configuration audits.");
        }

        return Results.Ok(await queries.ListConfigurationAuditsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateConfigurationAuditAsync(ClaimsPrincipal principal, CreateConfigurationAuditRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage configuration audits.", () => commands.CreateConfigurationAuditAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<OperationsCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (!permissionMatrix.HasPermission(principal, permission))
        {
            return Forbidden(forbiddenDetail);
        }

        var result = await action();
        return result.Status switch
        {
            OperationsCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            OperationsCommandStatus.Success => Results.Ok(result.Value),
            OperationsCommandStatus.NotFound => Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, result.ErrorCode ?? ApiErrorCodes.ResourceNotFound, "Resource not found.", result.ErrorMessage)),
            OperationsCommandStatus.ValidationError => Results.BadRequest(ApiProblemDetailsFactory.Create(StatusCodes.Status400BadRequest, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Validation failed.", result.ErrorMessage)),
            OperationsCommandStatus.Conflict => Results.Conflict(ApiProblemDetailsFactory.Create(StatusCodes.Status409Conflict, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Request conflict.", result.ErrorMessage)),
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
