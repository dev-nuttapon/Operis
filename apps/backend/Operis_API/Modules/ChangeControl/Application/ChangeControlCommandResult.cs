namespace Operis_API.Modules.ChangeControl.Application;

public enum ChangeControlCommandStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

public sealed record ChangeControlCommandResult<T>(
    ChangeControlCommandStatus Status,
    T? Value = default,
    string? ErrorMessage = null,
    string? ErrorCode = null);
