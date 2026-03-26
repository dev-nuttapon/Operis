using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.ChangeControl.Contracts;

public sealed record ChangeControlListQuery(
    string? Search,
    string? Status,
    string? Priority,
    Guid? ProjectId,
    int Page = 1,
    int PageSize = 10);

public sealed record ChangeImpactRequest(
    string ScopeImpact,
    string ScheduleImpact,
    string QualityImpact,
    string SecurityImpact,
    string PerformanceImpact,
    string RiskImpact);

public sealed record ChangeImpactResponse(
    Guid Id,
    Guid ChangeRequestId,
    string ScopeImpact,
    string ScheduleImpact,
    string QualityImpact,
    string SecurityImpact,
    string PerformanceImpact,
    string RiskImpact);

public sealed record CreateChangeRequestRequest(
    Guid ProjectId,
    string Code,
    string Title,
    string RequestedBy,
    string Reason,
    string Priority,
    Guid? TargetBaselineId,
    ChangeImpactRequest Impact,
    IReadOnlyList<Guid>? LinkedRequirementIds,
    IReadOnlyList<Guid>? LinkedConfigurationItemIds);

public sealed record UpdateChangeRequestRequest(
    string Title,
    string RequestedBy,
    string Reason,
    string Priority,
    Guid? TargetBaselineId,
    ChangeImpactRequest Impact,
    IReadOnlyList<Guid>? LinkedRequirementIds,
    IReadOnlyList<Guid>? LinkedConfigurationItemIds);

public sealed record ChangeDecisionRequest(string Reason);
public sealed record ChangeImplementationRequest(string Summary);

public sealed record ChangeRequestListItemResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    string Priority,
    string RequestedBy,
    string Status,
    string? TargetBaselineName,
    string? ApprovalStatus,
    DateTimeOffset UpdatedAt);

public sealed record ChangeRequestResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    string RequestedBy,
    string Reason,
    string Status,
    string Priority,
    Guid? TargetBaselineId,
    string? TargetBaselineName,
    IReadOnlyList<Guid> LinkedRequirementIds,
    IReadOnlyList<Guid> LinkedConfigurationItemIds,
    ChangeImpactResponse Impact,
    string? DecisionRationale,
    string? ImplementationSummary,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    IReadOnlyList<ChangeHistoryItem> History,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ChangeHistoryItem(
    Guid Id,
    string EventType,
    string? Summary,
    string? Reason,
    string? ActorUserId,
    DateTimeOffset OccurredAt);

public sealed record CreateConfigurationItemRequest(
    Guid ProjectId,
    string Code,
    string Name,
    string ItemType,
    string OwnerModule);

public sealed record UpdateConfigurationItemRequest(
    string Name,
    string ItemType,
    string OwnerModule);

public sealed record ConfigurationItemListItemResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Name,
    string ItemType,
    string OwnerModule,
    string Status,
    string? BaselineRef,
    DateTimeOffset UpdatedAt);

public sealed record ConfigurationItemResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Name,
    string ItemType,
    string OwnerModule,
    string Status,
    string? BaselineRef,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateBaselineRegistryRequest(
    Guid ProjectId,
    string BaselineName,
    string BaselineType,
    string SourceEntityType,
    string SourceEntityId,
    Guid ChangeRequestId);

public sealed record BaselineOverrideRequest(
    Guid? SupersededByBaselineId,
    bool EmergencyOverride,
    string? Reason);

public sealed record BaselineRegistryListItemResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string BaselineName,
    string BaselineType,
    string SourceEntityType,
    string SourceEntityId,
    string Status,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    Guid? ChangeRequestId,
    Guid? SupersededByBaselineId,
    DateTimeOffset UpdatedAt);

public sealed record BaselineRegistryResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string BaselineName,
    string BaselineType,
    string SourceEntityType,
    string SourceEntityId,
    string Status,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    Guid? ChangeRequestId,
    Guid? SupersededByBaselineId,
    string? OverrideReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
