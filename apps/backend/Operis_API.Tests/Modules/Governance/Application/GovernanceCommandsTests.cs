using Operis_API.Modules.Governance.Application;
using Operis_API.Modules.Governance.Contracts;
using Operis_API.Modules.Governance.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Governance.Application;

public sealed class GovernanceCommandsTests
{
    [Fact]
    public async Task ApproveTailoringRecordAsync_WhenSubmitted_StoresApproverAndRationale()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = "PRJ-001",
            Name = "Governance Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.Set<TailoringRecordEntity>().Add(new TailoringRecordEntity
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            ProjectId = projectId,
            RequesterUserId = "pm@example.com",
            RequestedChange = "Skip one template",
            Reason = "Pilot project",
            ImpactSummary = "Low",
            Status = "submitted",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var auditWriter = new FakeAuditLogWriter();
        var businessWriter = new FakeBusinessAuditEventWriter();
        var sut = new GovernanceCommands(dbContext, auditWriter, businessWriter);

        var result = await sut.ApproveTailoringRecordAsync(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "compliance@example.com",
            new TailoringDecisionRequest("approved", "Approved with compensating control"),
            CancellationToken.None);

        Assert.Equal(GovernanceCommandStatus.Success, result.Status);
        Assert.Equal("approved", result.Value?.Status);
        Assert.Equal("compliance@example.com", result.Value?.ApprovedBy);
        Assert.Contains(businessWriter.Entries, entry => entry.EventType == "tailoring_approved" && entry.Reason == "Approved with compensating control");
    }

    [Fact]
    public async Task BaselineProjectPlanAsync_WhenApproved_MovesToBaseline()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = "PRJ-002",
            Name = "Baseline Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.Set<ProjectPlanEntity>().Add(new ProjectPlanEntity
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            ProjectId = projectId,
            Name = "Plan A",
            ScopeSummary = "Scope",
            LifecycleModel = "Waterfall",
            StartDate = new DateOnly(2026, 3, 1),
            TargetEndDate = new DateOnly(2026, 6, 1),
            OwnerUserId = "pm@example.com",
            Status = "approved",
            MilestonesJson = "[]",
            RolesJson = "[]",
            RiskApproach = "Managed",
            QualityApproach = "Checked",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new GovernanceCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter());
        var result = await sut.BaselineProjectPlanAsync(Guid.Parse("22222222-2222-2222-2222-222222222222"), CancellationToken.None);

        Assert.Equal(GovernanceCommandStatus.Success, result.Status);
        Assert.Equal("baseline", result.Value?.Status);
    }

    [Fact]
    public async Task ApproveProjectPlanAsync_WithoutReason_ReturnsValidationError()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = "PRJ-003",
            Name = "Approval Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.Set<ProjectPlanEntity>().Add(new ProjectPlanEntity
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            ProjectId = projectId,
            Name = "Plan B",
            ScopeSummary = "Scope",
            LifecycleModel = "Iterative",
            StartDate = new DateOnly(2026, 3, 1),
            TargetEndDate = new DateOnly(2026, 5, 1),
            OwnerUserId = "pm@example.com",
            Status = "review",
            MilestonesJson = "[]",
            RolesJson = "[]",
            RiskApproach = "Managed",
            QualityApproach = "Checked",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new GovernanceCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter());
        var result = await sut.ApproveProjectPlanAsync(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "approver@example.com",
            new ProjectPlanApprovalRequest(""),
            CancellationToken.None);

        Assert.Equal(GovernanceCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.ApprovalReasonRequired, result.ErrorCode);
    }
}
