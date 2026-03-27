namespace Operis_API.Modules.Defects.Application;

public enum DefectCommandStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

public sealed record DefectCommandResult<T>(
    DefectCommandStatus Status,
    T? Value = default,
    string? ErrorCode = null,
    string? ErrorMessage = null);
