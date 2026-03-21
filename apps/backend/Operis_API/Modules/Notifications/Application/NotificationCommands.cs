using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;

namespace Operis_API.Modules.Notifications;

public sealed class NotificationCommands(OperisDbContext dbContext) : INotificationCommands
{
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
}
