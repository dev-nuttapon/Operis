using Operis_API.Modules.Governance.Application;
using Operis_API.Modules.Governance.Contracts;
using Operis_API.Modules.Governance.Infrastructure;
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
}
