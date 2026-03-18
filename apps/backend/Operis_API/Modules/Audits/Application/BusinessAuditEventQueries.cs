using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Audits.Application;

public sealed class BusinessAuditEventQueries(OperisDbContext dbContext) : IBusinessAuditEventQueries
{
    public async Task<PagedResult<BusinessAuditEventItem>> ListAsync(BusinessAuditEventListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery = dbContext.BusinessAuditEvents.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Module))
        {
            baseQuery = baseQuery.Where(x => x.Module == query.Module);
        }

        if (!string.IsNullOrWhiteSpace(query.EventType))
        {
            baseQuery = baseQuery.Where(x => x.EventType == query.EventType);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            baseQuery = baseQuery.Where(x => x.EntityType == query.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            baseQuery = baseQuery.Where(x => x.EntityId == query.EntityId);
        }

        if (!string.IsNullOrWhiteSpace(query.Actor))
        {
            var search = $"%{query.Actor.Trim()}%";
            baseQuery = baseQuery.Where(x =>
                (x.ActorDisplayName != null && EF.Functions.ILike(x.ActorDisplayName, search))
                || (x.ActorEmail != null && EF.Functions.ILike(x.ActorEmail, search))
                || (x.ActorUserId != null && EF.Functions.ILike(x.ActorUserId, search)));
        }

        if (query.From.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.OccurredAt >= query.From);
        }

        if (query.To.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.OccurredAt <= query.To);
        }

        var total = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(x => x.OccurredAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new BusinessAuditEventItem(
                x.Id,
                x.OccurredAt,
                x.Module,
                x.EventType,
                x.EntityType,
                x.EntityId,
                x.Summary,
                x.Reason,
                x.ActorUserId,
                x.ActorEmail,
                x.ActorDisplayName,
                x.MetadataJson))
            .ToListAsync(cancellationToken);

        return new PagedResult<BusinessAuditEventItem>(items, total, page, pageSize);
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 5, 100);
        var skip = (normalizedPage - 1) * normalizedPageSize;
        return (normalizedPage, normalizedPageSize, skip);
    }
}
