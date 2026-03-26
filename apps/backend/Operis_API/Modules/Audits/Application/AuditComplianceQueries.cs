using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Contracts;
using Operis_API.Modules.Audits.Infrastructure;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Audits.Application;

public sealed class AuditComplianceQueries(OperisDbContext dbContext) : IAuditComplianceQueries
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<PagedResult<AuditEventItem>> ListAuditEventsAsync(AuditEventListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize, 50, 200);
        var baseQuery = dbContext.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            baseQuery = baseQuery.Where(x => x.EntityType == query.EntityType.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            baseQuery = baseQuery.Where(x => x.Action == query.Action.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.ActorUserId))
        {
            var actorUserId = query.ActorUserId.Trim();
            baseQuery = baseQuery.Where(x => x.ActorUserId == actorUserId || x.ActorEmail == actorUserId);
        }

        if (!string.IsNullOrWhiteSpace(query.Outcome))
        {
            baseQuery = baseQuery.Where(x => x.Status == query.Outcome.Trim());
        }

        if (query.ProjectId.HasValue)
        {
            var projectKey = query.ProjectId.Value.ToString();
            baseQuery = baseQuery.Where(x =>
                (x.MetadataJson != null && EF.Functions.ILike(x.MetadataJson, $"%{projectKey}%"))
                || (x.EntityId != null && x.EntityId == projectKey));
        }

        if (query.From.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.OccurredAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.OccurredAt <= query.To.Value);
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderByDescending(x => x.OccurredAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new AuditEventItem(
                x.Id,
                x.OccurredAt,
                x.ActorUserId,
                x.ActorEmail,
                x.ActorDisplayName,
                x.EntityType,
                x.EntityId,
                x.Action,
                x.Status,
                x.Reason))
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditEventItem>(items, total, page, pageSize);
    }

    public async Task<PagedResult<AuditPlanListItem>> ListAuditPlansAsync(AuditPlanListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize, 25, 100);
        var baseQuery =
            from plan in dbContext.Set<AuditPlanEntity>().AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on plan.ProjectId equals project.Id
            select new { Plan = plan, ProjectName = project.Name };

        if (query.ProjectId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Plan.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(x => x.Plan.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerUserId))
        {
            var ownerUserId = query.OwnerUserId.Trim();
            baseQuery = baseQuery.Where(x => x.Plan.OwnerUserId == ownerUserId);
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var rows = await baseQuery
            .OrderByDescending(x => x.Plan.PlannedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var planIds = rows.Select(x => x.Plan.Id).ToArray();
        var openFindingLookup = await dbContext.Set<AuditFindingEntity>().AsNoTracking()
            .Where(x => planIds.Contains(x.AuditPlanId) && x.Status != "closed")
            .GroupBy(x => x.AuditPlanId)
            .Select(group => new { AuditPlanId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.AuditPlanId, x => x.Count, cancellationToken);

        var items = rows.Select(x => new AuditPlanListItem(
            x.Plan.Id,
            x.Plan.ProjectId,
            x.ProjectName,
            x.Plan.Title,
            x.Plan.Scope,
            x.Plan.PlannedAt,
            x.Plan.Status,
            x.Plan.OwnerUserId,
            openFindingLookup.GetValueOrDefault(x.Plan.Id),
            x.Plan.UpdatedAt)).ToList();

        return new PagedResult<AuditPlanListItem>(items, total, page, pageSize);
    }

    public async Task<AuditPlanDetailResponse?> GetAuditPlanAsync(Guid auditPlanId, CancellationToken cancellationToken)
    {
        var item = await (
            from plan in dbContext.Set<AuditPlanEntity>().AsNoTracking()
            where plan.Id == auditPlanId
            join project in dbContext.Projects.AsNoTracking() on plan.ProjectId equals project.Id
            select new { Plan = plan, ProjectName = project.Name }).SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var findings = await dbContext.Set<AuditFindingEntity>().AsNoTracking()
            .Where(x => x.AuditPlanId == auditPlanId)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new AuditFindingItem(x.Id, x.AuditPlanId, item.Plan.Title, x.Code, x.Title, x.Severity, x.Status, x.OwnerUserId, x.DueDate, x.ResolutionSummary, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var history = await LoadHistoryAsync("audit_plan", auditPlanId.ToString(), cancellationToken);
        return new AuditPlanDetailResponse(
            item.Plan.Id,
            item.Plan.ProjectId,
            item.ProjectName,
            item.Plan.Title,
            item.Plan.Scope,
            item.Plan.Criteria,
            item.Plan.PlannedAt,
            item.Plan.Status,
            item.Plan.OwnerUserId,
            findings,
            history,
            item.Plan.CreatedAt,
            item.Plan.UpdatedAt);
    }

    public async Task<PagedResult<EvidenceExportItem>> ListEvidenceExportsAsync(EvidenceExportListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize, 25, 100);
        var baseQuery = dbContext.Set<EvidenceExportEntity>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.ScopeType))
        {
            baseQuery = baseQuery.Where(x => x.ScopeType == query.ScopeType.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            baseQuery = baseQuery.Where(x => x.Status == query.Status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.RequestedBy))
        {
            var requestedBy = query.RequestedBy.Trim();
            baseQuery = baseQuery.Where(x => x.RequestedBy == requestedBy);
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderByDescending(x => x.RequestedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new EvidenceExportItem(x.Id, x.RequestedBy, x.ScopeType, x.ScopeRef, x.RequestedAt, x.Status, x.OutputRef, x.FailureReason))
            .ToListAsync(cancellationToken);

        return new PagedResult<EvidenceExportItem>(items, total, page, pageSize);
    }

    public async Task<EvidenceExportDetailResponse?> GetEvidenceExportAsync(Guid exportId, CancellationToken cancellationToken)
    {
        var item = await dbContext.Set<EvidenceExportEntity>().AsNoTracking()
            .Where(x => x.Id == exportId)
            .SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var history = await LoadHistoryAsync("evidence_export", exportId.ToString(), cancellationToken);
        return new EvidenceExportDetailResponse(
            item.Id,
            item.RequestedBy,
            item.ScopeType,
            item.ScopeRef,
            item.RequestedAt,
            item.Status,
            item.OutputRef,
            item.From,
            item.To,
            DeserializeStringList(item.IncludedArtifactTypesJson),
            item.FailureReason,
            history);
    }

    private async Task<IReadOnlyList<BusinessAuditEventItem>> LoadHistoryAsync(string entityType, string entityId, CancellationToken cancellationToken) =>
        await dbContext.BusinessAuditEvents.AsNoTracking()
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => new BusinessAuditEventItem(x.Id, x.OccurredAt, x.Module, x.EventType, x.EntityType, x.EntityId, x.Summary, x.Reason, x.ActorUserId, x.ActorEmail, x.ActorDisplayName, x.MetadataJson))
            .ToListAsync(cancellationToken);

    private static IReadOnlyList<string> DeserializeStringList(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<string>>(json, SerializerOptions) ?? [];

    private static (int Page, int PageSize, int Skip) NormalizePaging(int? page, int? pageSize, int defaultPageSize, int maxPageSize)
    {
        var normalizedPage = page.GetValueOrDefault(1);
        if (normalizedPage < 1)
        {
            normalizedPage = 1;
        }

        var normalizedPageSize = pageSize.GetValueOrDefault(defaultPageSize);
        normalizedPageSize = Math.Clamp(normalizedPageSize, 1, maxPageSize);
        return (normalizedPage, normalizedPageSize, (normalizedPage - 1) * normalizedPageSize);
    }
}

