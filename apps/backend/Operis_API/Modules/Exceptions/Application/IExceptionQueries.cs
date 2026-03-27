using Operis_API.Modules.Exceptions.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Exceptions.Application;

public interface IExceptionQueries
{
    Task<PagedResult<WaiverListItemResponse>> ListWaiversAsync(WaiverListQuery query, CancellationToken cancellationToken);
    Task<WaiverDetailResponse?> GetWaiverAsync(Guid waiverId, CancellationToken cancellationToken);
}
