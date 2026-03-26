using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public sealed class MasterDataCatalogQueries(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter) : IMasterDataCatalogQueries
{
    public async Task<PagedResult<MasterDataCatalogResponse>> ListAsync(MasterDataCatalogListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = NormalizePaging(query.Page, query.PageSize);
        var source = dbContext.MasterDataItems.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Domain))
        {
            source = source.Where(x => x.Domain == query.Domain);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(x => x.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            source = source.Where(x => EF.Functions.ILike(x.Domain, search) || EF.Functions.ILike(x.Code, search) || EF.Functions.ILike(x.Name, search));
        }

        var descending = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        source = (query.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "domain" => descending ? source.OrderByDescending(x => x.Domain).ThenByDescending(x => x.DisplayOrder).ThenByDescending(x => x.Name) : source.OrderBy(x => x.Domain).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Name),
            "status" => descending ? source.OrderByDescending(x => x.Status).ThenByDescending(x => x.DisplayOrder).ThenByDescending(x => x.Name) : source.OrderBy(x => x.Status).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Name),
            _ => descending ? source.OrderByDescending(x => x.DisplayOrder).ThenByDescending(x => x.Name) : source.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
        };

        var total = await source.CountAsync(cancellationToken);
        var items = await source.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        var responses = await BuildResponsesAsync(items, cancellationToken);

        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "list", EntityType: "master_data_item", StatusCode: StatusCodes.Status200OK, Metadata: new { total, page, pageSize, query.Domain, query.Status, query.Search }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PagedResult<MasterDataCatalogResponse>(responses, total, page, pageSize);
    }

    public async Task<MasterDataCatalogResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await dbContext.MasterDataItems.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        auditLogWriter.Append(new AuditLogEntry(Module: "users", Action: "get", EntityType: "master_data_item", EntityId: id.ToString(), StatusCode: item is null ? StatusCodes.Status404NotFound : StatusCodes.Status200OK));
        await dbContext.SaveChangesAsync(cancellationToken);
        if (item is null)
        {
            return null;
        }

        return (await BuildResponsesAsync([item], cancellationToken)).Single();
    }

    private async Task<IReadOnlyList<MasterDataCatalogResponse>> BuildResponsesAsync(IReadOnlyList<Infrastructure.MasterDataItemEntity> items, CancellationToken cancellationToken)
    {
        var itemIds = items.Select(x => x.Id).ToArray();
        var changes = await dbContext.MasterDataChanges.AsNoTracking()
            .Where(x => itemIds.Contains(x.MasterDataItemId))
            .OrderByDescending(x => x.ChangedAt)
            .ToListAsync(cancellationToken);

        return items.Select(item =>
        {
            var itemChanges = changes.Where(change => change.MasterDataItemId == item.Id)
                .Select(change => new MasterDataChangeResponse(change.Id, change.ChangeType, change.ChangedBy, change.ChangedAt, change.Reason))
                .ToList();
            var lastChange = itemChanges.FirstOrDefault();
            return new MasterDataCatalogResponse(
                item.Id,
                item.Domain,
                item.Code,
                item.Name,
                item.Status,
                item.DisplayOrder,
                lastChange?.ChangedBy,
                lastChange?.ChangedAt,
                itemChanges);
        }).ToList();
    }

    private static (int Page, int PageSize, int Skip) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);
        return (normalizedPage, normalizedPageSize, (normalizedPage - 1) * normalizedPageSize);
    }
}
