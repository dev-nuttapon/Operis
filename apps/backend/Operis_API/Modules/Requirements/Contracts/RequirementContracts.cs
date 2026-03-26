using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Requirements.Contracts;

public sealed record RequirementListItem(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    string Priority,
    string OwnerUserId,
    string Status,
    string? BaselineStatus,
    int MissingLinkCount,
    Guid? CurrentVersionId,
    int? CurrentVersionNumber,
    DateTimeOffset UpdatedAt);

public sealed record RequirementVersionItem(
    Guid Id,
    Guid RequirementId,
    int VersionNumber,
    string BusinessReason,
    string AcceptanceCriteria,
    string? SecurityImpact,
    string? PerformanceImpact,
    string Status,
    DateTimeOffset CreatedAt);

public sealed record RequirementHistoryItem(
    Guid Id,
    string EventType,
    string? Summary,
    string? Reason,
    string? ActorUserId,
    DateTimeOffset OccurredAt);

public sealed record TraceabilityLinkItem(
    Guid Id,
    string SourceType,
    string SourceId,
    string TargetType,
    string TargetId,
    string LinkRule,
    string Status,
    string CreatedBy,
    DateTimeOffset CreatedAt);

public sealed record RequirementDetailResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    string Description,
    string Priority,
    string OwnerUserId,
    string Status,
    Guid? CurrentVersionId,
    IReadOnlyList<RequirementVersionItem> Versions,
    IReadOnlyList<TraceabilityLinkItem> TraceabilityLinks,
    IReadOnlyList<RequirementHistoryItem> History,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record RequirementBaselineItem(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string BaselineName,
    IReadOnlyList<Guid> RequirementIds,
    string Status,
    string ApprovedBy,
    DateTimeOffset ApprovedAt);

public sealed record TraceabilityMatrixRow(
    Guid RequirementId,
    string RequirementCode,
    string RequirementTitle,
    Guid ProjectId,
    string ProjectName,
    string RequirementStatus,
    string? BaselineStatus,
    int MissingLinkCount,
    IReadOnlyList<TraceabilityLinkItem> Links);

public sealed record CreateRequirementRequest(
    Guid ProjectId,
    string Code,
    string Title,
    string Description,
    string Priority,
    string OwnerUserId,
    string BusinessReason,
    string AcceptanceCriteria,
    string? SecurityImpact,
    string? PerformanceImpact);

public sealed record UpdateRequirementRequest(
    string Title,
    string Description,
    string Priority,
    string OwnerUserId,
    string BusinessReason,
    string AcceptanceCriteria,
    string? SecurityImpact,
    string? PerformanceImpact);

public sealed record RequirementDecisionRequest(string Reason);

public sealed record CreateRequirementBaselineRequest(
    Guid ProjectId,
    string BaselineName,
    IReadOnlyList<Guid> RequirementIds,
    string Reason);

public sealed record CreateTraceabilityLinkRequest(
    string SourceType,
    string SourceId,
    string TargetType,
    string TargetId,
    string LinkRule);

public sealed record RequirementListQuery(
    string? Search,
    Guid? ProjectId,
    string? Priority,
    string? Status,
    string? OwnerUserId,
    string? BaselineStatus,
    bool? MissingDownstreamLinks,
    int Page = 1,
    int PageSize = 10);

public sealed record RequirementBaselineListQuery(
    Guid? ProjectId,
    string? Status,
    int Page = 1,
    int PageSize = 10);

public sealed record TraceabilityListQuery(
    Guid? ProjectId,
    string? BaselineStatus,
    bool? MissingCoverage,
    int Page = 1,
    int PageSize = 10);
