namespace Operis_API.Modules.Risks.Application;

public enum RiskCommandStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

public sealed record RiskCommandResult<T>(
    RiskCommandStatus Status,
    T? Value = default,
    string? ErrorMessage = null,
    string? ErrorCode = null);
