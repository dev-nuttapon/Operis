namespace Operis_API.Modules.Operations.Infrastructure;

public sealed class AccessReviewEntity
{
    public Guid Id { get; init; }
    public string ScopeType { get; set; } = string.Empty;
    public string ScopeRef { get; set; } = string.Empty;
    public string ReviewCycle { get; set; } = string.Empty;
    public string? ReviewedBy { get; set; }
    public string Status { get; set; } = "Scheduled";
    public string? Decision { get; set; }
    public string? DecisionRationale { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class SecurityReviewEntity
{
    public Guid Id { get; init; }
    public string ScopeType { get; set; } = string.Empty;
    public string ScopeRef { get; set; } = string.Empty;
    public string ControlsReviewed { get; set; } = string.Empty;
    public string? FindingsSummary { get; set; }
    public string Status { get; set; } = "Planned";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class ExternalDependencyEntity
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string DependencyType { get; set; } = string.Empty;
    public Guid? SupplierId { get; set; }
    public string OwnerUserId { get; set; } = string.Empty;
    public string Criticality { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTimeOffset? ReviewDueAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class SupplierEntity
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string SupplierType { get; set; } = string.Empty;
    public string OwnerUserId { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string Criticality { get; set; } = string.Empty;
    public DateTimeOffset? ReviewDueAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class SupplierAgreementEntity
{
    public Guid Id { get; init; }
    public Guid SupplierId { get; set; }
    public string AgreementType { get; set; } = string.Empty;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string SlaTerms { get; set; } = string.Empty;
    public string EvidenceRef { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class ConfigurationAuditEntity
{
    public Guid Id { get; init; }
    public string ScopeRef { get; set; } = string.Empty;
    public DateTimeOffset PlannedAt { get; set; }
    public string Status { get; set; } = "Planned";
    public int FindingCount { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class AccessRecertificationScheduleEntity
{
    public Guid Id { get; init; }
    public string ScopeType { get; set; } = string.Empty;
    public string ScopeRef { get; set; } = string.Empty;
    public DateTimeOffset PlannedAt { get; set; }
    public string ReviewOwnerUserId { get; set; } = string.Empty;
    public string Status { get; set; } = "planned";
    public string? SubjectUsersJson { get; set; }
    public string? ExceptionNotes { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class AccessRecertificationDecisionEntity
{
    public Guid Id { get; init; }
    public Guid ScheduleId { get; set; }
    public string SubjectUserId { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string DecidedBy { get; set; } = string.Empty;
    public DateTimeOffset DecidedAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class SecurityIncidentEntity
{
    public Guid Id { get; init; }
    public Guid? ProjectId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTimeOffset ReportedAt { get; set; }
    public string OwnerUserId { get; set; } = string.Empty;
    public string Status { get; set; } = "reported";
    public string? ResolutionSummary { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class VulnerabilityRecordEntity
{
    public Guid Id { get; init; }
    public string AssetRef { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTimeOffset IdentifiedAt { get; set; }
    public DateTimeOffset? PatchDueAt { get; set; }
    public string OwnerUserId { get; set; } = string.Empty;
    public string Status { get; set; } = "open";
    public string? VerificationSummary { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class SecretRotationEntity
{
    public Guid Id { get; init; }
    public string SecretScope { get; set; } = string.Empty;
    public DateTimeOffset PlannedAt { get; set; }
    public DateTimeOffset? RotatedAt { get; set; }
    public string? VerifiedBy { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public string Status { get; set; } = "planned";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class PrivilegedAccessEventEntity
{
    public Guid Id { get; init; }
    public string RequestedBy { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public string? UsedBy { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string Status { get; set; } = "requested";
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class DataClassificationPolicyEntity
{
    public Guid Id { get; init; }
    public string PolicyCode { get; set; } = string.Empty;
    public string ClassificationLevel { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public string? HandlingRule { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class BackupEvidenceEntity
{
    public Guid Id { get; init; }
    public string BackupScope { get; set; } = string.Empty;
    public DateTimeOffset ExecutedAt { get; set; }
    public string ExecutedBy { get; set; } = string.Empty;
    public string Status { get; set; } = "planned";
    public string? EvidenceRef { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class RestoreVerificationEntity
{
    public Guid Id { get; init; }
    public Guid BackupEvidenceId { get; set; }
    public DateTimeOffset ExecutedAt { get; set; }
    public string ExecutedBy { get; set; } = string.Empty;
    public string Status { get; set; } = "planned";
    public string ResultSummary { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class DrDrillEntity
{
    public Guid Id { get; init; }
    public string ScopeRef { get; set; } = string.Empty;
    public DateTimeOffset PlannedAt { get; set; }
    public DateTimeOffset? ExecutedAt { get; set; }
    public string Status { get; set; } = "planned";
    public int FindingCount { get; set; }
    public string? Summary { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class LegalHoldEntity
{
    public Guid Id { get; init; }
    public string ScopeType { get; set; } = string.Empty;
    public string ScopeRef { get; set; } = string.Empty;
    public DateTimeOffset PlacedAt { get; set; }
    public string PlacedBy { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset? ReleasedAt { get; set; }
    public string? ReleasedBy { get; set; }
    public string? ReleaseReason { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
