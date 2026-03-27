using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Audits.Contracts;

namespace Operis_API.Modules.Metrics.Contracts;

public sealed record MetricDefinitionListItem(
    Guid Id,
    string Code,
    string Name,
    string MetricType,
    string OwnerUserId,
    decimal TargetValue,
    decimal ThresholdValue,
    string Status,
    DateTimeOffset UpdatedAt);

public sealed record MetricCollectionScheduleItem(
    Guid Id,
    Guid MetricDefinitionId,
    string MetricCode,
    string MetricName,
    string CollectionFrequency,
    string CollectorType,
    DateTimeOffset NextRunAt,
    string Status,
    DateTimeOffset UpdatedAt);

public sealed record MetricResultItem(
    Guid Id,
    Guid MetricDefinitionId,
    string MetricCode,
    string MetricName,
    string MetricType,
    DateTimeOffset MeasuredAt,
    decimal MeasuredValue,
    decimal TargetValue,
    decimal ThresholdValue,
    string Status,
    string SourceRef,
    Guid? QualityGateResultId);

public sealed record MetricTrendPoint(
    Guid MetricDefinitionId,
    string MetricCode,
    DateTimeOffset MeasuredAt,
    decimal MeasuredValue,
    string Status);

public sealed record MetricCurrentVsTargetItem(
    Guid MetricDefinitionId,
    string MetricCode,
    string MetricName,
    decimal CurrentValue,
    decimal TargetValue,
    decimal ThresholdValue,
    string Status);

public sealed record MetricsDashboardSummary(
    int BreachCount,
    int OpenActions,
    IReadOnlyList<MetricTrendPoint> Trend,
    IReadOnlyList<MetricCurrentVsTargetItem> CurrentVsTarget);

public sealed record MetricResultsResponse(
    IReadOnlyList<MetricResultItem> Items,
    int Total,
    int Page,
    int PageSize,
    MetricsDashboardSummary Summary);

public sealed record QualityGateResultItem(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string GateType,
    DateTimeOffset EvaluatedAt,
    string Result,
    string? Reason,
    string? OverrideReason,
    string? EvaluatedByUserId,
    string? OverriddenByUserId,
    IReadOnlyList<MetricResultItem> Metrics);

public sealed record MetricReviewItem(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string ReviewPeriod,
    string ReviewedBy,
    string Status,
    string? Summary,
    int OpenActionCount,
    DateTimeOffset UpdatedAt);

public sealed record TrendReportItem(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    Guid MetricDefinitionId,
    string MetricCode,
    string MetricName,
    DateOnly PeriodFrom,
    DateOnly PeriodTo,
    string Status,
    string? ReportRef,
    string? TrendDirection,
    decimal? Variance,
    string? RecommendedAction,
    DateTimeOffset UpdatedAt);

public sealed record CreateMetricDefinitionRequest(
    string Code,
    string Name,
    string MetricType,
    string OwnerUserId,
    decimal? TargetValue,
    decimal? ThresholdValue);

public sealed record UpdateMetricDefinitionRequest(
    string Name,
    string MetricType,
    string OwnerUserId,
    decimal? TargetValue,
    decimal? ThresholdValue,
    string Status);

public sealed record CreateMetricCollectionScheduleRequest(
    Guid MetricDefinitionId,
    string CollectionFrequency,
    string CollectorType,
    string Status = "draft");

public sealed record EvaluateQualityGateMetricInput(
    Guid MetricDefinitionId,
    decimal MeasuredValue,
    DateTimeOffset? MeasuredAt,
    string SourceRef);

public sealed record EvaluateQualityGateRequest(
    Guid ProjectId,
    string GateType,
    string? Reason,
    IReadOnlyList<EvaluateQualityGateMetricInput> MetricInputs);

public sealed record OverrideQualityGateRequest(string Reason);
public sealed record CreateMetricReviewRequest(Guid ProjectId, string ReviewPeriod, string ReviewedBy, string? Summary, int OpenActionCount = 0);
public sealed record UpdateMetricReviewRequest(string ReviewPeriod, string ReviewedBy, string Status, string? Summary, int OpenActionCount = 0);
public sealed record CreateTrendReportRequest(Guid ProjectId, Guid? MetricDefinitionId, DateOnly? PeriodFrom, DateOnly? PeriodTo, string Status, string? ReportRef, string? TrendDirection, decimal? Variance, string? RecommendedAction);
public sealed record UpdateTrendReportRequest(Guid ProjectId, Guid? MetricDefinitionId, DateOnly? PeriodFrom, DateOnly? PeriodTo, string Status, string? ReportRef, string? TrendDirection, decimal? Variance, string? RecommendedAction);

public sealed record MetricDefinitionListQuery(
    [FromQuery] string? Search,
    [FromQuery] string? MetricType,
    [FromQuery] string? Status,
    [FromQuery] string? OwnerUserId,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record MetricCollectionScheduleListQuery(
    [FromQuery] Guid? MetricDefinitionId,
    [FromQuery] string? Status,
    [FromQuery] string? CollectorType,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record MetricResultListQuery(
    [FromQuery] Guid? MetricDefinitionId,
    [FromQuery] Guid? ProjectId,
    [FromQuery] string? Status,
    [FromQuery] string? GateType,
    [FromQuery] DateTimeOffset? From,
    [FromQuery] DateTimeOffset? To,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record QualityGateListQuery(
    [FromQuery] Guid? ProjectId,
    [FromQuery] string? GateType,
    [FromQuery] string? Result,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record MetricReviewListQuery(
    [FromQuery] Guid? ProjectId,
    [FromQuery] string? Status,
    [FromQuery] string? ReviewedBy,
    [FromQuery] string? Search,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record TrendReportListQuery(
    [FromQuery] Guid? ProjectId,
    [FromQuery] Guid? MetricDefinitionId,
    [FromQuery] string? Status,
    [FromQuery] DateOnly? PeriodFrom,
    [FromQuery] DateOnly? PeriodTo,
    [FromQuery] string? Search,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record MetricDefinitionCommandResponse(
    Guid Id,
    string Code,
    string Status);

public sealed record QualityGateOverrideResponse(
    Guid Id,
    string Result,
    string OverrideReason);
