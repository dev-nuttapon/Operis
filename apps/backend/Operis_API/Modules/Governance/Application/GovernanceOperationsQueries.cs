using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.ChangeControl.Infrastructure;
using Operis_API.Modules.Governance.Contracts;
using Operis_API.Modules.Governance.Infrastructure;
using Operis_API.Modules.Operations.Infrastructure;
using Operis_API.Modules.Requirements.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Governance.Application;

public sealed class GovernanceOperationsQueries(OperisDbContext dbContext, IAuditLogWriter auditLogWriter) : IGovernanceOperationsQueries
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly IReadOnlyDictionary<string, string> ProcessAreaLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["process-assets-planning"] = "Process Assets & Planning",
        ["requirements-traceability"] = "Requirements & Traceability",
        ["document-governance"] = "Document Governance",
        ["change-configuration"] = "Change & Configuration",
        ["verification-release"] = "Verification & Release",
        ["audit-capa"] = "Audit & CAPA",
        ["security-resilience"] = "Security & Resilience"
    };

    private static readonly HashSet<string> OverdueProjectPlanStates = new(StringComparer.OrdinalIgnoreCase) { "review" };
    private static readonly HashSet<string> OverdueTailoringStates = new(StringComparer.OrdinalIgnoreCase) { "submitted" };
    private static readonly HashSet<string> OverdueChangeRequestStates = new(StringComparer.OrdinalIgnoreCase) { "submitted" };
    private static readonly HashSet<string> OverdueUatStates = new(StringComparer.OrdinalIgnoreCase) { "submitted" };
    private static readonly HashSet<string> OpenAuditFindingStates = new(StringComparer.OrdinalIgnoreCase) { "open", "in_progress", "submitted" };
    private static readonly HashSet<string> OpenSecurityReviewStates = new(StringComparer.OrdinalIgnoreCase) { "planned", "draft", "submitted", "open", "in_review" };
    private static readonly HashSet<string> OpenSecurityIncidentStates = new(StringComparer.OrdinalIgnoreCase) { "reported", "triaged", "investigating", "contained" };
    private static readonly HashSet<string> OpenVulnerabilityStates = new(StringComparer.OrdinalIgnoreCase) { "open", "planned", "scheduled" };
    private static readonly HashSet<string> OpenCapaStates = new(StringComparer.OrdinalIgnoreCase) { "open", "planned", "in_progress", "verified" };

    public async Task<ComplianceDashboardResponse> GetComplianceDashboardAsync(ComplianceDashboardQuery query, string? actor, CancellationToken cancellationToken)
    {
        var filters = await ResolveFiltersAsync(query, actor, cancellationToken);
        var bundle = await LoadDashboardBundleAsync(filters.ProjectId, cancellationToken);
        var periodStart = DateTimeOffset.UtcNow.AddDays(-filters.PeriodDays);
        var generatedAt = DateTimeOffset.UtcNow;

        var computations = bundle.Projects
            .Select(project => BuildProjectComputation(project, bundle, periodStart))
            .ToList();

        await PersistSnapshotsAsync(computations, periodStart, generatedAt, actor, cancellationToken);

        var projectRows = computations.Select(x => x.Project).ToList();
        if (filters.ShowOnlyAtRisk)
        {
            projectRows = projectRows.Where(x => !string.Equals(x.ReadinessState, "good", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var projectIds = projectRows.Select(x => x.ProjectId).ToHashSet();
        var processAreaRows = computations
            .SelectMany(x => x.ProcessAreas)
            .Where(x => projectIds.Contains(x.ProjectId))
            .Where(x => filters.ProcessArea is null || string.Equals(x.ProcessArea, filters.ProcessArea, StringComparison.OrdinalIgnoreCase))
            .GroupBy(x => x.ProcessArea, StringComparer.OrdinalIgnoreCase)
            .Select(group => new ComplianceProcessAreaResponse(
                group.Key,
                ProcessAreaLabels.TryGetValue(group.Key, out var label) ? label : group.Key,
                group.Count(),
                group.Count(x => !string.Equals(x.ReadinessState, "good", StringComparison.OrdinalIgnoreCase)),
                group.Sum(x => x.MissingArtifactCount),
                group.Sum(x => x.OverdueApprovalCount),
                group.Sum(x => x.StaleBaselineCount),
                group.Sum(x => x.OpenCapaCount),
                group.Sum(x => x.OpenAuditFindingCount),
                group.Sum(x => x.OpenSecurityItemCount)))
            .OrderBy(x => x.Label)
            .ToList();

        var summary = new ComplianceDashboardSummaryResponse(
            projectRows.Count(x => string.Equals(x.ReadinessState, "good", StringComparison.OrdinalIgnoreCase)),
            projectRows.Count(x => x.MissingArtifactCount > 0),
            projectRows.Sum(x => x.OverdueApprovalCount),
            projectRows.Sum(x => x.StaleBaselineCount),
            projectRows.Sum(x => x.OpenCapaCount),
            projectRows.Sum(x => x.OpenAuditFindingCount),
            projectRows.Sum(x => x.OpenSecurityItemCount));

        return new ComplianceDashboardResponse(
            summary,
            projectRows,
            processAreaRows,
            generatedAt,
            new ComplianceDashboardFiltersResponse(filters.ProjectId, filters.ProcessArea, filters.PeriodDays, filters.ShowOnlyAtRisk));
    }

    public async Task<ComplianceDrilldownResponse> GetComplianceDrilldownAsync(ComplianceDashboardDrilldownQuery query, CancellationToken cancellationToken)
    {
        var issueType = NormalizeIssueType(query.IssueType);
        var processArea = NormalizeProcessArea(query.ProcessArea);
        var bundle = await LoadDashboardBundleAsync(query.ProjectId, cancellationToken);
        var periodStart = DateTimeOffset.UtcNow.AddDays(-30);

        var rows = bundle.Projects
            .SelectMany(project => BuildDrilldownRows(project, bundle, periodStart, issueType, processArea))
            .OrderBy(x => x.Scope)
            .ThenBy(x => x.Title)
            .ToList();

        return new ComplianceDrilldownResponse(issueType, query.ProjectId, processArea, DateTimeOffset.UtcNow, rows);
    }

    public async Task<PagedResult<ManagementReviewListItemResponse>> ListManagementReviewsAsync(ManagementReviewListQuery query, CancellationToken cancellationToken)
    {
        var source = (
            from review in dbContext.ManagementReviews.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on review.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new
            {
                Review = review,
                ProjectName = project == null ? null : project.Name,
                OpenActionCount = dbContext.ManagementReviewActions.Count(action =>
                    action.ReviewId == review.Id
                    && !string.Equals(action.Status, "closed", StringComparison.OrdinalIgnoreCase))
            }).AsQueryable();

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Review.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            source = source.Where(x => x.Review.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.FacilitatorUserId))
        {
            var facilitator = query.FacilitatorUserId.Trim();
            source = source.Where(x => x.Review.FacilitatorUserId == facilitator);
        }

        if (query.ScheduledFrom.HasValue)
        {
            var from = query.ScheduledFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            source = source.Where(x => x.Review.ScheduledAt >= from);
        }

        if (query.ScheduledTo.HasValue)
        {
            var toExclusive = query.ScheduledTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            source = source.Where(x => x.Review.ScheduledAt < toExclusive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Review.ReviewCode, pattern) ||
                EF.Functions.ILike(x.Review.Title, pattern) ||
                EF.Functions.ILike(x.Review.ReviewPeriod, pattern) ||
                (x.ProjectName != null && EF.Functions.ILike(x.ProjectName, pattern)));
        }

        source = source.OrderByDescending(x => x.Review.ScheduledAt).ThenBy(x => x.Review.Title);
        return await PageAsync(
            source.Select(x => new ManagementReviewListItemResponse(
                x.Review.Id,
                x.Review.ProjectId,
                x.ProjectName,
                x.Review.ReviewCode,
                x.Review.Title,
                x.Review.ReviewPeriod,
                x.Review.ScheduledAt,
                x.Review.FacilitatorUserId,
                x.Review.Status,
                x.OpenActionCount,
                x.Review.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<ManagementReviewDetailResponse?> GetManagementReviewAsync(Guid id, CancellationToken cancellationToken)
    {
        var review = await (
            from managementReview in dbContext.ManagementReviews.AsNoTracking()
            where managementReview.Id == id
            join project in dbContext.Projects.AsNoTracking() on managementReview.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new
            {
                Review = managementReview,
                ProjectName = project == null ? null : project.Name
            }).SingleOrDefaultAsync(cancellationToken);

        if (review is null)
        {
            return null;
        }

        var items = await dbContext.ManagementReviewItems.AsNoTracking()
            .Where(x => x.ReviewId == id)
            .OrderBy(x => x.ItemType)
            .ThenBy(x => x.Title)
            .Select(x => new ManagementReviewItemResponse(x.Id, x.ItemType, x.Title, x.Summary, x.Decision, x.OwnerUserId, x.DueAt, x.Status, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var actions = await dbContext.ManagementReviewActions.AsNoTracking()
            .Where(x => x.ReviewId == id)
            .OrderBy(x => x.Status)
            .ThenBy(x => x.DueAt)
            .ThenBy(x => x.Title)
            .Select(x => new ManagementReviewActionResponse(x.Id, x.Title, x.Description, x.OwnerUserId, x.DueAt, x.Status, x.IsMandatory, x.LinkedEntityType, x.LinkedEntityId, x.ClosedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var history = await dbContext.BusinessAuditEvents.AsNoTracking()
            .Where(x => x.EntityType == "management_review" && x.EntityId == id.ToString())
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => new WorkflowOverrideLogResponse(x.Id, x.EntityType, x.EntityId ?? string.Empty, x.ActorUserId ?? "unknown", x.ActorUserId ?? "unknown", x.Reason ?? x.Summary ?? string.Empty, x.OccurredAt))
            .ToListAsync(cancellationToken);

        return new ManagementReviewDetailResponse(
            review.Review.Id,
            review.Review.ProjectId,
            review.ProjectName,
            review.Review.ReviewCode,
            review.Review.Title,
            review.Review.ReviewPeriod,
            review.Review.ScheduledAt,
            review.Review.FacilitatorUserId,
            review.Review.Status,
            review.Review.AgendaSummary,
            review.Review.MinutesSummary,
            review.Review.DecisionSummary,
            review.Review.EscalationEntityType,
            review.Review.EscalationEntityId,
            review.Review.ClosedBy,
            review.Review.ClosedAt,
            items,
            actions,
            history,
            review.Review.CreatedAt,
            review.Review.UpdatedAt);
    }

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

    private async Task<ResolvedDashboardFilters> ResolveFiltersAsync(ComplianceDashboardQuery query, string? actor, CancellationToken cancellationToken)
    {
        var preference = actor is null
            ? null
            : await dbContext.ComplianceDashboardPreferences.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == actor, cancellationToken);

        var resolvedProjectId = query.ProjectId ?? preference?.DefaultProjectId;
        var resolvedProcessArea = NormalizeProcessArea(query.ProcessArea ?? preference?.DefaultProcessArea);
        var resolvedPeriodDays = query.PeriodDays ?? preference?.DefaultPeriodDays ?? 30;
        var resolvedShowOnlyAtRisk = query.ShowOnlyAtRisk ?? preference?.DefaultShowOnlyAtRisk ?? false;
        if (resolvedPeriodDays is < 7 or > 365)
        {
            resolvedPeriodDays = 30;
        }

        return new ResolvedDashboardFilters(resolvedProjectId, resolvedProcessArea, resolvedPeriodDays, resolvedShowOnlyAtRisk);
    }

    private async Task<DashboardBundle> LoadDashboardBundleAsync(Guid? projectId, CancellationToken cancellationToken)
    {
        var projects = await dbContext.Projects.AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .Where(x => projectId == null || x.Id == projectId.Value)
            .OrderBy(x => x.Code)
            .Select(x => new DashboardProject(x.Id, x.Code, x.Name, x.Status, x.Phase, x.Methodology))
            .ToListAsync(cancellationToken);

        var projectIds = projects.Select(x => x.Id).ToArray();
        if (projectIds.Length == 0)
        {
            return DashboardBundle.Empty;
        }

        var projectPlans = await dbContext.ProjectPlans.AsNoTracking()
            .Where(x => projectIds.Contains(x.ProjectId))
            .Select(x => new ProjectPlanLite(x.Id, x.ProjectId, x.Name, x.Status, x.UpdatedAt, x.ApprovedAt))
            .ToListAsync(cancellationToken);

        var tailoringRecords = await dbContext.TailoringRecords.AsNoTracking()
            .Where(x => projectIds.Contains(x.ProjectId))
            .Select(x => new TailoringLite(x.Id, x.ProjectId, x.RequestedChange, x.Status, x.UpdatedAt, x.ApprovedAt))
            .ToListAsync(cancellationToken);

        var requirements = await dbContext.Requirements.AsNoTracking()
            .Where(x => projectIds.Contains(x.ProjectId))
            .Select(x => new RequirementLite(x.Id, x.ProjectId, x.Code, x.Title, x.Status))
            .ToListAsync(cancellationToken);

        var requirementBaselines = await dbContext.RequirementBaselines.AsNoTracking()
            .Where(x => projectIds.Contains(x.ProjectId))
            .Select(x => new RequirementBaselineLite(x.Id, x.ProjectId, x.BaselineName, x.Status, x.ApprovedAt))
            .ToListAsync(cancellationToken);

        var documents = await dbContext.Documents.AsNoTracking()
            .Where(x => x.ProjectId != null && projectIds.Contains(x.ProjectId.Value) && !x.IsDeleted)
            .Select(x => new DocumentLite(x.Id, x.ProjectId!.Value, x.Title, x.Status, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var testPlans = await dbContext.TestPlans.AsNoTracking()
            .Where(x => projectIds.Contains(x.ProjectId))
            .Select(x => new TestPlanLite(x.Id, x.ProjectId, x.Title, x.Status, x.UpdatedAt, x.ApprovedAt, x.BaselinedAt))
            .ToListAsync(cancellationToken);

        var changeRequests = await dbContext.ChangeRequests.AsNoTracking()
            .Where(x => projectIds.Contains(x.ProjectId))
            .Select(x => new ChangeRequestLite(x.Id, x.ProjectId, x.Code, x.Title, x.Status, x.UpdatedAt, x.ApprovedAt))
            .ToListAsync(cancellationToken);

        var baselines = await dbContext.BaselineRegistry.AsNoTracking()
            .Where(x => projectIds.Contains(x.ProjectId))
            .Select(x => new BaselineLite(x.Id, x.ProjectId, x.BaselineName, x.BaselineType, x.SourceEntityType, x.SourceEntityId, x.Status, x.ApprovedAt, x.UpdatedAt, x.SupersededByBaselineId))
            .ToListAsync(cancellationToken);

        var uatSignoffs = await dbContext.UatSignoffs.AsNoTracking()
            .Where(x => projectIds.Contains(x.ProjectId))
            .Select(x => new UatLite(x.Id, x.ProjectId, x.ScopeSummary, x.Status, x.UpdatedAt, x.ApprovedAt))
            .ToListAsync(cancellationToken);

        var auditPlans = await dbContext.AuditPlans.AsNoTracking()
            .Where(x => projectIds.Contains(x.ProjectId))
            .Select(x => new AuditPlanLite(x.Id, x.ProjectId, x.Title))
            .ToListAsync(cancellationToken);

        var auditPlanIds = auditPlans.Select(x => x.Id).ToArray();
        var auditFindings = await dbContext.AuditFindings.AsNoTracking()
            .Where(x => auditPlanIds.Contains(x.AuditPlanId))
            .Select(x => new AuditFindingLite(x.Id, x.AuditPlanId, x.Code, x.Title, x.Severity, x.Status, x.DueDate))
            .ToListAsync(cancellationToken);

        var securityReviews = await dbContext.SecurityReviews.AsNoTracking()
            .Select(x => new SecurityReviewLite(x.Id, x.ScopeType, x.ScopeRef, x.Status))
            .ToListAsync(cancellationToken);

        var securityIncidents = await dbContext.SecurityIncidents.AsNoTracking()
            .Where(x => x.ProjectId != null && projectIds.Contains(x.ProjectId.Value))
            .Select(x => new SecurityIncidentLite(x.Id, x.ProjectId, x.Code, x.Title, x.Severity, x.Status))
            .ToListAsync(cancellationToken);

        var vulnerabilities = await dbContext.VulnerabilityRecords.AsNoTracking()
            .Select(x => new VulnerabilityLite(x.Id, x.AssetRef, x.Title, x.Severity, x.Status, x.PatchDueAt))
            .ToListAsync(cancellationToken);

        var capaRecords = await dbContext.CapaRecords.AsNoTracking()
            .Select(x => new CapaLite(x.Id, x.SourceType, x.SourceRef, x.Title, x.Status))
            .ToListAsync(cancellationToken);

        return new DashboardBundle(
            projects,
            projectPlans,
            tailoringRecords,
            requirements,
            requirementBaselines,
            documents,
            testPlans,
            changeRequests,
            baselines,
            uatSignoffs,
            auditPlans,
            auditFindings,
            securityReviews,
            securityIncidents,
            vulnerabilities,
            capaRecords);
    }

    private ProjectComputation BuildProjectComputation(DashboardProject project, DashboardBundle bundle, DateTimeOffset periodStart)
    {
        var plans = bundle.ProjectPlans.Where(x => x.ProjectId == project.Id).ToList();
        var tailoring = bundle.TailoringRecords.Where(x => x.ProjectId == project.Id).ToList();
        var requirements = bundle.Requirements.Where(x => x.ProjectId == project.Id).ToList();
        var requirementBaselines = bundle.RequirementBaselines.Where(x => x.ProjectId == project.Id).ToList();
        var documents = bundle.Documents.Where(x => x.ProjectId == project.Id).ToList();
        var testPlans = bundle.TestPlans.Where(x => x.ProjectId == project.Id).ToList();
        var changeRequests = bundle.ChangeRequests.Where(x => x.ProjectId == project.Id).ToList();
        var baselines = bundle.Baselines.Where(x => x.ProjectId == project.Id).ToList();
        var uatSignoffs = bundle.UatSignoffs.Where(x => x.ProjectId == project.Id).ToList();
        var auditPlanIds = bundle.AuditPlans.Where(x => x.ProjectId == project.Id).Select(x => x.Id).ToHashSet();
        var auditFindings = bundle.AuditFindings.Where(x => auditPlanIds.Contains(x.AuditPlanId)).ToList();
        var securityReviews = bundle.SecurityReviews.Where(x => IsProjectScoped(x.ScopeType, x.ScopeRef, project.Id)).ToList();
        var securityIncidents = bundle.SecurityIncidents.Where(x => x.ProjectId == project.Id).ToList();
        var vulnerabilities = bundle.Vulnerabilities.Where(x => TryParseProjectReference(x.AssetRef) == project.Id).ToList();

        var capaProjectIds = BuildCapaProjectMap(bundle);
        var capaRecords = bundle.CapaRecords.Where(x => capaProjectIds.TryGetValue(x.Id, out var projectId) && projectId == project.Id).ToList();

        var missingPlan = !plans.Any(x => x.Status is "approved" or "baseline");
        var missingTailoring = !string.IsNullOrWhiteSpace(project.Methodology) && !tailoring.Any(x => x.Status is "approved" or "applied");
        var missingRequirementBaseline = requirements.Count > 0 && !requirementBaselines.Any(x => string.Equals(x.Status, "locked", StringComparison.OrdinalIgnoreCase));
        var missingDocumentBaseline = documents.Count > 0 && !baselines.Any(x => IsDocumentBaseline(x) && IsApprovedBaseline(x.Status));
        var missingTestPlan = RequiresTestPlan(project.Phase) && !testPlans.Any(x => x.Status is "approved" or "baseline");

        var overdueProjectPlanCount = plans.Count(x => OverdueProjectPlanStates.Contains(x.Status) && x.UpdatedAt < periodStart);
        var overdueTailoringCount = tailoring.Count(x => OverdueTailoringStates.Contains(x.Status) && x.UpdatedAt < periodStart);
        var overdueChangeRequestCount = changeRequests.Count(x => OverdueChangeRequestStates.Contains(x.Status) && x.UpdatedAt < periodStart);
        var overdueUatCount = uatSignoffs.Count(x => OverdueUatStates.Contains(x.Status) && x.UpdatedAt < periodStart);

        var staleProjectPlanCount = plans.Count(x => string.Equals(x.Status, "baseline", StringComparison.OrdinalIgnoreCase) && x.UpdatedAt < periodStart);
        var staleRequirementBaselineCount = requirementBaselines.Count(x => x.ApprovedAt < periodStart);
        var staleDocumentBaselineCount = baselines.Count(x => IsDocumentBaseline(x) && IsApprovedBaseline(x.Status) && x.SupersededByBaselineId == null && (x.ApprovedAt ?? x.UpdatedAt) < periodStart);
        var staleChangeBaselineCount = baselines.Count(x => IsChangeBaseline(x) && IsApprovedBaseline(x.Status) && x.SupersededByBaselineId == null && (x.ApprovedAt ?? x.UpdatedAt) < periodStart);

        var openAuditFindingCount = auditFindings.Count(x => OpenAuditFindingStates.Contains(x.Status));
        var openSecurityItemCount = securityReviews.Count(x => OpenSecurityReviewStates.Contains(x.Status))
            + securityIncidents.Count(x => OpenSecurityIncidentStates.Contains(x.Status))
            + vulnerabilities.Count(x => OpenVulnerabilityStates.Contains(x.Status));
        var openCapaCount = capaRecords.Count(x => OpenCapaStates.Contains(x.Status));

        var processAreas = new List<ProcessAreaComputation>
        {
            BuildProcessArea(project, "process-assets-planning", missingPlan.ToCount() + missingTailoring.ToCount(), overdueProjectPlanCount + overdueTailoringCount, staleProjectPlanCount, 0, 0, 0),
            BuildProcessArea(project, "requirements-traceability", missingRequirementBaseline.ToCount(), 0, staleRequirementBaselineCount, 0, 0, 0),
            BuildProcessArea(project, "document-governance", missingDocumentBaseline.ToCount(), 0, staleDocumentBaselineCount, 0, 0, 0),
            BuildProcessArea(project, "change-configuration", 0, overdueChangeRequestCount, staleChangeBaselineCount, 0, 0, 0),
            BuildProcessArea(project, "verification-release", missingTestPlan.ToCount(), overdueUatCount, 0, 0, 0, 0),
            BuildProcessArea(project, "audit-capa", 0, 0, 0, openCapaCount, openAuditFindingCount, 0),
            BuildProcessArea(project, "security-resilience", 0, 0, 0, 0, 0, openSecurityItemCount)
        };

        var totalMissing = processAreas.Sum(x => x.MissingArtifactCount);
        var totalOverdue = processAreas.Sum(x => x.OverdueApprovalCount);
        var totalStale = processAreas.Sum(x => x.StaleBaselineCount);
        var totalCapa = processAreas.Sum(x => x.OpenCapaCount);
        var totalAudit = processAreas.Sum(x => x.OpenAuditFindingCount);
        var totalSecurity = processAreas.Sum(x => x.OpenSecurityItemCount);

        var readinessScore = Math.Max(0, 100 - (totalMissing * 20) - (totalOverdue * 15) - (totalStale * 10) - (totalCapa * 10) - (totalAudit * 12) - (totalSecurity * 15));
        var readinessState = totalSecurity > 0 || totalAudit > 0 || totalMissing >= 2 || readinessScore < 60
            ? "critical"
            : totalMissing > 0 || totalOverdue > 0 || totalStale > 0 || totalCapa > 0 || readinessScore < 85
                ? "at_risk"
                : "good";

        return new ProjectComputation(
            new ComplianceProjectReadinessResponse(
                project.Id,
                project.Code,
                project.Name,
                project.Status,
                project.Phase,
                readinessScore,
                readinessState,
                totalMissing,
                totalOverdue,
                totalStale,
                totalCapa,
                totalAudit,
                totalSecurity),
            processAreas);
    }

    private IEnumerable<ComplianceDrilldownRowResponse> BuildDrilldownRows(DashboardProject project, DashboardBundle bundle, DateTimeOffset periodStart, string issueType, string? processArea)
    {
        var computation = BuildProjectComputation(project, bundle, periodStart);
        var auditPlanIds = bundle.AuditPlans.Where(x => x.ProjectId == project.Id).Select(x => x.Id).ToHashSet();
        var capaProjectIds = BuildCapaProjectMap(bundle);
        var scope = $"{project.Code} · {project.Name}";

        var rows = new List<(string ProcessArea, ComplianceDrilldownRowResponse Row)>();

        if (string.Equals(issueType, "missing-artifact", StringComparison.OrdinalIgnoreCase))
        {
            if (computation.ProcessAreas.First(x => x.ProcessArea == "process-assets-planning").MissingArtifactCount > 0)
            {
                var plans = bundle.ProjectPlans.Where(x => x.ProjectId == project.Id).ToList();
                var tailoring = bundle.TailoringRecords.Where(x => x.ProjectId == project.Id).ToList();
                if (!plans.Any(x => x.Status is "approved" or "baseline"))
                {
                    rows.Add(("process-assets-planning", new ComplianceDrilldownRowResponse(issueType, "project_plan", project.Id.ToString(), "Missing approved project plan", "governance", "/app/project-plans", "missing", scope, null, "Create or approve a project plan.")));
                }

                if (!string.IsNullOrWhiteSpace(project.Methodology) && !tailoring.Any(x => x.Status is "approved" or "applied"))
                {
                    rows.Add(("process-assets-planning", new ComplianceDrilldownRowResponse(issueType, "tailoring_record", project.Id.ToString(), "Missing approved tailoring record", "governance", "/app/tailoring-records", "missing", scope, null, "Tailoring is required for projects with methodology context.")));
                }
            }

            if (computation.ProcessAreas.First(x => x.ProcessArea == "requirements-traceability").MissingArtifactCount > 0)
            {
                rows.Add(("requirements-traceability", new ComplianceDrilldownRowResponse(issueType, "requirement_baseline", project.Id.ToString(), "Missing requirement baseline", "requirements", "/app/requirements/baselines", "missing", scope, null, "Requirements exist without a governed baseline.")));
            }

            if (computation.ProcessAreas.First(x => x.ProcessArea == "document-governance").MissingArtifactCount > 0)
            {
                rows.Add(("document-governance", new ComplianceDrilldownRowResponse(issueType, "document_baseline", project.Id.ToString(), "Missing document baseline", "documents", "/app/documents", "missing", scope, null, "Documents exist without an approved document baseline.")));
            }

            if (computation.ProcessAreas.First(x => x.ProcessArea == "verification-release").MissingArtifactCount > 0)
            {
                rows.Add(("verification-release", new ComplianceDrilldownRowResponse(issueType, "test_plan", project.Id.ToString(), "Missing approved test plan", "verification", "/app/test-plans", "missing", scope, null, "Project phase requires an approved or baselined test plan.")));
            }
        }

        if (string.Equals(issueType, "overdue-approval", StringComparison.OrdinalIgnoreCase))
        {
            rows.AddRange(bundle.ProjectPlans
                .Where(x => x.ProjectId == project.Id && OverdueProjectPlanStates.Contains(x.Status) && x.UpdatedAt < periodStart)
                .Select(x => ("process-assets-planning", new ComplianceDrilldownRowResponse(issueType, "project_plan", x.Id.ToString(), x.Name, "governance", "/app/project-plans", x.Status, scope, x.UpdatedAt.ToString("O"), "Awaiting plan approval beyond dashboard period."))));
            rows.AddRange(bundle.TailoringRecords
                .Where(x => x.ProjectId == project.Id && OverdueTailoringStates.Contains(x.Status) && x.UpdatedAt < periodStart)
                .Select(x => ("process-assets-planning", new ComplianceDrilldownRowResponse(issueType, "tailoring_record", x.Id.ToString(), x.RequestedChange, "governance", "/app/tailoring-records", x.Status, scope, x.UpdatedAt.ToString("O"), "Awaiting tailoring approval beyond dashboard period."))));
            rows.AddRange(bundle.ChangeRequests
                .Where(x => x.ProjectId == project.Id && OverdueChangeRequestStates.Contains(x.Status) && x.UpdatedAt < periodStart)
                .Select(x => ("change-configuration", new ComplianceDrilldownRowResponse(issueType, "change_request", x.Id.ToString(), $"{x.Code} · {x.Title}", "change-control", $"/app/change-control/change-requests/{x.Id}", x.Status, scope, x.UpdatedAt.ToString("O"), "Awaiting change approval beyond dashboard period."))));
            rows.AddRange(bundle.UatSignoffs
                .Where(x => x.ProjectId == project.Id && OverdueUatStates.Contains(x.Status) && x.UpdatedAt < periodStart)
                .Select(x => ("verification-release", new ComplianceDrilldownRowResponse(issueType, "uat_signoff", x.Id.ToString(), x.ScopeSummary, "verification", "/app/uat-signoffs", x.Status, scope, x.UpdatedAt.ToString("O"), "Awaiting UAT approval beyond dashboard period."))));
        }

        if (string.Equals(issueType, "stale-baseline", StringComparison.OrdinalIgnoreCase))
        {
            rows.AddRange(bundle.ProjectPlans
                .Where(x => x.ProjectId == project.Id && string.Equals(x.Status, "baseline", StringComparison.OrdinalIgnoreCase) && x.UpdatedAt < periodStart)
                .Select(x => ("process-assets-planning", new ComplianceDrilldownRowResponse(issueType, "project_plan", x.Id.ToString(), x.Name, "governance", "/app/project-plans", x.Status, scope, x.UpdatedAt.ToString("O"), "Project plan baseline is stale."))));
            rows.AddRange(bundle.RequirementBaselines
                .Where(x => x.ProjectId == project.Id && x.ApprovedAt < periodStart)
                .Select(x => ("requirements-traceability", new ComplianceDrilldownRowResponse(issueType, "requirement_baseline", x.Id.ToString(), x.BaselineName, "requirements", "/app/requirements/baselines", x.Status, scope, x.ApprovedAt.ToString("O"), "Requirement baseline has not been refreshed within the selected period."))));
            rows.AddRange(bundle.Baselines
                .Where(x => x.ProjectId == project.Id && IsApprovedBaseline(x.Status) && x.SupersededByBaselineId == null && (x.ApprovedAt ?? x.UpdatedAt) < periodStart)
                .Select(x => (IsDocumentBaseline(x) ? "document-governance" : "change-configuration",
                    new ComplianceDrilldownRowResponse(issueType, "baseline_registry", x.Id.ToString(), x.BaselineName, "change-control", "/app/change-control/baseline-registry", x.Status, scope, (x.ApprovedAt ?? x.UpdatedAt).ToString("O"), "Baseline has exceeded the selected freshness window."))));
        }

        if (string.Equals(issueType, "open-capa", StringComparison.OrdinalIgnoreCase))
        {
            rows.AddRange(bundle.CapaRecords
                .Where(x => OpenCapaStates.Contains(x.Status) && capaProjectIds.TryGetValue(x.Id, out var projectId) && projectId == project.Id)
                .Select(x => ("audit-capa", new ComplianceDrilldownRowResponse(issueType, "capa_record", x.Id.ToString(), x.Title, "operations", "/app/operations/capa", x.Status, scope, null, $"{x.SourceType}:{x.SourceRef}"))));
        }

        if (string.Equals(issueType, "open-audit-finding", StringComparison.OrdinalIgnoreCase))
        {
            rows.AddRange(bundle.AuditFindings
                .Where(x => auditPlanIds.Contains(x.AuditPlanId) && OpenAuditFindingStates.Contains(x.Status))
                .Select(x => ("audit-capa", new ComplianceDrilldownRowResponse(issueType, "audit_finding", x.Id.ToString(), $"{x.Code} · {x.Title}", "audits", "/app/audit-plans", x.Status, scope, x.DueDate?.ToString("yyyy-MM-dd"), x.Severity))));
        }

        if (string.Equals(issueType, "open-security-item", StringComparison.OrdinalIgnoreCase))
        {
            rows.AddRange(bundle.SecurityReviews
                .Where(x => IsProjectScoped(x.ScopeType, x.ScopeRef, project.Id) && OpenSecurityReviewStates.Contains(x.Status))
                .Select(x => ("security-resilience", new ComplianceDrilldownRowResponse(issueType, "security_review", x.Id.ToString(), $"Security review · {x.ScopeRef}", "operations", "/app/operations/security-reviews", x.Status, scope, null, x.ScopeType))));
            rows.AddRange(bundle.SecurityIncidents
                .Where(x => x.ProjectId == project.Id && OpenSecurityIncidentStates.Contains(x.Status))
                .Select(x => ("security-resilience", new ComplianceDrilldownRowResponse(issueType, "security_incident", x.Id.ToString(), $"{x.Code} · {x.Title}", "operations", "/app/operations/security-incidents", x.Status, scope, null, x.Severity))));
            rows.AddRange(bundle.Vulnerabilities
                .Where(x => TryParseProjectReference(x.AssetRef) == project.Id && OpenVulnerabilityStates.Contains(x.Status))
                .Select(x => ("security-resilience", new ComplianceDrilldownRowResponse(issueType, "vulnerability", x.Id.ToString(), x.Title, "operations", "/app/operations/vulnerabilities", x.Status, scope, x.PatchDueAt?.ToString("O"), x.Severity))));
        }

        return rows
            .Where(x => processArea is null || string.Equals(x.ProcessArea, processArea, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Row);
    }

    private async Task PersistSnapshotsAsync(IReadOnlyList<ProjectComputation> computations, DateTimeOffset periodStart, DateTimeOffset generatedAt, string? actor, CancellationToken cancellationToken)
    {
        if (computations.Count == 0)
        {
            return;
        }

        var periodEnd = generatedAt;
        var projectIds = computations.Select(x => x.Project.ProjectId).ToArray();
        var existingSnapshots = await dbContext.ComplianceSnapshots
            .Where(x => projectIds.Contains(x.ProjectId) && x.Status == "published")
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var computation in computations)
        {
            var snapshots = new List<ComplianceSnapshotEntity>
            {
                CreateSnapshot(computation.Project.ProjectId, "overall", periodStart, periodEnd, generatedAt, actor, computation.Project.ReadinessScore, computation.Project.ReadinessState, computation.Project.MissingArtifactCount, computation.Project.OverdueApprovalCount, computation.Project.StaleBaselineCount, computation.Project.OpenCapaCount, computation.Project.OpenAuditFindingCount, computation.Project.OpenSecurityItemCount)
            };

            snapshots.AddRange(computation.ProcessAreas.Select(area =>
                CreateSnapshot(area.ProjectId, area.ProcessArea, periodStart, periodEnd, generatedAt, actor, area.ReadinessScore, area.ReadinessState, area.MissingArtifactCount, area.OverdueApprovalCount, area.StaleBaselineCount, area.OpenCapaCount, area.OpenAuditFindingCount, area.OpenSecurityItemCount)));

            foreach (var snapshot in snapshots)
            {
                foreach (var existing in existingSnapshots.Where(x => x.ProjectId == snapshot.ProjectId && string.Equals(x.ProcessArea, snapshot.ProcessArea, StringComparison.OrdinalIgnoreCase)))
                {
                    existing.Status = "superseded";
                    existing.SupersededBySnapshotId = snapshot.Id;
                    existing.UpdatedAt = now;
                }
            }

            dbContext.ComplianceSnapshots.AddRange(snapshots);
        }

        auditLogWriter.Append(new AuditLogEntry(
            Module: "governance",
            Action: "generate_snapshot",
            EntityType: "compliance_snapshot",
            StatusCode: StatusCodes.Status200OK,
            ActorEmail: actor,
            Metadata: new
            {
                periodStart,
                generatedAt,
                projectCount = computations.Count
            },
            Audience: LogAudience.AuditOnly));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ComplianceSnapshotEntity CreateSnapshot(Guid projectId, string processArea, DateTimeOffset periodStart, DateTimeOffset periodEnd, DateTimeOffset generatedAt, string? actor, int readinessScore, string readinessState, int missingArtifactCount, int overdueApprovalCount, int staleBaselineCount, int openCapaCount, int openAuditFindingCount, int openSecurityItemCount)
    {
        return new ComplianceSnapshotEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            ProcessArea = processArea,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            ReadinessScore = readinessScore,
            Status = "published",
            MissingArtifactCount = missingArtifactCount,
            OverdueApprovalCount = overdueApprovalCount,
            StaleBaselineCount = staleBaselineCount,
            OpenCapaCount = openCapaCount,
            OpenAuditFindingCount = openAuditFindingCount,
            OpenSecurityItemCount = openSecurityItemCount,
            DetailsJson = JsonSerializer.Serialize(new
            {
                readinessState,
                missingArtifactCount,
                overdueApprovalCount,
                staleBaselineCount,
                openCapaCount,
                openAuditFindingCount,
                openSecurityItemCount
            }, SerializerOptions),
            GeneratedAt = generatedAt,
            GeneratedBy = actor ?? "system",
            CreatedAt = generatedAt,
            UpdatedAt = generatedAt
        };
    }

    private static ProcessAreaComputation BuildProcessArea(DashboardProject project, string processArea, int missingArtifactCount, int overdueApprovalCount, int staleBaselineCount, int openCapaCount, int openAuditFindingCount, int openSecurityItemCount)
    {
        var score = Math.Max(0, 100 - (missingArtifactCount * 25) - (overdueApprovalCount * 18) - (staleBaselineCount * 12) - (openCapaCount * 10) - (openAuditFindingCount * 15) - (openSecurityItemCount * 18));
        var state = openSecurityItemCount > 0 || openAuditFindingCount > 0 || missingArtifactCount > 0 || score < 60
            ? "critical"
            : overdueApprovalCount > 0 || staleBaselineCount > 0 || openCapaCount > 0 || score < 85
                ? "at_risk"
                : "good";

        return new ProcessAreaComputation(project.Id, processArea, score, state, missingArtifactCount, overdueApprovalCount, staleBaselineCount, openCapaCount, openAuditFindingCount, openSecurityItemCount);
    }

    private static Dictionary<Guid, Guid> BuildCapaProjectMap(DashboardBundle bundle)
    {
        var result = new Dictionary<Guid, Guid>();
        var projectPlans = bundle.ProjectPlans.ToDictionary(x => x.Id, x => x.ProjectId);
        var tailoring = bundle.TailoringRecords.ToDictionary(x => x.Id, x => x.ProjectId);
        var requirements = bundle.Requirements.ToDictionary(x => x.Id, x => x.ProjectId);
        var changeRequests = bundle.ChangeRequests.ToDictionary(x => x.Id, x => x.ProjectId);
        var securityIncidents = bundle.SecurityIncidents.Where(x => x.ProjectId != null).ToDictionary(x => x.Id, x => x.ProjectId!.Value);
        var auditPlanProjects = bundle.AuditPlans.ToDictionary(x => x.Id, x => x.ProjectId);
        var auditFindings = bundle.AuditFindings.ToDictionary(x => x.Id, x => auditPlanProjects.TryGetValue(x.AuditPlanId, out var projectId) ? projectId : Guid.Empty);

        foreach (var capa in bundle.CapaRecords)
        {
            Guid? projectId = capa.SourceType.ToLowerInvariant() switch
            {
                "project" when Guid.TryParse(capa.SourceRef, out var parsedProject) => parsedProject,
                "project_plan" when Guid.TryParse(capa.SourceRef, out var projectPlanId) && projectPlans.TryGetValue(projectPlanId, out var projectIdFromPlan) => projectIdFromPlan,
                "tailoring_record" when Guid.TryParse(capa.SourceRef, out var tailoringId) && tailoring.TryGetValue(tailoringId, out var projectIdFromTailoring) => projectIdFromTailoring,
                "requirement" when Guid.TryParse(capa.SourceRef, out var requirementId) && requirements.TryGetValue(requirementId, out var projectIdFromRequirement) => projectIdFromRequirement,
                "change_request" when Guid.TryParse(capa.SourceRef, out var changeRequestId) && changeRequests.TryGetValue(changeRequestId, out var projectIdFromChange) => projectIdFromChange,
                "security_incident" when Guid.TryParse(capa.SourceRef, out var incidentId) && securityIncidents.TryGetValue(incidentId, out var projectIdFromIncident) => projectIdFromIncident,
                "audit_finding" when Guid.TryParse(capa.SourceRef, out var findingId) && auditFindings.TryGetValue(findingId, out var projectIdFromFinding) && projectIdFromFinding != Guid.Empty => projectIdFromFinding,
                _ => null
            };

            if (projectId.HasValue)
            {
                result[capa.Id] = projectId.Value;
            }
        }

        return result;
    }

    private static bool RequiresTestPlan(string? phase) =>
        !string.IsNullOrWhiteSpace(phase) &&
        (phase.Contains("verification", StringComparison.OrdinalIgnoreCase)
         || phase.Contains("test", StringComparison.OrdinalIgnoreCase)
         || phase.Contains("uat", StringComparison.OrdinalIgnoreCase)
         || phase.Contains("release", StringComparison.OrdinalIgnoreCase)
         || phase.Contains("deploy", StringComparison.OrdinalIgnoreCase));

    private static bool IsProjectScoped(string scopeType, string scopeRef, Guid projectId) =>
        string.Equals(scopeType, "project", StringComparison.OrdinalIgnoreCase)
        && Guid.TryParse(scopeRef, out var parsedProjectId)
        && parsedProjectId == projectId;

    private static Guid? TryParseProjectReference(string assetRef) =>
        Guid.TryParse(assetRef, out var projectId) ? projectId : null;

    private static bool IsDocumentBaseline(BaselineLite baseline) =>
        string.Equals(baseline.SourceEntityType, "document", StringComparison.OrdinalIgnoreCase)
        || string.Equals(baseline.BaselineType, "document", StringComparison.OrdinalIgnoreCase);

    private static bool IsChangeBaseline(BaselineLite baseline) =>
        string.Equals(baseline.SourceEntityType, "change_request", StringComparison.OrdinalIgnoreCase)
        || string.Equals(baseline.SourceEntityType, "configuration_item", StringComparison.OrdinalIgnoreCase)
        || string.Equals(baseline.BaselineType, "configuration", StringComparison.OrdinalIgnoreCase);

    private static bool IsApprovedBaseline(string status) =>
        string.Equals(status, "approved", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeIssueType(string? issueType) => issueType?.Trim().ToLowerInvariant() switch
    {
        "missing-artifact" => "missing-artifact",
        "overdue-approval" => "overdue-approval",
        "stale-baseline" => "stale-baseline",
        "open-capa" => "open-capa",
        "open-audit-finding" => "open-audit-finding",
        "open-security-item" => "open-security-item",
        _ => "missing-artifact"
    };

    private static string? NormalizeProcessArea(string? processArea) => processArea?.Trim().ToLowerInvariant() switch
    {
        "process-assets-planning" => "process-assets-planning",
        "requirements-traceability" => "requirements-traceability",
        "document-governance" => "document-governance",
        "change-configuration" => "change-configuration",
        "verification-release" => "verification-release",
        "audit-capa" => "audit-capa",
        "security-resilience" => "security-resilience",
        _ => null
    };

    private static async Task<PagedResult<T>> PageAsync<T>(IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((normalizedPage - 1) * normalizedPageSize).Take(normalizedPageSize).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, total, normalizedPage, normalizedPageSize);
    }

    private sealed record ResolvedDashboardFilters(Guid? ProjectId, string? ProcessArea, int PeriodDays, bool ShowOnlyAtRisk);

    private sealed record DashboardBundle(
        IReadOnlyList<DashboardProject> Projects,
        IReadOnlyList<ProjectPlanLite> ProjectPlans,
        IReadOnlyList<TailoringLite> TailoringRecords,
        IReadOnlyList<RequirementLite> Requirements,
        IReadOnlyList<RequirementBaselineLite> RequirementBaselines,
        IReadOnlyList<DocumentLite> Documents,
        IReadOnlyList<TestPlanLite> TestPlans,
        IReadOnlyList<ChangeRequestLite> ChangeRequests,
        IReadOnlyList<BaselineLite> Baselines,
        IReadOnlyList<UatLite> UatSignoffs,
        IReadOnlyList<AuditPlanLite> AuditPlans,
        IReadOnlyList<AuditFindingLite> AuditFindings,
        IReadOnlyList<SecurityReviewLite> SecurityReviews,
        IReadOnlyList<SecurityIncidentLite> SecurityIncidents,
        IReadOnlyList<VulnerabilityLite> Vulnerabilities,
        IReadOnlyList<CapaLite> CapaRecords)
    {
        public static DashboardBundle Empty { get; } = new(
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            []);
    }

    private sealed record DashboardProject(Guid Id, string Code, string Name, string Status, string? Phase, string? Methodology);
    private sealed record ProjectPlanLite(Guid Id, Guid ProjectId, string Name, string Status, DateTimeOffset UpdatedAt, DateTimeOffset? ApprovedAt);
    private sealed record TailoringLite(Guid Id, Guid ProjectId, string RequestedChange, string Status, DateTimeOffset UpdatedAt, DateTimeOffset? ApprovedAt);
    private sealed record RequirementLite(Guid Id, Guid ProjectId, string Code, string Title, string Status);
    private sealed record RequirementBaselineLite(Guid Id, Guid ProjectId, string BaselineName, string Status, DateTimeOffset ApprovedAt);
    private sealed record DocumentLite(Guid Id, Guid ProjectId, string Title, string Status, DateTimeOffset UpdatedAt);
    private sealed record TestPlanLite(Guid Id, Guid ProjectId, string Title, string Status, DateTimeOffset UpdatedAt, DateTimeOffset? ApprovedAt, DateTimeOffset? BaselinedAt);
    private sealed record ChangeRequestLite(Guid Id, Guid ProjectId, string Code, string Title, string Status, DateTimeOffset UpdatedAt, DateTimeOffset? ApprovedAt);
    private sealed record BaselineLite(Guid Id, Guid ProjectId, string BaselineName, string BaselineType, string SourceEntityType, string SourceEntityId, string Status, DateTimeOffset? ApprovedAt, DateTimeOffset UpdatedAt, Guid? SupersededByBaselineId);
    private sealed record UatLite(Guid Id, Guid ProjectId, string ScopeSummary, string Status, DateTimeOffset UpdatedAt, DateTimeOffset? ApprovedAt);
    private sealed record AuditPlanLite(Guid Id, Guid ProjectId, string Title);
    private sealed record AuditFindingLite(Guid Id, Guid AuditPlanId, string Code, string Title, string Severity, string Status, DateOnly? DueDate);
    private sealed record SecurityReviewLite(Guid Id, string ScopeType, string ScopeRef, string Status);
    private sealed record SecurityIncidentLite(Guid Id, Guid? ProjectId, string Code, string Title, string Severity, string Status);
    private sealed record VulnerabilityLite(Guid Id, string AssetRef, string Title, string Severity, string Status, DateTimeOffset? PatchDueAt);
    private sealed record CapaLite(Guid Id, string SourceType, string SourceRef, string Title, string Status);
    private sealed record ProcessAreaComputation(Guid ProjectId, string ProcessArea, int ReadinessScore, string ReadinessState, int MissingArtifactCount, int OverdueApprovalCount, int StaleBaselineCount, int OpenCapaCount, int OpenAuditFindingCount, int OpenSecurityItemCount);
    private sealed record ProjectComputation(ComplianceProjectReadinessResponse Project, IReadOnlyList<ProcessAreaComputation> ProcessAreas);
}

file static class GovernanceComplianceExtensions
{
    public static int ToCount(this bool value) => value ? 1 : 0;
}
