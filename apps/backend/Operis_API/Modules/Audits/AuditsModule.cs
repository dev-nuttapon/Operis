using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Contracts;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;

namespace Operis_API.Modules.Audits;

public sealed class AuditsModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/audit-logs")
            .WithTags("Audit Logs")
            .RequireAuthorization();

        group.MapGet("/", ListAuditLogsAsync)
            .WithName("AuditLogs_List");

        return endpoints;
    }

    private static async Task<IResult> ListAuditLogsAsync(
        OperisDbContext dbContext,
        IAuditLogWriter auditLogWriter,
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
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 10, 100);
        var skip = (normalizedPage - 1) * normalizedPageSize;
        var query = dbContext.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(module))
        {
            query = query.Where(x => x.Module == module.Trim());
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(x => x.Action == action.Trim());
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(x => x.EntityType == entityType.Trim());
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            var normalizedEntityId = entityId.Trim();
            query = query.Where(x => x.EntityId != null && x.EntityId.Contains(normalizedEntityId));
        }

        if (!string.IsNullOrWhiteSpace(actor))
        {
            var normalizedActor = actor.Trim().ToLowerInvariant();
            query = query.Where(x =>
                (x.ActorEmail != null && x.ActorEmail.ToLower().Contains(normalizedActor))
                || (x.ActorDisplayName != null && x.ActorDisplayName.ToLower().Contains(normalizedActor))
                || (x.ActorUserId != null && x.ActorUserId.ToLower().Contains(normalizedActor)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim());
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.OccurredAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.OccurredAt <= to.Value);
        }

        query = ApplySorting(query, sortBy, sortOrder);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(skip)
            .Take(normalizedPageSize)
            .Select(x => new AuditLogResponse(
                x.Id,
                x.OccurredAt,
                x.Module,
                x.Action,
                x.EntityType,
                x.EntityId,
                x.ActorType,
                x.ActorUserId,
                x.ActorEmail,
                x.ActorDisplayName,
                x.DepartmentId,
                x.TenantId,
                x.RequestId,
                x.TraceId,
                x.CorrelationId,
                x.HttpMethod,
                x.RequestPath,
                x.IpAddress,
                x.UserAgent,
                x.Status,
                x.StatusCode,
                x.ErrorCode,
                x.ErrorMessage,
                x.Reason,
                x.Source,
                x.BeforeJson,
                x.AfterJson,
                x.ChangesJson,
                x.MetadataJson,
                x.IsSensitive,
                x.RetentionClass))
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "audits",
            Action: "list",
            EntityType: "audit_log",
            StatusCode: StatusCodes.Status200OK,
            Metadata: new
            {
                count = items.Count,
                module,
                action,
                entityType,
                entityId,
                actor,
                status,
                sortBy,
                sortOrder,
                from,
                to,
                total,
                page = normalizedPage,
                pageSize = normalizedPageSize
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new PagedResult<AuditLogResponse>(items, total, normalizedPage, normalizedPageSize));
    }

    private static IQueryable<AuditLogEntity> ApplySorting(IQueryable<AuditLogEntity> query, string? sortBy, string? sortOrder)
    {
        var desc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return sortBy?.ToLowerInvariant() switch
        {
            "module" => desc ? query.OrderByDescending(x => x.Module).ThenByDescending(x => x.OccurredAt) : query.OrderBy(x => x.Module).ThenByDescending(x => x.OccurredAt),
            "action" => desc ? query.OrderByDescending(x => x.Action).ThenByDescending(x => x.OccurredAt) : query.OrderBy(x => x.Action).ThenByDescending(x => x.OccurredAt),
            "status" => desc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.OccurredAt) : query.OrderBy(x => x.Status).ThenByDescending(x => x.OccurredAt),
            _ => desc ? query.OrderByDescending(x => x.OccurredAt) : query.OrderBy(x => x.OccurredAt)
        };
    }
}
