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
