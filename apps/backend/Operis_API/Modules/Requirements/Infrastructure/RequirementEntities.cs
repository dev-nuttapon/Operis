namespace Operis_API.Modules.Requirements.Infrastructure;

public sealed record RequirementEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Priority { get; init; } = "medium";
    public string OwnerUserId { get; init; } = string.Empty;
    public string Status { get; init; } = "draft";
    public Guid? CurrentVersionId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record RequirementVersionEntity
{
    public Guid Id { get; init; }
    public Guid RequirementId { get; init; }
    public int VersionNumber { get; init; }
    public string BusinessReason { get; init; } = string.Empty;
    public string AcceptanceCriteria { get; init; } = string.Empty;
    public string? SecurityImpact { get; init; }
    public string? PerformanceImpact { get; init; }
    public string Status { get; init; } = "draft";
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record RequirementBaselineEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string BaselineName { get; init; } = string.Empty;
    public string RequirementIdsJson { get; init; } = "[]";
    public string? Reason { get; init; }
    public string ApprovedBy { get; init; } = string.Empty;
    public DateTimeOffset ApprovedAt { get; init; }
    public string Status { get; init; } = "locked";
}

public sealed record TraceabilityLinkEntity
{
    public Guid Id { get; init; }
    public string SourceType { get; init; } = string.Empty;
    public string SourceId { get; init; } = string.Empty;
    public string TargetType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string LinkRule { get; init; } = string.Empty;
    public string Status { get; init; } = "created";
    public string CreatedBy { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
