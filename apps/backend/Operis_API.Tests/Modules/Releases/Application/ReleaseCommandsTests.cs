using Operis_API.Modules.Metrics.Infrastructure;
using Operis_API.Modules.Releases.Application;
using Operis_API.Modules.Releases.Contracts;
using Operis_API.Modules.Releases.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Releases.Application;

public sealed class ReleaseCommandsTests
{
    [Fact]
    public async Task ExecuteReleaseAsync_WithIncompleteChecklist_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var releaseId = Guid.NewGuid();
        dbContext.Releases.Add(new ReleaseEntity
        {
            Id = releaseId,
            ProjectId = projectId,
            ReleaseCode = "REL-001",
            Title = "Release 1",
            Status = "approved",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.DeploymentChecklists.Add(new DeploymentChecklistEntity
        {
            Id = Guid.NewGuid(),
            ReleaseId = releaseId,
            ChecklistItem = "Confirm backup",
            OwnerUserId = "ops@example.com",
            Status = "approved",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new ReleaseCommands(dbContext, new FakeAuditLogWriter(), new ReleaseQueries(dbContext));
        var result = await sut.ExecuteReleaseAsync(releaseId, new ExecuteReleaseRequest(null), "approver@example.com", CancellationToken.None);

        Assert.Equal(ReleaseCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.ReleaseChecklistIncomplete, result.ErrorCode);
    }

    [Fact]
    public async Task PublishReleaseNoteAsync_WithoutApprovedRelease_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = SeedProject(dbContext);
        var releaseId = Guid.NewGuid();
        var noteId = Guid.NewGuid();
        dbContext.Releases.Add(new ReleaseEntity
        {
            Id = releaseId,
            ProjectId = projectId,
            ReleaseCode = "REL-002",
            Title = "Release 2",
            Status = "draft",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.ReleaseNotes.Add(new ReleaseNoteEntity
        {
            Id = noteId,
            ReleaseId = releaseId,
            Summary = "Summary",
            IncludedChanges = "Changes",
            Status = "draft",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new ReleaseCommands(dbContext, new FakeAuditLogWriter(), new ReleaseQueries(dbContext));
        var result = await sut.PublishReleaseNoteAsync(noteId, "approver@example.com", CancellationToken.None);

        Assert.Equal(ReleaseCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.ReleaseNotesReleaseRequired, result.ErrorCode);
    }

    private static Guid SeedProject(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext)
    {
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = $"REL-{projectId.ToString()[..8]}",
            Name = "Release Project",
            ProjectType = "internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.QualityGateResults.Add(new QualityGateResultEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            GateType = "release_readiness",
            EvaluatedAt = DateTimeOffset.UtcNow,
            Result = "passed",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        return projectId;
    }
}
