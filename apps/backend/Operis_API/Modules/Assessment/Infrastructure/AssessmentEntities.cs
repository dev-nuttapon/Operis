namespace Operis_API.Modules.Assessment.Infrastructure;

public sealed record AssessmentPackageEntity
{
    public Guid Id { get; init; }
    public string PackageCode { get; init; } = string.Empty;
    public Guid? ProjectId { get; init; }
    public string? ProcessArea { get; init; }
    public string ScopeSummary { get; init; } = string.Empty;
    public string Status { get; init; } = "draft";
    public string EvidenceReferencesJson { get; init; } = "[]";
    public string CreatedByUserId { get; init; } = string.Empty;
    public DateTimeOffset? PreparedAt { get; init; }
    public string? PreparedByUserId { get; init; }
    public DateTimeOffset? SharedAt { get; init; }
    public string? SharedByUserId { get; init; }
    public DateTimeOffset? ArchivedAt { get; init; }
    public string? ArchivedByUserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record AssessmentFindingEntity
{
    public Guid Id { get; init; }
    public Guid PackageId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Severity { get; init; } = "medium";
    public string Status { get; init; } = "open";
    public string EvidenceEntityType { get; init; } = string.Empty;
    public string EvidenceEntityId { get; init; } = string.Empty;
    public string? EvidenceRoute { get; init; }
    public string? OwnerUserId { get; init; }
    public string? AcceptanceSummary { get; init; }
    public string? ClosureSummary { get; init; }
    public string CreatedByUserId { get; init; } = string.Empty;
    public DateTimeOffset? AcceptedAt { get; init; }
    public string? AcceptedByUserId { get; init; }
    public DateTimeOffset? ClosedAt { get; init; }
    public string? ClosedByUserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record AssessmentNoteEntity
{
    public Guid Id { get; init; }
    public Guid PackageId { get; init; }
    public string NoteType { get; init; } = string.Empty;
    public string Note { get; init; } = string.Empty;
    public string CreatedByUserId { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record ControlCatalogEntity
{
    public Guid Id { get; init; }
    public string ControlCode { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ControlSet { get; init; } = string.Empty;
    public string? ProcessArea { get; init; }
    public string Status { get; init; } = "draft";
    public string? Description { get; init; }
    public Guid? ProjectId { get; init; }
    public string CreatedByUserId { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record ControlMappingEntity
{
    public Guid Id { get; init; }
    public Guid ControlId { get; init; }
    public Guid? ProjectId { get; init; }
    public string TargetModule { get; init; } = string.Empty;
    public string TargetEntityType { get; init; } = string.Empty;
    public string TargetEntityId { get; init; } = string.Empty;
    public string TargetRoute { get; init; } = string.Empty;
    public string EvidenceStatus { get; init; } = "referenced";
    public string Status { get; init; } = "draft";
    public string? Notes { get; init; }
    public string CreatedByUserId { get; init; } = string.Empty;
    public DateTimeOffset? ActivatedAt { get; init; }
    public DateTimeOffset? RetiredAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record ControlCoverageSnapshotEntity
{
    public Guid Id { get; init; }
    public Guid ControlId { get; init; }
    public Guid? ProjectId { get; init; }
    public string CoverageStatus { get; init; } = "gap";
    public int ActiveMappingCount { get; init; }
    public int EvidenceCount { get; init; }
    public int GapCount { get; init; }
    public DateTimeOffset GeneratedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
