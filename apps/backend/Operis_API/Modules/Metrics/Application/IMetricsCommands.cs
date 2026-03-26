using Operis_API.Modules.Metrics.Contracts;

namespace Operis_API.Modules.Metrics.Application;

public interface IMetricsCommands
{
    Task<MetricsCommandResult<MetricDefinitionCommandResponse>> CreateMetricDefinitionAsync(CreateMetricDefinitionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<MetricDefinitionCommandResponse>> UpdateMetricDefinitionAsync(Guid metricDefinitionId, UpdateMetricDefinitionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<MetricCollectionScheduleItem>> CreateMetricCollectionScheduleAsync(CreateMetricCollectionScheduleRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<QualityGateResultItem>> EvaluateQualityGateAsync(EvaluateQualityGateRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<MetricsCommandResult<QualityGateOverrideResponse>> OverrideQualityGateAsync(Guid qualityGateResultId, OverrideQualityGateRequest request, string? actorUserId, CancellationToken cancellationToken);
}
