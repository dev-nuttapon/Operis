namespace Operis_API.Modules.Audits.Contracts;

public sealed record AuditEventItem(
    Guid Id,
    DateTimeOffset OccurredAt,
    string? ActorUserId,
    string? ActorEmail,
    string? ActorDisplayName,
    string EntityType,
    string? EntityId,
    string Action,
    string Outcome,
    string? Reason);

public sealed record AuditLogListItem(
    Guid Id,
    DateTimeOffset OccurredAt,
    string Module,
    string Action,
    string EntityType,
    string? EntityId,
    string? ActorUserId,
    string? ActorEmail,
    string? ActorDisplayName,
    string Status,
    string? HttpMethod,
    string? RequestPath);

public sealed record AuditLogResponse(
    Guid Id,
    DateTimeOffset OccurredAt,
    string Module,
    string Action,
    string EntityType,
    string? EntityId,
    string ActorType,
    string? ActorUserId,
    string? ActorEmail,
    string? ActorDisplayName,
    Guid? DepartmentId,
    string? TenantId,
    string? RequestId,
    string? TraceId,
    string? CorrelationId,
    string? HttpMethod,
    string? RequestPath,
    string? IpAddress,
    string? UserAgent,
    string Status,
    int? StatusCode,
    string? ErrorCode,
    string? ErrorMessage,
    string? Reason,
    string Source,
    string? BeforeJson,
    string? AfterJson,
    string? ChangesJson,
    string? MetadataJson,
    bool IsSensitive,
    string? RetentionClass);

public sealed record BusinessAuditEventItem(
    Guid Id,
    DateTimeOffset OccurredAt,
    string Module,
    string EventType,
    string EntityType,
    string? EntityId,
    string? Summary,
    string? Reason,
    string? ActorUserId,
    string? ActorEmail,
    string? ActorDisplayName,
    string? MetadataJson);

public sealed record AuditPlanListItem(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Title,
    string Scope,
    DateTimeOffset PlannedAt,
    string Status,
    string OwnerUserId,
    int OpenFindingCount,
    DateTimeOffset UpdatedAt);

public sealed record AuditFindingItem(
    Guid Id,
    Guid AuditPlanId,
    string AuditPlanTitle,
    string Code,
    string Title,
    string Severity,
    string Status,
    string OwnerUserId,
    DateOnly? DueDate,
    string? ResolutionSummary,
    DateTimeOffset UpdatedAt);

public sealed record AuditPlanDetailResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Title,
    string Scope,
    string Criteria,
    DateTimeOffset PlannedAt,
    string Status,
    string OwnerUserId,
    IReadOnlyList<AuditFindingItem> Findings,
    IReadOnlyList<BusinessAuditEventItem> History,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record EvidenceExportItem(
    Guid Id,
    string RequestedBy,
    string ScopeType,
    string ScopeRef,
    DateTimeOffset RequestedAt,
    string Status,
    string? OutputRef,
    string? FailureReason);

public sealed record EvidenceExportDetailResponse(
    Guid Id,
    string RequestedBy,
    string ScopeType,
    string ScopeRef,
    DateTimeOffset RequestedAt,
    string Status,
    string? OutputRef,
    DateTimeOffset? From,
    DateTimeOffset? To,
    IReadOnlyList<string> IncludedArtifactTypes,
    string? FailureReason,
    IReadOnlyList<BusinessAuditEventItem> History);

public sealed record CreateAuditPlanRequest(
    Guid ProjectId,
    string Title,
    string Scope,
    string Criteria,
    DateTimeOffset PlannedAt,
    string OwnerUserId);

public sealed record UpdateAuditPlanRequest(
    string Title,
    string Scope,
    string Criteria,
    DateTimeOffset PlannedAt,
    string Status,
    string OwnerUserId);

public sealed record CreateAuditFindingRequest(
    Guid AuditPlanId,
    string Code,
    string Title,
    string Description,
    string Severity,
    string OwnerUserId,
    DateOnly? DueDate);

public sealed record UpdateAuditFindingRequest(
    string Title,
    string Description,
    string Severity,
    string Status,
    string OwnerUserId,
    DateOnly? DueDate,
    string? ResolutionSummary);

public sealed record CloseAuditFindingRequest(string ResolutionSummary);

public sealed record CreateEvidenceExportRequest(
    string ScopeType,
    string ScopeRef,
    DateTimeOffset? From,
    DateTimeOffset? To,
    IReadOnlyList<string>? IncludedArtifactTypes);

public sealed record AuditEventListQuery(
    Guid? ProjectId,
    string? EntityType,
    string? Action,
    string? ActorUserId,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? Outcome,
    int Page = 1,
    int PageSize = 50);

public sealed record AuditPlanListQuery(
    Guid? ProjectId,
    string? Status,
    string? OwnerUserId,
    int Page = 1,
    int PageSize = 25);

public sealed record EvidenceExportListQuery(
    string? ScopeType,
    string? Status,
    string? RequestedBy,
    int Page = 1,
    int PageSize = 25);
