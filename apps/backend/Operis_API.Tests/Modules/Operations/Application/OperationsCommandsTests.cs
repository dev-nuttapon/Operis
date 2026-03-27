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

    [Fact]
    public async Task AddAccessRecertificationDecisionAsync_WithoutRationale_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var scheduleId = Guid.NewGuid();
        dbContext.AccessRecertificationSchedules.Add(new AccessRecertificationScheduleEntity
        {
            Id = scheduleId,
            ScopeType = "role",
            ScopeRef = "finance-approver",
            PlannedAt = DateTimeOffset.UtcNow.AddDays(7),
            ReviewOwnerUserId = "owner@example.com",
            Status = "planned",
            SubjectUsersJson = "[\"user-1\"]",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new OperationsCommands(dbContext, new FakeAuditLogWriter());
        var result = await sut.AddAccessRecertificationDecisionAsync(
            scheduleId,
            new AddAccessRecertificationDecisionRequest("user-1", "revoked", ""),
            "owner@example.com",
            CancellationToken.None);

        Assert.Equal(OperationsCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.AccessRecertificationDecisionRationaleRequired, result.ErrorCode);
    }

    [Fact]
    public async Task CompleteAccessRecertificationAsync_WithPendingDecisions_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var scheduleId = Guid.NewGuid();
        dbContext.AccessRecertificationSchedules.Add(new AccessRecertificationScheduleEntity
        {
            Id = scheduleId,
            ScopeType = "role",
            ScopeRef = "finance-approver",
            PlannedAt = DateTimeOffset.UtcNow.AddDays(7),
            ReviewOwnerUserId = "owner@example.com",
            Status = "approved",
            SubjectUsersJson = "[\"user-1\",\"user-2\"]",
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.AccessRecertificationDecisions.Add(new AccessRecertificationDecisionEntity
        {
            Id = Guid.NewGuid(),
            ScheduleId = scheduleId,
            SubjectUserId = "user-1",
            Decision = "kept",
            Reason = "Still required",
            DecidedBy = "owner@example.com",
            DecidedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new OperationsCommands(dbContext, new FakeAuditLogWriter());
        var result = await sut.CompleteAccessRecertificationAsync(scheduleId, "owner@example.com", CancellationToken.None);

        Assert.Equal(OperationsCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.AccessRecertificationPendingDecisions, result.ErrorCode);
    }
}
