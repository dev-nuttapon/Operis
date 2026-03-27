namespace Operis_API.Modules.Notifications;

public sealed record NotificationQueueListQuery(
    string? Channel,
    string? Status,
    string? Search,
    int Page = 1,
    int PageSize = 25);
