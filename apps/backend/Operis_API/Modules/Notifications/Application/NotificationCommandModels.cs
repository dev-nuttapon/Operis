namespace Operis_API.Modules.Notifications;

public sealed record NotificationUpdateResult(
    bool Succeeded,
    bool NotFound,
    string? ErrorCode,
    string? ErrorMessage,
    int UpdatedCount)
{
    public static NotificationUpdateResult Success(int updatedCount = 1) => new(true, false, null, null, updatedCount);
    public static NotificationUpdateResult Fail(string errorCode, string errorMessage) => new(false, false, errorCode, errorMessage, 0);
    public static NotificationUpdateResult Missing() => new(false, true, null, null, 0);
}
