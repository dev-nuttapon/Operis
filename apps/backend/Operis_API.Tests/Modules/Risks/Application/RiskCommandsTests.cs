using Operis_API.Modules.Risks.Application;
using Operis_API.Modules.Risks.Contracts;
using Operis_API.Modules.Risks.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Risks.Application;

public sealed class RiskCommandsTests
{
    [Fact]
    public async Task MitigateRiskAsync_WithoutMitigationPlan_ReturnsValidationError()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var riskId = Guid.NewGuid();
        dbContext.Risks.Add(new RiskEntity
        {
            Id = riskId,
            ProjectId = projectId,
            Code = "RISK-001",
            Title = "Unmitigated risk",
            Description = "Description",
            Probability = 4,
            Impact = 5,
            OwnerUserId = "owner@example.com",
            Status = "assessed",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new RiskCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new RiskQueries(dbContext));

        var result = await sut.MitigateRiskAsync(riskId, new RiskTransitionRequest("Need a plan", null, null), "owner@example.com", CancellationToken.None);

        Assert.Equal(RiskCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.RiskMitigationRequired, result.ErrorCode);
    }

    [Fact]
    public async Task IssueLifecycle_WithCompletedActions_ResolvesAndCloses()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var issueId = Guid.NewGuid();
        var actionId = Guid.NewGuid();

        dbContext.Issues.Add(new IssueEntity
        {
            Id = issueId,
            ProjectId = projectId,
            Code = "ISSUE-001",
            Title = "Open blocker",
            Description = "Description",
            OwnerUserId = "owner@example.com",
            Status = "in_progress",
            Severity = "high",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.IssueActions.Add(new IssueActionEntity
        {
            Id = actionId,
            IssueId = issueId,
            ActionDescription = "Fix it",
            AssignedTo = "engineer@example.com",
            Status = "completed",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new RiskCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new RiskQueries(dbContext));

        var resolveResult = await sut.ResolveIssueAsync(issueId, new IssueResolutionRequest("Implemented and verified"), "owner@example.com", CancellationToken.None);
        var closeResult = await sut.CloseIssueAsync(issueId, new IssueResolutionRequest("Closed after verification"), "owner@example.com", CancellationToken.None);

        Assert.Equal(RiskCommandStatus.Success, resolveResult.Status);
        Assert.Equal("resolved", resolveResult.Value?.Status);
        Assert.Equal(RiskCommandStatus.Success, closeResult.Status);
        Assert.Equal("closed", closeResult.Value?.Status);
    }

    [Fact]
    public async Task ResolveIssueAsync_WithOpenActions_ReturnsValidationError()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var issueId = Guid.NewGuid();

        dbContext.Issues.Add(new IssueEntity
        {
            Id = issueId,
            ProjectId = projectId,
            Code = "ISSUE-002",
            Title = "Blocked",
            Description = "Description",
            OwnerUserId = "owner@example.com",
            Status = "in_progress",
            Severity = "critical",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.IssueActions.Add(new IssueActionEntity
        {
            Id = Guid.NewGuid(),
            IssueId = issueId,
            ActionDescription = "Pending fix",
            AssignedTo = "engineer@example.com",
            Status = "open",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new RiskCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new RiskQueries(dbContext));

        var result = await sut.ResolveIssueAsync(issueId, new IssueResolutionRequest("Attempted closure"), "owner@example.com", CancellationToken.None);

        Assert.Equal(RiskCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.IssueOpenActionsExist, result.ErrorCode);
    }

    [Fact]
    public async Task CreateRiskAsync_AppendsBusinessAuditEvent()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        await dbContext.SaveChangesAsync();
        var businessWriter = new FakeBusinessAuditEventWriter();
        var sut = new RiskCommands(dbContext, new FakeAuditLogWriter(), businessWriter, new RiskQueries(dbContext));

        var result = await sut.CreateRiskAsync(
            new CreateRiskRequest(projectId, "RISK-010", "Capacity risk", "Description", 3, 4, "owner@example.com", null, null, null, null, null),
            "owner@example.com",
            CancellationToken.None);

        Assert.Equal(RiskCommandStatus.Success, result.Status);
        Assert.Contains(businessWriter.Entries, entry => entry.EventType == "risk_created" && entry.EntityType == "risk");
    }

    [Fact]
    public async Task ListIssuesAsync_HidesSensitiveIssuesWithoutPermission()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);

        dbContext.Issues.Add(new IssueEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Code = "ISSUE-PUBLIC",
            Title = "Public issue",
            Description = "Description",
            OwnerUserId = "owner@example.com",
            Status = "open",
            Severity = "low",
            IsSensitive = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.Issues.Add(new IssueEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Code = "ISSUE-SECRET",
            Title = "Sensitive issue",
            Description = "Description",
            OwnerUserId = "owner@example.com",
            Status = "open",
            Severity = "high",
            IsSensitive = true,
            SensitiveContext = "incident_linked",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new RiskQueries(dbContext);

        var result = await sut.ListIssuesAsync(new IssueListQuery(null, projectId, null, null, null, null, null), canReadSensitive: false, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("ISSUE-PUBLIC", result.Items[0].Code);
    }

    private static Guid SeedProject(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext)
    {
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = $"PRJ-{projectId.ToString()[..8]}",
            Name = "Risks Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        return projectId;
    }
}
