namespace Operis_API.Modules.Notifications;

public sealed record NotificationDetailContract(
    Guid Id,
    string Title,
    string Description,
    string Source,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);
