using Operis_API.Modules.Operations.Application;
using Operis_API.Modules.Operations.Contracts;
using Operis_API.Modules.Operations.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Operations.Application;

public sealed class OperationsCommandsTests
{
    [Fact]
    public async Task ApproveAccessReviewAsync_WithoutDecision_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var reviewId = Guid.NewGuid();
        dbContext.AccessReviews.Add(new AccessReviewEntity
        {
            Id = reviewId,
            ScopeType = "role",
            ScopeRef = "finance-approver",
            ReviewCycle = "Q2-2026",
            ReviewedBy = "auditor@example.com",
            Status = "Scheduled",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new OperationsCommands(dbContext, new FakeAuditLogWriter());
        var result = await sut.ApproveAccessReviewAsync(reviewId, new ApproveAccessReviewRequest("", "Documented rationale"), "compliance@example.com", CancellationToken.None);

        Assert.Equal(OperationsCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.AccessReviewDecisionRequired, result.ErrorCode);
    }

    [Fact]
    public async Task CreateExternalDependencyAsync_WithoutOwner_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var sut = new OperationsCommands(dbContext, new FakeAuditLogWriter());

        var result = await sut.CreateExternalDependencyAsync(
            new CreateExternalDependencyRequest("Keycloak", "identity_provider", "", "high", DateTimeOffset.UtcNow.AddDays(30), "active"),
            "system@example.com",
            CancellationToken.None);

        Assert.Equal(OperationsCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.DependencyOwnerRequired, result.ErrorCode);
    }
}
