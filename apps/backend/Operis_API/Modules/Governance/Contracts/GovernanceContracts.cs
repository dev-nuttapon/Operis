using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Governance.Contracts;

public sealed record GovernanceListQuery(
    string? Search,
    string? Status,
    string? OwnerUserId,
    Guid? ProjectId,
    int Page = 1,
    int PageSize = 10);

public sealed record ProcessAssetListItemResponse(
    Guid Id,
    string Code,
    string Name,
    string Category,
    string Status,
    string OwnerUserId,
    ProcessAssetVersionSummaryResponse? CurrentVersion,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    DateTimeOffset UpdatedAt);

public sealed record ProcessAssetVersionSummaryResponse(
    Guid Id,
    int VersionNumber,
    string Title,
    string Status,
    string? ChangeSummary,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset UpdatedAt);

public sealed record ProcessAssetResponse(
    Guid Id,
    string Code,
    string Name,
    string Category,
    string Status,
    string OwnerUserId,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    Guid? CurrentVersionId,
    IReadOnlyList<ProcessAssetVersionDetailResponse> Versions,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ProcessAssetVersionDetailResponse(
    Guid Id,
    int VersionNumber,
    string Title,
    string Summary,
    string? ContentRef,
    string Status,
    string? ChangeSummary,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateProcessAssetRequest(
    string Code,
    string Name,
    string Category,
    string OwnerUserId,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string InitialVersionTitle,
    string InitialVersionSummary,
    string? InitialContentRef);

public sealed record UpdateProcessAssetRequest(
    string Code,
    string Name,
    string Category,
    string OwnerUserId,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? EffectiveTo);

public sealed record CreateProcessAssetVersionRequest(
    string Title,
    string Summary,
    string? ContentRef,
    string? ChangeSummary);

public sealed record UpdateProcessAssetVersionRequest(
    string Title,
    string Summary,
    string? ContentRef,
    string? ChangeSummary);

public sealed record ProcessAssetApprovalRequest(string ChangeSummary);

public sealed record QaChecklistItemRequest(
    string ItemText,
    bool Mandatory,
    string ApplicablePhase,
    string EvidenceRule);

public sealed record QaChecklistItemResponse(
    string ItemText,
    bool Mandatory,
    string ApplicablePhase,
    string EvidenceRule);

public sealed record QaChecklistResponse(
    Guid Id,
    string Code,
    string Name,
    string Scope,
    string Status,
    string OwnerUserId,
    IReadOnlyList<QaChecklistItemResponse> Items,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateQaChecklistRequest(
    string Code,
    string Name,
    string Scope,
    string OwnerUserId,
    IReadOnlyList<QaChecklistItemRequest> Items);

public sealed record UpdateQaChecklistRequest(
    string Code,
    string Name,
    string Scope,
    string OwnerUserId,
    IReadOnlyList<QaChecklistItemRequest> Items);

public sealed record QaChecklistListItemResponse(
    Guid Id,
    string Code,
    string Name,
    string Scope,
    string Status,
    string OwnerUserId,
    DateTimeOffset UpdatedAt);

public sealed record ProjectPlanResponse(
    Guid Id,
    Guid ProjectId,
    string Name,
    string ScopeSummary,
    string LifecycleModel,
    DateOnly StartDate,
    DateOnly TargetEndDate,
    string OwnerUserId,
    string Status,
    IReadOnlyList<string> Milestones,
    IReadOnlyList<string> Roles,
    string RiskApproach,
    string QualityApproach,
    string? ApprovalReason,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ProjectPlanListItemResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Name,
    string LifecycleModel,
    string Status,
    string OwnerUserId,
    DateOnly StartDate,
    DateOnly TargetEndDate,
    DateTimeOffset UpdatedAt);

public sealed record CreateProjectPlanRequest(
    Guid ProjectId,
    string Name,
    string ScopeSummary,
    string LifecycleModel,
    DateOnly StartDate,
    DateOnly TargetEndDate,
    string OwnerUserId,
    IReadOnlyList<string> Milestones,
    IReadOnlyList<string> Roles,
    string RiskApproach,
    string QualityApproach);

public sealed record UpdateProjectPlanRequest(
    string Name,
    string ScopeSummary,
    string LifecycleModel,
    DateOnly StartDate,
    DateOnly TargetEndDate,
    string OwnerUserId,
    IReadOnlyList<string> Milestones,
    IReadOnlyList<string> Roles,
    string RiskApproach,
    string QualityApproach);

public sealed record ProjectPlanApprovalRequest(string Reason);

public sealed record StakeholderResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Name,
    string RoleName,
    string InfluenceLevel,
    string ContactChannel,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateStakeholderRequest(
    Guid ProjectId,
    string Name,
    string RoleName,
    string InfluenceLevel,
    string ContactChannel);

public sealed record UpdateStakeholderRequest(
    string Name,
    string RoleName,
    string InfluenceLevel,
    string ContactChannel,
    string Status);

public sealed record TailoringRecordResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string RequesterUserId,
    string RequestedChange,
    string Reason,
    string ImpactSummary,
    string Status,
    string? ApproverUserId,
    DateTimeOffset? ApprovedAt,
    Guid? ImpactedProcessAssetId,
    string? ApprovalRationale,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TailoringRecordListItemResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string RequestedChange,
    string Status,
    string RequesterUserId,
    string? ApproverUserId,
    DateTimeOffset UpdatedAt);

public sealed record CreateTailoringRecordRequest(
    Guid ProjectId,
    string RequesterUserId,
    string RequestedChange,
    string Reason,
    string ImpactSummary,
    Guid? ImpactedProcessAssetId);

public sealed record UpdateTailoringRecordRequest(
    string RequestedChange,
    string Reason,
    string ImpactSummary,
    Guid? ImpactedProcessAssetId);

public sealed record TailoringDecisionRequest(string Decision, string Reason);

public sealed record GovernanceMutationResponse(Guid Id, string Status, DateTimeOffset UpdatedAt, string? ApprovedBy = null, DateTimeOffset? ApprovedAt = null);
