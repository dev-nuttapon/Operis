using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public sealed record ProjectHistoryListQuery(
    Guid ProjectId,
    string? Search,
    int Page = 1,
    int PageSize = 10);

public interface IProjectHistoryQueries
{
    Task<PagedResult<ProjectHistoryItem>> ListAsync(ProjectHistoryListQuery query, CancellationToken cancellationToken);
}

public sealed class ProjectHistoryQueries(OperisDbContext dbContext) : IProjectHistoryQueries
{
    public async Task<PagedResult<ProjectHistoryItem>> ListAsync(ProjectHistoryListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var baseQuery = dbContext.ProjectHistories
            .AsNoTracking()
            .Where(x => x.ProjectId == query.ProjectId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            baseQuery = baseQuery.Where(x =>
                EF.Functions.ILike(x.EventType, search)
                || (x.Summary != null && EF.Functions.ILike(x.Summary, search))
                || (x.Reason != null && EF.Functions.ILike(x.Reason, search))
                || (x.ActorDisplayName != null && EF.Functions.ILike(x.ActorDisplayName, search))
                || (x.ActorEmail != null && EF.Functions.ILike(x.ActorEmail, search)));
        }

        var total = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(x => x.OccurredAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new ProjectHistoryItem(
                x.Id,
                x.ProjectId,
                x.EventType,
                x.Summary,
                x.Reason,
                x.ActorUserId,
                x.ActorEmail,
                x.ActorDisplayName,
                x.BeforeJson,
                x.AfterJson,
                x.MetadataJson,
                x.OccurredAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjectHistoryItem>(items, total, page, pageSize);
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 5, 100);
        var skip = (normalizedPage - 1) * normalizedPageSize;
        return (normalizedPage, normalizedPageSize, skip);
    }
}
