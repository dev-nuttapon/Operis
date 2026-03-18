using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Contracts;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Audits.Application;

public sealed class AuditLogQueries(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter) : IAuditLogQueries
{
    public async Task<PagedResult<AuditLogListItem>> ListAuditLogsAsync(AuditLogListQuery request, CancellationToken cancellationToken)
    {
        var normalizedPage = request.Page < 1 ? 1 : request.Page;
        var normalizedPageSize = Math.Clamp(request.PageSize, 10, 100);
        var skip = (normalizedPage - 1) * normalizedPageSize;
        var query = dbContext.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Module))
        {
            query = query.Where(x => x.Module == request.Module.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(x => x.Action == request.Action.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(x => x.EntityType == request.EntityType.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.EntityId))
        {
            var normalizedEntityId = request.EntityId.Trim();
            query = query.Where(x => x.EntityId != null && x.EntityId.Contains(normalizedEntityId));
        }

        if (!string.IsNullOrWhiteSpace(request.Actor))
        {
            var actorPattern = $"%{request.Actor.Trim()}%";
            query = query.Where(x =>
                (x.ActorEmail != null && EF.Functions.ILike(x.ActorEmail, actorPattern))
                || (x.ActorDisplayName != null && EF.Functions.ILike(x.ActorDisplayName, actorPattern))
                || (x.ActorUserId != null && EF.Functions.ILike(x.ActorUserId, actorPattern)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(x => x.Status == request.Status.Trim());
        }

        if (request.From.HasValue)
        {
            query = query.Where(x => x.OccurredAt >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(x => x.OccurredAt <= request.To.Value);
        }

        query = ApplySorting(query, request.SortBy, request.SortOrder);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(skip)
            .Take(normalizedPageSize)
            .Select(x => new AuditLogListItem(
                x.Id,
                x.OccurredAt,
                x.Module,
                x.Action,
                x.EntityType,
                x.EntityId,
                x.ActorUserId,
                x.ActorEmail,
                x.ActorDisplayName,
                x.Status,
                x.HttpMethod,
                x.RequestPath))
            .ToListAsync(cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "audits",
            Action: "list",
            EntityType: "audit_log",
            StatusCode: StatusCodes.Status200OK,
            Audience: LogAudience.AuditOnly,
            Metadata: new
            {
                count = items.Count,
                request.Module,
                request.Action,
                request.EntityType,
                request.EntityId,
                request.Actor,
                request.Status,
                request.SortBy,
                request.SortOrder,
                request.From,
                request.To,
                total,
                page = normalizedPage,
                pageSize = normalizedPageSize
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<AuditLogListItem>(items, total, normalizedPage, normalizedPageSize);
    }

    public async Task<AuditLogResponse?> GetAuditLogAsync(Guid auditLogId, CancellationToken cancellationToken)
    {
        return await dbContext.AuditLogs
            .AsNoTracking()
            .Where(x => x.Id == auditLogId)
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
            .SingleOrDefaultAsync(cancellationToken);
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
