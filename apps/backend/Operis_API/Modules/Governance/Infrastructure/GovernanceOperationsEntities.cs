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

public sealed class ComplianceSnapshotEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; set; }
    public string ProcessArea { get; set; } = "overall";
    public DateTimeOffset PeriodStart { get; set; }
    public DateTimeOffset PeriodEnd { get; set; }
    public int ReadinessScore { get; set; }
    public string Status { get; set; } = "published";
    public int MissingArtifactCount { get; set; }
    public int OverdueApprovalCount { get; set; }
    public int StaleBaselineCount { get; set; }
    public int OpenCapaCount { get; set; }
    public int OpenAuditFindingCount { get; set; }
    public int OpenSecurityItemCount { get; set; }
    public string DetailsJson { get; set; } = "{}";
    public DateTimeOffset GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = "system";
    public Guid? SupersededBySnapshotId { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ComplianceDashboardPreferenceEntity
{
    public Guid Id { get; init; }
    public string UserId { get; set; } = string.Empty;
    public Guid? DefaultProjectId { get; set; }
    public string? DefaultProcessArea { get; set; }
    public int DefaultPeriodDays { get; set; } = 30;
    public bool DefaultShowOnlyAtRisk { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ManagementReviewEntity
{
    public Guid Id { get; init; }
    public Guid? ProjectId { get; set; }
    public string ReviewCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ReviewPeriod { get; set; } = string.Empty;
    public DateTimeOffset ScheduledAt { get; set; }
    public string FacilitatorUserId { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public string? AgendaSummary { get; set; }
    public string? MinutesSummary { get; set; }
    public string? DecisionSummary { get; set; }
    public string? EscalationEntityType { get; set; }
    public string? EscalationEntityId { get; set; }
    public string? ClosedBy { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ManagementReviewItemEntity
{
    public Guid Id { get; init; }
    public Guid ReviewId { get; set; }
    public string ItemType { get; set; } = "agenda";
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Decision { get; set; }
    public string? OwnerUserId { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public string Status { get; set; } = "open";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ManagementReviewActionEntity
{
    public Guid Id { get; init; }
    public Guid ReviewId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerUserId { get; set; } = string.Empty;
    public DateTimeOffset? DueAt { get; set; }
    public string Status { get; set; } = "open";
    public bool IsMandatory { get; set; }
    public string? LinkedEntityType { get; set; }
    public string? LinkedEntityId { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
