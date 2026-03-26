using Operis_API.Modules.ChangeControl.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.ChangeControl.Application;

public interface IChangeControlQueries
{
    Task<PagedResult<ChangeRequestListItemResponse>> ListChangeRequestsAsync(ChangeControlListQuery query, CancellationToken cancellationToken);
    Task<ChangeRequestResponse?> GetChangeRequestAsync(Guid changeRequestId, CancellationToken cancellationToken);
    Task<PagedResult<ConfigurationItemListItemResponse>> ListConfigurationItemsAsync(ChangeControlListQuery query, CancellationToken cancellationToken);
    Task<ConfigurationItemResponse?> GetConfigurationItemAsync(Guid configurationItemId, CancellationToken cancellationToken);
    Task<PagedResult<BaselineRegistryListItemResponse>> ListBaselineRegistryAsync(ChangeControlListQuery query, CancellationToken cancellationToken);
    Task<BaselineRegistryResponse?> GetBaselineRegistryAsync(Guid baselineRegistryId, CancellationToken cancellationToken);
}
