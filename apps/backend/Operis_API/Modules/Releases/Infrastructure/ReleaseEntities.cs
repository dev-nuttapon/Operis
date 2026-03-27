namespace Operis_API.Modules.Releases.Infrastructure;

public sealed record ReleaseEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string ReleaseCode { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public DateTimeOffset? PlannedAt { get; init; }
    public DateTimeOffset? ReleasedAt { get; init; }
    public string Status { get; init; } = "draft";
    public Guid? QualityGateResultId { get; init; }
    public string? QualityGateOverrideReason { get; init; }
    public string? ApprovedByUserId { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record DeploymentChecklistEntity
{
    public Guid Id { get; init; }
    public Guid ReleaseId { get; init; }
    public string ChecklistItem { get; init; } = string.Empty;
    public string OwnerUserId { get; init; } = string.Empty;
    public string Status { get; init; } = "draft";
    public DateTimeOffset? CompletedAt { get; init; }
    public string? EvidenceRef { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record ReleaseNoteEntity
{
    public Guid Id { get; init; }
    public Guid ReleaseId { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string IncludedChanges { get; init; } = string.Empty;
    public string? KnownIssues { get; init; }
    public string Status { get; init; } = "draft";
    public DateTimeOffset? PublishedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
