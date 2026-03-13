using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Application;
using Operis_API.Modules.Users.Domain;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Users.Application;

public sealed class UserRegistrationQueriesTests
{
    [Fact]
    public async Task GetRegistrationPasswordSetupAsync_ReturnsExpectedDetailFlags()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var departmentId = Guid.NewGuid();
        var jobTitleId = Guid.NewGuid();

        dbContext.Departments.Add(new DepartmentEntity
        {
            Id = departmentId,
            Name = "Quality",
            DisplayOrder = 1,
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.JobTitles.Add(new JobTitleEntity
        {
            Id = jobTitleId,
            Name = "Reviewer",
            DisplayOrder = 1,
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.UserRegistrationRequests.Add(new UserRegistrationRequestEntity
        {
            Id = Guid.NewGuid(),
            Email = "candidate@example.com",
            FirstName = "Casey",
            LastName = "Ng",
            DepartmentId = departmentId,
            JobTitleId = jobTitleId,
            Status = RegistrationRequestStatus.Approved,
            RequestedAt = DateTimeOffset.UtcNow.AddDays(-2),
            PasswordSetupToken = "setup-token",
            PasswordSetupExpiresAt = DateTimeOffset.UtcNow.AddDays(2),
            PasswordSetupCompletedAt = null
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var sut = new UserRegistrationQueries(dbContext, auditLogWriter);

        var result = await sut.GetRegistrationPasswordSetupAsync("setup-token", CancellationToken.None);

        Assert.Equal(RegistrationPasswordSetupQueryStatus.Success, result.Status);
        Assert.NotNull(result.Response);
        Assert.False(result.Response!.IsExpired);
        Assert.False(result.Response.IsCompleted);
        Assert.Equal("Quality", result.Response.DepartmentName);
        Assert.Equal("Reviewer", result.Response.JobTitleName);
        Assert.Single(auditLogWriter.Entries);
    }
}
