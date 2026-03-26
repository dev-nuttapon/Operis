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
        return await PageAsync(source.Select(x => new AccessReviewResponse(x.Id, x.ScopeType, x.ScopeRef, x.ReviewCycle, x.ReviewedBy, x.Status, x.Decision, x.DecisionRationale, x.CreatedAt, x.UpdatedAt)), query.Page, query.PageSize, cancellationToken);
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
        return await PageAsync(source.Select(x => new SecurityReviewResponse(x.Id, x.ScopeType, x.ScopeRef, x.ControlsReviewed, x.FindingsSummary, x.Status, x.CreatedAt, x.UpdatedAt)), query.Page, query.PageSize, cancellationToken);
    }

    public async Task<PagedResult<ExternalDependencyResponse>> ListExternalDependenciesAsync(ExternalDependencyListQuery query, CancellationToken cancellationToken)
    {
        var source = dbContext.ExternalDependencies.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.DependencyType)) source = source.Where(x => x.DependencyType == query.DependencyType);
        if (!string.IsNullOrWhiteSpace(query.Criticality)) source = source.Where(x => x.Criticality == query.Criticality);
        if (!string.IsNullOrWhiteSpace(query.Status)) source = source.Where(x => x.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.Name, search) || EF.Functions.ILike(x.OwnerUserId, search));
        }

        var descending = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "reviewdueat" => descending ? source.OrderByDescending(x => x.ReviewDueAt) : source.OrderBy(x => x.ReviewDueAt),
            _ => descending ? source.OrderByDescending(x => x.CreatedAt) : source.OrderBy(x => x.CreatedAt)
        };

        return await PageAsync(source.Select(x => new ExternalDependencyResponse(x.Id, x.Name, x.DependencyType, x.OwnerUserId, x.Criticality, x.Status, x.ReviewDueAt, x.CreatedAt, x.UpdatedAt)), query.Page, query.PageSize, cancellationToken);
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
        return await PageAsync(source.Select(x => new ConfigurationAuditResponse(x.Id, x.ScopeRef, x.PlannedAt, x.Status, x.FindingCount, x.CreatedAt, x.UpdatedAt)), query.Page, query.PageSize, cancellationToken);
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
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((normalizedPage - 1) * normalizedPageSize).Take(normalizedPageSize).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, total, normalizedPage, normalizedPageSize);
    }
}
