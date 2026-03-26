namespace Operis_API.Modules.Risks.Infrastructure;

public sealed record RiskEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Probability { get; init; }
    public int Impact { get; init; }
    public string OwnerUserId { get; init; } = string.Empty;
    public string? MitigationPlan { get; init; }
    public string? Cause { get; init; }
    public string? Effect { get; init; }
    public string? ContingencyPlan { get; init; }
    public string Status { get; init; } = "draft";
    public DateTimeOffset? NextReviewAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record RiskReviewEntity
{
    public Guid Id { get; init; }
    public Guid RiskId { get; init; }
    public string ReviewedBy { get; init; } = string.Empty;
    public DateTimeOffset ReviewedAt { get; init; }
    public string Decision { get; init; } = string.Empty;
    public string? Notes { get; init; }
}

public sealed record IssueEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string OwnerUserId { get; init; } = string.Empty;
    public DateOnly? DueDate { get; init; }
    public string Status { get; init; } = "open";
    public string Severity { get; init; } = "medium";
    public string? RootIssue { get; init; }
    public string? Dependencies { get; init; }
    public string? ResolutionSummary { get; init; }
    public bool IsSensitive { get; init; }
    public string? SensitiveContext { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record IssueActionEntity
{
    public Guid Id { get; init; }
    public Guid IssueId { get; init; }
    public string ActionDescription { get; init; } = string.Empty;
    public string AssignedTo { get; init; } = string.Empty;
    public DateOnly? DueDate { get; init; }
    public string Status { get; init; } = "open";
    public string? VerificationNote { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
