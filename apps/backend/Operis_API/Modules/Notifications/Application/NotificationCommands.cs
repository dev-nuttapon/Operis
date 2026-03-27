using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Shared.Contracts;
using Operis_API.Modules.Notifications.Infrastructure;

namespace Operis_API.Modules.Notifications;

public sealed class NotificationCommands(OperisDbContext dbContext) : INotificationCommands
{
    private static readonly string[] QueueStatuses = ["queued", "sent", "failed", "retried", "closed"];

    public async Task<NotificationUpdateResult> MarkReadAsync(
        Guid notificationId,
        string? currentUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return NotificationUpdateResult.Fail("notifications.user_required", "User is required.");
        }

        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.RecipientUserId == currentUserId, cancellationToken);

        if (notification is null)
        {
            return NotificationUpdateResult.Missing();
        }

        if (string.Equals(notification.Status, "read", StringComparison.OrdinalIgnoreCase))
        {
            return NotificationUpdateResult.Success(0);
        }

        notification.Status = "read";
        notification.ReadAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return NotificationUpdateResult.Success(1);
    }

    public async Task<NotificationUpdateResult> MarkAllReadAsync(string? currentUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return NotificationUpdateResult.Fail("notifications.user_required", "User is required.");
        }

        var items = await dbContext.Notifications
            .Where(x => x.RecipientUserId == currentUserId && x.Status == "unread")
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return NotificationUpdateResult.Success(0);
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var item in items)
        {
            item.Status = "read";
            item.ReadAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return NotificationUpdateResult.Success(items.Count);
    }

    public async Task<NotificationUpdateResult> SeedAsync(string? currentUserId, int count, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return NotificationUpdateResult.Fail("notifications.user_required", "User is required.");
        }

        var total = Math.Clamp(count, 1, 25);
        var now = DateTimeOffset.UtcNow;

        for (var i = 0; i < total; i += 1)
        {
            dbContext.Notifications.Add(new Notifications.Infrastructure.NotificationEntity
            {
                Id = Guid.NewGuid(),
                RecipientUserId = currentUserId,
                Title = $"Sample notification {i + 1}",
                Description = "This is a sample notification for UI testing.",
                Source = "system",
                Status = "unread",
                CreatedAt = now.AddMinutes(-5 * i)
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return NotificationUpdateResult.Success(total);
    }

    public async Task<NotificationQueueCommandResult> EnqueueAsync(CreateNotificationQueueRequest request, string? actor, CancellationToken cancellationToken)
    {
        var channel = Required(request.Channel, 64);
        var targetRef = Required(request.TargetRef, 512);
        var payloadRef = Required(request.PayloadRef, 512);
        if (channel is null || targetRef is null || payloadRef is null)
        {
            return NotificationQueueCommandResult.Fail(ApiErrorCodes.RequestValidationFailed, "Channel, target, and payload reference are required.");
        }

        var status = NormalizeQueueStatus(request.Status);
        var entity = new NotificationQueueEntity
        {
            Id = Guid.NewGuid(),
            Channel = channel,
            TargetRef = targetRef,
            PayloadRef = payloadRef,
            QueuedAt = DateTimeOffset.UtcNow,
            Status = status,
            RetryCount = 0,
            LastError = Optional(request.LastError, 2000),
            LastRetriedAt = null
        };

        dbContext.NotificationQueue.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NotificationQueueCommandResult.Success(ToQueueContract(entity));
    }

    public async Task<NotificationQueueCommandResult> RetryAsync(Guid id, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.NotificationQueue.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotificationQueueCommandResult.Missing();
        }

        if (!string.Equals(entity.Status, "failed", StringComparison.OrdinalIgnoreCase))
        {
            return NotificationQueueCommandResult.Fail(ApiErrorCodes.NotificationRetryInvalidState, "Notification retry requires a failed queue item.");
        }

        entity.Status = "retried";
        entity.RetryCount += 1;
        entity.LastRetriedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NotificationQueueCommandResult.Success(ToQueueContract(entity));
    }

    private static string? Required(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string? Optional(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeQueueStatus(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "sent" => "sent",
        "failed" => "failed",
        "retried" => "retried",
        "closed" => "closed",
        _ => "queued"
    };

    private static NotificationQueueItemContract ToQueueContract(NotificationQueueEntity entity) =>
        new(entity.Id, entity.Channel, entity.TargetRef, entity.PayloadRef, entity.QueuedAt, entity.Status, entity.RetryCount, entity.LastError, entity.LastRetriedAt);
}
