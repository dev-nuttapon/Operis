namespace Operis_API.Modules.Audits.Application;

public enum AuditComplianceCommandStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

public sealed record AuditComplianceCommandResult<T>(
    AuditComplianceCommandStatus Status,
    T? Value = default,
    string? ErrorCode = null,
    string? ErrorMessage = null);
