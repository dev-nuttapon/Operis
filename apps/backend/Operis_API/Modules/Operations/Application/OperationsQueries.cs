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
