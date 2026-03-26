using Operis_API.Modules.Users.Application;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Configuration;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Security;
using Operis_API.Tests.Support;
using Microsoft.Extensions.Options;

namespace Operis_API.Tests.Modules.Users.Application;

public sealed class AdminSecurityCommandsTests
{
    [Fact]
    public async Task ApplyPermissionMatrixAsync_WhenPermissionKeyIsInvalid_ReturnsValidationError()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.AppRoles.Add(new AppRoleEntity
        {
            Id = Guid.NewGuid(),
            Name = "System Admin",
            KeycloakRoleName = "operis:system_admin",
            DisplayOrder = 1,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var sut = new AdminSecurityCommands(
            dbContext,
            new TestReferenceDataCache(),
            new FakeAuditLogWriter(),
            new PermissionMatrix(dbContext),
            Options.Create(new Phase0SecurityOptions()));

        var result = await sut.ApplyPermissionMatrixAsync(
            "admin@example.com",
            new ApplyPermissionMatrixRequest(
                "phase-0 rollout",
                [new ApplyPermissionMatrixRoleRequest(dbContext.AppRoles.Single().Id, ["not-a-real-permission"])]),
            CancellationToken.None);

        Assert.Equal(AdminSecurityCommandStatus.ValidationError, result.Status);
        Assert.Equal(ApiErrorCodes.InvalidPermissionKey, result.ErrorCode);
    }
}
