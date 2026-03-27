using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Knowledge;
using Operis_API.Modules.Knowledge.Application;
using Operis_API.Modules.Knowledge.Contracts;
using Operis_API.Shared.Security;

namespace Operis_API.Tests.Modules.Knowledge;

public sealed class KnowledgeModuleHandlerTests
{
    [Fact]
    public async Task PublishLessonLearnedAsync_WithoutManagePermission_ReturnsForbidden()
    {
        var result = await InvokePublishLessonLearnedAsync(CreateKnowledgeViewerPrincipal(), new FakeKnowledgeCommands());

        var httpContext = Operis_API.Tests.Support.TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static async Task<IResult> InvokePublishLessonLearnedAsync(ClaimsPrincipal principal, IKnowledgeCommands commands)
    {
        var method = typeof(KnowledgeModule).GetMethod("PublishLessonLearnedAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("KnowledgeModule.PublishLessonLearnedAsync was not found.");

        var task = (Task<IResult>)method.Invoke(null, [principal, Guid.NewGuid(), new PublishLessonLearnedRequest("AUD-1", "Sprint retrospective", "Lesson summary", ["evidence-1"]), commands, new PermissionMatrix(), CancellationToken.None])!;
        return await task;
    }

    private static ClaimsPrincipal CreateKnowledgeViewerPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:knowledge_viewer")], "TestAuth"));

    private sealed class FakeKnowledgeCommands : IKnowledgeCommands
    {
        public Task<KnowledgeCommandResult<LessonLearnedItem>> CreateLessonLearnedAsync(CreateLessonLearnedRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<KnowledgeCommandResult<LessonLearnedItem>> UpdateLessonLearnedAsync(Guid lessonId, UpdateLessonLearnedRequest request, string? actorUserId, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<KnowledgeCommandResult<LessonLearnedItem>> PublishLessonLearnedAsync(Guid lessonId, PublishLessonLearnedRequest request, string? actorUserId, CancellationToken cancellationToken) =>
            Task.FromResult(new KnowledgeCommandResult<LessonLearnedItem>(KnowledgeCommandStatus.Success, new LessonLearnedItem(lessonId, Guid.NewGuid(), "Knowledge Project", "Retrospective insight", request.Summary ?? "Lesson summary", "process", actorUserId ?? "pm@example.com", "published", request.SourceRef, request.Context, null, null, null, request.LinkedEvidence ?? [], DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)));
    }
}
