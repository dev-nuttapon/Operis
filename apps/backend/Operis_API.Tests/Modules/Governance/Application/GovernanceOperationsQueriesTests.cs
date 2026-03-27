using Operis_API.Modules.Governance.Application;
using Operis_API.Modules.Governance.Contracts;
using Operis_API.Modules.Governance.Infrastructure;
using Operis_API.Modules.Requirements.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Governance.Application;

public sealed class GovernanceOperationsQueriesTests
{
    [Fact]
    public async Task GetComplianceDashboardAsync_WithMissingRequirementBaseline_FlagsProjectAsAtRisk()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = "CMP-001",
            Name = "Compliance Project",
            ProjectType = "internal",
            Status = "active",
            Phase = "verification",
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.ProjectPlans.Add(new ProjectPlanEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = "Project Plan",
            ScopeSummary = "Scope",
            LifecycleModel = "iterative",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            TargetEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            OwnerUserId = "pm@example.com",
            Status = "baseline",
            MilestonesJson = "[]",
            RolesJson = "[]",
            RiskApproach = "Standard",
            QualityApproach = "Standard",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.Requirements.Add(new RequirementEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Code = "REQ-001",
            Title = "Requirement",
            Description = "Need governance baseline",
            Priority = "high",
            OwnerUserId = "ba@example.com",
            Status = "approved",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.TestPlans.Add(new Operis_API.Modules.Verification.Infrastructure.TestPlanEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Code = "TP-001",
            Title = "Verification plan",
            ScopeSummary = "Scope",
            OwnerUserId = "qa@example.com",
            Status = "approved",
            LinkedRequirementIdsJson = "[]",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var auditWriter = new FakeAuditLogWriter();
        var sut = new GovernanceOperationsQueries(dbContext, auditWriter);

        var result = await sut.GetComplianceDashboardAsync(new ComplianceDashboardQuery(projectId, null, 30, false), "compliance@example.com", CancellationToken.None);

        var project = Assert.Single(result.Projects);
        Assert.Equal(projectId, project.ProjectId);
        Assert.True(project.MissingArtifactCount > 0);
        Assert.Equal("at_risk", project.ReadinessState);
        Assert.Contains(result.ProcessAreas, area => area.ProcessArea == "requirements-traceability" && area.MissingArtifactCount > 0);
        Assert.Contains(auditWriter.Entries, entry => entry.Action == "generate_snapshot" && entry.EntityType == "compliance_snapshot");
    }
}
