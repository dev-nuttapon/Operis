using Operis_API.Modules.ChangeControl.Application;
using Operis_API.Modules.ChangeControl.Contracts;
using Operis_API.Modules.ChangeControl.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.ChangeControl.Application;

public sealed class ChangeControlCommandsTests
{
    [Fact]
    public async Task ApproveChangeRequestAsync_WithCompleteImpact_MakesBaselineEligible()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var baselineId = SeedLockedRequirementBaseline(dbContext, projectId);
        var changeRequestId = Guid.NewGuid();

        dbContext.ChangeRequests.Add(new ChangeRequestEntity
        {
            Id = changeRequestId,
            ProjectId = projectId,
            Code = "CR-001",
            Title = "Adjust baseline",
            RequestedBy = "pm@example.com",
            Reason = "Need baseline revision",
            Status = "submitted",
            Priority = "high",
            TargetBaselineId = baselineId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.ChangeImpacts.Add(new ChangeImpactEntity
        {
            Id = Guid.NewGuid(),
            ChangeRequestId = changeRequestId,
            ScopeImpact = "Scope",
            ScheduleImpact = "Schedule",
            QualityImpact = "Quality",
            SecurityImpact = "Security",
            PerformanceImpact = "Performance",
            RiskImpact = "Risk"
        });
        await dbContext.SaveChangesAsync();

        var auditWriter = new FakeAuditLogWriter();
        var businessWriter = new FakeBusinessAuditEventWriter();
        var sut = new ChangeControlCommands(dbContext, auditWriter, businessWriter, new ChangeControlQueries(dbContext));

        var approveResult = await sut.ApproveChangeRequestAsync(changeRequestId, new ChangeDecisionRequest("Approved after review"), "approver@example.com", CancellationToken.None);
        var baselineResult = await sut.CreateBaselineRegistryAsync(
            new CreateBaselineRegistryRequest(projectId, "BL-REQ-2", "requirements", "requirement_baseline", baselineId.ToString(), changeRequestId),
            "pm@example.com",
            CancellationToken.None);

        Assert.Equal(ChangeControlCommandStatus.Success, approveResult.Status);
        Assert.Equal(ChangeControlCommandStatus.Success, baselineResult.Status);
        Assert.Equal("proposed", baselineResult.Value?.Status);
        Assert.Contains(businessWriter.Entries, entry => entry.EventType == "change_request_approved");
    }

    [Fact]
    public async Task CreateBaselineRegistryAsync_WithoutApprovedChangeRequest_ReturnsValidationError()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var baselineId = SeedLockedRequirementBaseline(dbContext, projectId);
        var changeRequestId = Guid.NewGuid();

        dbContext.ChangeRequests.Add(new ChangeRequestEntity
        {
            Id = changeRequestId,
            ProjectId = projectId,
            Code = "CR-002",
            Title = "Pending change",
            RequestedBy = "pm@example.com",
            Reason = "Pending approval",
            Status = "submitted",
            Priority = "medium",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.ChangeImpacts.Add(new ChangeImpactEntity
        {
            Id = Guid.NewGuid(),
            ChangeRequestId = changeRequestId,
            ScopeImpact = "Scope",
            ScheduleImpact = "Schedule",
            QualityImpact = "Quality",
            SecurityImpact = "Security",
            PerformanceImpact = "Performance",
            RiskImpact = "Risk"
        });
        await dbContext.SaveChangesAsync();

        var sut = new ChangeControlCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new ChangeControlQueries(dbContext));

        var result = await sut.CreateBaselineRegistryAsync(
            new CreateBaselineRegistryRequest(projectId, "BL-REQ-3", "requirements", "requirement_baseline", baselineId.ToString(), changeRequestId),
            "pm@example.com",
            CancellationToken.None);

        Assert.Equal(ChangeControlCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.ApprovedChangeRequestRequired, result.ErrorCode);
    }

    [Fact]
    public async Task SupersedeBaselineRegistryAsync_EmergencyOverrideWithoutReason_IsDenied()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var baselineRecordId = Guid.NewGuid();

        dbContext.BaselineRegistry.Add(new BaselineRegistryEntity
        {
            Id = baselineRecordId,
            ProjectId = projectId,
            BaselineName = "BL-CFG-1",
            BaselineType = "configuration",
            SourceEntityType = "configuration_item",
            SourceEntityId = Guid.NewGuid().ToString(),
            Status = "locked",
            ApprovedBy = "approver@example.com",
            ApprovedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new ChangeControlCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new ChangeControlQueries(dbContext));

        var result = await sut.SupersedeBaselineRegistryAsync(
            baselineRecordId,
            new BaselineOverrideRequest(null, true, null),
            "admin@example.com",
            canEmergencyOverride: true,
            CancellationToken.None);

        Assert.Equal(ChangeControlCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.EmergencyOverrideReasonRequired, result.ErrorCode);
    }

    private static Guid SeedProject(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext)
    {
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = $"PRJ-{projectId.ToString()[..8]}",
            Name = "Change Control Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        return projectId;
    }

    private static Guid SeedLockedRequirementBaseline(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext, Guid projectId)
    {
        var baselineId = Guid.NewGuid();
        dbContext.Set<Operis_API.Modules.Requirements.Infrastructure.RequirementBaselineEntity>().Add(new Operis_API.Modules.Requirements.Infrastructure.RequirementBaselineEntity
        {
            Id = baselineId,
            ProjectId = projectId,
            BaselineName = "Requirements Locked Baseline",
            RequirementIdsJson = "[]",
            ApprovedBy = "approver@example.com",
            ApprovedAt = DateTimeOffset.UtcNow,
            Status = "locked"
        });
        return baselineId;
    }
}
