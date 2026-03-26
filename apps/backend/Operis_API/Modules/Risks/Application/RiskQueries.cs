using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Risks.Contracts;
using Operis_API.Modules.Risks.Infrastructure;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Risks.Application;

public sealed class RiskQueries(OperisDbContext dbContext) : IRiskQueries
{
    public async Task<PagedResult<RiskListItemResponse>> ListRisksAsync(RiskListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source =
            from risk in dbContext.Set<RiskEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on risk.ProjectId equals project.Id
            select new { Risk = risk, ProjectName = project.Name };

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Risk.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            source = source.Where(x => x.Risk.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerUserId))
        {
            var owner = query.OwnerUserId.Trim();
            source = source.Where(x => x.Risk.OwnerUserId == owner);
        }

        if (query.RiskLevel.HasValue)
        {
            var riskLevel = query.RiskLevel.Value;
            source = source.Where(x => x.Risk.Probability * x.Risk.Impact == riskLevel);
        }

        if (query.NextReviewBefore.HasValue)
        {
            source = source.Where(x => x.Risk.NextReviewAt.HasValue && x.Risk.NextReviewAt.Value <= query.NextReviewBefore.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Risk.Code, search)
                || EF.Functions.ILike(x.Risk.Title, search)
                || EF.Functions.ILike(x.Risk.Description, search)
                || EF.Functions.ILike(x.ProjectName, search));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(x => x.Risk.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new RiskListItemResponse(
                x.Risk.Id,
                x.Risk.ProjectId,
                x.ProjectName,
                x.Risk.Code,
                x.Risk.Title,
                x.Risk.Probability,
                x.Risk.Impact,
                x.Risk.OwnerUserId,
                x.Risk.Status,
                x.Risk.NextReviewAt,
                x.Risk.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<RiskListItemResponse>(items, total, page, pageSize);
    }

    public async Task<RiskDetailResponse?> GetRiskAsync(Guid riskId, CancellationToken cancellationToken)
    {
        var item = await (
            from risk in dbContext.Set<RiskEntity>().AsNoTracking()
            where risk.Id == riskId
            join project in dbContext.Projects.AsNoTracking() on risk.ProjectId equals project.Id
            select new { Risk = risk, ProjectName = project.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var reviews = await dbContext.Set<RiskReviewEntity>().AsNoTracking()
            .Where(x => x.RiskId == riskId)
            .OrderByDescending(x => x.ReviewedAt)
            .Select(x => new RiskReviewItemResponse(x.Id, x.RiskId, x.ReviewedBy, x.ReviewedAt, x.Decision, x.Notes))
            .ToListAsync(cancellationToken);

        var history = await dbContext.BusinessAuditEvents.AsNoTracking()
            .Where(x => x.EntityType == "risk" && x.EntityId == riskId.ToString())
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => new RiskHistoryItem(x.Id, x.EventType, x.Summary, x.Reason, x.ActorUserId, x.OccurredAt))
            .ToListAsync(cancellationToken);

        return new RiskDetailResponse(
            item.Risk.Id,
            item.Risk.ProjectId,
            item.ProjectName,
            item.Risk.Code,
            item.Risk.Title,
            item.Risk.Description,
            item.Risk.Probability,
            item.Risk.Impact,
            item.Risk.OwnerUserId,
            item.Risk.MitigationPlan,
            item.Risk.Cause,
            item.Risk.Effect,
            item.Risk.ContingencyPlan,
            item.Risk.Status,
            item.Risk.NextReviewAt,
            reviews,
            history,
            item.Risk.CreatedAt,
            item.Risk.UpdatedAt);
    }

    public async Task<PagedResult<IssueListItemResponse>> ListIssuesAsync(IssueListQuery query, bool canReadSensitive, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var actionCounts =
            from action in dbContext.Set<IssueActionEntity>().AsNoTracking()
            group action by action.IssueId into grouped
            select new
            {
                IssueId = grouped.Key,
                OpenCount = grouped.Count(item => item.Status != "completed" && item.Status != "verified")
            };

        var source =
            from issue in dbContext.Set<IssueEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on issue.ProjectId equals project.Id
            join actionCount in actionCounts on issue.Id equals actionCount.IssueId into actionJoin
            from actionCount in actionJoin.DefaultIfEmpty()
            select new { Issue = issue, ProjectName = project.Name, OpenActionCount = (int?)actionCount.OpenCount };

        if (!canReadSensitive)
        {
            source = source.Where(x => !x.Issue.IsSensitive);
        }

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Issue.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            source = source.Where(x => x.Issue.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerUserId))
        {
            var owner = query.OwnerUserId.Trim();
            source = source.Where(x => x.Issue.OwnerUserId == owner);
        }

        if (!string.IsNullOrWhiteSpace(query.Severity))
        {
            var severity = query.Severity.Trim().ToLowerInvariant();
            source = source.Where(x => x.Issue.Severity == severity);
        }

        if (query.DueBefore.HasValue)
        {
            source = source.Where(x => x.Issue.DueDate.HasValue && x.Issue.DueDate.Value <= query.DueBefore.Value);
        }

        if (query.DueAfter.HasValue)
        {
            source = source.Where(x => x.Issue.DueDate.HasValue && x.Issue.DueDate.Value >= query.DueAfter.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Issue.Code, search)
                || EF.Functions.ILike(x.Issue.Title, search)
                || EF.Functions.ILike(x.ProjectName, search));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderBy(x => x.Issue.DueDate ?? DateOnly.MaxValue)
            .ThenByDescending(x => x.Issue.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new IssueListItemResponse(
                x.Issue.Id,
                x.Issue.ProjectId,
                x.ProjectName,
                x.Issue.Code,
                x.Issue.Title,
                x.Issue.Severity,
                x.Issue.OwnerUserId,
                x.Issue.DueDate,
                x.Issue.Status,
                x.OpenActionCount ?? 0,
                x.Issue.IsSensitive,
                x.Issue.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<IssueListItemResponse>(items, total, page, pageSize);
    }

    public async Task<IssueDetailResponse?> GetIssueAsync(Guid issueId, bool canReadSensitive, CancellationToken cancellationToken)
    {
        var item = await (
            from issue in dbContext.Set<IssueEntity>().AsNoTracking()
            where issue.Id == issueId
            join project in dbContext.Projects.AsNoTracking() on issue.ProjectId equals project.Id
            select new { Issue = issue, ProjectName = project.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (item is null || (item.Issue.IsSensitive && !canReadSensitive))
        {
            return null;
        }

        var actions = await dbContext.Set<IssueActionEntity>().AsNoTracking()
            .Where(x => x.IssueId == issueId)
            .OrderBy(x => x.DueDate ?? DateOnly.MaxValue)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new IssueActionResponse(x.Id, x.IssueId, x.ActionDescription, x.AssignedTo, x.DueDate, x.Status, x.VerificationNote, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var history = await dbContext.BusinessAuditEvents.AsNoTracking()
            .Where(x => (x.EntityType == "issue" || x.EntityType == "issue_action") && x.EntityId == issueId.ToString())
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => new RiskHistoryItem(x.Id, x.EventType, x.Summary, x.Reason, x.ActorUserId, x.OccurredAt))
            .ToListAsync(cancellationToken);

        return new IssueDetailResponse(
            item.Issue.Id,
            item.Issue.ProjectId,
            item.ProjectName,
            item.Issue.Code,
            item.Issue.Title,
            item.Issue.Description,
            item.Issue.OwnerUserId,
            item.Issue.DueDate,
            item.Issue.Status,
            item.Issue.Severity,
            item.Issue.RootIssue,
            item.Issue.Dependencies,
            item.Issue.ResolutionSummary,
            item.Issue.IsSensitive,
            item.Issue.SensitiveContext,
            actions,
            history,
            item.Issue.CreatedAt,
            item.Issue.UpdatedAt);
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize switch
        {
            <= 0 => 25,
            > 100 => 100,
            _ => pageSize
        };

        return (normalizedPage, normalizedPageSize, (normalizedPage - 1) * normalizedPageSize);
    }
}
