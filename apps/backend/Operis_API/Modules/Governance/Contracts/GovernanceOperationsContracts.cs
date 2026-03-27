namespace Operis_API.Modules.Governance.Contracts;

public sealed record RaciMapListQuery(
    string? Search,
    string? Status,
    string? ProcessCode,
    int Page = 1,
    int PageSize = 25);

public sealed record ApprovalEvidenceListQuery(
    string? EntityType,
    string? ActorUserId,
    string? Outcome,
    DateOnly? ApprovedFrom,
    DateOnly? ApprovedTo,
    int Page = 1,
    int PageSize = 50);

public sealed record WorkflowOverrideListQuery(
    string? EntityType,
    string? RequestedBy,
    string? ApprovedBy,
    DateOnly? OccurredFrom,
    DateOnly? OccurredTo,
    int Page = 1,
    int PageSize = 50);

public sealed record SlaRuleListQuery(
    string? Search,
    string? Status,
    string? ScopeType,
    int Page = 1,
    int PageSize = 25);

public sealed record RetentionPolicyListQuery(
    string? Search,
    string? Status,
    string? AppliesTo,
    int Page = 1,
    int PageSize = 25);

public sealed record RaciMapResponse(
    Guid Id,
    string ProcessCode,
    string RoleName,
    string ResponsibilityType,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateRaciMapRequest(
    string ProcessCode,
    string RoleName,
    string ResponsibilityType,
    string Status,
    string? Reason);

public sealed record UpdateRaciMapRequest(
    string ProcessCode,
    string RoleName,
    string ResponsibilityType,
    string Status,
    string? Reason);

public sealed record ApprovalEvidenceLogResponse(
    Guid Id,
    string EntityType,
    string EntityId,
    string ApproverUserId,
    DateTimeOffset ApprovedAt,
    string Reason,
    string Outcome);

public sealed record WorkflowOverrideLogResponse(
    Guid Id,
    string EntityType,
    string EntityId,
    string RequestedBy,
    string ApprovedBy,
    string Reason,
    DateTimeOffset OccurredAt);

public sealed record SlaRuleResponse(
    Guid Id,
    string ScopeType,
    string ScopeRef,
    int TargetDurationHours,
    string EscalationPolicyId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateSlaRuleRequest(
    string ScopeType,
    string ScopeRef,
    int TargetDurationHours,
    string EscalationPolicyId,
    string Status,
    string? Reason);

public sealed record UpdateSlaRuleRequest(
    string ScopeType,
    string ScopeRef,
    int TargetDurationHours,
    string EscalationPolicyId,
    string Status,
    string? Reason);

public sealed record RetentionPolicyResponse(
    Guid Id,
    string PolicyCode,
    string AppliesTo,
    int RetentionPeriodDays,
    string? ArchiveRule,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateRetentionPolicyRequest(
    string PolicyCode,
    string AppliesTo,
    int RetentionPeriodDays,
    string? ArchiveRule,
    string Status,
    string? Reason);

public sealed record UpdateRetentionPolicyRequest(
    string PolicyCode,
    string AppliesTo,
    int RetentionPeriodDays,
    string? ArchiveRule,
    string Status,
    string? Reason);
