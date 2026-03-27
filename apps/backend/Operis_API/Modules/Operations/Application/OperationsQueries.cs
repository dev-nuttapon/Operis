using System.Text.Json;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Operations.Contracts;
using Operis_API.Modules.Operations.Infrastructure;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Operations.Application;

public sealed class OperationsQueries(OperisDbContext dbContext) : IOperationsQueries
{
    public async Task<PagedResult<AccessReviewResponse>> ListAccessReviewsAsync(AccessReviewListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.AccessReviews.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.ScopeType)) source = source.Where(x => x.ScopeType == query.ScopeType);
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.ScopeRef, search) || EF.Functions.ILike(x.ReviewCycle, search));
        }

        source = ApplyOrdering(source, query.SortBy, query.SortOrder, x => x.CreatedAt, x => x.ScopeRef);
        return await PageAsync(
            source.Select(x => new AccessReviewResponse(x.Id, x.ScopeType, x.ScopeRef, x.ReviewCycle, x.ReviewedBy, x.Status, x.Decision, x.DecisionRationale, x.CreatedAt, x.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<SecurityReviewResponse>> ListSecurityReviewsAsync(SecurityReviewListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.SecurityReviews.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.ScopeType)) source = source.Where(x => x.ScopeType == query.ScopeType);
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.ScopeRef, search) || EF.Functions.ILike(x.ControlsReviewed, search));
        }

        source = ApplyOrdering(source, query.SortBy, query.SortOrder, x => x.CreatedAt, x => x.ScopeRef);
        return await PageAsync(
            source.Select(x => new SecurityReviewResponse(x.Id, x.ScopeType, x.ScopeRef, x.ControlsReviewed, x.FindingsSummary, x.Status, x.CreatedAt, x.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<ExternalDependencyResponse>> ListExternalDependenciesAsync(ExternalDependencyListQuery query, CancellationToken cancellationToken)
    {
        var source =
            from dependency in dbContext.ExternalDependencies.AsNoTracking()
            join supplier in dbContext.Suppliers.AsNoTracking() on dependency.SupplierId equals supplier.Id into supplierJoin
            from supplier in supplierJoin.DefaultIfEmpty()
            select new { dependency, supplier };

        if (!string.IsNullOrWhiteSpace(query.DependencyType)) source = source.Where(x => x.dependency.DependencyType == query.DependencyType);
        if (query.SupplierId.HasValue) source = source.Where(x => x.dependency.SupplierId == query.SupplierId.Value);
        if (!string.IsNullOrWhiteSpace(query.Criticality)) source = source.Where(x => x.dependency.Criticality == query.Criticality);
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.dependency.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.dependency.Name, search) ||
                EF.Functions.ILike(x.dependency.OwnerUserId, search) ||
                (x.supplier != null && EF.Functions.ILike(x.supplier.Name, search)));
        }

        var descending = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "reviewdueat" => descending ? source.OrderByDescending(x => x.dependency.ReviewDueAt) : source.OrderBy(x => x.dependency.ReviewDueAt),
            "suppliername" => descending ? source.OrderByDescending(x => x.supplier != null ? x.supplier.Name : string.Empty) : source.OrderBy(x => x.supplier != null ? x.supplier.Name : string.Empty),
            _ => descending ? source.OrderByDescending(x => x.dependency.CreatedAt) : source.OrderBy(x => x.dependency.CreatedAt)
        };

        return await PageAsync(
            source.Select(x => new ExternalDependencyResponse(
                x.dependency.Id,
                x.dependency.Name,
                x.dependency.DependencyType,
                x.dependency.SupplierId,
                x.supplier != null ? x.supplier.Name : null,
                x.dependency.OwnerUserId,
                x.dependency.Criticality,
                x.dependency.Status,
                x.dependency.ReviewDueAt,
                x.dependency.CreatedAt,
                x.dependency.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<ConfigurationAuditResponse>> ListConfigurationAuditsAsync(ConfigurationAuditListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.ConfigurationAudits.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.ScopeRef)) source = source.Where(x => x.ScopeRef == query.ScopeRef);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.ScopeRef, search));
        }

        source = ApplyOrdering(source, query.SortBy, query.SortOrder, x => x.PlannedAt, x => x.ScopeRef);
        return await PageAsync(
            source.Select(x => new ConfigurationAuditResponse(x.Id, x.ScopeRef, x.PlannedAt, x.Status, x.FindingCount, x.CreatedAt, x.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<SupplierResponse>> ListSuppliersAsync(SupplierListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.Suppliers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.SupplierType)) source = source.Where(x => x.SupplierType == query.SupplierType);
        if (!string.IsNullOrWhiteSpace(query.OwnerUserId)) source = source.Where(x => x.OwnerUserId == query.OwnerUserId);
        if (!string.IsNullOrWhiteSpace(query.Criticality)) source = source.Where(x => x.Criticality == query.Criticality);
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status);
        if (query.ReviewDueBefore.HasValue) source = source.Where(x => x.ReviewDueAt != null && x.ReviewDueAt <= query.ReviewDueBefore.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.Name, search) || EF.Functions.ILike(x.OwnerUserId, search));
        }

        var descending = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "reviewdueat" => descending ? source.OrderByDescending(x => x.ReviewDueAt) : source.OrderBy(x => x.ReviewDueAt),
            "name" => descending ? source.OrderByDescending(x => x.Name) : source.OrderBy(x => x.Name),
            _ => descending ? source.OrderByDescending(x => x.CreatedAt) : source.OrderBy(x => x.CreatedAt)
        };

        var normalizedPage = NormalizePage(query.Page);
        var normalizedPageSize = NormalizePageSize(query.PageSize);
        var total = await source.CountAsync(cancellationToken);
        var items = await source.Skip((normalizedPage - 1) * normalizedPageSize).Take(normalizedPageSize)
            .Select(x => new SupplierResponse(
                x.Id,
                x.Name,
                x.SupplierType,
                x.OwnerUserId,
                x.Criticality,
                x.Status,
                x.ReviewDueAt,
                dbContext.SupplierAgreements.Count(y => y.SupplierId == x.Id && y.Status == "Active"),
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<SupplierResponse>(items, total, normalizedPage, normalizedPageSize);
    }

    public async Task<SupplierResponse?> GetSupplierAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Suppliers.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SupplierResponse(
                x.Id,
                x.Name,
                x.SupplierType,
                x.OwnerUserId,
                x.Criticality,
                x.Status,
                x.ReviewDueAt,
                dbContext.SupplierAgreements.Count(y => y.SupplierId == x.Id && y.Status == "Active"),
                x.CreatedAt,
                x.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<SupplierAgreementResponse>> ListSupplierAgreementsAsync(SupplierAgreementListQuery query, CancellationToken cancellationToken)
    {
        var source =
            from agreement in dbContext.SupplierAgreements.AsNoTracking()
            join supplier in dbContext.Suppliers.AsNoTracking() on agreement.SupplierId equals supplier.Id
            select new { agreement, supplier };

        if (query.SupplierId.HasValue) source = source.Where(x => x.agreement.SupplierId == query.SupplierId.Value);
        if (!string.IsNullOrWhiteSpace(query.AgreementType)) source = source.Where(x => x.agreement.AgreementType == query.AgreementType);
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.agreement.Status == query.Status);
        if (query.EffectiveToBefore.HasValue) source = source.Where(x => x.agreement.EffectiveTo != null && x.agreement.EffectiveTo <= query.EffectiveToBefore.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.supplier.Name, search) ||
                EF.Functions.ILike(x.agreement.AgreementType, search) ||
                EF.Functions.ILike(x.agreement.EvidenceRef, search));
        }

        var descending = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "effectiveto" => descending ? source.OrderByDescending(x => x.agreement.EffectiveTo) : source.OrderBy(x => x.agreement.EffectiveTo),
            "suppliername" => descending ? source.OrderByDescending(x => x.supplier.Name) : source.OrderBy(x => x.supplier.Name),
            _ => descending ? source.OrderByDescending(x => x.agreement.CreatedAt) : source.OrderBy(x => x.agreement.CreatedAt)
        };

        return await PageAsync(
            source.Select(x => new SupplierAgreementResponse(
                x.agreement.Id,
                x.agreement.SupplierId,
                x.supplier.Name,
                x.agreement.AgreementType,
                x.agreement.EffectiveFrom,
                x.agreement.EffectiveTo,
                x.agreement.SlaTerms,
                x.agreement.EvidenceRef,
                x.agreement.Status,
                x.agreement.CreatedAt,
                x.agreement.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<AccessRecertificationResponse>> ListAccessRecertificationsAsync(AccessRecertificationListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.AccessRecertificationSchedules.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.ScopeType)) source = source.Where(x => x.ScopeType == query.ScopeType.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.ReviewOwnerUserId)) source = source.Where(x => x.ReviewOwnerUserId == query.ReviewOwnerUserId.Trim());
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status.Trim().ToLowerInvariant());
        if (query.PlannedBefore.HasValue) source = source.Where(x => x.PlannedAt <= query.PlannedBefore.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.ScopeRef, search) || EF.Functions.ILike(x.ReviewOwnerUserId, search));
        }

        var descending = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "scoperef" => descending ? source.OrderByDescending(x => x.ScopeRef) : source.OrderBy(x => x.ScopeRef),
            _ => descending ? source.OrderByDescending(x => x.PlannedAt) : source.OrderBy(x => x.PlannedAt)
        };

        var normalizedPage = NormalizePage(query.Page);
        var normalizedPageSize = NormalizePageSize(query.PageSize);
        var total = await source.CountAsync(cancellationToken);
        var schedules = await source.Skip((normalizedPage - 1) * normalizedPageSize).Take(normalizedPageSize).ToListAsync(cancellationToken);
        var scheduleIds = schedules.Select(x => x.Id).ToArray();
        var decisions = await dbContext.AccessRecertificationDecisions.AsNoTracking()
            .Where(x => scheduleIds.Contains(x.ScheduleId))
            .OrderBy(x => x.SubjectUserId)
            .Select(x => new AccessRecertificationDecisionResponse(x.Id, x.ScheduleId, x.SubjectUserId, x.Decision, x.Reason, x.DecidedBy, x.DecidedAt))
            .ToListAsync(cancellationToken);
        var decisionsBySchedule = decisions.GroupBy(x => x.ScheduleId).ToDictionary(x => x.Key, x => (IReadOnlyList<AccessRecertificationDecisionResponse>)x.ToList());

        var items = schedules.Select(schedule =>
        {
            var subjectUserIds = DeserializeSubjects(schedule.SubjectUsersJson);
            var scheduleDecisions = decisionsBySchedule.GetValueOrDefault(schedule.Id, []);
            var completedCount = scheduleDecisions.Count;
            var pendingCount = Math.Max(0, subjectUserIds.Count - completedCount);
            return new AccessRecertificationResponse(
                schedule.Id,
                schedule.ScopeType,
                schedule.ScopeRef,
                schedule.PlannedAt,
                schedule.ReviewOwnerUserId,
                schedule.Status,
                subjectUserIds,
                scheduleDecisions,
                schedule.ExceptionNotes,
                completedCount,
                pendingCount,
                schedule.CreatedAt,
                schedule.UpdatedAt,
                schedule.CompletedAt);
        }).ToList();

        return new PagedResult<AccessRecertificationResponse>(items, total, normalizedPage, normalizedPageSize);
    }

    public async Task<AccessRecertificationResponse?> GetAccessRecertificationAsync(Guid id, CancellationToken cancellationToken)
    {
        var schedule = await dbContext.AccessRecertificationSchedules.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (schedule is null)
        {
            return null;
        }

        var decisions = await dbContext.AccessRecertificationDecisions.AsNoTracking()
            .Where(x => x.ScheduleId == id)
            .OrderBy(x => x.SubjectUserId)
            .Select(x => new AccessRecertificationDecisionResponse(x.Id, x.ScheduleId, x.SubjectUserId, x.Decision, x.Reason, x.DecidedBy, x.DecidedAt))
            .ToListAsync(cancellationToken);

        var subjectUserIds = DeserializeSubjects(schedule.SubjectUsersJson);
        var completedCount = decisions.Count;
        var pendingCount = Math.Max(0, subjectUserIds.Count - completedCount);
        return new AccessRecertificationResponse(
            schedule.Id,
            schedule.ScopeType,
            schedule.ScopeRef,
            schedule.PlannedAt,
            schedule.ReviewOwnerUserId,
            schedule.Status,
            subjectUserIds,
            decisions,
            schedule.ExceptionNotes,
            completedCount,
            pendingCount,
            schedule.CreatedAt,
            schedule.UpdatedAt,
            schedule.CompletedAt);
    }

    public async Task<PagedResult<SecurityIncidentResponse>> ListSecurityIncidentsAsync(SecurityIncidentListQuery query, CancellationToken cancellationToken)
    {
        var source =
            from incident in dbContext.SecurityIncidents.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on incident.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new { incident, project };

        if (query.ProjectId.HasValue) source = source.Where(x => x.incident.ProjectId == query.ProjectId.Value);
        if (!string.IsNullOrWhiteSpace(query.Severity)) source = source.Where(x => x.incident.Severity == query.Severity.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.OwnerUserId)) source = source.Where(x => x.incident.OwnerUserId == query.OwnerUserId.Trim());
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.incident.Status == query.Status.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.incident.Code, search) ||
                EF.Functions.ILike(x.incident.Title, search) ||
                EF.Functions.ILike(x.incident.OwnerUserId, search) ||
                (x.project != null && EF.Functions.ILike(x.project.Name, search)));
        }

        var descending = !string.Equals(query.SortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "code" => descending ? source.OrderByDescending(x => x.incident.Code) : source.OrderBy(x => x.incident.Code),
            "title" => descending ? source.OrderByDescending(x => x.incident.Title) : source.OrderBy(x => x.incident.Title),
            _ => descending ? source.OrderByDescending(x => x.incident.ReportedAt) : source.OrderBy(x => x.incident.ReportedAt)
        };

        return await PageAsync(
            source.Select(x => new SecurityIncidentResponse(
                x.incident.Id,
                x.incident.ProjectId,
                x.project != null ? x.project.Name : null,
                x.incident.Code,
                x.incident.Title,
                x.incident.Severity,
                x.incident.ReportedAt,
                x.incident.OwnerUserId,
                x.incident.Status,
                x.incident.ResolutionSummary,
                x.incident.CreatedAt,
                x.incident.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<SecurityIncidentResponse?> GetSecurityIncidentAsync(Guid id, CancellationToken cancellationToken)
    {
        return await (
            from incident in dbContext.SecurityIncidents.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on incident.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            where incident.Id == id
            select new SecurityIncidentResponse(
                incident.Id,
                incident.ProjectId,
                project != null ? project.Name : null,
                incident.Code,
                incident.Title,
                incident.Severity,
                incident.ReportedAt,
                incident.OwnerUserId,
                incident.Status,
                incident.ResolutionSummary,
                incident.CreatedAt,
                incident.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<VulnerabilityResponse>> ListVulnerabilitiesAsync(VulnerabilityListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.VulnerabilityRecords.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Severity)) source = source.Where(x => x.Severity == query.Severity.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.OwnerUserId)) source = source.Where(x => x.OwnerUserId == query.OwnerUserId.Trim());
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.AssetRef, search) ||
                EF.Functions.ILike(x.Title, search) ||
                EF.Functions.ILike(x.OwnerUserId, search));
        }

        var descending = !string.Equals(query.SortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "patchdueat" => descending ? source.OrderByDescending(x => x.PatchDueAt) : source.OrderBy(x => x.PatchDueAt),
            "assetref" => descending ? source.OrderByDescending(x => x.AssetRef) : source.OrderBy(x => x.AssetRef),
            _ => descending ? source.OrderByDescending(x => x.IdentifiedAt) : source.OrderBy(x => x.IdentifiedAt)
        };

        return await PageAsync(
            source.Select(x => new VulnerabilityResponse(x.Id, x.AssetRef, x.Title, x.Severity, x.IdentifiedAt, x.PatchDueAt, x.OwnerUserId, x.Status, x.VerificationSummary, x.CreatedAt, x.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<SecretRotationResponse>> ListSecretRotationsAsync(SecretRotationListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.SecretRotations.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Touchpoint)) source = source.Where(x => x.Touchpoint == query.Touchpoint.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.SecretScope)) source = source.Where(x => x.SecretScope == query.SecretScope.Trim());
        if (!string.IsNullOrWhiteSpace(query.VerifiedBy)) source = source.Where(x => x.VerifiedBy == query.VerifiedBy.Trim());
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Touchpoint, search) ||
                EF.Functions.ILike(x.SecretScope, search) ||
                (x.EvidenceRef != null && EF.Functions.ILike(x.EvidenceRef, search)) ||
                (x.VerifiedBy != null && EF.Functions.ILike(x.VerifiedBy, search)));
        }

        var descending = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "secretscope" => descending ? source.OrderByDescending(x => x.SecretScope) : source.OrderBy(x => x.SecretScope),
            "verifiedat" => descending ? source.OrderByDescending(x => x.VerifiedAt) : source.OrderBy(x => x.VerifiedAt),
            _ => descending ? source.OrderByDescending(x => x.PlannedAt) : source.OrderBy(x => x.PlannedAt)
        };

        return await PageAsync(
            source.Select(x => new SecretRotationResponse(x.Id, x.Touchpoint, x.SecretScope, x.EvidenceRef, x.PlannedAt, x.RotatedAt, x.VerifiedBy, x.VerifiedAt, x.Status, x.CreatedAt, x.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<PrivilegedAccessEventResponse>> ListPrivilegedAccessEventsAsync(PrivilegedAccessEventListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.PrivilegedAccessEvents.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.RequestedBy)) source = source.Where(x => x.RequestedBy == query.RequestedBy.Trim());
        if (!string.IsNullOrWhiteSpace(query.ApprovedBy)) source = source.Where(x => x.ApprovedBy == query.ApprovedBy.Trim());
        if (!string.IsNullOrWhiteSpace(query.UsedBy)) source = source.Where(x => x.UsedBy == query.UsedBy.Trim());
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.RequestedBy, search) ||
                (x.ApprovedBy != null && EF.Functions.ILike(x.ApprovedBy, search)) ||
                (x.UsedBy != null && EF.Functions.ILike(x.UsedBy, search)) ||
                EF.Functions.ILike(x.Reason, search));
        }

        var descending = !string.Equals(query.SortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "usedat" => descending ? source.OrderByDescending(x => x.UsedAt) : source.OrderBy(x => x.UsedAt),
            "requestedby" => descending ? source.OrderByDescending(x => x.RequestedBy) : source.OrderBy(x => x.RequestedBy),
            _ => descending ? source.OrderByDescending(x => x.RequestedAt) : source.OrderBy(x => x.RequestedAt)
        };

        return await PageAsync(
            source.Select(x => new PrivilegedAccessEventResponse(x.Id, x.RequestedBy, x.ApprovedBy, x.UsedBy, x.RequestedAt, x.ApprovedAt, x.UsedAt, x.ReviewedAt, x.Status, x.Reason, x.CreatedAt, x.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<ClassificationPolicyResponse>> ListClassificationPoliciesAsync(ClassificationPolicyListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.DataClassificationPolicies.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.ClassificationLevel)) source = source.Where(x => x.ClassificationLevel == query.ClassificationLevel.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.Scope)) source = source.Where(x => x.Scope == query.Scope.Trim());
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.PolicyCode, search) ||
                EF.Functions.ILike(x.Scope, search) ||
                (x.HandlingRule != null && EF.Functions.ILike(x.HandlingRule, search)));
        }

        var descending = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "classificationlevel" => descending ? source.OrderByDescending(x => x.ClassificationLevel) : source.OrderBy(x => x.ClassificationLevel),
            _ => descending ? source.OrderByDescending(x => x.PolicyCode) : source.OrderBy(x => x.PolicyCode)
        };

        return await PageAsync(
            source.Select(x => new ClassificationPolicyResponse(x.Id, x.PolicyCode, x.ClassificationLevel, x.Scope, x.Status, x.HandlingRule, x.CreatedAt, x.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<BackupEvidenceResponse>> ListBackupEvidenceAsync(BackupEvidenceListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.BackupEvidence.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.BackupScope)) source = source.Where(x => x.BackupScope == query.BackupScope.Trim());
        if (!string.IsNullOrWhiteSpace(query.ExecutedBy)) source = source.Where(x => x.ExecutedBy == query.ExecutedBy.Trim());
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status.Trim().ToLowerInvariant());
        if (query.ExecutedAfter.HasValue) source = source.Where(x => x.ExecutedAt >= query.ExecutedAfter.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.BackupScope, search) ||
                EF.Functions.ILike(x.ExecutedBy, search) ||
                (x.EvidenceRef != null && EF.Functions.ILike(x.EvidenceRef, search)));
        }

        var descending = !string.Equals(query.SortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "backupscope" => descending ? source.OrderByDescending(x => x.BackupScope) : source.OrderBy(x => x.BackupScope),
            _ => descending ? source.OrderByDescending(x => x.ExecutedAt) : source.OrderBy(x => x.ExecutedAt)
        };

        return await PageAsync(
            source.Select(x => new BackupEvidenceResponse(x.Id, x.BackupScope, x.ExecutedAt, x.ExecutedBy, x.Status, x.EvidenceRef, x.CreatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<RestoreVerificationResponse>> ListRestoreVerificationsAsync(RestoreVerificationListQuery query, CancellationToken cancellationToken)
    {
        var source =
            from verification in dbContext.RestoreVerifications.AsNoTracking()
            join backup in dbContext.BackupEvidence.AsNoTracking() on verification.BackupEvidenceId equals backup.Id
            select new { verification, backup };

        if (query.BackupEvidenceId.HasValue) source = source.Where(x => x.verification.BackupEvidenceId == query.BackupEvidenceId.Value);
        if (!string.IsNullOrWhiteSpace(query.ExecutedBy)) source = source.Where(x => x.verification.ExecutedBy == query.ExecutedBy.Trim());
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.verification.Status == query.Status.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.backup.BackupScope, search) ||
                EF.Functions.ILike(x.verification.ExecutedBy, search) ||
                EF.Functions.ILike(x.verification.ResultSummary, search));
        }

        var descending = !string.Equals(query.SortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "backupscope" => descending ? source.OrderByDescending(x => x.backup.BackupScope) : source.OrderBy(x => x.backup.BackupScope),
            _ => descending ? source.OrderByDescending(x => x.verification.ExecutedAt) : source.OrderBy(x => x.verification.ExecutedAt)
        };

        return await PageAsync(
            source.Select(x => new RestoreVerificationResponse(x.verification.Id, x.verification.BackupEvidenceId, x.backup.BackupScope, x.verification.ExecutedAt, x.verification.ExecutedBy, x.verification.Status, x.verification.ResultSummary, x.verification.CreatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<DrDrillResponse>> ListDrDrillsAsync(DrDrillListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.DrDrills.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.ScopeRef)) source = source.Where(x => x.ScopeRef == query.ScopeRef.Trim());
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status.Trim().ToLowerInvariant());
        if (query.PlannedAfter.HasValue) source = source.Where(x => x.PlannedAt >= query.PlannedAfter.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.ScopeRef, search) ||
                (x.Summary != null && EF.Functions.ILike(x.Summary, search)));
        }

        var descending = !string.Equals(query.SortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "scoperef" => descending ? source.OrderByDescending(x => x.ScopeRef) : source.OrderBy(x => x.ScopeRef),
            _ => descending ? source.OrderByDescending(x => x.PlannedAt) : source.OrderBy(x => x.PlannedAt)
        };

        return await PageAsync(
            source.Select(x => new DrDrillResponse(x.Id, x.ScopeRef, x.PlannedAt, x.ExecutedAt, x.Status, x.FindingCount, x.Summary, x.CreatedAt, x.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<LegalHoldResponse>> ListLegalHoldsAsync(LegalHoldListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.LegalHolds.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.ScopeType)) source = source.Where(x => x.ScopeType == query.ScopeType.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.ScopeRef, search) ||
                EF.Functions.ILike(x.PlacedBy, search) ||
                EF.Functions.ILike(x.Reason, search) ||
                (x.ReleaseReason != null && EF.Functions.ILike(x.ReleaseReason, search)));
        }

        var descending = !string.Equals(query.SortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "scoperef" => descending ? source.OrderByDescending(x => x.ScopeRef) : source.OrderBy(x => x.ScopeRef),
            _ => descending ? source.OrderByDescending(x => x.PlacedAt) : source.OrderBy(x => x.PlacedAt)
        };

        return await PageAsync(
            source.Select(x => new LegalHoldResponse(x.Id, x.ScopeType, x.ScopeRef, x.PlacedAt, x.PlacedBy, x.Status, x.Reason, x.ReleasedAt, x.ReleasedBy, x.ReleaseReason, x.CreatedAt, x.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<CapaRecordResponse>> ListCapaRecordsAsync(CapaRecordListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.CapaRecords.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.SourceType)) source = source.Where(x => x.SourceType == query.SourceType.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.OwnerUserId)) source = source.Where(x => x.OwnerUserId == query.OwnerUserId.Trim());
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.SourceRef, search) ||
                EF.Functions.ILike(x.Title, search) ||
                EF.Functions.ILike(x.OwnerUserId, search) ||
                (x.RootCauseSummary != null && EF.Functions.ILike(x.RootCauseSummary, search)));
        }

        source = ApplyOrdering(source, query.SortBy, query.SortOrder, x => x.CreatedAt, x => x.SourceRef);
        var page = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);
        var total = await source.CountAsync(cancellationToken);
        var records = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var ids = records.Select(x => x.Id).ToArray();
        var actions = await dbContext.CapaActions.AsNoTracking()
            .Where(x => ids.Contains(x.CapaRecordId))
            .OrderBy(x => x.DueDate)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new CapaActionResponse(x.Id, x.CapaRecordId, x.ActionDescription, x.AssignedTo, x.DueDate, x.Status, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var reviews = await (
            from review in dbContext.CapaEffectivenessReviews.AsNoTracking()
            join record in dbContext.CapaRecords.AsNoTracking() on review.CapaRecordId equals record.Id
            where ids.Contains(review.CapaRecordId)
            orderby review.ReviewedAt descending, review.CreatedAt descending
            select new CapaEffectivenessReviewResponse(
                review.Id,
                review.CapaRecordId,
                record.Title,
                record.OwnerUserId,
                record.Status,
                review.EffectivenessResult,
                review.EvidenceRef,
                review.ReviewSummary,
                review.Status,
                review.ReviewedBy,
                review.ReviewedAt,
                review.ReopenedAt,
                review.ReopenedBy,
                review.ReopenReason,
                review.CreatedAt,
                review.UpdatedAt))
            .ToListAsync(cancellationToken);

        var actionLookup = actions.GroupBy(x => x.CapaRecordId).ToDictionary(x => x.Key, x => (IReadOnlyList<CapaActionResponse>)x.ToList());
        var reviewLookup = reviews.GroupBy(x => x.CapaRecordId).ToDictionary(x => x.Key, x => (IReadOnlyList<CapaEffectivenessReviewResponse>)x.ToList());
        var items = records.Select(x => new CapaRecordResponse(x.Id, x.SourceType, x.SourceRef, x.Title, x.OwnerUserId, x.RootCauseSummary, x.Status, actionLookup.GetValueOrDefault(x.Id, []), reviewLookup.GetValueOrDefault(x.Id, []), x.CreatedAt, x.UpdatedAt, x.VerifiedAt, x.VerifiedBy, x.ClosedAt, x.ClosedBy)).ToList();
        return new PagedResult<CapaRecordResponse>(items, total, page, pageSize);
    }

    public async Task<CapaRecordResponse?> GetCapaRecordAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await dbContext.CapaRecords.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (record is null)
        {
            return null;
        }

        var actions = await dbContext.CapaActions.AsNoTracking()
            .Where(x => x.CapaRecordId == id)
            .OrderBy(x => x.DueDate)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new CapaActionResponse(x.Id, x.CapaRecordId, x.ActionDescription, x.AssignedTo, x.DueDate, x.Status, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var reviews = await dbContext.CapaEffectivenessReviews.AsNoTracking()
            .Where(x => x.CapaRecordId == id)
            .OrderByDescending(x => x.ReviewedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new CapaEffectivenessReviewResponse(x.Id, x.CapaRecordId, record.Title, record.OwnerUserId, record.Status, x.EffectivenessResult, x.EvidenceRef, x.ReviewSummary, x.Status, x.ReviewedBy, x.ReviewedAt, x.ReopenedAt, x.ReopenedBy, x.ReopenReason, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new CapaRecordResponse(record.Id, record.SourceType, record.SourceRef, record.Title, record.OwnerUserId, record.RootCauseSummary, record.Status, actions, reviews, record.CreatedAt, record.UpdatedAt, record.VerifiedAt, record.VerifiedBy, record.ClosedAt, record.ClosedBy);
    }

    public async Task<PagedResult<CapaEffectivenessReviewResponse>> ListCapaEffectivenessReviewsAsync(CapaEffectivenessReviewListQuery query, CancellationToken cancellationToken)
    {
        var source =
            from review in dbContext.CapaEffectivenessReviews.AsNoTracking()
            join record in dbContext.CapaRecords.AsNoTracking() on review.CapaRecordId equals record.Id
            select new { review, record };

        if (query.CapaRecordId.HasValue) source = source.Where(x => x.review.CapaRecordId == query.CapaRecordId.Value);
        if (!string.IsNullOrWhiteSpace(query.EffectivenessResult)) source = source.Where(x => x.review.EffectivenessResult == query.EffectivenessResult.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.review.Status == query.Status.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.ReviewedBy)) source = source.Where(x => x.review.ReviewedBy == query.ReviewedBy.Trim());
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.record.Title, search) ||
                EF.Functions.ILike(x.record.SourceRef, search) ||
                EF.Functions.ILike(x.review.ReviewedBy, search) ||
                EF.Functions.ILike(x.review.EvidenceRef, search) ||
                (x.review.ReviewSummary != null && EF.Functions.ILike(x.review.ReviewSummary, search)));
        }

        var descending = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "reviewedat" => descending ? source.OrderByDescending(x => x.review.ReviewedAt) : source.OrderBy(x => x.review.ReviewedAt),
            "status" => descending ? source.OrderByDescending(x => x.review.Status) : source.OrderBy(x => x.review.Status),
            _ => descending ? source.OrderByDescending(x => x.review.CreatedAt) : source.OrderBy(x => x.review.CreatedAt)
        };

        return await PageAsync(
            source.Select(x => new CapaEffectivenessReviewResponse(
                x.review.Id,
                x.review.CapaRecordId,
                x.record.Title,
                x.record.OwnerUserId,
                x.record.Status,
                x.review.EffectivenessResult,
                x.review.EvidenceRef,
                x.review.ReviewSummary,
                x.review.Status,
                x.review.ReviewedBy,
                x.review.ReviewedAt,
                x.review.ReopenedAt,
                x.review.ReopenedBy,
                x.review.ReopenReason,
                x.review.CreatedAt,
                x.review.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<EscalationEventResponse>> ListEscalationEventsAsync(EscalationEventListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.EscalationEvents.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.ScopeType)) source = source.Where(x => x.ScopeType == query.ScopeType.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.EscalatedTo)) source = source.Where(x => x.EscalatedTo == query.EscalatedTo.Trim());
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.ScopeRef, search) ||
                EF.Functions.ILike(x.EscalatedTo, search) ||
                EF.Functions.ILike(x.TriggerReason, search));
        }

        source = ApplyOrdering(source, query.SortBy, query.SortOrder, x => x.TriggeredAt, x => x.ScopeRef);
        return await PageAsync(
            source.Select(x => new EscalationEventResponse(x.Id, x.ScopeType, x.ScopeRef, x.TriggeredAt, x.TriggerReason, x.EscalatedTo, x.Status, x.CreatedAt, x.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<AutomationJobResponse>> ListAutomationJobsAsync(AutomationJobListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.AutomationJobs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.JobType)) source = source.Where(x => x.JobType == query.JobType.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.ScopeRef)) source = source.Where(x => x.ScopeRef == query.ScopeRef.Trim());
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.JobName, search) ||
                EF.Functions.ILike(x.JobType, search) ||
                EF.Functions.ILike(x.ScopeRef, search) ||
                EF.Functions.ILike(x.ScheduleRef, search));
        }

        var descending = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "jobname" => descending ? source.OrderByDescending(x => x.JobName) : source.OrderBy(x => x.JobName),
            "latestrunat" => descending ? source.OrderByDescending(x => x.LatestRunAt) : source.OrderBy(x => x.LatestRunAt),
            _ => descending ? source.OrderByDescending(x => x.CreatedAt) : source.OrderBy(x => x.CreatedAt)
        };

        return await PageAsync(
            source.Select(x => new AutomationJobResponse(x.Id, x.JobName, x.JobType, x.ScopeRef, x.ScheduleRef, x.Status, x.LatestRunStatus, x.LatestRunAt, x.FailureSummary, x.CreatedBy, x.CreatedAt, x.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<AutomationJobResponse?> GetAutomationJobAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.AutomationJobs.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AutomationJobResponse(x.Id, x.JobName, x.JobType, x.ScopeRef, x.ScheduleRef, x.Status, x.LatestRunStatus, x.LatestRunAt, x.FailureSummary, x.CreatedBy, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<AutomationJobRunResponse>> ListAutomationJobRunsAsync(AutomationJobRunListQuery query, CancellationToken cancellationToken)
    {
        var source =
            from run in dbContext.AutomationJobRuns.AsNoTracking()
            join job in dbContext.AutomationJobs.AsNoTracking() on run.JobId equals job.Id
            select new { run, job };

        if (query.JobId.HasValue) source = source.Where(x => x.run.JobId == query.JobId.Value);
        if (!string.IsNullOrWhiteSpace(query.JobType)) source = source.Where(x => x.job.JobType == query.JobType.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.run.Status == query.Status.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.TriggeredBy)) source = source.Where(x => x.run.TriggeredBy == query.TriggeredBy.Trim());
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.job.JobName, search) ||
                EF.Functions.ILike(x.job.ScopeRef, search) ||
                EF.Functions.ILike(x.run.TriggeredBy, search) ||
                (x.run.ErrorSummary != null && EF.Functions.ILike(x.run.ErrorSummary, search)));
        }

        var descending = !string.Equals(query.SortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "jobname" => descending ? source.OrderByDescending(x => x.job.JobName) : source.OrderBy(x => x.job.JobName),
            "status" => descending ? source.OrderByDescending(x => x.run.Status) : source.OrderBy(x => x.run.Status),
            _ => descending ? source.OrderByDescending(x => x.run.QueuedAt) : source.OrderBy(x => x.run.QueuedAt)
        };

        var page = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);
        var total = await source.CountAsync(cancellationToken);
        var runs = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var runIds = runs.Select(x => x.run.Id).ToArray();
        var evidenceRefs = await dbContext.AutomationJobEvidenceRefs.AsNoTracking()
            .Where(x => runIds.Contains(x.JobRunId))
            .OrderBy(x => x.CreatedAt)
            .Select(x => new AutomationJobEvidenceRefResponse(x.Id, x.JobRunId, x.EntityType, x.EntityId, x.Route, x.EvidenceRef, x.CreatedAt))
            .ToListAsync(cancellationToken);
        var evidenceLookup = evidenceRefs.GroupBy(x => x.JobRunId).ToDictionary(x => x.Key, x => (IReadOnlyList<AutomationJobEvidenceRefResponse>)x.ToList());

        var items = runs.Select(item => new AutomationJobRunResponse(
            item.run.Id,
            item.run.JobId,
            item.job.JobName,
            item.job.JobType,
            item.run.Status,
            item.run.TriggeredBy,
            item.run.TriggerReason,
            item.run.QueuedAt,
            item.run.StartedAt,
            item.run.CompletedAt,
            item.run.ErrorSummary,
            item.run.RemediationPath,
            evidenceLookup.GetValueOrDefault(item.run.Id, []),
            item.run.CreatedAt)).ToList();

        return new PagedResult<AutomationJobRunResponse>(items, total, page, pageSize);
    }

    private static IQueryable<T> ApplyOrdering<T, TDate, TString>(IQueryable<T> source, string? sortBy, string? sortOrder, Expression<Func<T, TDate>> dateSelector, Expression<Func<T, TString>> stringSelector)
    {
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "scoperef" => descending ? source.OrderByDescending(stringSelector) : source.OrderBy(stringSelector),
            _ => descending ? source.OrderByDescending(dateSelector) : source.OrderBy(dateSelector)
        };
    }

    private static async Task<PagedResult<T>> PageAsync<T>(IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var normalizedPage = NormalizePage(page);
        var normalizedPageSize = NormalizePageSize(pageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((normalizedPage - 1) * normalizedPageSize).Take(normalizedPageSize).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, total, normalizedPage, normalizedPageSize);
    }

    private static int NormalizePage(int page) => page <= 0 ? 1 : page;
    private static int NormalizePageSize(int pageSize) => pageSize <= 0 ? 25 : Math.Min(pageSize, 100);

    private static IReadOnlyList<string> DeserializeSubjects(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json)?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList()
                ?? [];
        }
        catch
        {
            return [];
        }
    }
}
