using Microsoft.EntityFrameworkCore;
using Operis_API.Modules.Users.Application;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Domain;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Users.Application;

public sealed class UserRegistrationCommandsTests
{
    [Fact]
    public async Task CompleteRegistrationPasswordSetupAsync_WhenPasswordsMismatch_DoesNotCallKeycloak()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.UserRegistrationRequests.Add(new UserRegistrationRequestEntity
        {
            Id = Guid.NewGuid(),
            Email = "candidate@example.com",
            FirstName = "Casey",
            LastName = "Ng",
            Status = RegistrationRequestStatus.Approved,
            RequestedAt = DateTimeOffset.UtcNow.AddDays(-1),
            ProvisionedUserId = "kc-user-1",
            PasswordSetupToken = "setup-token",
            PasswordSetupExpiresAt = DateTimeOffset.UtcNow.AddDays(2)
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var keycloakAdminClient = new FakeKeycloakAdminClient();
        var sut = new UserRegistrationCommands(dbContext, auditLogWriter, keycloakAdminClient);

        var result = await sut.CompleteRegistrationPasswordSetupAsync(
            "setup-token",
            new CompleteRegistrationPasswordSetupRequest("Password123!", "Password1234!"),
            CancellationToken.None);

        Assert.Equal(RegistrationCommandStatus.ValidationError, result.Status);
        Assert.Equal(0, keycloakAdminClient.UpdatePasswordCalls);
        Assert.Empty(auditLogWriter.Entries);
    }

    [Fact]
    public async Task CompleteRegistrationPasswordSetupAsync_WhenSuccessful_UpdatesPasswordOnceAndPersistsCompletion()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.UserRegistrationRequests.Add(new UserRegistrationRequestEntity
        {
            Id = Guid.NewGuid(),
            Email = "candidate@example.com",
            FirstName = "Casey",
            LastName = "Ng",
            Status = RegistrationRequestStatus.Approved,
            RequestedAt = DateTimeOffset.UtcNow.AddDays(-1),
            ProvisionedUserId = "kc-user-1",
            PasswordSetupToken = "setup-token",
            PasswordSetupExpiresAt = DateTimeOffset.UtcNow.AddDays(2)
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var keycloakAdminClient = new FakeKeycloakAdminClient();
        var sut = new UserRegistrationCommands(dbContext, auditLogWriter, keycloakAdminClient);

        var result = await sut.CompleteRegistrationPasswordSetupAsync(
            "setup-token",
            new CompleteRegistrationPasswordSetupRequest("Password123!", "Password123!"),
            CancellationToken.None);

        var persisted = await dbContext.UserRegistrationRequests.SingleAsync(x => x.PasswordSetupToken == "setup-token");

        Assert.Equal(RegistrationCommandStatus.Success, result.Status);
        Assert.Equal(1, keycloakAdminClient.UpdatePasswordCalls);
        Assert.NotNull(persisted.PasswordSetupCompletedAt);
        Assert.Single(auditLogWriter.Entries);
    }
}
