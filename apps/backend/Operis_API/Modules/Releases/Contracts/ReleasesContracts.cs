namespace Operis_API.Modules.Releases.Contracts;

public sealed record ReleaseListQuery(Guid? ProjectId, string? Status, string? Search, int? Page, int? PageSize);
public sealed record DeploymentChecklistListQuery(Guid? ReleaseId, string? Status, int? Page, int? PageSize);
public sealed record ReleaseNoteListQuery(Guid? ReleaseId, string? Status, int? Page, int? PageSize);

public sealed record ReleaseListItem(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string ReleaseCode,
    string Title,
    DateTimeOffset? PlannedAt,
    DateTimeOffset? ReleasedAt,
    string Status,
    string? LatestQualityGateResult,
    int ChecklistCompleted,
    int ChecklistTotal,
    DateTimeOffset UpdatedAt);

public sealed record DeploymentChecklistItem(
    Guid Id,
    Guid ReleaseId,
    string ReleaseCode,
    string ChecklistItem,
    string OwnerUserId,
    string Status,
    DateTimeOffset? CompletedAt,
    string? EvidenceRef,
    DateTimeOffset UpdatedAt);

public sealed record ReleaseNoteItem(
    Guid Id,
    Guid ReleaseId,
    string ReleaseCode,
    string Summary,
    string IncludedChanges,
    string? KnownIssues,
    string Status,
    DateTimeOffset? PublishedAt,
    DateTimeOffset UpdatedAt);

public sealed record ReleaseDetailResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string ReleaseCode,
    string Title,
    DateTimeOffset? PlannedAt,
    DateTimeOffset? ReleasedAt,
    string Status,
    string? QualityGateResult,
    string? QualityGateOverrideReason,
    string? ApprovedByUserId,
    DateTimeOffset? ApprovedAt,
    IReadOnlyList<DeploymentChecklistItem> ChecklistItems,
    IReadOnlyList<ReleaseNoteItem> Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateReleaseRequest(Guid ProjectId, string ReleaseCode, string Title, DateTimeOffset? PlannedAt);
public sealed record UpdateReleaseRequest(string Title, DateTimeOffset? PlannedAt);
public sealed record ApproveReleaseRequest(string? Reason);
public sealed record ExecuteReleaseRequest(string? OverrideReason);
public sealed record CreateDeploymentChecklistRequest(Guid ReleaseId, string ChecklistItem, string OwnerUserId, string Status, string? EvidenceRef);
public sealed record UpdateDeploymentChecklistRequest(string ChecklistItem, string OwnerUserId, string Status, DateTimeOffset? CompletedAt, string? EvidenceRef);
public sealed record CreateReleaseNoteRequest(Guid ReleaseId, string Summary, string IncludedChanges, string? KnownIssues);

public sealed record ReleaseCommandResponse(Guid Id, string ReleaseCode, string Status);
public sealed record ReleaseNotePublishResponse(Guid Id, Guid ReleaseId, string Status, DateTimeOffset? PublishedAt);
