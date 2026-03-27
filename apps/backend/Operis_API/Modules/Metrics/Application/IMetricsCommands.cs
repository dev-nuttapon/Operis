using Operis_API.Modules.Metrics.Contracts;
using Operis_API.Shared.Contracts;

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
    Task<MetricsCommandResult<PerformanceBaselineCommandResponse>> CreatePerformanceBaselineAsync(CreatePerformanceBaselineRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<PerformanceBaselineCommandResponse>> UpdatePerformanceBaselineAsync(Guid performanceBaselineId, UpdatePerformanceBaselineRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<CapacityReviewItem>> CreateCapacityReviewAsync(CreateCapacityReviewRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<CapacityReviewItem>> UpdateCapacityReviewAsync(Guid capacityReviewId, UpdateCapacityReviewRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<SlowOperationReviewItem>> CreateSlowOperationReviewAsync(CreateSlowOperationReviewRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<SlowOperationReviewItem>> UpdateSlowOperationReviewAsync(Guid slowOperationReviewId, UpdateSlowOperationReviewRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<PerformanceGateItem>> EvaluatePerformanceGateAsync(EvaluatePerformanceGateRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<PerformanceGateOverrideResponse>> OverridePerformanceGateAsync(Guid performanceGateId, OverridePerformanceGateRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<AdoptionRuleItem>> CreateAdoptionRuleAsync(CreateAdoptionRuleRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<AdoptionRuleItem>> UpdateAdoptionRuleAsync(Guid adoptionRuleId, UpdateAdoptionRuleRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<PagedResult<AdoptionScorecardItem>>> EvaluateAdoptionRulesAsync(EvaluateAdoptionRulesRequest request, string? actorUserId, CancellationToken cancellationToken);
}
