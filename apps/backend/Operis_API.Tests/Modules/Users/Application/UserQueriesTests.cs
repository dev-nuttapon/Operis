using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Application;
using Operis_API.Modules.Users.Domain;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Users.Application;

public sealed class UserQueriesTests
{
    [Fact]
    public async Task ListUsersAsync_WhenIdentityIsExcluded_DoesNotCallKeycloak()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.Users.AddRange(
            new UserEntity
            {
                Id = "user-1",
                Status = UserStatus.Active,
                CreatedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
                CreatedBy = "system",
            },
            new UserEntity
            {
                Id = "user-2",
                Status = UserStatus.Active,
                CreatedAt = new DateTimeOffset(2026, 3, 2, 0, 0, 0, TimeSpan.Zero),
                CreatedBy = "system",
            });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var keycloakAdminClient = new FakeKeycloakAdminClient();
        var referenceDataCache = new TestReferenceDataCache();
        var sut = new UserQueries(dbContext, auditLogWriter, keycloakAdminClient, referenceDataCache);

        var result = await sut.ListUsersAsync(
            new UserListQuery(IncludeIdentity: false, SortBy: "createdAt", SortOrder: "desc", Page: 1, PageSize: 10),
            CancellationToken.None);

        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("user-2", result.Items[0].Id);
        Assert.Equal(0, keycloakAdminClient.GetUserByIdCalls);
        Assert.Equal(0, keycloakAdminClient.GetUserRealmRolesCalls);
        Assert.Single(auditLogWriter.Entries);
    }
}
