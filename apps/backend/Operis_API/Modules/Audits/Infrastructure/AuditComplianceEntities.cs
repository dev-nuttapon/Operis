namespace Operis_API.Modules.Audits.Infrastructure;

public sealed record AuditPlanEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
    public string Criteria { get; init; } = string.Empty;
    public DateTimeOffset PlannedAt { get; init; }
    public string Status { get; init; } = "planned";
    public string OwnerUserId { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record AuditFindingEntity
{
    public Guid Id { get; init; }
    public Guid AuditPlanId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Status { get; init; } = "open";
    public string OwnerUserId { get; init; } = string.Empty;
    public DateOnly? DueDate { get; init; }
    public string? ResolutionSummary { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record EvidenceExportEntity
{
    public Guid Id { get; init; }
    public string RequestedBy { get; init; } = string.Empty;
    public string ScopeType { get; init; } = string.Empty;
    public string ScopeRef { get; init; } = string.Empty;
    public DateTimeOffset RequestedAt { get; init; }
    public string Status { get; init; } = "requested";
    public string? OutputRef { get; init; }
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
    public string IncludedArtifactTypesJson { get; init; } = "[]";
    public string? FailureReason { get; init; }
}

public sealed record EvidenceRuleEntity
{
    public Guid Id { get; init; }
    public string RuleCode { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ProcessArea { get; init; } = string.Empty;
    public string ArtifactType { get; init; } = string.Empty;
    public Guid? ProjectId { get; init; }
    public string Status { get; init; } = "draft";
    public string ExpressionType { get; init; } = "required";
    public string? Reason { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record EvidenceRuleResultEntity
{
    public Guid Id { get; init; }
    public string ScopeType { get; init; } = "portfolio";
    public string ScopeRef { get; init; } = "all-projects";
    public Guid? ProjectId { get; init; }
    public string? ProcessArea { get; init; }
    public string Status { get; init; } = "completed";
    public int EvaluatedRuleCount { get; init; }
    public int MissingItemCount { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
    public Guid? SupersededByResultId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record EvidenceMissingItemEntity
{
    public Guid Id { get; init; }
    public Guid ResultId { get; init; }
    public Guid RuleId { get; init; }
    public Guid? ProjectId { get; init; }
    public string ProcessArea { get; init; } = string.Empty;
    public string ArtifactType { get; init; } = string.Empty;
    public string ReasonCode { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Module { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public string? Metadata { get; init; }
    public DateTimeOffset DetectedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
