using Operis_API.Modules.Governance.Application;
using Operis_API.Modules.Governance.Contracts;
using Operis_API.Modules.Governance.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Governance.Application;

public sealed class GovernanceOperationsCommandsTests
{
    [Fact]
    public async Task CreateSlaRuleAsync_WithoutEscalationPolicy_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var sut = new GovernanceOperationsCommands(dbContext, new FakeAuditLogWriter());

        var result = await sut.CreateSlaRuleAsync(new CreateSlaRuleRequest("workflow", "approval-flow", 8, "", "draft", null), "compliance@example.com", CancellationToken.None);

        Assert.Equal(GovernanceCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.SlaEscalationPolicyRequired, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateRetentionPolicyAsync_WithInvalidTransition_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var policyId = Guid.NewGuid();
        dbContext.RetentionPolicies.Add(new RetentionPolicyEntity
        {
            Id = policyId,
            PolicyCode = "RET-001",
            AppliesTo = "documents",
            RetentionPeriodDays = 365,
            ArchiveRule = "archive",
            Status = "draft",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new GovernanceOperationsCommands(dbContext, new FakeAuditLogWriter());
        var result = await sut.UpdateRetentionPolicyAsync(policyId, new UpdateRetentionPolicyRequest("RET-001", "documents", 365, "archive", "active", null), "compliance@example.com", CancellationToken.None);

        Assert.Equal(GovernanceCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.InvalidWorkflowTransition, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateDesignReviewAsync_ApproveWithoutDecisionReason_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        var architectureId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();

        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = "ARC-100",
            Name = "Architecture Project",
            ProjectType = "internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.ArchitectureRecords.Add(new ArchitectureRecordEntity
        {
            Id = architectureId,
            ProjectId = projectId,
            Title = "Core service design",
            ArchitectureType = "application",
            OwnerUserId = "owner@example.com",
            Status = "reviewed",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.DesignReviews.Add(new DesignReviewEntity
        {
            Id = reviewId,
            ArchitectureRecordId = architectureId,
            ReviewType = "architecture",
            Status = "in_review",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new GovernanceOperationsCommands(dbContext, new FakeAuditLogWriter());
        var result = await sut.UpdateDesignReviewAsync(reviewId, new UpdateDesignReviewRequest(architectureId, "architecture", "architect@example.com", "approved", null, "Summary", "Concern", "evidence"), "architect@example.com", CancellationToken.None);

        Assert.Equal(GovernanceCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.DesignReviewDecisionReasonRequired, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateIntegrationReviewAsync_ApplyWithoutApproval_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var reviewId = Guid.NewGuid();
        dbContext.IntegrationReviews.Add(new IntegrationReviewEntity
        {
            Id = reviewId,
            ScopeRef = "payments-api",
            IntegrationType = "api",
            Status = "in_review",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new GovernanceOperationsCommands(dbContext, new FakeAuditLogWriter());
        var result = await sut.UpdateIntegrationReviewAsync(reviewId, new UpdateIntegrationReviewRequest("payments-api", "api", "architect@example.com", "applied", "Apply now", "Risk", "Impact", "evidence"), "architect@example.com", CancellationToken.None);

        Assert.Equal(GovernanceCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.IntegrationReviewApprovalRequired, result.ErrorCode);
    }
}
