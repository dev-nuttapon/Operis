using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Modules.Requirements.Application;
using Operis_API.Modules.Verification.Application;
using Operis_API.Modules.Verification.Contracts;
using Operis_API.Modules.Verification.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Verification.Application;

public sealed class VerificationCommandsTests
{
    [Fact]
    public async Task ApproveTestPlanAsync_WithoutCriteria_ReturnsValidationError()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var testPlanId = Guid.NewGuid();

        dbContext.TestPlans.Add(new TestPlanEntity
        {
            Id = testPlanId,
            ProjectId = projectId,
            Code = "TP-001",
            Title = "Plan",
            ScopeSummary = "Scope",
            OwnerUserId = "qa@example.com",
            Status = "review",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var requirementQueries = new RequirementQueries(dbContext);
        var sut = new VerificationCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new VerificationQueries(dbContext), new RequirementCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), requirementQueries));
        var result = await sut.ApproveTestPlanAsync(testPlanId, new VerificationDecisionRequest("approve"), "approver@example.com", CancellationToken.None);

        Assert.Equal(VerificationCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.TestPlanCriteriaRequired, result.ErrorCode);
    }

    [Fact]
    public async Task ApproveUatSignoffAsync_WithoutEvidence_ReturnsValidationError()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var uatId = Guid.NewGuid();

        dbContext.UatSignoffs.Add(new UatSignoffEntity
        {
            Id = uatId,
            ProjectId = projectId,
            ReleaseId = "REL-001",
            ScopeSummary = "Release scope",
            Status = "submitted",
            SubmittedBy = "pm@example.com",
            SubmittedAt = DateTimeOffset.UtcNow,
            EvidenceRefsJson = "[]",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var requirementQueries = new RequirementQueries(dbContext);
        var sut = new VerificationCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new VerificationQueries(dbContext), new RequirementCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), requirementQueries));
        var result = await sut.ApproveUatSignoffAsync(uatId, new VerificationDecisionRequest("approve"), "approver@example.com", CancellationToken.None);

        Assert.Equal(VerificationCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.UatEvidenceRequired, result.ErrorCode);
    }

    private static Guid SeedProject(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext)
    {
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = $"PRJ-{projectId.ToString()[..8]}",
            Name = "Verification Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        return projectId;
    }
}
