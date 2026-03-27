using Operis_API.Modules.Learning.Application;
using Operis_API.Modules.Learning.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Learning.Application;

public sealed class LearningCommandsTests
{
    [Fact]
    public async Task CreateRoleTrainingRequirementAsync_WithoutRole_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var courseId = Guid.NewGuid();
        dbContext.TrainingCourses.Add(new Operis_API.Modules.Learning.Infrastructure.TrainingCourseEntity
        {
            Id = courseId,
            Title = "Secure Coding",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new LearningCommands(dbContext, new FakeAuditLogWriter(), new LearningQueries(dbContext));
        var result = await sut.CreateRoleTrainingRequirementAsync(
            new CreateRoleTrainingRequirementRequest(courseId, Guid.Empty, 30, 12, null),
            "training.manager@example.com",
            CancellationToken.None);

        Assert.Equal(LearningCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.TrainingRequirementRoleRequired, result.ErrorCode);
    }

    [Fact]
    public async Task RecordTrainingCompletionAsync_CompletedWithoutDate_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        dbContext.Projects.Add(new ProjectEntity
        {
            Id = projectId,
            Code = "LRN-001",
            Name = "Learning Project",
            ProjectType = "internal",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.ProjectRoles.Add(new ProjectRoleEntity
        {
            Id = roleId,
            ProjectId = projectId,
            Name = "QA Lead",
            Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.TrainingCourses.Add(new Operis_API.Modules.Learning.Infrastructure.TrainingCourseEntity
        {
            Id = courseId,
            CourseCode = "TRN-001",
            Title = "CMMI Foundations",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        dbContext.RoleTrainingRequirements.Add(new Operis_API.Modules.Learning.Infrastructure.RoleTrainingRequirementEntity
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            ProjectRoleId = roleId,
            RequiredWithinDays = 15,
            RenewalIntervalMonths = 12,
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new LearningCommands(dbContext, new FakeAuditLogWriter(), new LearningQueries(dbContext));
        var result = await sut.RecordTrainingCompletionAsync(
            new RecordTrainingCompletionRequest(courseId, roleId, projectId, "qa.lead@example.com", "completed", DateTimeOffset.UtcNow, null, null, null, null),
            "training.manager@example.com",
            CancellationToken.None);

        Assert.Equal(LearningCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.TrainingCompletionDateRequired, result.ErrorCode);
    }
}
