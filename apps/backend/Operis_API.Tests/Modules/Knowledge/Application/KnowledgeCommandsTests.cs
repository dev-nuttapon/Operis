using Operis_API.Modules.Knowledge.Application;
using Operis_API.Modules.Knowledge.Contracts;
using Operis_API.Modules.Knowledge.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Knowledge.Application;

public sealed class KnowledgeCommandsTests
{
    [Fact]
    public async Task PublishLessonLearnedAsync_WithoutContext_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var lessonId = await SeedReviewedLessonAsync(dbContext, context: null, sourceRef: "AUD-1");
        var sut = new KnowledgeCommands(dbContext, new FakeAuditLogWriter(), new KnowledgeQueries(dbContext));

        var result = await sut.PublishLessonLearnedAsync(lessonId, new PublishLessonLearnedRequest("AUD-1", null, "Published summary", ["evidence-1"]), "pm@example.com", CancellationToken.None);

        Assert.Equal(KnowledgeCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.LessonContextRequired, result.ErrorCode);
    }

    [Fact]
    public async Task PublishLessonLearnedAsync_WithoutSourceOrEvidence_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var lessonId = await SeedReviewedLessonAsync(dbContext, context: "Sprint retrospective", sourceRef: null);
        var sut = new KnowledgeCommands(dbContext, new FakeAuditLogWriter(), new KnowledgeQueries(dbContext));

        var result = await sut.PublishLessonLearnedAsync(lessonId, new PublishLessonLearnedRequest(null, "Sprint retrospective", "Published summary", []), "pm@example.com", CancellationToken.None);

        Assert.Equal(KnowledgeCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.LessonSourceRequired, result.ErrorCode);
    }

    private static async Task<Guid> SeedReviewedLessonAsync(Operis_API.Infrastructure.Persistence.OperisDbContext dbContext, string? context, string? sourceRef)
    {
        var projectId = Guid.NewGuid();
        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = $"PRJ-{projectId.ToString()[..8]}",
            Name = "Knowledge Project",
            ProjectType = "Internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });

        var lessonId = Guid.NewGuid();
        dbContext.LessonsLearned.Add(new LessonLearnedEntity
        {
            Id = lessonId,
            ProjectId = projectId,
            Title = "Retrospective insight",
            Summary = "Keep evidence trails current.",
            LessonType = "process",
            OwnerUserId = "pm@example.com",
            Status = "reviewed",
            SourceRef = sourceRef,
            Context = context,
            LinkedEvidenceJson = "[]",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();
        return lessonId;
    }
}
