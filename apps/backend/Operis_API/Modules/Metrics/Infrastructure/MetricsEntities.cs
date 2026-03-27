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

public sealed record MetricReviewEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string ReviewPeriod { get; init; } = string.Empty;
    public string ReviewedBy { get; init; } = string.Empty;
    public string Status { get; init; } = "planned";
    public string? Summary { get; init; }
    public int OpenActionCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record TrendReportEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid MetricDefinitionId { get; init; }
    public DateOnly PeriodFrom { get; init; }
    public DateOnly PeriodTo { get; init; }
    public string Status { get; init; } = "draft";
    public string? ReportRef { get; init; }
    public string? TrendDirection { get; init; }
    public decimal? Variance { get; init; }
    public string? RecommendedAction { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record PerformanceBaselineEntity
{
    public Guid Id { get; init; }
    public string ScopeType { get; init; } = string.Empty;
    public string ScopeRef { get; init; } = string.Empty;
    public string MetricName { get; init; } = string.Empty;
    public decimal TargetValue { get; init; }
    public decimal ThresholdValue { get; init; }
    public string Status { get; init; } = "draft";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record CapacityReviewEntity
{
    public Guid Id { get; init; }
    public string ScopeRef { get; init; } = string.Empty;
    public string ReviewPeriod { get; init; } = string.Empty;
    public string ReviewedBy { get; init; } = string.Empty;
    public string Status { get; init; } = "planned";
    public string Summary { get; init; } = string.Empty;
    public int ActionCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record SlowOperationReviewEntity
{
    public Guid Id { get; init; }
    public string OperationType { get; init; } = string.Empty;
    public string OperationKey { get; init; } = string.Empty;
    public int ObservedLatencyMs { get; init; }
    public int? FrequencyPerHour { get; init; }
    public string Status { get; init; } = "open";
    public string OwnerUserId { get; init; } = string.Empty;
    public string? OptimizationSummary { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record PerformanceGateResultEntity
{
    public Guid Id { get; init; }
    public string ScopeRef { get; init; } = string.Empty;
    public DateTimeOffset EvaluatedAt { get; init; }
    public string Result { get; init; } = "pending";
    public string? Reason { get; init; }
    public string? OverrideReason { get; init; }
    public string? EvidenceRef { get; init; }
    public string? EvaluatedByUserId { get; init; }
    public string? OverriddenByUserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record AdoptionRuleEntity
{
    public Guid Id { get; init; }
    public string RuleCode { get; init; } = string.Empty;
    public string ProcessArea { get; init; } = string.Empty;
    public string ScopeType { get; init; } = string.Empty;
    public decimal ThresholdPercentage { get; init; }
    public string Status { get; init; } = "draft";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record AdoptionScoreEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid AdoptionRuleId { get; init; }
    public string ProcessArea { get; init; } = string.Empty;
    public decimal ScorePercentage { get; init; }
    public string ScoreState { get; init; } = "calculated";
    public int EvidenceCount { get; init; }
    public int ExpectedCount { get; init; }
    public DateTimeOffset CalculatedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record AdoptionAnomalyEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid AdoptionRuleId { get; init; }
    public string ProcessArea { get; init; } = string.Empty;
    public string Severity { get; init; } = "medium";
    public string Summary { get; init; } = string.Empty;
    public string Status { get; init; } = "open";
    public DateTimeOffset DetectedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
