using Operis_API.Modules.Assessment.Application;
using Operis_API.Modules.Assessment.Contracts;
using Operis_API.Modules.Assessment.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Assessment.Application;

public sealed class AssessmentCommandsTests
{
    [Fact]
    public async Task CreatePackageAsync_WithoutScope_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var sut = new AssessmentCommands(dbContext, new FakeAuditLogWriter(), new AssessmentQueries(dbContext));

        var result = await sut.CreatePackageAsync(
            new CreateAssessmentPackageRequest(null, null, "Assess governance evidence"),
            "auditor@example.com",
            CancellationToken.None);

        Assert.Equal(AssessmentCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.AssessmentPackageScopeRequired, result.ErrorCode);
    }

    [Fact]
    public async Task CreateFindingAsync_WithoutTitle_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var packageId = Guid.NewGuid();

        dbContext.AssessmentPackages.Add(new AssessmentPackageEntity
        {
            Id = packageId,
            PackageCode = "APK-TEST-001",
            ProjectId = projectId,
            ProcessArea = "audit-capa",
            ScopeSummary = "Audit evidence bundle",
            Status = "draft",
            EvidenceReferencesJson = "[]",
            CreatedByUserId = "auditor@example.com",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new AssessmentCommands(dbContext, new FakeAuditLogWriter(), new AssessmentQueries(dbContext));
        var result = await sut.CreateFindingAsync(
            new CreateAssessmentFindingRequest(packageId, "", "Missing closure proof", "medium", "audit_plan", Guid.NewGuid().ToString(), null, null),
            "auditor@example.com",
            CancellationToken.None);

        Assert.Equal(AssessmentCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.AssessmentFindingTitleRequired, result.ErrorCode);
    }

    [Fact]
    public async Task CreateControlCatalogItemAsync_WithoutControlCode_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var sut = new AssessmentCommands(dbContext, new FakeAuditLogWriter(), new AssessmentQueries(dbContext));

        var result = await sut.CreateControlCatalogItemAsync(
            new CreateControlCatalogItemRequest("", "Control title", "cmmi", "project_governance", null, null),
            "auditor@example.com",
            CancellationToken.None);

        Assert.Equal(AssessmentCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.ControlCodeRequired, result.ErrorCode);
    }

    [Fact]
    public async Task CreateControlMappingAsync_WithoutTarget_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var controlId = Guid.NewGuid();

        dbContext.ControlCatalog.Add(new ControlCatalogEntity
        {
            Id = controlId,
            ControlCode = "CTRL-001",
            Title = "Control title",
            ControlSet = "cmmi",
            ProcessArea = "project_governance",
            Status = "draft",
            ProjectId = projectId,
            CreatedByUserId = "auditor@example.com",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new AssessmentCommands(dbContext, new FakeAuditLogWriter(), new AssessmentQueries(dbContext));
        var result = await sut.CreateControlMappingAsync(
            new CreateControlMappingRequest(controlId, projectId, "", "", "", "", null, null),
            "auditor@example.com",
            CancellationToken.None);

        Assert.Equal(AssessmentCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.ControlMappingTargetRequired, result.ErrorCode);
    }

    private static Guid SeedProject(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext)
    {
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = $"PRJ-{projectId.ToString()[..8]}",
            Name = "Assessment Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });

        return projectId;
    }
}
