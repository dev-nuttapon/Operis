using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Metrics;
using Operis_API.Modules.Metrics.Application;
using Operis_API.Modules.Metrics.Contracts;
using Operis_API.Shared.Security;

namespace Operis_API.Tests.Modules.Metrics;

public sealed class MetricsModuleHandlerTests
{
    [Fact]
    public async Task OverrideQualityGateAsync_WithoutOverridePermission_ReturnsForbidden()
    {
        var result = await InvokeOverrideQualityGateAsync(CreateMetricsViewerPrincipal(), new FakeMetricsCommands());

        var httpContext = Operis_API.Tests.Support.TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task OverridePerformanceGateAsync_WithoutOverridePermission_ReturnsForbidden()
    {
        var result = await InvokeOverridePerformanceGateAsync(CreateMetricsViewerPrincipal(), new FakeMetricsCommands());

        var httpContext = Operis_API.Tests.Support.TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task EvaluateAdoptionRulesAsync_WithoutManagePermission_ReturnsForbidden()
    {
        var result = await InvokeEvaluateAdoptionRulesAsync(CreateMetricsViewerPrincipal(), new FakeMetricsCommands());

        var httpContext = Operis_API.Tests.Support.TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokeOverrideQualityGateAsync(ClaimsPrincipal principal, IMetricsCommands commands)
    {
        var method = typeof(MetricsModule).GetMethod("OverrideQualityGateAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("MetricsModule.OverrideQualityGateAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal, Guid.NewGuid(), new OverrideQualityGateRequest("Override for incident"), commands, new PermissionMatrix(), CancellationToken.None])!;
        return await task;
    }

    private static async Task<IResult> InvokeOverridePerformanceGateAsync(ClaimsPrincipal principal, IMetricsCommands commands)
    {
        var method = typeof(MetricsModule).GetMethod("OverridePerformanceGateAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("MetricsModule.OverridePerformanceGateAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal, Guid.NewGuid(), new OverridePerformanceGateRequest("Override for incident"), commands, new PermissionMatrix(), CancellationToken.None])!;
        return await task;
    }

    private static async Task<IResult> InvokeEvaluateAdoptionRulesAsync(ClaimsPrincipal principal, IMetricsCommands commands)
    {
        var method = typeof(MetricsModule).GetMethod("EvaluateAdoptionRulesAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("MetricsModule.EvaluateAdoptionRulesAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal, new EvaluateAdoptionRulesRequest(null, null, null), commands, new PermissionMatrix(), CancellationToken.None])!;
        return await task;
    }

    private static ClaimsPrincipal CreateMetricsViewerPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:metrics_viewer")], "TestAuth"));

    private sealed class FakeMetricsCommands : IMetricsCommands
    {
        public Task<MetricsCommandResult<MetricDefinitionCommandResponse>> CreateMetricDefinitionAsync(CreateMetricDefinitionRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<MetricDefinitionCommandResponse>> UpdateMetricDefinitionAsync(Guid metricDefinitionId, UpdateMetricDefinitionRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<MetricCollectionScheduleItem>> CreateMetricCollectionScheduleAsync(CreateMetricCollectionScheduleRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<QualityGateResultItem>> EvaluateQualityGateAsync(EvaluateQualityGateRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<MetricReviewItem>> CreateMetricReviewAsync(CreateMetricReviewRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<MetricReviewItem>> UpdateMetricReviewAsync(Guid metricReviewId, UpdateMetricReviewRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<TrendReportItem>> CreateTrendReportAsync(CreateTrendReportRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<TrendReportItem>> UpdateTrendReportAsync(Guid trendReportId, UpdateTrendReportRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<PerformanceBaselineCommandResponse>> CreatePerformanceBaselineAsync(CreatePerformanceBaselineRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<PerformanceBaselineCommandResponse>> UpdatePerformanceBaselineAsync(Guid performanceBaselineId, UpdatePerformanceBaselineRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<CapacityReviewItem>> CreateCapacityReviewAsync(CreateCapacityReviewRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<CapacityReviewItem>> UpdateCapacityReviewAsync(Guid capacityReviewId, UpdateCapacityReviewRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<SlowOperationReviewItem>> CreateSlowOperationReviewAsync(CreateSlowOperationReviewRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<SlowOperationReviewItem>> UpdateSlowOperationReviewAsync(Guid slowOperationReviewId, UpdateSlowOperationReviewRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<PerformanceGateItem>> EvaluatePerformanceGateAsync(EvaluatePerformanceGateRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<AdoptionRuleItem>> CreateAdoptionRuleAsync(CreateAdoptionRuleRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<AdoptionRuleItem>> UpdateAdoptionRuleAsync(Guid adoptionRuleId, UpdateAdoptionRuleRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<Operis_API.Shared.Contracts.PagedResult<AdoptionScorecardItem>>> EvaluateAdoptionRulesAsync(EvaluateAdoptionRulesRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<MetricsCommandResult<PerformanceGateOverrideResponse>> OverridePerformanceGateAsync(Guid performanceGateId, OverridePerformanceGateRequest request, string? actorUserId, CancellationToken cancellationToken) =>
            Task.FromResult(new MetricsCommandResult<PerformanceGateOverrideResponse>(MetricsCommandStatus.Success, new PerformanceGateOverrideResponse(performanceGateId, "overridden", request.Reason)));
        public Task<MetricsCommandResult<QualityGateOverrideResponse>> OverrideQualityGateAsync(Guid qualityGateResultId, OverrideQualityGateRequest request, string? actorUserId, CancellationToken cancellationToken) =>
            Task.FromResult(new MetricsCommandResult<QualityGateOverrideResponse>(MetricsCommandStatus.Success, new QualityGateOverrideResponse(qualityGateResultId, "overridden", request.Reason)));
    }
}
