using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Governance.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Governance.Application;

public sealed class GovernanceOperationsQueries(OperisDbContext dbContext) : IGovernanceOperationsQueries
{
    public async Task<PagedResult<RaciMapResponse>> ListRaciMapsAsync(RaciMapListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.RaciMaps.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.ProcessCode))
        {
            source = source.Where(x => x.ProcessCode == query.ProcessCode);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.ProcessCode, pattern) || EF.Functions.ILike(x.RoleName, pattern));
        }

        source = source.OrderBy(x => x.ProcessCode).ThenBy(x => x.RoleName);
        return await PageAsync(source.Select(x => new RaciMapResponse(x.Id, x.ProcessCode, x.RoleName, x.ResponsibilityType, x.Status, x.CreatedAt, x.UpdatedAt)), query.Page, query.PageSize, cancellationToken);
    }

    public async Task<PagedResult<ApprovalEvidenceLogResponse>> ListApprovalEvidenceAsync(ApprovalEvidenceListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.ApprovalEvidenceLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            source = source.Where(x => x.EntityType == query.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(query.ActorUserId))
        {
            var pattern = $"%{query.ActorUserId.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.ApproverUserId, pattern));
        }

        if (!string.IsNullOrWhiteSpace(query.Outcome))
        {
            source = source.Where(x => x.Outcome == query.Outcome);
        }

        if (query.ApprovedFrom is not null)
        {
            var from = query.ApprovedFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            source = source.Where(x => x.ApprovedAt >= from);
        }

        if (query.ApprovedTo is not null)
        {
            var toExclusive = query.ApprovedTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            source = source.Where(x => x.ApprovedAt < toExclusive);
        }

        source = source.OrderByDescending(x => x.ApprovedAt);
        return await PageAsync(source.Select(x => new ApprovalEvidenceLogResponse(x.Id, x.EntityType, x.EntityId, x.ApproverUserId, x.ApprovedAt, x.Reason, x.Outcome)), query.Page, query.PageSize, cancellationToken);
    }

    public async Task<PagedResult<WorkflowOverrideLogResponse>> ListWorkflowOverridesAsync(WorkflowOverrideListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.WorkflowOverrideLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            source = source.Where(x => x.EntityType == query.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(query.RequestedBy))
        {
            var pattern = $"%{query.RequestedBy.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.RequestedBy, pattern));
        }

        if (!string.IsNullOrWhiteSpace(query.ApprovedBy))
        {
            var pattern = $"%{query.ApprovedBy.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.ApprovedBy, pattern));
        }

        if (query.OccurredFrom is not null)
        {
            var from = query.OccurredFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            source = source.Where(x => x.OccurredAt >= from);
        }

        if (query.OccurredTo is not null)
        {
            var toExclusive = query.OccurredTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            source = source.Where(x => x.OccurredAt < toExclusive);
        }

        source = source.OrderByDescending(x => x.OccurredAt);
        return await PageAsync(source.Select(x => new WorkflowOverrideLogResponse(x.Id, x.EntityType, x.EntityId, x.RequestedBy, x.ApprovedBy, x.Reason, x.OccurredAt)), query.Page, query.PageSize, cancellationToken);
    }

    public async Task<PagedResult<SlaRuleResponse>> ListSlaRulesAsync(SlaRuleListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.SlaRules.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.ScopeType))
        {
            source = source.Where(x => x.ScopeType == query.ScopeType);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.ScopeType, pattern) || EF.Functions.ILike(x.ScopeRef, pattern) || EF.Functions.ILike(x.EscalationPolicyId, pattern));
        }

        source = source.OrderBy(x => x.ScopeType).ThenBy(x => x.ScopeRef);
        return await PageAsync(source.Select(x => new SlaRuleResponse(x.Id, x.ScopeType, x.ScopeRef, x.TargetDurationHours, x.EscalationPolicyId, x.Status, x.CreatedAt, x.UpdatedAt)), query.Page, query.PageSize, cancellationToken);
    }

    public async Task<PagedResult<RetentionPolicyResponse>> ListRetentionPoliciesAsync(RetentionPolicyListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.RetentionPolicies.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.AppliesTo))
        {
            source = source.Where(x => x.AppliesTo == query.AppliesTo);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.PolicyCode, pattern) || EF.Functions.ILike(x.AppliesTo, pattern) || (x.ArchiveRule != null && EF.Functions.ILike(x.ArchiveRule, pattern)));
        }

        source = source.OrderBy(x => x.PolicyCode);
        return await PageAsync(source.Select(x => new RetentionPolicyResponse(x.Id, x.PolicyCode, x.AppliesTo, x.RetentionPeriodDays, x.ArchiveRule, x.Status, x.CreatedAt, x.UpdatedAt)), query.Page, query.PageSize, cancellationToken);
    }

    public async Task<PagedResult<ArchitectureRecordResponse>> ListArchitectureRecordsAsync(ArchitectureRecordListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.ArchitectureRecords.AsNoTracking()
            .Join(
                dbContext.Projects.AsNoTracking(),
                architecture => architecture.ProjectId,
                project => project.Id,
                (architecture, project) => new { Architecture = architecture, ProjectName = project.Name })
            .AsQueryable();

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Architecture.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Architecture.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerUserId))
        {
            source = source.Where(x => x.Architecture.OwnerUserId == query.OwnerUserId);
        }

        if (!string.IsNullOrWhiteSpace(query.ArchitectureType))
        {
            source = source.Where(x => x.Architecture.ArchitectureType == query.ArchitectureType);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Architecture.Title, pattern) ||
                EF.Functions.ILike(x.ProjectName, pattern) ||
                (x.Architecture.CurrentVersionId != null && EF.Functions.ILike(x.Architecture.CurrentVersionId, pattern)));
        }

        source = source.OrderBy(x => x.Architecture.Title);
        return await PageAsync(
            source.Select(x => new ArchitectureRecordResponse(
                x.Architecture.Id,
                x.Architecture.ProjectId,
                x.ProjectName,
                x.Architecture.Title,
                x.Architecture.ArchitectureType,
                x.Architecture.OwnerUserId,
                x.Architecture.Status,
                x.Architecture.CurrentVersionId,
                x.Architecture.Summary,
                x.Architecture.SecurityImpact,
                x.Architecture.EvidenceRef,
                x.Architecture.ApprovedBy,
                x.Architecture.ApprovedAt,
                x.Architecture.CreatedAt,
                x.Architecture.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<ArchitectureRecordResponse?> GetArchitectureRecordAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.ArchitectureRecords.AsNoTracking()
            .Where(x => x.Id == id)
            .Join(
                dbContext.Projects.AsNoTracking(),
                architecture => architecture.ProjectId,
                project => project.Id,
                (architecture, project) => new ArchitectureRecordResponse(
                    architecture.Id,
                    architecture.ProjectId,
                    project.Name,
                    architecture.Title,
                    architecture.ArchitectureType,
                    architecture.OwnerUserId,
                    architecture.Status,
                    architecture.CurrentVersionId,
                    architecture.Summary,
                    architecture.SecurityImpact,
                    architecture.EvidenceRef,
                    architecture.ApprovedBy,
                    architecture.ApprovedAt,
                    architecture.CreatedAt,
                    architecture.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<DesignReviewResponse>> ListDesignReviewsAsync(DesignReviewListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.DesignReviews.AsNoTracking()
            .Join(
                dbContext.ArchitectureRecords.AsNoTracking(),
                review => review.ArchitectureRecordId,
                architecture => architecture.Id,
                (review, architecture) => new { Review = review, ArchitectureTitle = architecture.Title })
            .AsQueryable();

        if (query.ArchitectureRecordId.HasValue)
        {
            source = source.Where(x => x.Review.ArchitectureRecordId == query.ArchitectureRecordId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Review.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.ReviewType))
        {
            source = source.Where(x => x.Review.ReviewType == query.ReviewType);
        }

        if (!string.IsNullOrWhiteSpace(query.ReviewedBy))
        {
            source = source.Where(x => x.Review.ReviewedBy == query.ReviewedBy);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.ArchitectureTitle, pattern) ||
                EF.Functions.ILike(x.Review.ReviewType, pattern) ||
                (x.Review.DecisionReason != null && EF.Functions.ILike(x.Review.DecisionReason, pattern)));
        }

        source = source.OrderByDescending(x => x.Review.UpdatedAt);
        return await PageAsync(
            source.Select(x => new DesignReviewResponse(
                x.Review.Id,
                x.Review.ArchitectureRecordId,
                x.ArchitectureTitle,
                x.Review.ReviewType,
                x.Review.ReviewedBy,
                x.Review.Status,
                x.Review.DecisionReason,
                x.Review.DesignSummary,
                x.Review.Concerns,
                x.Review.EvidenceRef,
                x.Review.DecidedAt,
                x.Review.CreatedAt,
                x.Review.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<IntegrationReviewResponse>> ListIntegrationReviewsAsync(IntegrationReviewListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.IntegrationReviews.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.IntegrationType))
        {
            source = source.Where(x => x.IntegrationType == query.IntegrationType);
        }

        if (!string.IsNullOrWhiteSpace(query.ReviewedBy))
        {
            source = source.Where(x => x.ReviewedBy == query.ReviewedBy);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.ScopeRef, pattern) ||
                EF.Functions.ILike(x.IntegrationType, pattern) ||
                (x.DependencyImpact != null && EF.Functions.ILike(x.DependencyImpact, pattern)));
        }

        source = source.OrderByDescending(x => x.UpdatedAt);
        return await PageAsync(
            source.Select(x => new IntegrationReviewResponse(
                x.Id,
                x.ScopeRef,
                x.IntegrationType,
                x.ReviewedBy,
                x.Status,
                x.DecisionReason,
                x.Risks,
                x.DependencyImpact,
                x.EvidenceRef,
                x.DecidedAt,
                x.AppliedAt,
                x.CreatedAt,
                x.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    private static async Task<PagedResult<T>> PageAsync<T>(IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((normalizedPage - 1) * normalizedPageSize).Take(normalizedPageSize).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, total, normalizedPage, normalizedPageSize);
    }
}
