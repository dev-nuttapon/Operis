using Operis_API.Modules.Audits.Application;
using Operis_API.Modules.Audits.Contracts;
using Operis_API.Modules.Audits.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Audits.Application;

public sealed class AuditComplianceCommandsTests
{
    [Fact]
    public async Task CreateEvidenceExportAsync_WithoutScope_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var sut = new AuditComplianceCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new AuditComplianceQueries(dbContext));

        var result = await sut.CreateEvidenceExportAsync(new CreateEvidenceExportRequest("", "", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, ["audit_logs"]), "auditor@example.com", CancellationToken.None);

        Assert.Equal(AuditComplianceCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.ExportScopeRequired, result.ErrorCode);
    }

    [Fact]
    public async Task CreateEvidenceExportAsync_WithInvalidDateRange_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var sut = new AuditComplianceCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new AuditComplianceQueries(dbContext));

        var result = await sut.CreateEvidenceExportAsync(new CreateEvidenceExportRequest("project", "PRJ-001", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(-1), ["audit_logs"]), "auditor@example.com", CancellationToken.None);

        Assert.Equal(AuditComplianceCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.ExportDateRangeRequired, result.ErrorCode);
    }

    [Fact]
    public async Task CloseAuditFindingAsync_WithoutResolutionSummary_ReturnsValidationError()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var auditPlanId = Guid.NewGuid();
        var findingId = Guid.NewGuid();

        dbContext.AuditPlans.Add(new AuditPlanEntity
        {
            Id = auditPlanId,
            ProjectId = projectId,
            Title = "Internal audit",
            Scope = "Project governance",
            Criteria = "CMMI",
            PlannedAt = DateTimeOffset.UtcNow,
            Status = "findings_issued",
            OwnerUserId = "auditor@example.com",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.AuditFindings.Add(new AuditFindingEntity
        {
            Id = findingId,
            AuditPlanId = auditPlanId,
            Code = "F-001",
            Title = "Open item",
            Description = "Finding description",
            Severity = "high",
            Status = "verified",
            OwnerUserId = "owner@example.com",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new AuditComplianceCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new AuditComplianceQueries(dbContext));
        var result = await sut.CloseAuditFindingAsync(findingId, new CloseAuditFindingRequest(""), "auditor@example.com", CancellationToken.None);

        Assert.Equal(AuditComplianceCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.AuditFindingResolutionRequired, result.ErrorCode);
    }

    private static Guid SeedProject(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext)
    {
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = $"PRJ-{projectId.ToString()[..8]}",
            Name = "Audit Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        return projectId;
    }
}
