namespace Operis_API.Modules.Notifications;

public sealed record NotificationListItem(
    Guid Id,
    string Title,
    string Description,
    string Source,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);

public sealed record CreateNotificationQueueRequest(
    string Channel,
    string TargetRef,
    string PayloadRef,
    string Status,
    string? LastError);

public sealed record NotificationQueueItemContract(
    Guid Id,
    string Channel,
    string TargetRef,
    string PayloadRef,
    DateTimeOffset QueuedAt,
    string Status,
    int RetryCount,
    string? LastError,
    DateTimeOffset? LastRetriedAt);
