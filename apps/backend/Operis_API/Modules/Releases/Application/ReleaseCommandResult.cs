namespace Operis_API.Modules.Releases.Application;

public enum ReleaseCommandStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

public sealed record ReleaseCommandResult<T>(
    ReleaseCommandStatus Status,
    T? Value = default,
    string? ErrorCode = null,
    string? ErrorMessage = null);
