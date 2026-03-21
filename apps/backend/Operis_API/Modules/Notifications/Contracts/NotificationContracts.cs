namespace Operis_API.Modules.Notifications;

public sealed record NotificationListItem(
    Guid Id,
    string Title,
    string Description,
    string Source,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);
