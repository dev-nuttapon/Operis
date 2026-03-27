using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Governance.Contracts;
using Operis_API.Modules.Governance.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Governance.Application;

public sealed class GovernanceQueries(OperisDbContext dbContext, IAuditLogWriter auditLogWriter) : IGovernanceQueries
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<PagedResult<ProcessAssetListItemResponse>> ListProcessAssetsAsync(GovernanceListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source = dbContext.Set<ProcessAssetEntity>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Status == query.Status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerUserId))
        {
            source = source.Where(x => x.OwnerUserId == query.OwnerUserId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.Code, search) || EF.Functions.ILike(x.Name, search));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await (
                from asset in source.OrderBy(x => x.Code).Skip(skip).Take(pageSize)
                join version in dbContext.Set<ProcessAssetVersionEntity>().AsNoTracking() on asset.CurrentVersionId equals version.Id into versionJoin
                from version in versionJoin.DefaultIfEmpty()
                select new ProcessAssetListItemResponse(
                    asset.Id,
                    asset.Code,
                    asset.Name,
                    asset.Category,
                    asset.Status,
                    asset.OwnerUserId,
                    version == null
                        ? null
                        : new ProcessAssetVersionSummaryResponse(
                            version.Id,
                            version.VersionNumber,
                            version.Title,
                            version.Status,
                            version.ChangeSummary,
                            version.ApprovedBy,
                            version.ApprovedAt,
                            version.UpdatedAt),
                    asset.EffectiveFrom,
                    asset.EffectiveTo,
                    asset.UpdatedAt))
            .ToListAsync(cancellationToken);

        await WriteReadAuditAsync("process_asset", new { total, page, pageSize, query.Search, query.Status, query.OwnerUserId }, cancellationToken);
        return new PagedResult<ProcessAssetListItemResponse>(items, total, page, pageSize);
    }

    public async Task<ProcessAssetResponse?> GetProcessAssetAsync(Guid processAssetId, CancellationToken cancellationToken)
    {
        var asset = await dbContext.Set<ProcessAssetEntity>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == processAssetId, cancellationToken);
        if (asset is null)
        {
            return null;
        }

        var versions = await dbContext.Set<ProcessAssetVersionEntity>()
            .AsNoTracking()
            .Where(x => x.ProcessAssetId == processAssetId)
            .OrderByDescending(x => x.VersionNumber)
            .Select(x => new ProcessAssetVersionDetailResponse(
                x.Id,
                x.VersionNumber,
                x.Title,
                x.Summary,
                x.ContentRef,
                x.Status,
                x.ChangeSummary,
                x.ApprovedBy,
                x.ApprovedAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        await WriteReadAuditAsync("process_asset", new { processAssetId }, cancellationToken);
        return new ProcessAssetResponse(
            asset.Id,
            asset.Code,
            asset.Name,
            asset.Category,
            asset.Status,
            asset.OwnerUserId,
            asset.EffectiveFrom,
            asset.EffectiveTo,
            asset.CurrentVersionId,
            versions,
            asset.CreatedAt,
            asset.UpdatedAt);
    }

    public async Task<PagedResult<QaChecklistListItemResponse>> ListQaChecklistsAsync(GovernanceListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source = dbContext.Set<QaChecklistEntity>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Status == query.Status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerUserId))
        {
            source = source.Where(x => x.OwnerUserId == query.OwnerUserId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.Code, search) || EF.Functions.ILike(x.Name, search));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderBy(x => x.Code)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new QaChecklistListItemResponse(x.Id, x.Code, x.Name, x.Scope, x.Status, x.OwnerUserId, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        await WriteReadAuditAsync("qa_checklist", new { total, page, pageSize, query.Search, query.Status }, cancellationToken);
        return new PagedResult<QaChecklistListItemResponse>(items, total, page, pageSize);
    }

    public async Task<QaChecklistResponse?> GetQaChecklistAsync(Guid qaChecklistId, CancellationToken cancellationToken)
    {
        var checklist = await dbContext.Set<QaChecklistEntity>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == qaChecklistId, cancellationToken);
        if (checklist is null)
        {
            return null;
        }

        await WriteReadAuditAsync("qa_checklist", new { qaChecklistId }, cancellationToken);
        return ToQaChecklistResponse(checklist);
    }

    public async Task<PagedResult<ProjectPlanListItemResponse>> ListProjectPlansAsync(GovernanceListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source =
            from plan in dbContext.Set<ProjectPlanEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on plan.ProjectId equals project.Id
            select new { Plan = plan, ProjectName = project.Name };

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Plan.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Plan.Status == query.Status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerUserId))
        {
            source = source.Where(x => x.Plan.OwnerUserId == query.OwnerUserId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.Plan.Name, search) || EF.Functions.ILike(x.ProjectName, search));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(x => x.Plan.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new ProjectPlanListItemResponse(
                x.Plan.Id,
                x.Plan.ProjectId,
                x.ProjectName,
                x.Plan.Name,
                x.Plan.LifecycleModel,
                x.Plan.Status,
                x.Plan.OwnerUserId,
                x.Plan.StartDate,
                x.Plan.TargetEndDate,
                x.Plan.UpdatedAt))
            .ToListAsync(cancellationToken);

        await WriteReadAuditAsync("project_plan", new { total, page, pageSize, query.ProjectId, query.Status, query.OwnerUserId }, cancellationToken);
        return new PagedResult<ProjectPlanListItemResponse>(items, total, page, pageSize);
    }

    public async Task<ProjectPlanResponse?> GetProjectPlanAsync(Guid projectPlanId, CancellationToken cancellationToken)
    {
        var plan = await dbContext.Set<ProjectPlanEntity>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == projectPlanId, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        await WriteReadAuditAsync("project_plan", new { projectPlanId }, cancellationToken);
        return ToProjectPlanResponse(plan);
    }

    public async Task<PagedResult<StakeholderResponse>> ListStakeholdersAsync(GovernanceListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source =
            from stakeholder in dbContext.Set<StakeholderEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on stakeholder.ProjectId equals project.Id
            select new { Stakeholder = stakeholder, ProjectName = project.Name };

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Stakeholder.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Stakeholder.Status == query.Status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Stakeholder.Name, search)
                || EF.Functions.ILike(x.Stakeholder.RoleName, search)
                || EF.Functions.ILike(x.ProjectName, search));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderBy(x => x.ProjectName)
            .ThenBy(x => x.Stakeholder.Name)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new StakeholderResponse(
                x.Stakeholder.Id,
                x.Stakeholder.ProjectId,
                x.ProjectName,
                x.Stakeholder.Name,
                x.Stakeholder.RoleName,
                x.Stakeholder.InfluenceLevel,
                x.Stakeholder.ContactChannel,
                x.Stakeholder.Status,
                x.Stakeholder.CreatedAt,
                x.Stakeholder.UpdatedAt))
            .ToListAsync(cancellationToken);

        await WriteReadAuditAsync("stakeholder", new { total, page, pageSize, query.ProjectId, query.Status }, cancellationToken);
        return new PagedResult<StakeholderResponse>(items, total, page, pageSize);
    }

    public async Task<StakeholderResponse?> GetStakeholderAsync(Guid stakeholderId, CancellationToken cancellationToken)
    {
        var item = await (
            from stakeholder in dbContext.Set<StakeholderEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on stakeholder.ProjectId equals project.Id
            where stakeholder.Id == stakeholderId
            select new StakeholderResponse(
                stakeholder.Id,
                stakeholder.ProjectId,
                project.Name,
                stakeholder.Name,
                stakeholder.RoleName,
                stakeholder.InfluenceLevel,
                stakeholder.ContactChannel,
                stakeholder.Status,
                stakeholder.CreatedAt,
                stakeholder.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        await WriteReadAuditAsync("stakeholder", new { stakeholderId }, cancellationToken);
        return item;
    }

    public async Task<PagedResult<TailoringRecordListItemResponse>> ListTailoringRecordsAsync(GovernanceListQuery query, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source =
            from tailoring in dbContext.Set<TailoringRecordEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on tailoring.ProjectId equals project.Id
            join criteria in dbContext.TailoringCriteria.AsNoTracking() on tailoring.TailoringCriteriaId equals criteria.Id into criteriaJoin
            from criteria in criteriaJoin.DefaultIfEmpty()
            select new { Tailoring = tailoring, ProjectName = project.Name, CriteriaTitle = criteria != null ? criteria.Title : null };

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Tailoring.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Tailoring.Status == query.Status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerUserId))
        {
            source = source.Where(x => x.Tailoring.RequesterUserId == query.OwnerUserId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Tailoring.RequestedChange, search)
                || EF.Functions.ILike(x.Tailoring.Reason, search)
                || EF.Functions.ILike(x.ProjectName, search));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(x => x.Tailoring.UpdatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new TailoringRecordListItemResponse(
                x.Tailoring.Id,
                x.Tailoring.ProjectId,
                x.ProjectName,
                x.Tailoring.TailoringCriteriaId,
                x.CriteriaTitle,
                x.Tailoring.TailoringReviewCycleId,
                x.Tailoring.RequestedChange,
                x.Tailoring.StandardReference,
                x.Tailoring.DeviationReason,
                x.Tailoring.ReviewDueAt,
                x.Tailoring.Status,
                x.Tailoring.RequesterUserId,
                x.Tailoring.ApproverUserId,
                (x.Tailoring.Status == "draft" || x.Tailoring.Status == "submitted") && x.Tailoring.ReviewDueAt != null && x.Tailoring.ReviewDueAt < now,
                x.Tailoring.UpdatedAt))
            .ToListAsync(cancellationToken);

        await WriteReadAuditAsync("tailoring_record", new { total, page, pageSize, query.ProjectId, query.Status, query.OwnerUserId }, cancellationToken);
        return new PagedResult<TailoringRecordListItemResponse>(items, total, page, pageSize);
    }

    public async Task<TailoringRecordResponse?> GetTailoringRecordAsync(Guid tailoringRecordId, CancellationToken cancellationToken)
    {
        var item = await (
            from tailoring in dbContext.Set<TailoringRecordEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on tailoring.ProjectId equals project.Id
            join criteria in dbContext.TailoringCriteria.AsNoTracking() on tailoring.TailoringCriteriaId equals criteria.Id into criteriaJoin
            from criteria in criteriaJoin.DefaultIfEmpty()
            join review in dbContext.TailoringReviewCycles.AsNoTracking() on tailoring.TailoringReviewCycleId equals review.Id into reviewJoin
            from review in reviewJoin.DefaultIfEmpty()
            where tailoring.Id == tailoringRecordId
            select new TailoringRecordResponse(
                tailoring.Id,
                tailoring.ProjectId,
                project.Name,
                tailoring.TailoringCriteriaId,
                criteria != null ? criteria.Title : null,
                tailoring.TailoringReviewCycleId,
                review != null ? review.Title : null,
                tailoring.RequesterUserId,
                tailoring.RequestedChange,
                tailoring.StandardReference,
                tailoring.DeviationReason,
                tailoring.Reason,
                tailoring.ImpactSummary,
                tailoring.ReviewDueAt,
                tailoring.Status,
                tailoring.ApproverUserId,
                tailoring.ApprovedAt,
                tailoring.ImpactedProcessAssetId,
                tailoring.ApprovalRationale,
                tailoring.CreatedAt,
                tailoring.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        await WriteReadAuditAsync("tailoring_record", new { tailoringRecordId }, cancellationToken);
        return item;
    }

    public async Task<PagedResult<TailoringCriteriaResponse>> ListTailoringCriteriaAsync(GovernanceListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source = dbContext.TailoringCriteria.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Status == query.Status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.CriterionCode, search) ||
                EF.Functions.ILike(x.StandardReference, search) ||
                EF.Functions.ILike(x.Title, search));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderBy(x => x.StandardReference)
            .ThenBy(x => x.CriterionCode)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new TailoringCriteriaResponse(
                x.Id,
                x.CriterionCode,
                x.StandardReference,
                x.Title,
                x.Description,
                x.Status,
                dbContext.TailoringRecords.Count(record => record.TailoringCriteriaId == x.Id),
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        await WriteReadAuditAsync("tailoring_criteria", new { total, page, pageSize, query.Status, query.Search }, cancellationToken);
        return new PagedResult<TailoringCriteriaResponse>(items, total, page, pageSize);
    }

    public async Task<PagedResult<TailoringReviewCycleResponse>> ListTailoringReviewCyclesAsync(GovernanceListQuery query, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source =
            from review in dbContext.TailoringReviewCycles.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on review.ProjectId equals project.Id
            select new { Review = review, ProjectName = project.Name };

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Review.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Review.Status == query.Status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Review.ReviewCode, search) ||
                EF.Functions.ILike(x.Review.Title, search) ||
                EF.Functions.ILike(x.ProjectName, search));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(x => x.Review.ReviewDueAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new TailoringReviewCycleResponse(
                x.Review.Id,
                x.Review.ProjectId,
                x.ProjectName,
                x.Review.ReviewCode,
                x.Review.Title,
                x.Review.OwnerUserId,
                x.Review.ReviewDueAt,
                x.Review.Status,
                x.Review.ApproverUserId,
                x.Review.ApprovedAt,
                x.Review.DecisionReason,
                dbContext.TailoringRecords.Count(record => record.TailoringReviewCycleId == x.Review.Id),
                dbContext.TailoringRecords.Count(record => record.TailoringReviewCycleId == x.Review.Id && (record.Status == "draft" || record.Status == "submitted")),
                (x.Review.Status == "draft" || x.Review.Status == "submitted") && x.Review.ReviewDueAt < now,
                x.Review.CreatedAt,
                x.Review.UpdatedAt))
            .ToListAsync(cancellationToken);

        await WriteReadAuditAsync("tailoring_review_cycle", new { total, page, pageSize, query.ProjectId, query.Status }, cancellationToken);
        return new PagedResult<TailoringReviewCycleResponse>(items, total, page, pageSize);
    }

    public async Task<TailoringReviewCycleResponse?> GetTailoringReviewCycleAsync(Guid tailoringReviewCycleId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var item = await (
            from review in dbContext.TailoringReviewCycles.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on review.ProjectId equals project.Id
            where review.Id == tailoringReviewCycleId
            select new TailoringReviewCycleResponse(
                review.Id,
                review.ProjectId,
                project.Name,
                review.ReviewCode,
                review.Title,
                review.OwnerUserId,
                review.ReviewDueAt,
                review.Status,
                review.ApproverUserId,
                review.ApprovedAt,
                review.DecisionReason,
                dbContext.TailoringRecords.Count(record => record.TailoringReviewCycleId == review.Id),
                dbContext.TailoringRecords.Count(record => record.TailoringReviewCycleId == review.Id && (record.Status == "draft" || record.Status == "submitted")),
                (review.Status == "draft" || review.Status == "submitted") && review.ReviewDueAt < now,
                review.CreatedAt,
                review.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        await WriteReadAuditAsync("tailoring_review_cycle", new { tailoringReviewCycleId }, cancellationToken);
        return item;
    }

    internal static QaChecklistResponse ToQaChecklistResponse(QaChecklistEntity entity) =>
        new(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Scope,
            entity.Status,
            entity.OwnerUserId,
            DeserializeChecklistItems(entity.ItemsJson),
            entity.CreatedAt,
            entity.UpdatedAt);

    internal static ProjectPlanResponse ToProjectPlanResponse(ProjectPlanEntity entity) =>
        new(
            entity.Id,
            entity.ProjectId,
            entity.Name,
            entity.ScopeSummary,
            entity.LifecycleModel,
            entity.StartDate,
            entity.TargetEndDate,
            entity.OwnerUserId,
            entity.Status,
            DeserializeStringList(entity.MilestonesJson),
            DeserializeStringList(entity.RolesJson),
            entity.RiskApproach,
            entity.QualityApproach,
            entity.ApprovalReason,
            entity.ApprovedBy,
            entity.ApprovedAt,
            entity.CreatedAt,
            entity.UpdatedAt);

    internal static IReadOnlyList<QaChecklistItemResponse> DeserializeChecklistItems(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<QaChecklistItemResponse>>(json, SerializerOptions) ?? [];
    }

    internal static IReadOnlyList<string> DeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(json, SerializerOptions) ?? [];
    }

    private async Task WriteReadAuditAsync(string entityType, object metadata, CancellationToken cancellationToken)
    {
        auditLogWriter.Append(new AuditLogEntry(
            Module: "governance",
            Action: "read",
            EntityType: entityType,
            StatusCode: StatusCodes.Status200OK,
            Metadata: metadata));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);
        return (normalizedPage, normalizedPageSize, (normalizedPage - 1) * normalizedPageSize);
    }
}
