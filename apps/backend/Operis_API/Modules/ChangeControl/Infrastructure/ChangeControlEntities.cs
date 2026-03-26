namespace Operis_API.Modules.ChangeControl.Infrastructure;

public sealed record ChangeRequestEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string RequestedBy { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = "draft";
    public string Priority { get; init; } = "medium";
    public Guid? TargetBaselineId { get; init; }
    public string LinkedRequirementIdsJson { get; init; } = "[]";
    public string LinkedConfigurationItemIdsJson { get; init; } = "[]";
    public string? DecisionRationale { get; init; }
    public string? ImplementationSummary { get; init; }
    public string? ApprovedBy { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record ChangeImpactEntity
{
    public Guid Id { get; init; }
    public Guid ChangeRequestId { get; init; }
    public string ScopeImpact { get; init; } = string.Empty;
    public string ScheduleImpact { get; init; } = string.Empty;
    public string QualityImpact { get; init; } = string.Empty;
    public string SecurityImpact { get; init; } = string.Empty;
    public string PerformanceImpact { get; init; } = string.Empty;
    public string RiskImpact { get; init; } = string.Empty;
}

public sealed record ConfigurationItemEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string ItemType { get; init; } = string.Empty;
    public string OwnerModule { get; init; } = string.Empty;
    public string Status { get; init; } = "draft";
    public string? BaselineRef { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record BaselineRegistryEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string BaselineName { get; init; } = string.Empty;
    public string BaselineType { get; init; } = string.Empty;
    public string SourceEntityType { get; init; } = string.Empty;
    public string SourceEntityId { get; init; } = string.Empty;
    public string Status { get; init; } = "proposed";
    public string? ApprovedBy { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public Guid? ChangeRequestId { get; init; }
    public Guid? SupersededByBaselineId { get; init; }
    public string? OverrideReason { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
