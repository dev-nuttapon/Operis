using Operis_API.Modules.Governance.Application;
using Operis_API.Modules.Governance.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Governance.Application;

public sealed class PolicyAcknowledgementCommandsTests
{
    [Fact]
    public async Task CreatePolicyAsync_WithoutTitle_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var sut = new GovernanceOperationsCommands(dbContext, new FakeAuditLogWriter());

        var result = await sut.CreatePolicyAsync(
            new CreatePolicyRequest("POL-001", "", null, DateTimeOffset.UtcNow, true),
            "policy.manager@example.com",
            CancellationToken.None);

        Assert.Equal(GovernanceCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.PolicyTitleRequired, result.ErrorCode);
    }

    [Fact]
    public async Task CreatePolicyCampaignAsync_WithoutScope_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var policyId = Guid.NewGuid();
        dbContext.Policies.Add(new Operis_API.Modules.Governance.Infrastructure.PolicyEntity
        {
            Id = policyId,
            PolicyCode = "POL-001",
            Title = "Security Policy",
            EffectiveDate = DateTimeOffset.UtcNow,
            RequiresAttestation = true,
            Status = "published",
            PublishedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new GovernanceOperationsCommands(dbContext, new FakeAuditLogWriter());
        var result = await sut.CreatePolicyCampaignAsync(
            new CreatePolicyCampaignRequest(policyId, "CMP-001", "Q1 Attestation", "all_users", "", DateTimeOffset.UtcNow.AddDays(7)),
            "policy.manager@example.com",
            CancellationToken.None);

        Assert.Equal(GovernanceCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.PolicyCampaignScopeRequired, result.ErrorCode);
    }
}
