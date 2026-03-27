using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Exceptions.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Exceptions.Application;

public sealed class ExceptionQueries(OperisDbContext dbContext) : IExceptionQueries
{
    public async Task<PagedResult<WaiverListItemResponse>> ListWaiversAsync(WaiverListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source =
            from waiver in dbContext.Waivers.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on waiver.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new { Waiver = waiver, ProjectName = project != null ? project.Name : null };

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Waiver.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.ProcessArea))
        {
            source = source.Where(x => x.Waiver.ProcessArea == query.ProcessArea.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Waiver.Status == query.Status.Trim().ToLowerInvariant());
        }

        if (query.OnlyExpired)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            source = source.Where(x => x.Waiver.ExpiresAt < today && x.Waiver.Status != "closed");
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Waiver.WaiverCode, search) ||
                EF.Functions.ILike(x.Waiver.ProcessArea, search) ||
                EF.Functions.ILike(x.Waiver.ScopeSummary, search) ||
                (x.ProjectName != null && EF.Functions.ILike(x.ProjectName, search)));
        }

        var total = await source.CountAsync(cancellationToken);
        var rows = await source
            .OrderByDescending(x => x.Waiver.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var waiverIds = rows.Select(x => x.Waiver.Id).ToArray();
        var controlCounts = await dbContext.CompensatingControls.AsNoTracking()
            .Where(x => waiverIds.Contains(x.WaiverId))
            .GroupBy(x => x.WaiverId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var todayValue = DateOnly.FromDateTime(DateTime.UtcNow);
        var items = rows.Select(x => new WaiverListItemResponse(
            x.Waiver.Id,
            x.Waiver.WaiverCode,
            x.Waiver.ProjectId,
            x.ProjectName,
            x.Waiver.ProcessArea,
            x.Waiver.ScopeSummary,
            x.Waiver.RequestedByUserId,
            x.Waiver.EffectiveFrom,
            x.Waiver.ExpiresAt,
            x.Waiver.ExpiresAt < todayValue && x.Waiver.Status != "closed",
            x.Waiver.Status,
            controlCounts.GetValueOrDefault(x.Waiver.Id),
            x.Waiver.UpdatedAt)).ToList();

        return new PagedResult<WaiverListItemResponse>(items, total, page, pageSize);
    }

    public async Task<WaiverDetailResponse?> GetWaiverAsync(Guid waiverId, CancellationToken cancellationToken)
    {
        var waiver = await (
            from entity in dbContext.Waivers.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on entity.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            where entity.Id == waiverId
            select new
            {
                Waiver = entity,
                ProjectName = project != null ? project.Name : null
            }).SingleOrDefaultAsync(cancellationToken);

        if (waiver is null)
        {
            return null;
        }

        var controls = await dbContext.CompensatingControls.AsNoTracking()
            .Where(x => x.WaiverId == waiverId)
            .OrderBy(x => x.ControlCode)
            .Select(x => new CompensatingControlResponse(x.Id, x.ControlCode, x.Description, x.OwnerUserId, x.Status, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var reviews = await dbContext.WaiverReviews.AsNoTracking()
            .Where(x => x.WaiverId == waiverId)
            .OrderByDescending(x => x.ReviewedAt)
            .Select(x => new WaiverReviewResponse(x.Id, x.ReviewType, x.OutcomeStatus, x.ReviewerUserId, x.Notes, x.ReviewedAt, x.NextReviewAt))
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new WaiverDetailResponse(
            waiver.Waiver.Id,
            waiver.Waiver.WaiverCode,
            waiver.Waiver.ProjectId,
            waiver.ProjectName,
            waiver.Waiver.ProcessArea,
            waiver.Waiver.ScopeSummary,
            waiver.Waiver.RequestedByUserId,
            waiver.Waiver.Justification,
            waiver.Waiver.EffectiveFrom,
            waiver.Waiver.ExpiresAt,
            waiver.Waiver.ExpiresAt < today && waiver.Waiver.Status != "closed",
            waiver.Waiver.Status,
            waiver.Waiver.DecisionReason,
            waiver.Waiver.DecisionByUserId,
            waiver.Waiver.DecisionAt,
            waiver.Waiver.ClosureReason,
            controls,
            reviews,
            waiver.Waiver.CreatedAt,
            waiver.Waiver.UpdatedAt);
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int? page, int? pageSize)
    {
        var normalizedPage = Math.Max(page.GetValueOrDefault(1), 1);
        var normalizedPageSize = Math.Clamp(pageSize.GetValueOrDefault(25), 1, 100);
        return (normalizedPage, normalizedPageSize, (normalizedPage - 1) * normalizedPageSize);
    }
}
