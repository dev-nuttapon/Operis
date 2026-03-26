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
