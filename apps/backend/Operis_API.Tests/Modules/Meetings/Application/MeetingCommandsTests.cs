using Operis_API.Modules.Meetings.Application;
using Operis_API.Modules.Meetings.Contracts;
using Operis_API.Modules.Meetings.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Meetings.Application;

public sealed class MeetingCommandsTests
{
    [Fact]
    public async Task UpdateMeetingMinutesAsync_ApproveWithoutAttendeesOrSummary_ReturnsValidationError()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var meetingId = Guid.NewGuid();

        dbContext.MeetingRecords.Add(new MeetingRecordEntity
        {
            Id = meetingId,
            ProjectId = projectId,
            MeetingType = "review",
            Title = "Review meeting",
            MeetingAt = DateTimeOffset.UtcNow,
            FacilitatorUserId = "ba@example.com",
            Status = "draft",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.MeetingMinutes.Add(new MeetingMinutesEntity
        {
            Id = Guid.NewGuid(),
            MeetingRecordId = meetingId,
            Status = "draft",
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new MeetingCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new MeetingQueries(dbContext));

        var result = await sut.UpdateMeetingMinutesAsync(meetingId, new UpdateMeetingMinutesRequest(null, null, null, "approved", null), "ba@example.com", CancellationToken.None);

        Assert.Equal(MeetingCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.MeetingAttendeesRequired, result.ErrorCode);
    }

    [Fact]
    public async Task ApplyDecisionAsync_WithoutApprovedStatus_ReturnsValidationError()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var decisionId = Guid.NewGuid();

        dbContext.Decisions.Add(new DecisionEntity
        {
            Id = decisionId,
            ProjectId = projectId,
            Code = "DEC-001",
            Title = "Decision",
            DecisionType = "approval",
            Rationale = "Rationale",
            ImpactedArtifactsJson = "[]",
            Status = "proposed",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new MeetingCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new MeetingQueries(dbContext));

        var result = await sut.ApplyDecisionAsync(decisionId, new DecisionTransitionRequest("apply"), "pm@example.com", CancellationToken.None);

        Assert.Equal(MeetingCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.InvalidWorkflowTransition, result.ErrorCode);
    }

    [Fact]
    public async Task DecisionLifecycle_ApproveThenApply_SucceedsAndAudits()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var decisionId = Guid.NewGuid();
        var businessWriter = new FakeBusinessAuditEventWriter();

        dbContext.Decisions.Add(new DecisionEntity
        {
            Id = decisionId,
            ProjectId = projectId,
            Code = "DEC-002",
            Title = "Approved decision",
            DecisionType = "governance",
            Rationale = "Clear rationale",
            ImpactedArtifactsJson = "[\"REQ-1\"]",
            Status = "proposed",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new MeetingCommands(dbContext, new FakeAuditLogWriter(), businessWriter, new MeetingQueries(dbContext));

        var approveResult = await sut.ApproveDecisionAsync(decisionId, new DecisionTransitionRequest("approved"), "approver@example.com", CancellationToken.None);
        var applyResult = await sut.ApplyDecisionAsync(decisionId, new DecisionTransitionRequest("applied"), "pm@example.com", CancellationToken.None);

        Assert.Equal(MeetingCommandStatus.Success, approveResult.Status);
        Assert.Equal("approved", approveResult.Value?.Status);
        Assert.Equal(MeetingCommandStatus.Success, applyResult.Status);
        Assert.Equal("applied", applyResult.Value?.Status);
        Assert.Contains(businessWriter.Entries, entry => entry.EventType == "decision_applied");
    }

    [Fact]
    public async Task ListDecisionsAsync_HidesRestrictedItemsWithoutPermission()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);

        dbContext.Decisions.Add(new DecisionEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Code = "DEC-PUB",
            Title = "Public decision",
            DecisionType = "approval",
            Rationale = "Rationale",
            ImpactedArtifactsJson = "[]",
            Status = "proposed",
            IsRestricted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.Decisions.Add(new DecisionEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Code = "DEC-SEC",
            Title = "Restricted decision",
            DecisionType = "change",
            Rationale = "Rationale",
            ImpactedArtifactsJson = "[]",
            Status = "proposed",
            IsRestricted = true,
            Classification = "confidential",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new MeetingQueries(dbContext);

        var result = await sut.ListDecisionsAsync(new DecisionListQuery(null, projectId, null, null, null), canReadRestricted: false, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("DEC-PUB", result.Items[0].Code);
    }

    private static Guid SeedProject(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext)
    {
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = $"PRJ-{projectId.ToString()[..8]}",
            Name = "Meetings Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        return projectId;
    }
}
