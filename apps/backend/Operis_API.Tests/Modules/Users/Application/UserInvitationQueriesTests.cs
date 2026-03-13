using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Application;
using Operis_API.Modules.Users.Domain;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Users.Application;

public sealed class UserInvitationQueriesTests
{
    [Fact]
    public async Task GetInvitationByTokenAsync_ReturnsExpiredStatusWhenPendingInvitationIsExpired()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var departmentId = Guid.NewGuid();
        var jobTitleId = Guid.NewGuid();

        dbContext.Departments.Add(new DepartmentEntity
        {
            Id = departmentId,
            Name = "Operations",
            DisplayOrder = 1,
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.JobTitles.Add(new JobTitleEntity
        {
            Id = jobTitleId,
            Name = "Analyst",
            DisplayOrder = 1,
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.UserInvitations.Add(new UserInvitationEntity
        {
            Id = Guid.NewGuid(),
            Email = "invitee@example.com",
            InvitationToken = "expired-token",
            InvitedBy = "admin@example.com",
            DepartmentId = departmentId,
            JobTitleId = jobTitleId,
            Status = InvitationStatus.Pending,
            InvitedAt = DateTimeOffset.UtcNow.AddDays(-3),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1)
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var sut = new UserInvitationQueries(dbContext, auditLogWriter);

        var result = await sut.GetInvitationByTokenAsync("expired-token", CancellationToken.None);

        Assert.Equal(InvitationDetailQueryStatus.Success, result.Status);
        Assert.NotNull(result.Response);
        Assert.Equal(InvitationStatus.Expired, result.Response!.Status);
        Assert.Equal("Operations", result.Response.DepartmentName);
        Assert.Equal("Analyst", result.Response.JobTitleName);
        Assert.Single(auditLogWriter.Entries);
    }
}
