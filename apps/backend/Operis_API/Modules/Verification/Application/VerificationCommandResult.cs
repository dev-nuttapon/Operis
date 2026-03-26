namespace Operis_API.Modules.Verification.Application;

public enum VerificationCommandStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

public sealed record VerificationCommandResult<T>(
    VerificationCommandStatus Status,
    T? Value = default,
    string? ErrorCode = null,
    string? ErrorMessage = null);
