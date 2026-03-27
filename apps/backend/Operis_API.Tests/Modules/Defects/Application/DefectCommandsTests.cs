using Operis_API.Modules.Defects.Application;
using Operis_API.Modules.Defects.Contracts;
using Operis_API.Modules.Defects.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Defects.Application;

public sealed class DefectCommandsTests
{
    [Fact]
    public async Task CloseDefectAsync_WithoutResolutionSummary_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var defectId = Guid.NewGuid();
        dbContext.Defects.Add(new DefectEntity
        {
            Id = defectId,
            ProjectId = projectId,
            Code = "DEF-001",
            Title = "Broken approval",
            Description = "Details",
            Severity = "high",
            OwnerUserId = "qa@example.com",
            Status = "resolved",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new DefectCommands(dbContext, new FakeAuditLogWriter(), new DefectQueries(dbContext));
        var result = await sut.CloseDefectAsync(defectId, new CloseDefectRequest(""), "qa@example.com", CancellationToken.None);

        Assert.Equal(DefectCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.DefectResolutionRequired, result.ErrorCode);
    }

    [Fact]
    public async Task CloseNonConformanceAsync_WithoutCorrectiveActionOrDisposition_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var itemId = Guid.NewGuid();
        dbContext.NonConformances.Add(new NonConformanceEntity
        {
            Id = itemId,
            ProjectId = projectId,
            Code = "NC-001",
            Title = "Missing review evidence",
            Description = "Details",
            SourceType = "audit",
            OwnerUserId = "qa@example.com",
            Status = "corrective_action",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new DefectCommands(dbContext, new FakeAuditLogWriter(), new DefectQueries(dbContext));
        var result = await sut.CloseNonConformanceAsync(itemId, new CloseNonConformanceRequest(null, null, null), "qa@example.com", CancellationToken.None);

        Assert.Equal(DefectCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.NonConformanceCorrectiveActionRequired, result.ErrorCode);
    }

    private static Guid SeedProject(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext)
    {
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = $"DEF-{projectId.ToString()[..8]}",
            Name = "Defect Project",
            ProjectType = "internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        return projectId;
    }
}
