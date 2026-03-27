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
            new CreateExternalDependencyRequest("Keycloak", "identity_provider", null, "", "high", DateTimeOffset.UtcNow.AddDays(30), "active"),
            "system@example.com",
            CancellationToken.None);

        Assert.Equal(OperationsCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.DependencyOwnerRequired, result.ErrorCode);
    }

    [Fact]
    public async Task CreateSupplierAgreementAsync_WithoutEvidence_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var supplierId = Guid.NewGuid();
        dbContext.Suppliers.Add(new SupplierEntity
        {
            Id = supplierId,
            Name = "Acme Services",
            SupplierType = "vendor",
            OwnerUserId = "owner@example.com",
            Criticality = "high",
            Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new OperationsCommands(dbContext, new FakeAuditLogWriter());
        var result = await sut.CreateSupplierAgreementAsync(
            new CreateSupplierAgreementRequest(supplierId, "sla", DateOnly.FromDateTime(DateTime.UtcNow), null, "24x7 support", "", "draft"),
            "system@example.com",
            CancellationToken.None);

        Assert.Equal(OperationsCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.SupplierAgreementEvidenceRequired, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateSupplierAsync_WithActiveAgreementAndArchiveStatus_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var supplierId = Guid.NewGuid();
        dbContext.Suppliers.Add(new SupplierEntity
        {
            Id = supplierId,
            Name = "Acme Services",
            SupplierType = "vendor",
            OwnerUserId = "owner@example.com",
            Criticality = "high",
            Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.SupplierAgreements.Add(new SupplierAgreementEntity
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            AgreementType = "msa",
            EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            EvidenceRef = "minio://evidence/msa.pdf",
            Status = "Active",
            SlaTerms = "governed support terms",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new OperationsCommands(dbContext, new FakeAuditLogWriter());
        var result = await sut.UpdateSupplierAsync(
            supplierId,
            new UpdateSupplierRequest("Acme Services", "vendor", "owner@example.com", "high", null, "archived"),
            "system@example.com",
            CancellationToken.None);

        Assert.Equal(OperationsCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.SupplierActiveAgreementExists, result.ErrorCode);
    }
}
