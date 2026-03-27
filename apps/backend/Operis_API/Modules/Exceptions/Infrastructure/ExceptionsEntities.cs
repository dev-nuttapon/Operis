namespace Operis_API.Modules.Exceptions.Infrastructure;

public sealed record WaiverEntity
{
    public Guid Id { get; init; }
    public string WaiverCode { get; init; } = string.Empty;
    public Guid? ProjectId { get; init; }
    public string ProcessArea { get; init; } = string.Empty;
    public string ScopeSummary { get; init; } = string.Empty;
    public string RequestedByUserId { get; init; } = string.Empty;
    public string Justification { get; init; } = string.Empty;
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly ExpiresAt { get; init; }
    public string Status { get; init; } = "draft";
    public string? DecisionReason { get; init; }
    public string? DecisionByUserId { get; init; }
    public DateTimeOffset? DecisionAt { get; init; }
    public string? ClosureReason { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record CompensatingControlEntity
{
    public Guid Id { get; init; }
    public Guid WaiverId { get; init; }
    public string ControlCode { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string OwnerUserId { get; init; } = string.Empty;
    public string Status { get; init; } = "active";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record WaiverReviewEntity
{
    public Guid Id { get; init; }
    public Guid WaiverId { get; init; }
    public string ReviewType { get; init; } = string.Empty;
    public string OutcomeStatus { get; init; } = string.Empty;
    public string ReviewerUserId { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTimeOffset ReviewedAt { get; init; }
    public DateTimeOffset? NextReviewAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
