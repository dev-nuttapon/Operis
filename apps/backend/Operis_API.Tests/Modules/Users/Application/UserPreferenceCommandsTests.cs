using Operis_API.Modules.Users.Application;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Domain;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Users.Application;

public sealed class UserPreferenceCommandsTests
{
    [Fact]
    public async Task UpdateCurrentUserPreferencesAsync_NormalizesAndPersistsPreferences()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.Users.Add(new UserEntity
        {
            Id = "user-1",
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system",
            PreferredLanguage = "en",
            PreferredTheme = "light"
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var sut = new UserPreferenceCommands(dbContext, auditLogWriter);

        var result = await sut.UpdateCurrentUserPreferencesAsync(
            "user-1",
            new UpdateUserPreferencesRequest(" TH-TH ", " INVALID "),
            CancellationToken.None);

        var persisted = await dbContext.Users.FindAsync("user-1");

        Assert.Equal(UserPreferenceCommandStatus.Success, result.Status);
        Assert.NotNull(persisted);
        Assert.Equal("th-th", persisted!.PreferredLanguage);
        Assert.Null(persisted.PreferredTheme);
        Assert.Single(auditLogWriter.Entries);
    }
}
