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

    [Fact]
    public async Task UpdatePrivilegedAccessEventAsync_UseWithoutApproval_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var eventId = Guid.NewGuid();
        dbContext.PrivilegedAccessEvents.Add(new PrivilegedAccessEventEntity
        {
            Id = eventId,
            RequestedBy = "requester@example.com",
            RequestedAt = DateTimeOffset.UtcNow,
            Status = "requested",
            Reason = "Break glass for incident triage",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new OperationsCommands(dbContext, new FakeAuditLogWriter());
        var result = await sut.UpdatePrivilegedAccessEventAsync(
            eventId,
            new UpdatePrivilegedAccessEventRequest("requester@example.com", null, "operator@example.com", DateTimeOffset.UtcNow, null, DateTimeOffset.UtcNow, null, "used", "Break glass for incident triage"),
            "security@example.com",
            CancellationToken.None);

        Assert.Equal(OperationsCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.PrivilegedAccessApprovalRequired, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateSecurityIncidentAsync_CloseWithoutResolutionSummary_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var incidentId = Guid.NewGuid();
        dbContext.SecurityIncidents.Add(new SecurityIncidentEntity
        {
            Id = incidentId,
            Code = "SEC-001",
            Title = "Credential misuse",
            Severity = "high",
            ReportedAt = DateTimeOffset.UtcNow.AddHours(-4),
            OwnerUserId = "owner@example.com",
            Status = "resolved",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new OperationsCommands(dbContext, new FakeAuditLogWriter());
        var result = await sut.UpdateSecurityIncidentAsync(
            incidentId,
            new UpdateSecurityIncidentRequest(null, "SEC-001", "Credential misuse", "high", DateTimeOffset.UtcNow.AddHours(-4), "owner@example.com", "closed", ""),
            "security@example.com",
            CancellationToken.None);

        Assert.Equal(OperationsCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.SecurityIncidentResolutionRequired, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateSecretRotationAsync_VerifyWithoutVerifier_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var rotationId = Guid.NewGuid();
        dbContext.SecretRotations.Add(new SecretRotationEntity
        {
            Id = rotationId,
            SecretScope = "keycloak/admin-client",
            PlannedAt = DateTimeOffset.UtcNow.AddDays(-1),
            RotatedAt = DateTimeOffset.UtcNow.AddHours(-2),
            Status = "rotated",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new OperationsCommands(dbContext, new FakeAuditLogWriter());
        var result = await sut.UpdateSecretRotationAsync(
            rotationId,
            new UpdateSecretRotationRequest("keycloak/admin-client", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddHours(-2), "", DateTimeOffset.UtcNow, "verified"),
            "security@example.com",
            CancellationToken.None);

        Assert.Equal(OperationsCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.SecretRotationVerificationRequired, result.ErrorCode);
    }
}
