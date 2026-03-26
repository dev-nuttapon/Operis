using Operis_API.Modules.Metrics.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Metrics.Application;

public interface IMetricsQueries
{
    Task<PagedResult<MetricDefinitionListItem>> ListMetricDefinitionsAsync(MetricDefinitionListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<MetricCollectionScheduleItem>> ListMetricCollectionSchedulesAsync(MetricCollectionScheduleListQuery query, CancellationToken cancellationToken);
    Task<MetricResultsResponse> ListMetricResultsAsync(MetricResultListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<QualityGateResultItem>> ListQualityGatesAsync(QualityGateListQuery query, CancellationToken cancellationToken);
}
