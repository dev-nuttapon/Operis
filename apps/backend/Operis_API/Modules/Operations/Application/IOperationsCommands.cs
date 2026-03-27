using Operis_API.Modules.Operations.Contracts;

namespace Operis_API.Modules.Operations.Application;

public interface IOperationsCommands
{
    Task<OperationsCommandResult<AccessReviewResponse>> CreateAccessReviewAsync(CreateAccessReviewRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<AccessReviewResponse>> UpdateAccessReviewAsync(Guid id, UpdateAccessReviewRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<AccessReviewResponse>> ApproveAccessReviewAsync(Guid id, ApproveAccessReviewRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<SecurityReviewResponse>> CreateSecurityReviewAsync(CreateSecurityReviewRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<SecurityReviewResponse>> UpdateSecurityReviewAsync(Guid id, UpdateSecurityReviewRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<ExternalDependencyResponse>> CreateExternalDependencyAsync(CreateExternalDependencyRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<ExternalDependencyResponse>> UpdateExternalDependencyAsync(Guid id, UpdateExternalDependencyRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<ConfigurationAuditResponse>> CreateConfigurationAuditAsync(CreateConfigurationAuditRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<SupplierResponse>> CreateSupplierAsync(CreateSupplierRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<SupplierResponse>> UpdateSupplierAsync(Guid id, UpdateSupplierRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<SupplierAgreementResponse>> CreateSupplierAgreementAsync(CreateSupplierAgreementRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<SupplierAgreementResponse>> UpdateSupplierAgreementAsync(Guid id, UpdateSupplierAgreementRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<AccessRecertificationResponse>> CreateAccessRecertificationAsync(CreateAccessRecertificationRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<AccessRecertificationResponse>> UpdateAccessRecertificationAsync(Guid id, UpdateAccessRecertificationRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<AccessRecertificationDecisionResponse>> AddAccessRecertificationDecisionAsync(Guid id, AddAccessRecertificationDecisionRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<AccessRecertificationResponse>> CompleteAccessRecertificationAsync(Guid id, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<SecurityIncidentResponse>> CreateSecurityIncidentAsync(CreateSecurityIncidentRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<SecurityIncidentResponse>> UpdateSecurityIncidentAsync(Guid id, UpdateSecurityIncidentRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<VulnerabilityResponse>> CreateVulnerabilityAsync(CreateVulnerabilityRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<VulnerabilityResponse>> UpdateVulnerabilityAsync(Guid id, UpdateVulnerabilityRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<SecretRotationResponse>> CreateSecretRotationAsync(CreateSecretRotationRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<SecretRotationResponse>> UpdateSecretRotationAsync(Guid id, UpdateSecretRotationRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<PrivilegedAccessEventResponse>> CreatePrivilegedAccessEventAsync(CreatePrivilegedAccessEventRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<PrivilegedAccessEventResponse>> UpdatePrivilegedAccessEventAsync(Guid id, UpdatePrivilegedAccessEventRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<ClassificationPolicyResponse>> CreateClassificationPolicyAsync(CreateClassificationPolicyRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<ClassificationPolicyResponse>> UpdateClassificationPolicyAsync(Guid id, UpdateClassificationPolicyRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<BackupEvidenceResponse>> CreateBackupEvidenceAsync(CreateBackupEvidenceRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<RestoreVerificationResponse>> CreateRestoreVerificationAsync(CreateRestoreVerificationRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<DrDrillResponse>> CreateDrDrillAsync(CreateDrDrillRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<DrDrillResponse>> UpdateDrDrillAsync(Guid id, UpdateDrDrillRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<LegalHoldResponse>> CreateLegalHoldAsync(CreateLegalHoldRequest request, string? actor, CancellationToken cancellationToken);
    Task<OperationsCommandResult<LegalHoldResponse>> ReleaseLegalHoldAsync(Guid id, ReleaseLegalHoldRequest request, string? actor, CancellationToken cancellationToken);
}
