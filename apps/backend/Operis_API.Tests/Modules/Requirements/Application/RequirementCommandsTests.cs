using Operis_API.Modules.Requirements.Application;
using Operis_API.Modules.Requirements.Contracts;
using Operis_API.Modules.Requirements.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Requirements.Application;

public sealed class RequirementCommandsTests
{
    [Fact]
    public async Task CreateBaselineAsync_WhenRequirementsApprovedAndTraceable_LocksBaselineAndRequirements()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var requirementId = SeedApprovedRequirement(dbContext, projectId, "REQ-001");
        SeedTraceability(dbContext, requirementId, "document", "DOC-1");
        SeedTraceability(dbContext, requirementId, "test", "TEST-1");
        await dbContext.SaveChangesAsync();

        var auditWriter = new FakeAuditLogWriter();
        var businessWriter = new FakeBusinessAuditEventWriter();
        var queries = new RequirementQueries(dbContext);
        var sut = new RequirementCommands(dbContext, auditWriter, businessWriter, queries);

        var result = await sut.CreateBaselineAsync(
            new CreateRequirementBaselineRequest(projectId, "Sprint Baseline", [requirementId], "Release-ready scope"),
            "pm@example.com",
            CancellationToken.None);

        Assert.Equal(RequirementCommandStatus.Success, result.Status);
        Assert.Equal("locked", result.Value?.Status);
        Assert.Contains(auditWriter.Entries, entry => entry.Module == "requirements" && entry.Action == "baseline_create");
        Assert.Contains(businessWriter.Entries, entry => entry.EventType == "requirement.baseline.created");

        var requirement = await dbContext.Requirements.FindAsync(requirementId);
        Assert.Equal("baselined", requirement?.Status);
    }

    [Fact]
    public async Task CreateBaselineAsync_WhenMandatoryTraceabilityMissing_ReturnsValidationError()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var requirementId = SeedApprovedRequirement(dbContext, projectId, "REQ-002");
        SeedTraceability(dbContext, requirementId, "document", "DOC-2");
        await dbContext.SaveChangesAsync();

        var sut = new RequirementCommands(dbContext, new FakeAuditLogWriter(), new FakeBusinessAuditEventWriter(), new RequirementQueries(dbContext));

        var result = await sut.CreateBaselineAsync(
            new CreateRequirementBaselineRequest(projectId, "Blocked Baseline", [requirementId], "Missing downstream test"),
            "pm@example.com",
            CancellationToken.None);

        Assert.Equal(RequirementCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.TraceabilityIncomplete, result.ErrorCode);
    }

    [Fact]
    public async Task TraceabilityCreateAndDelete_AreAudited()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var requirementId = SeedApprovedRequirement(dbContext, projectId, "REQ-003");
        await dbContext.SaveChangesAsync();

        var auditWriter = new FakeAuditLogWriter();
        var businessWriter = new FakeBusinessAuditEventWriter();
        var sut = new RequirementCommands(dbContext, auditWriter, businessWriter, new RequirementQueries(dbContext));

        var createResult = await sut.CreateTraceabilityLinkAsync(
            new CreateTraceabilityLinkRequest("requirement", requirementId.ToString(), "document", "DOC-3", "implements"),
            "ba@example.com",
            CancellationToken.None);

        Assert.Equal(RequirementCommandStatus.Success, createResult.Status);

        var deleteResult = await sut.DeleteTraceabilityLinkAsync(createResult.Value!.Id, "ba@example.com", CancellationToken.None);

        Assert.Equal(RequirementCommandStatus.Success, deleteResult.Status);
        Assert.Contains(auditWriter.Entries, entry => entry.Action == "traceability_create");
        Assert.Contains(auditWriter.Entries, entry => entry.Action == "traceability_delete");
        Assert.Contains(businessWriter.Entries, entry => entry.EventType == "traceability.created");
        Assert.Contains(businessWriter.Entries, entry => entry.EventType == "traceability.deleted");
    }

    private static Guid SeedProject(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext)
    {
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = $"PRJ-{projectId.ToString()[..8]}",
            Name = "Requirements Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });

        return projectId;
    }

    private static Guid SeedApprovedRequirement(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext, Guid projectId, string code)
    {
        var requirementId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        dbContext.Requirements.Add(new RequirementEntity
        {
            Id = requirementId,
            ProjectId = projectId,
            Code = code,
            Title = $"{code} title",
            Description = "Requirement description",
            Priority = "high",
            OwnerUserId = "owner@example.com",
            Status = "approved",
            CurrentVersionId = versionId,
            CreatedAt = now,
            UpdatedAt = now
        });

        dbContext.RequirementVersions.Add(new RequirementVersionEntity
        {
            Id = versionId,
            RequirementId = requirementId,
            VersionNumber = 1,
            BusinessReason = "Business need",
            AcceptanceCriteria = "Acceptance criteria",
            SecurityImpact = "Medium",
            PerformanceImpact = "Low",
            Status = "approved",
            CreatedAt = now
        });

        return requirementId;
    }

    private static void SeedTraceability(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext, Guid requirementId, string targetType, string targetId)
    {
        dbContext.TraceabilityLinks.Add(new TraceabilityLinkEntity
        {
            Id = Guid.NewGuid(),
            SourceType = "requirement",
            SourceId = requirementId.ToString(),
            TargetType = targetType,
            TargetId = targetId,
            LinkRule = "implements",
            Status = "created",
            CreatedBy = "seed",
            CreatedAt = DateTimeOffset.UtcNow
        });
    }
}
