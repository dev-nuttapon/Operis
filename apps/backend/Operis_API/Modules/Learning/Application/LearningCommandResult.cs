namespace Operis_API.Modules.Learning.Application;

public enum LearningCommandStatus
{
    Success,
    ValidationError,
    NotFound,
    Conflict
}

public sealed record LearningCommandResult<T>(
    LearningCommandStatus Status,
    T? Value = default,
    string? ErrorMessage = null,
    string? ErrorCode = null);
