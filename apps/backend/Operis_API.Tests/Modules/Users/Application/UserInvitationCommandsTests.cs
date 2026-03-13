using Microsoft.EntityFrameworkCore;
using Operis_API.Modules.Users.Application;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Domain;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Users.Application;

public sealed class UserInvitationCommandsTests
{
    [Fact]
    public async Task AcceptInvitationAsync_WhenExpired_DoesNotProvisionUser()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.UserInvitations.Add(new UserInvitationEntity
        {
            Id = Guid.NewGuid(),
            Email = "invitee@example.com",
            InvitationToken = "invite-token",
            InvitedBy = "admin@example.com",
            Status = InvitationStatus.Pending,
            InvitedAt = DateTimeOffset.UtcNow.AddDays(-5),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1)
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var keycloakAdminClient = new FakeKeycloakAdminClient();
        var sut = new UserInvitationCommands(dbContext, auditLogWriter, keycloakAdminClient);

        var result = await sut.AcceptInvitationAsync(
            "invite-token",
            new AcceptInvitationRequest("Pat", "Lee", "Password123!", "Password123!"),
            CancellationToken.None);

        Assert.Equal(InvitationCommandStatus.ValidationError, result.Status);
        Assert.Equal(0, keycloakAdminClient.CreateUserCalls);
        Assert.Equal(0, await dbContext.Users.CountAsync());
        Assert.Empty(auditLogWriter.Entries);
    }

    [Fact]
    public async Task AcceptInvitationAsync_WhenSuccessful_ProvisionsUserOnceAndMarksInvitationAccepted()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.UserInvitations.Add(new UserInvitationEntity
        {
            Id = Guid.NewGuid(),
            Email = "invitee@example.com",
            InvitationToken = "invite-token",
            InvitedBy = "admin@example.com",
            Status = InvitationStatus.Pending,
            InvitedAt = DateTimeOffset.UtcNow.AddDays(-1),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(2)
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var keycloakAdminClient = new FakeKeycloakAdminClient
        {
            CreateUserResult = new KeycloakCreateUserResult(true, false, "kc-user-1", null)
        };
        var sut = new UserInvitationCommands(dbContext, auditLogWriter, keycloakAdminClient);

        var result = await sut.AcceptInvitationAsync(
            "invite-token",
            new AcceptInvitationRequest("Pat", "Lee", "Password123!", "Password123!"),
            CancellationToken.None);

        var invitation = await dbContext.UserInvitations.SingleAsync(x => x.InvitationToken == "invite-token");
        var user = await dbContext.Users.FindAsync("kc-user-1");

        Assert.Equal(InvitationCommandStatus.Success, result.Status);
        Assert.Equal(1, keycloakAdminClient.CreateUserCalls);
        Assert.NotNull(user);
        Assert.Equal(InvitationStatus.Accepted, invitation.Status);
        Assert.NotNull(invitation.AcceptedAt);
        Assert.Single(auditLogWriter.Entries);
    }
}
