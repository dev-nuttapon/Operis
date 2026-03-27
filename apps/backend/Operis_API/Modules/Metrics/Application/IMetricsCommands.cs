using Operis_API.Modules.Metrics.Contracts;

namespace Operis_API.Modules.Metrics.Application;

public interface IMetricsCommands
{
    Task<MetricsCommandResult<MetricDefinitionCommandResponse>> CreateMetricDefinitionAsync(CreateMetricDefinitionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<MetricDefinitionCommandResponse>> UpdateMetricDefinitionAsync(Guid metricDefinitionId, UpdateMetricDefinitionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<MetricCollectionScheduleItem>> CreateMetricCollectionScheduleAsync(CreateMetricCollectionScheduleRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<QualityGateResultItem>> EvaluateQualityGateAsync(EvaluateQualityGateRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<QualityGateOverrideResponse>> OverrideQualityGateAsync(Guid qualityGateResultId, OverrideQualityGateRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<MetricReviewItem>> CreateMetricReviewAsync(CreateMetricReviewRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<MetricReviewItem>> UpdateMetricReviewAsync(Guid metricReviewId, UpdateMetricReviewRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<TrendReportItem>> CreateTrendReportAsync(CreateTrendReportRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<TrendReportItem>> UpdateTrendReportAsync(Guid trendReportId, UpdateTrendReportRequest request, string? actorUserId, CancellationToken cancellationToken);
}
