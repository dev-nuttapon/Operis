using Operis_API.Modules.Metrics.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Metrics.Application;

public interface IMetricsQueries
{
    Task<PagedResult<MetricDefinitionListItem>> ListMetricDefinitionsAsync(MetricDefinitionListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<MetricCollectionScheduleItem>> ListMetricCollectionSchedulesAsync(MetricCollectionScheduleListQuery query, CancellationToken cancellationToken);
    Task<MetricResultsResponse> ListMetricResultsAsync(MetricResultListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<QualityGateResultItem>> ListQualityGatesAsync(QualityGateListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<MetricReviewItem>> ListMetricReviewsAsync(MetricReviewListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<TrendReportItem>> ListTrendReportsAsync(TrendReportListQuery query, CancellationToken cancellationToken);
    Task<TrendReportItem?> GetTrendReportAsync(Guid trendReportId, CancellationToken cancellationToken);
    Task<PagedResult<PerformanceBaselineItem>> ListPerformanceBaselinesAsync(PerformanceBaselineListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<CapacityReviewItem>> ListCapacityReviewsAsync(CapacityReviewListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<SlowOperationReviewItem>> ListSlowOperationReviewsAsync(SlowOperationReviewListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<PerformanceGateItem>> ListPerformanceGatesAsync(PerformanceGateListQuery query, CancellationToken cancellationToken);
}
