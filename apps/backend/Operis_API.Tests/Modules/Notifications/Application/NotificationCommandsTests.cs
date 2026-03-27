using Operis_API.Modules.Notifications;
using Operis_API.Modules.Notifications.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Notifications.Application;

public sealed class NotificationCommandsTests
{
    [Fact]
    public async Task RetryAsync_WithoutFailedState_ReturnsStableErrorCode()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var queueId = Guid.NewGuid();
        dbContext.NotificationQueue.Add(new NotificationQueueEntity
        {
            Id = queueId,
            Channel = "email",
            TargetRef = "ops@example.com",
            PayloadRef = "minio://notifications/capa-1.json",
            QueuedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            Status = "queued",
            RetryCount = 0
        });
        await dbContext.SaveChangesAsync();

        var sut = new NotificationCommands(dbContext);
        var result = await sut.RetryAsync(queueId, "ops@example.com", CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ApiErrorCodes.NotificationRetryInvalidState, result.ErrorCode);
    }
}
