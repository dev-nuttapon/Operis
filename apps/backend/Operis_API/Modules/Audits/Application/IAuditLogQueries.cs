using Operis_API.Modules.Audits.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Audits.Application;

public interface IAuditLogQueries
{
    Task<PagedResult<AuditLogResponse>> ListAuditLogsAsync(AuditLogListQuery query, CancellationToken cancellationToken);
}
