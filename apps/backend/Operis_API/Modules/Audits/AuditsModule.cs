using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Application;
using Operis_API.Modules.Audits.Contracts;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;
using System.Security.Claims;

namespace Operis_API.Modules.Audits;

public sealed class AuditsModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuditLogQueries, AuditLogQueries>();
        services.AddScoped<IBusinessAuditEventQueries, BusinessAuditEventQueries>();
        services.AddScoped<IAuditComplianceQueries, AuditComplianceQueries>();
        services.AddScoped<IAuditComplianceCommands, AuditComplianceCommands>();
        services.AddScoped<IBusinessAuditEventWriter, BusinessAuditEventWriter>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/audit-logs")
            .WithTags("Audit Logs")
            .RequireAuthorization();

        group.MapGet("/", ListAuditLogsAsync)
            .WithName("AuditLogs_List");

        group.MapGet("/{auditLogId:guid}", GetAuditLogAsync)
            .WithName("AuditLogs_Get");

        var businessGroup = endpoints.MapGroup("/api/v1/audit-events")
            .WithTags("Audit Events")
            .RequireAuthorization();

        businessGroup.MapGet("/", ListAuditEventsAsync)
            .WithName("AuditEvents_List");

        var plans = endpoints.MapGroup("/api/v1/audit-plans")
            .WithTags("Audit Plans")
            .RequireAuthorization();

        plans.MapGet("/", ListAuditPlansAsync);
        plans.MapPost("/", CreateAuditPlanAsync);
        plans.MapGet("/{auditPlanId:guid}", GetAuditPlanAsync);
        plans.MapPut("/{auditPlanId:guid}", UpdateAuditPlanAsync);

        var findings = endpoints.MapGroup("/api/v1/audit-findings")
            .WithTags("Audit Findings")
            .RequireAuthorization();

        findings.MapPost("/", CreateAuditFindingAsync);
        findings.MapPut("/{auditFindingId:guid}", UpdateAuditFindingAsync);
        findings.MapPut("/{auditFindingId:guid}/close", CloseAuditFindingAsync);

        var exports = endpoints.MapGroup("/api/v1/evidence-exports")
            .WithTags("Evidence Exports")
            .RequireAuthorization();

        exports.MapPost("/", CreateEvidenceExportAsync);
        exports.MapGet("/", ListEvidenceExportsAsync);
        exports.MapGet("/{exportId:guid}", GetEvidenceExportAsync);

        return endpoints;
    }

    private static async Task<IResult> ListAuditLogsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IAuditLogQueries queries,
        string? module,
        string? action,
        string? entityType,
        string? entityId,
        string? actor,
        string? status,
        string? sortBy,
        string? sortOrder,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.AuditLogs.Read))
        {
            return Results.Forbid();
        }

        var result = await queries.ListAuditLogsAsync(
            new AuditLogListQuery(module, action, entityType, entityId, actor, status, sortBy, sortOrder, from, to, page, pageSize),
            cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetAuditLogAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IAuditLogQueries queries,
        Guid auditLogId,
        CancellationToken cancellationToken = default)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.AuditLogs.Read))
        {
            return Results.Forbid();
        }

        var result = await queries.GetAuditLogAsync(auditLogId, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> ListAuditEventsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IAuditComplianceQueries queries,
        Guid? projectId,
        string? entityType,
        string? action,
        string? actorUserId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        string? outcome,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.AuditLogs.Read))
        {
            return Results.Forbid();
        }

        var result = await queries.ListAuditEventsAsync(
            new AuditEventListQuery(projectId, entityType, action, actorUserId, from, to, outcome, page, pageSize),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> ListAuditPlansAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IAuditComplianceQueries queries,
        Guid? projectId,
        string? status,
        string? ownerUserId,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.AuditLogs.Read))
        {
            return Results.Forbid();
        }

        return Results.Ok(await queries.ListAuditPlansAsync(new AuditPlanListQuery(projectId, status, ownerUserId, page, pageSize), cancellationToken));
    }

    private static async Task<IResult> GetAuditPlanAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IAuditComplianceQueries queries,
        Guid auditPlanId,
        CancellationToken cancellationToken = default)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.AuditLogs.Read))
        {
            return Results.Forbid();
        }

        var item = await queries.GetAuditPlanAsync(auditPlanId, cancellationToken);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> CreateAuditPlanAsync(ClaimsPrincipal principal, CreateAuditPlanRequest request, IAuditComplianceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.AuditLogs.Manage, () => commands.CreateAuditPlanAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateAuditPlanAsync(ClaimsPrincipal principal, Guid auditPlanId, UpdateAuditPlanRequest request, IAuditComplianceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.AuditLogs.Manage, () => commands.UpdateAuditPlanAsync(auditPlanId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> CreateAuditFindingAsync(ClaimsPrincipal principal, CreateAuditFindingRequest request, IAuditComplianceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.AuditLogs.Manage, () => commands.CreateAuditFindingAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateAuditFindingAsync(ClaimsPrincipal principal, Guid auditFindingId, UpdateAuditFindingRequest request, IAuditComplianceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.AuditLogs.Manage, () => commands.UpdateAuditFindingAsync(auditFindingId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> CloseAuditFindingAsync(ClaimsPrincipal principal, Guid auditFindingId, CloseAuditFindingRequest request, IAuditComplianceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.AuditLogs.Manage, () => commands.CloseAuditFindingAsync(auditFindingId, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> CreateEvidenceExportAsync(ClaimsPrincipal principal, CreateEvidenceExportRequest request, IAuditComplianceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.AuditLogs.Export, () => commands.CreateEvidenceExportAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> ListEvidenceExportsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IAuditComplianceQueries queries,
        string? scopeType,
        string? status,
        string? requestedBy,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (!permissionMatrix.HasAnyPermission(principal, Permissions.AuditLogs.Read, Permissions.AuditLogs.Export))
        {
            return Results.Forbid();
        }

        return Results.Ok(await queries.ListEvidenceExportsAsync(new EvidenceExportListQuery(scopeType, status, requestedBy, page, pageSize), cancellationToken));
    }

    private static async Task<IResult> GetEvidenceExportAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IAuditComplianceQueries queries,
        Guid exportId,
        CancellationToken cancellationToken = default)
    {
        if (!permissionMatrix.HasAnyPermission(principal, Permissions.AuditLogs.Read, Permissions.AuditLogs.Export))
        {
            return Results.Forbid();
        }

        var item = await queries.GetEvidenceExportAsync(exportId, cancellationToken);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, Func<Task<AuditComplianceCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (!permissionMatrix.HasPermission(principal, permission))
        {
            return Results.Forbid();
        }

        var result = await action();
        return result.Status switch
        {
            AuditComplianceCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            AuditComplianceCommandStatus.Success => Results.Ok(result.Value),
            AuditComplianceCommandStatus.NotFound => Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, result.ErrorCode ?? ApiErrorCodes.ResourceNotFound, "Resource not found.", result.ErrorMessage)),
            AuditComplianceCommandStatus.ValidationError => Results.BadRequest(ApiProblemDetailsFactory.Create(StatusCodes.Status400BadRequest, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Validation failed.", result.ErrorMessage)),
            AuditComplianceCommandStatus.Conflict => Results.Conflict(ApiProblemDetailsFactory.Create(StatusCodes.Status409Conflict, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Request conflict.", result.ErrorMessage)),
            _ => Results.Problem(ApiProblemDetailsFactory.Create(StatusCodes.Status500InternalServerError, ApiErrorCodes.InternalFailure, "Request failed.", result.ErrorMessage))
        };
    }

    private static string? ResolveActor(ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Email)
        ?? principal.FindFirstValue("preferred_username")
        ?? principal.FindFirstValue("sub")
        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
}
