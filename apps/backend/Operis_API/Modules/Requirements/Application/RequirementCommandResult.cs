namespace Operis_API.Modules.Requirements.Application;

public enum RequirementCommandStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

public sealed record RequirementCommandResult<T>(
    RequirementCommandStatus Status,
    T? Value = default,
    string? ErrorMessage = null,
    string? ErrorCode = null);
