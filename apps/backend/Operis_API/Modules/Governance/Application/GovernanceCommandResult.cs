namespace Operis_API.Modules.Governance.Application;

public enum GovernanceCommandStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

public sealed record GovernanceCommandResult<T>(
    GovernanceCommandStatus Status,
    T? Value = default,
    string? ErrorMessage = null,
    string? ErrorCode = null);
