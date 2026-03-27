using Operis_API.Modules.Metrics.Application;
using Operis_API.Modules.Metrics.Contracts;
using Operis_API.Modules.Metrics.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Metrics.Application;

public sealed class MetricsCommandsTests
{
    [Fact]
    public async Task CreateMetricDefinitionAsync_WithoutTarget_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var sut = new MetricsCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new MetricsQueries(dbContext));

        var result = await sut.CreateMetricDefinitionAsync(new CreateMetricDefinitionRequest("DEFECT_DENSITY", "Defect Density", "quality", "qa@example.com", null, 10m), "qa@example.com", CancellationToken.None);

        Assert.Equal(MetricsCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.MetricTargetRequired, result.ErrorCode);
    }

    [Fact]
    public async Task OverrideQualityGateAsync_WithoutReason_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var gateId = Guid.NewGuid();
        dbContext.QualityGateResults.Add(new QualityGateResultEntity
        {
            Id = gateId,
            ProjectId = projectId,
            GateType = "release_readiness",
            EvaluatedAt = DateTimeOffset.UtcNow,
            Result = "failed",
            Reason = "Threshold breached",
            EvaluatedByUserId = "qa@example.com",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new MetricsCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new MetricsQueries(dbContext));
        var result = await sut.OverrideQualityGateAsync(gateId, new OverrideQualityGateRequest(""), "compliance@example.com", CancellationToken.None);

        Assert.Equal(MetricsCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.QualityGateOverrideReasonRequired, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateMetricReviewAsync_CloseWithOpenActions_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var reviewId = Guid.NewGuid();
        dbContext.MetricReviews.Add(new MetricReviewEntity
        {
            Id = reviewId,
            ProjectId = projectId,
            ReviewPeriod = "2026-Q1",
            ReviewedBy = "qa@example.com",
            Status = "actions_tracked",
            OpenActionCount = 2,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new MetricsCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new MetricsQueries(dbContext));
        var result = await sut.UpdateMetricReviewAsync(reviewId, new UpdateMetricReviewRequest("2026-Q1", "qa@example.com", "closed", "Follow-up pending", 1), "qa@example.com", CancellationToken.None);

        Assert.Equal(MetricsCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.MetricReviewOpenActionsExist, result.ErrorCode);
    }

    [Fact]
    public async Task CreateTrendReportAsync_WithoutMetric_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        await dbContext.SaveChangesAsync();
        var sut = new MetricsCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new MetricsQueries(dbContext));

        var result = await sut.CreateTrendReportAsync(new CreateTrendReportRequest(projectId, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)), DateOnly.FromDateTime(DateTime.UtcNow), "approved", null, "up", 4.2m, "Investigate variance"), "qa@example.com", CancellationToken.None);

        Assert.Equal(MetricsCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.TrendMetricRequired, result.ErrorCode);
    }

    private static Guid SeedProject(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext)
    {
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = $"PRJ-{projectId.ToString()[..8]}",
            Name = "Metrics Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        return projectId;
    }
}
