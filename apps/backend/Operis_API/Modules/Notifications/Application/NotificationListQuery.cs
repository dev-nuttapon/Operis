namespace Operis_API.Modules.Notifications;

public sealed record NotificationListQuery(
    int Page,
    int PageSize,
    bool? UnreadOnly);
