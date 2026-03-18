using Operis_API.Modules.Audits.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Audits.Application;

public interface IBusinessAuditEventQueries
{
    Task<PagedResult<BusinessAuditEventItem>> ListAsync(BusinessAuditEventListQuery query, CancellationToken cancellationToken);
}
