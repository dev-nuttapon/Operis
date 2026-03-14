using Operis_API.Modules.Activities.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Activities.Application;

public interface IActivityLogQueries
{
    Task<PagedResult<ActivityLogResponse>> ListActivityLogsAsync(ActivityLogListQuery request, CancellationToken cancellationToken);
}
