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

        businessGroup.MapGet("/", ListBusinessAuditEventsAsync)
            .WithName("AuditEvents_List");

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

    private static async Task<IResult> ListBusinessAuditEventsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IBusinessAuditEventQueries queries,
        string? module,
        string? eventType,
        string? entityType,
        string? entityId,
        string? actor,
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

        var result = await queries.ListAsync(
            new BusinessAuditEventListQuery(module, eventType, entityType, entityId, actor, from, to, page, pageSize),
            cancellationToken);

        return Results.Ok(result);
    }
}
