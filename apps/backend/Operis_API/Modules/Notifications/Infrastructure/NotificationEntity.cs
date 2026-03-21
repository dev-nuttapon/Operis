namespace Operis_API.Modules.Notifications.Infrastructure;

public sealed class NotificationEntity
{
    public Guid Id { get; set; }
    public string RecipientUserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = "unread";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
