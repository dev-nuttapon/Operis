using Operis_API.Modules.Audits.Contracts;

namespace Operis_API.Modules.Audits.Application;

public interface IAuditComplianceCommands
{
    Task<AuditComplianceCommandResult<AuditPlanDetailResponse>> CreateAuditPlanAsync(CreateAuditPlanRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<AuditComplianceCommandResult<AuditPlanDetailResponse>> UpdateAuditPlanAsync(Guid auditPlanId, UpdateAuditPlanRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<AuditComplianceCommandResult<AuditFindingItem>> CreateAuditFindingAsync(CreateAuditFindingRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<AuditComplianceCommandResult<AuditFindingItem>> UpdateAuditFindingAsync(Guid auditFindingId, UpdateAuditFindingRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<AuditComplianceCommandResult<AuditFindingItem>> CloseAuditFindingAsync(Guid auditFindingId, CloseAuditFindingRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<AuditComplianceCommandResult<EvidenceExportDetailResponse>> CreateEvidenceExportAsync(CreateEvidenceExportRequest request, string? actorUserId, CancellationToken cancellationToken);
}

