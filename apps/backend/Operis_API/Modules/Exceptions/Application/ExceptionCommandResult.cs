namespace Operis_API.Modules.Exceptions.Application;

public enum ExceptionCommandStatus
{
    Success,
    ValidationError,
    NotFound,
    Conflict
}

public sealed record ExceptionCommandResult<T>(
    ExceptionCommandStatus Status,
    T? Value = default,
    string? ErrorMessage = null,
    string? ErrorCode = null);
