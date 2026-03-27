namespace Operis_API.Modules.Defects.Infrastructure;

public sealed record DefectEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string OwnerUserId { get; init; } = string.Empty;
    public string Status { get; init; } = "open";
    public string? DetectedInPhase { get; init; }
    public string? ResolutionSummary { get; init; }
    public string? CorrectiveActionRef { get; init; }
    public string? AffectedArtifactRefsJson { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record NonConformanceEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SourceType { get; init; } = string.Empty;
    public string OwnerUserId { get; init; } = string.Empty;
    public string Status { get; init; } = "open";
    public string? CorrectiveActionRef { get; init; }
    public string? RootCause { get; init; }
    public string? ResolutionSummary { get; init; }
    public string? AcceptedDisposition { get; init; }
    public string? LinkedFindingRefsJson { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
