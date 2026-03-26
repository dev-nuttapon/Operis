using Operis_API.Modules.Audits.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Audits.Application;

public interface IAuditComplianceQueries
{
    Task<PagedResult<AuditEventItem>> ListAuditEventsAsync(AuditEventListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<AuditPlanListItem>> ListAuditPlansAsync(AuditPlanListQuery query, CancellationToken cancellationToken);
    Task<AuditPlanDetailResponse?> GetAuditPlanAsync(Guid auditPlanId, CancellationToken cancellationToken);
    Task<PagedResult<EvidenceExportItem>> ListEvidenceExportsAsync(EvidenceExportListQuery query, CancellationToken cancellationToken);
    Task<EvidenceExportDetailResponse?> GetEvidenceExportAsync(Guid exportId, CancellationToken cancellationToken);
}

