namespace Operis_API.Modules.Meetings.Application;

public enum MeetingCommandStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

public sealed record MeetingCommandResult<T>(
    MeetingCommandStatus Status,
    T? Value = default,
    string? ErrorMessage = null,
    string? ErrorCode = null);
