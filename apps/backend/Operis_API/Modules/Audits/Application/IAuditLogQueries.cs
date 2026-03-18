using Operis_API.Modules.Audits.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Audits.Application;

public interface IAuditLogQueries
{
    Task<PagedResult<AuditLogListItem>> ListAuditLogsAsync(AuditLogListQuery query, CancellationToken cancellationToken);
    Task<AuditLogResponse?> GetAuditLogAsync(Guid auditLogId, CancellationToken cancellationToken);
}
