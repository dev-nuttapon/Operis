using Operis_API.Modules.Users.Application;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Domain;
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
        var sut = new UserManagementCommands(dbContext, auditLogWriter, keycloakAdminClient, new FakeKeycloakUserCache());

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
        var sut = new UserManagementCommands(dbContext, auditLogWriter, keycloakAdminClient, new FakeKeycloakUserCache());

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

    [Fact]
    public async Task UpdateUserAsync_WhenRemovingLastAdminRole_ReturnsValidationError()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var adminRoleId = Guid.NewGuid();
        dbContext.AppRoles.Add(new AppRoleEntity
        {
            Id = adminRoleId,
            Name = "System Admin",
            KeycloakRoleName = "operis:system_admin",
            DisplayOrder = 1,
            CreatedAt = DateTimeOffset.UtcNow
        });
        dbContext.Users.Add(new UserEntity
        {
            Id = "kc-admin",
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "seed"
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var keycloakAdminClient = new FakeKeycloakAdminClient();
        keycloakAdminClient.UserRealmRoles["kc-admin"] = [new KeycloakRole("role-1", "operis:system_admin", null, false)];
        var sut = new UserManagementCommands(dbContext, auditLogWriter, keycloakAdminClient, new TestKeycloakUserCache());

        var result = await sut.UpdateUserAsync(
            "kc-admin",
            new UpdateUserRequest("admin@example.com", "Admin", "User", null, null, null, [], "role cleanup"),
            CancellationToken.None);

        Assert.Equal(UserCommandStatus.ValidationError, result.Status);
        Assert.Equal(Shared.Contracts.ApiErrorCodes.LastAdminRemovalBlocked, result.ErrorCode);
    }
}
