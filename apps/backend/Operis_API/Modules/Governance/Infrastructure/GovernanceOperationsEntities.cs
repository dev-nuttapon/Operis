namespace Operis_API.Modules.Governance.Infrastructure;

public sealed class RaciMapEntity
{
    public Guid Id { get; init; }
    public string ProcessCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string ResponsibilityType { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ApprovalEvidenceLogEntity
{
    public Guid Id { get; init; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ApproverUserId { get; set; } = string.Empty;
    public DateTimeOffset ApprovedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
}

public sealed class WorkflowOverrideLogEntity
{
    public Guid Id { get; init; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
}

public sealed class SlaRuleEntity
{
    public Guid Id { get; init; }
    public string ScopeType { get; set; } = string.Empty;
    public string ScopeRef { get; set; } = string.Empty;
    public int TargetDurationHours { get; set; }
    public string EscalationPolicyId { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class RetentionPolicyEntity
{
    public Guid Id { get; init; }
    public string PolicyCode { get; set; } = string.Empty;
    public string AppliesTo { get; set; } = string.Empty;
    public int RetentionPeriodDays { get; set; }
    public string? ArchiveRule { get; set; }
    public string Status { get; set; } = "draft";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ArchitectureRecordEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ArchitectureType { get; set; } = string.Empty;
    public string OwnerUserId { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public string? CurrentVersionId { get; set; }
    public string? Summary { get; set; }
    public string? SecurityImpact { get; set; }
    public string? EvidenceRef { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class DesignReviewEntity
{
    public Guid Id { get; init; }
    public Guid ArchitectureRecordId { get; set; }
    public string ReviewType { get; set; } = string.Empty;
    public string? ReviewedBy { get; set; }
    public string Status { get; set; } = "draft";
    public string? DecisionReason { get; set; }
    public string? DesignSummary { get; set; }
    public string? Concerns { get; set; }
    public string? EvidenceRef { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class IntegrationReviewEntity
{
    public Guid Id { get; init; }
    public string ScopeRef { get; set; } = string.Empty;
    public string IntegrationType { get; set; } = string.Empty;
    public string? ReviewedBy { get; set; }
    public string Status { get; set; } = "draft";
    public string? DecisionReason { get; set; }
    public string? Risks { get; set; }
    public string? DependencyImpact { get; set; }
    public string? EvidenceRef { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
    public DateTimeOffset? AppliedAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
