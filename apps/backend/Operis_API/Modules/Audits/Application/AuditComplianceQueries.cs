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

    public async Task<PagedResult<EvidenceRuleListItem>> ListEvidenceRulesAsync(EvidenceRuleListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize, 25, 100);
        var baseQuery = dbContext.EvidenceRules.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            baseQuery = baseQuery.Where(x => EF.Functions.ILike(x.RuleCode, pattern) || EF.Functions.ILike(x.Title, pattern));
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            baseQuery = baseQuery.Where(x => x.Status == query.Status.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(query.ProcessArea))
        {
            baseQuery = baseQuery.Where(x => x.ProcessArea == query.ProcessArea.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(query.ArtifactType))
        {
            baseQuery = baseQuery.Where(x => x.ArtifactType == query.ArtifactType.Trim().ToLowerInvariant());
        }

        if (query.ProjectId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.ProjectId == query.ProjectId.Value);
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderBy(x => x.ProcessArea)
            .ThenBy(x => x.RuleCode)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new EvidenceRuleListItem(x.Id, x.RuleCode, x.Title, x.ProcessArea, x.ArtifactType, x.ProjectId, x.Status, x.ExpressionType, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<EvidenceRuleListItem>(items, total, page, pageSize);
    }

    public async Task<EvidenceRuleDetailResponse?> GetEvidenceRuleAsync(Guid ruleId, CancellationToken cancellationToken)
    {
        var rule = await dbContext.EvidenceRules.AsNoTracking().SingleOrDefaultAsync(x => x.Id == ruleId, cancellationToken);
        if (rule is null)
        {
            return null;
        }

        var history = await LoadHistoryAsync("evidence_rule", ruleId.ToString(), cancellationToken);
        return new EvidenceRuleDetailResponse(rule.Id, rule.RuleCode, rule.Title, rule.ProcessArea, rule.ArtifactType, rule.ProjectId, rule.Status, rule.ExpressionType, rule.Reason, rule.CreatedAt, rule.UpdatedAt, history);
    }

    public async Task<PagedResult<EvidenceRuleResultListItem>> ListEvidenceRuleResultsAsync(EvidenceRuleResultListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize, 25, 100);
        var baseQuery =
            from result in dbContext.EvidenceRuleResults.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on result.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new { Result = result, ProjectCode = project != null ? project.Code : null };

        if (!string.IsNullOrWhiteSpace(query.ScopeType))
        {
            baseQuery = baseQuery.Where(x => x.Result.ScopeType == query.ScopeType.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            baseQuery = baseQuery.Where(x => x.Result.Status == query.Status.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(query.ProcessArea))
        {
            baseQuery = baseQuery.Where(x => x.Result.ProcessArea == query.ProcessArea.Trim().ToLowerInvariant());
        }

        if (query.ProjectId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Result.ProjectId == query.ProjectId.Value);
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderByDescending(x => x.Result.CompletedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new EvidenceRuleResultListItem(
                x.Result.Id,
                x.Result.ScopeType,
                x.Result.ScopeRef,
                x.Result.ProjectId,
                x.ProjectCode,
                x.Result.ProcessArea,
                x.Result.Status,
                x.Result.EvaluatedRuleCount,
                x.Result.MissingItemCount,
                x.Result.StartedAt,
                x.Result.CompletedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<EvidenceRuleResultListItem>(items, total, page, pageSize);
    }

    public async Task<EvidenceRuleResultDetailResponse?> GetEvidenceRuleResultAsync(Guid resultId, CancellationToken cancellationToken)
    {
        var result = await (
            from entry in dbContext.EvidenceRuleResults.AsNoTracking()
            where entry.Id == resultId
            join project in dbContext.Projects.AsNoTracking() on entry.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new { Result = entry, ProjectCode = project != null ? project.Code : null }).SingleOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return null;
        }

        var missingEntities = await dbContext.EvidenceMissingItems.AsNoTracking()
            .Where(x => x.ResultId == resultId)
            .OrderBy(x => x.ProcessArea)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);

        var missingProjectIds = missingEntities.Where(x => x.ProjectId.HasValue).Select(x => x.ProjectId!.Value).Distinct().ToArray();
        var projectLookup = await dbContext.Projects.AsNoTracking()
            .Where(x => (result.Result.ProjectId.HasValue && x.Id == result.Result.ProjectId.Value) || missingProjectIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

        var missingItems = missingEntities
            .Select(x => new EvidenceMissingItemResponse(
                x.Id,
                x.RuleId,
                x.ProjectId,
                x.ProjectId.HasValue && projectLookup.ContainsKey(x.ProjectId.Value) ? projectLookup[x.ProjectId.Value] : null,
                x.ProcessArea,
                x.ArtifactType,
                x.ReasonCode,
                x.Title,
                x.Module,
                x.Route,
                x.Scope,
                x.EntityType,
                x.EntityId,
                x.Metadata,
                x.DetectedAt))
            .ToList();

        var history = await LoadHistoryAsync("evidence_rule_result", resultId.ToString(), cancellationToken);
        return new EvidenceRuleResultDetailResponse(
            result.Result.Id,
            result.Result.ScopeType,
            result.Result.ScopeRef,
            result.Result.ProjectId,
            result.ProjectCode,
            result.Result.ProcessArea,
            result.Result.Status,
            result.Result.EvaluatedRuleCount,
            result.Result.MissingItemCount,
            result.Result.StartedAt,
            result.Result.CompletedAt,
            missingItems,
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
