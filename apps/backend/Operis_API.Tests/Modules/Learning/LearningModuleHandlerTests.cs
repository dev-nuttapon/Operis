using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Operis_API.Modules.Learning;
using Operis_API.Modules.Learning.Application;
using Operis_API.Modules.Learning.Contracts;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Learning;

public sealed class LearningModuleHandlerTests
{
    [Fact]
    public async Task ListTrainingCoursesAsync_WithoutPermission_ReturnsForbidden()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var queries = new LearningQueries(dbContext);
        var method = typeof(LearningModule).GetMethod("ListTrainingCoursesAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("LearningModule.ListTrainingCoursesAsync was not found.");

        var task = (Task<IResult>)method.Invoke(
            null,
            [CreateUnauthorizedPrincipal(), new TrainingCourseListQuery(null, null, 1, 25), queries, new PermissionMatrix(), CancellationToken.None])!;

        var result = await task;
        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task CreateTrainingCourseAsync_WithoutManagePermission_ReturnsForbidden()
    {
        var method = typeof(LearningModule).GetMethod("CreateTrainingCourseAsync", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("LearningModule.CreateTrainingCourseAsync was not found.");

        var commands = new FakeLearningCommands();
        var task = (Task<IResult>)method.Invoke(
            null,
            [CreateUnauthorizedPrincipal(), new CreateTrainingCourseRequest(null, "Secure Coding", null, null, null, null, 12), commands, new PermissionMatrix(), CancellationToken.None])!;

        var result = await task;
        var httpContext = TestHttpContextFactory.Create();
        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    private static ClaimsPrincipal CreateUnauthorizedPrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "operis:employee"), new Claim(ClaimTypes.Email, "user@example.com")], "TestAuth"));

    private sealed class FakeLearningCommands : ILearningCommands
    {
        public Task<LearningCommandResult<TrainingCourseResponse>> CreateTrainingCourseAsync(CreateTrainingCourseRequest request, string? actor, CancellationToken cancellationToken) =>
            Task.FromResult(new LearningCommandResult<TrainingCourseResponse>(LearningCommandStatus.Success, new TrainingCourseResponse(Guid.NewGuid(), request.CourseCode, request.Title, request.Description, request.Provider, request.DeliveryMode, request.AudienceScope, request.ValidityMonths, "draft", 0, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)));

        public Task<LearningCommandResult<TrainingCourseResponse>> UpdateTrainingCourseAsync(Guid courseId, UpdateTrainingCourseRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<LearningCommandResult<TrainingCourseResponse>> TransitionTrainingCourseAsync(Guid courseId, TransitionTrainingCourseRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<LearningCommandResult<RoleTrainingRequirementResponse>> CreateRoleTrainingRequirementAsync(CreateRoleTrainingRequirementRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<LearningCommandResult<RoleTrainingRequirementResponse>> UpdateRoleTrainingRequirementAsync(Guid requirementId, UpdateRoleTrainingRequirementRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<LearningCommandResult<TrainingCompletionResponse>> RecordTrainingCompletionAsync(RecordTrainingCompletionRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<LearningCommandResult<TrainingCompletionResponse>> UpdateTrainingCompletionAsync(Guid completionId, UpdateTrainingCompletionRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<LearningCommandResult<CompetencyReviewResponse>> CreateCompetencyReviewAsync(CreateCompetencyReviewRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<LearningCommandResult<CompetencyReviewResponse>> UpdateCompetencyReviewAsync(Guid reviewId, UpdateCompetencyReviewRequest request, string? actor, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
