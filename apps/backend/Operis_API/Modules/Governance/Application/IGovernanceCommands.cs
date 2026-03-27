using Operis_API.Modules.Governance.Contracts;

namespace Operis_API.Modules.Governance.Application;

public interface IGovernanceCommands
{
    Task<GovernanceCommandResult<ProcessAssetResponse>> CreateProcessAssetAsync(CreateProcessAssetRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<ProcessAssetResponse>> UpdateProcessAssetAsync(Guid processAssetId, UpdateProcessAssetRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<ProcessAssetResponse>> CreateProcessAssetVersionAsync(Guid processAssetId, CreateProcessAssetVersionRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<ProcessAssetResponse>> UpdateProcessAssetVersionAsync(Guid processAssetId, Guid versionId, UpdateProcessAssetVersionRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> SubmitProcessAssetVersionReviewAsync(Guid processAssetId, Guid versionId, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> ApproveProcessAssetVersionAsync(Guid processAssetId, Guid versionId, string actor, ProcessAssetApprovalRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> ActivateProcessAssetVersionAsync(Guid processAssetId, Guid versionId, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> DeprecateProcessAssetAsync(Guid processAssetId, CancellationToken cancellationToken);

    Task<GovernanceCommandResult<QaChecklistResponse>> CreateQaChecklistAsync(CreateQaChecklistRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<QaChecklistResponse>> UpdateQaChecklistAsync(Guid qaChecklistId, UpdateQaChecklistRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> ApproveQaChecklistAsync(Guid qaChecklistId, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> ActivateQaChecklistAsync(Guid qaChecklistId, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> DeprecateQaChecklistAsync(Guid qaChecklistId, CancellationToken cancellationToken);

    Task<GovernanceCommandResult<ProjectPlanResponse>> CreateProjectPlanAsync(CreateProjectPlanRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<ProjectPlanResponse>> UpdateProjectPlanAsync(Guid projectPlanId, UpdateProjectPlanRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> SubmitProjectPlanReviewAsync(Guid projectPlanId, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> ApproveProjectPlanAsync(Guid projectPlanId, string actor, ProjectPlanApprovalRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> BaselineProjectPlanAsync(Guid projectPlanId, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> SupersedeProjectPlanAsync(Guid projectPlanId, string actor, ProjectPlanApprovalRequest request, CancellationToken cancellationToken);

    Task<GovernanceCommandResult<StakeholderResponse>> CreateStakeholderAsync(CreateStakeholderRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<StakeholderResponse>> UpdateStakeholderAsync(Guid stakeholderId, UpdateStakeholderRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> ArchiveStakeholderAsync(Guid stakeholderId, CancellationToken cancellationToken);

    Task<GovernanceCommandResult<TailoringRecordResponse>> CreateTailoringRecordAsync(CreateTailoringRecordRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<TailoringRecordResponse>> UpdateTailoringRecordAsync(Guid tailoringRecordId, UpdateTailoringRecordRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> SubmitTailoringRecordAsync(Guid tailoringRecordId, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> ApproveTailoringRecordAsync(Guid tailoringRecordId, string actor, TailoringDecisionRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> ApplyTailoringRecordAsync(Guid tailoringRecordId, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> ArchiveTailoringRecordAsync(Guid tailoringRecordId, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<TailoringCriteriaResponse>> CreateTailoringCriteriaAsync(TailoringCriteriaRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<TailoringCriteriaResponse>> UpdateTailoringCriteriaAsync(Guid tailoringCriteriaId, TailoringCriteriaRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<TailoringReviewCycleResponse>> CreateTailoringReviewCycleAsync(CreateTailoringReviewCycleRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<TailoringReviewCycleResponse>> UpdateTailoringReviewCycleAsync(Guid tailoringReviewCycleId, UpdateTailoringReviewCycleRequest request, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<GovernanceMutationResponse>> TransitionTailoringReviewCycleAsync(Guid tailoringReviewCycleId, string actor, TransitionTailoringReviewCycleRequest request, CancellationToken cancellationToken);
}
