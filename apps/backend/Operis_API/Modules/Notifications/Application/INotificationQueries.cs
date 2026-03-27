using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Notifications;

public interface INotificationQueries
{
    Task<PagedResult<NotificationListItem>> ListAsync(
        NotificationListQuery query,
        string? currentUserId,
        CancellationToken cancellationToken);

    Task<NotificationDetailContract?> GetByIdAsync(
        Guid notificationId,
        string? currentUserId,
        CancellationToken cancellationToken);
    Task<PagedResult<NotificationQueueItemContract>> ListQueueAsync(
        NotificationQueueListQuery query,
        CancellationToken cancellationToken);
}
