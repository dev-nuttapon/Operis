namespace Operis_API.Modules.Assessment.Application;

public enum AssessmentCommandStatus
{
    Success,
    ValidationError,
    NotFound,
    Conflict
}

public sealed record AssessmentCommandResult<T>(
    AssessmentCommandStatus Status,
    T? Value = default,
    string? ErrorMessage = null,
    string? ErrorCode = null);
