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
