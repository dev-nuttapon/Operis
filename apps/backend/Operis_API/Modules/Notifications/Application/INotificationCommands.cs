namespace Operis_API.Modules.Notifications;

public interface INotificationCommands
{
    Task<NotificationUpdateResult> MarkReadAsync(Guid notificationId, string? currentUserId, CancellationToken cancellationToken);
    Task<NotificationUpdateResult> MarkAllReadAsync(string? currentUserId, CancellationToken cancellationToken);
    Task<NotificationUpdateResult> SeedAsync(string? currentUserId, int count, CancellationToken cancellationToken);
    Task<NotificationQueueCommandResult> EnqueueAsync(CreateNotificationQueueRequest request, string? actor, CancellationToken cancellationToken);
    Task<NotificationQueueCommandResult> RetryAsync(Guid id, string? actor, CancellationToken cancellationToken);
}
