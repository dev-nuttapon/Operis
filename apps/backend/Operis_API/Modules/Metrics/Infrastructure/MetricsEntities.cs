namespace Operis_API.Modules.Metrics.Infrastructure;

public sealed record MetricDefinitionEntity
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string MetricType { get; init; } = string.Empty;
    public string OwnerUserId { get; init; } = string.Empty;
    public decimal TargetValue { get; init; }
    public decimal ThresholdValue { get; init; }
    public string Status { get; init; } = "draft";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record MetricCollectionScheduleEntity
{
    public Guid Id { get; init; }
    public Guid MetricDefinitionId { get; init; }
    public string CollectionFrequency { get; init; } = string.Empty;
    public string CollectorType { get; init; } = string.Empty;
    public DateTimeOffset NextRunAt { get; init; }
    public string Status { get; init; } = "draft";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record MetricResultEntity
{
    public Guid Id { get; init; }
    public Guid MetricDefinitionId { get; init; }
    public Guid? QualityGateResultId { get; init; }
    public DateTimeOffset MeasuredAt { get; init; }
    public decimal MeasuredValue { get; init; }
    public string Status { get; init; } = "within_target";
    public string SourceRef { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record QualityGateResultEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string GateType { get; init; } = string.Empty;
    public DateTimeOffset EvaluatedAt { get; init; }
    public string Result { get; init; } = "pending";
    public string? Reason { get; init; }
    public string? OverrideReason { get; init; }
    public string? EvaluatedByUserId { get; init; }
    public string? OverriddenByUserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
