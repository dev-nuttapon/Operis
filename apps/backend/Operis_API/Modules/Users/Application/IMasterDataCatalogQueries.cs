using Operis_API.Modules.Users.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Users.Application;

public sealed record MasterDataCatalogListQuery(
    string? Domain,
    string? Status,
    string? Search,
    string? SortBy,
    string? SortOrder,
    int Page = 1,
    int PageSize = 10);

public interface IMasterDataCatalogQueries
{
    Task<PagedResult<MasterDataCatalogResponse>> ListAsync(MasterDataCatalogListQuery query, CancellationToken cancellationToken);
    Task<MasterDataCatalogResponse?> GetAsync(Guid id, CancellationToken cancellationToken);
}
