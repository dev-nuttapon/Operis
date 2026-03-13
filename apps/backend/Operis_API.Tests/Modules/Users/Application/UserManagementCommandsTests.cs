using Operis_API.Modules.Users.Application;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Users.Application;

public sealed class UserManagementCommandsTests
{
    [Fact]
    public async Task CreateUserAsync_WhenRoleSelectionIsInvalid_DoesNotCallKeycloak()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.AppRoles.Add(new AppRoleEntity
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            KeycloakRoleName = "admin",
            DisplayOrder = 1,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var keycloakAdminClient = new FakeKeycloakAdminClient();
        var sut = new UserManagementCommands(dbContext, auditLogWriter, keycloakAdminClient);

        var result = await sut.CreateUserAsync(
            new CreateUserRequest(
                "user@example.com",
                "Test",
                "User",
                "Password123!",
                "Password123!",
                "admin@example.com",
                null,
                null,
                [Guid.NewGuid()]),
            CancellationToken.None);

        Assert.Equal(UserCommandStatus.ValidationError, result.Status);
        Assert.Equal(0, keycloakAdminClient.FindUserByEmailCalls);
        Assert.Equal(0, keycloakAdminClient.CreateUserCalls);
        Assert.Equal(0, keycloakAdminClient.AssignRealmRolesCalls);
        Assert.Empty(auditLogWriter.Entries);
    }

    [Fact]
    public async Task CreateUserAsync_WhenSuccessfulWithoutRoles_ProvisionUserWithoutRoleSync()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var auditLogWriter = new FakeAuditLogWriter();
        var keycloakAdminClient = new FakeKeycloakAdminClient
        {
            CreateUserResult = new KeycloakCreateUserResult(true, false, "kc-user-1", null)
        };
        var sut = new UserManagementCommands(dbContext, auditLogWriter, keycloakAdminClient);

        var result = await sut.CreateUserAsync(
            new CreateUserRequest(
                "user@example.com",
                "Test",
                "User",
                "Password123!",
                "Password123!",
                "admin@example.com",
                null,
                null,
                []),
            CancellationToken.None);

        var persistedUser = await dbContext.Users.FindAsync("kc-user-1");

        Assert.Equal(UserCommandStatus.Success, result.Status);
        Assert.NotNull(persistedUser);
        Assert.Equal(1, keycloakAdminClient.FindUserByEmailCalls);
        Assert.Equal(1, keycloakAdminClient.CreateUserCalls);
        Assert.Equal(0, keycloakAdminClient.AssignRealmRolesCalls);
        Assert.Single(auditLogWriter.Entries);
    }
}
