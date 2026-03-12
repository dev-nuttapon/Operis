using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public sealed class UserReferenceDataQueries(
    OperisDbContext dbContext,
    IReferenceDataCache referenceDataCache) : IUserReferenceDataQueries
{
    public async Task<IReadOnlyList<AppRoleResponse>> ListRolesAsync(CancellationToken cancellationToken)
    {
        return (await referenceDataCache.GetAppRolesAsync(dbContext, cancellationToken))
            .Select(x => new AppRoleResponse(x.Id, x.Name, x.KeycloakRoleName, x.Description, x.DisplayOrder))
            .ToList();
    }

    public async Task<PagedResult<MasterDataResponse>> ListDepartmentsAsync(ReferenceDataQuery query, CancellationToken cancellationToken)
    {
        var (normalizedPage, normalizedPageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var items = (await referenceDataCache.GetDepartmentsAsync(dbContext, cancellationToken))
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var normalizedSearch = query.Search.Trim().ToLowerInvariant();
            items = items.Where(x => x.Name.ToLowerInvariant().Contains(normalizedSearch));
        }

        items = ApplyMasterDataSorting(items, query.SortBy, query.SortOrder);
        var total = items.Count();
        var pagedItems = items
            .Skip(skip)
            .Take(normalizedPageSize)
            .Select(x => new MasterDataResponse(x.Id, x.Name, x.DisplayOrder, x.CreatedAt, x.UpdatedAt, x.DeletedReason, x.DeletedBy, x.DeletedAt))
            .ToList();

        return new PagedResult<MasterDataResponse>(pagedItems, total, normalizedPage, normalizedPageSize);
    }

    public async Task<PagedResult<MasterDataResponse>> ListJobTitlesAsync(ReferenceDataQuery query, CancellationToken cancellationToken)
    {
        var (normalizedPage, normalizedPageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var items = (await referenceDataCache.GetJobTitlesAsync(dbContext, cancellationToken))
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var normalizedSearch = query.Search.Trim().ToLowerInvariant();
            items = items.Where(x => x.Name.ToLowerInvariant().Contains(normalizedSearch));
        }

        items = ApplyMasterDataSorting(items, query.SortBy, query.SortOrder);
        var total = items.Count();
        var pagedItems = items
            .Skip(skip)
            .Take(normalizedPageSize)
            .Select(x => new MasterDataResponse(x.Id, x.Name, x.DisplayOrder, x.CreatedAt, x.UpdatedAt, x.DeletedReason, x.DeletedBy, x.DeletedAt))
            .ToList();

        return new PagedResult<MasterDataResponse>(pagedItems, total, normalizedPage, normalizedPageSize);
    }

    private static IEnumerable<TItem> ApplyMasterDataSorting<TItem>(
        IEnumerable<TItem> items,
        string? sortBy,
        string? sortOrder)
        where TItem : class
    {
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "name" => descending
                ? items.OrderByDescending(GetName).ThenByDescending(GetDisplayOrder)
                : items.OrderBy(GetName).ThenBy(GetDisplayOrder),
            "displayorder" => descending
                ? items.OrderByDescending(GetDisplayOrder).ThenByDescending(GetName)
                : items.OrderBy(GetDisplayOrder).ThenBy(GetName),
            _ => items.OrderBy(GetDisplayOrder).ThenBy(GetName)
        };

        static string GetName(TItem item) =>
            item switch
            {
                CachedDepartmentItem department => department.Name,
                CachedJobTitleItem jobTitle => jobTitle.Name,
                _ => string.Empty
            };

        static int GetDisplayOrder(TItem item) =>
            item switch
            {
                CachedDepartmentItem department => department.DisplayOrder,
                CachedJobTitleItem jobTitle => jobTitle.DisplayOrder,
                _ => 0
            };
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);
        var skip = (normalizedPage - 1) * normalizedPageSize;
        return (normalizedPage, normalizedPageSize, skip);
    }
}
