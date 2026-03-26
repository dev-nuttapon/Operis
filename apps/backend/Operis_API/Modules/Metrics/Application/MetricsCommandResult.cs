namespace Operis_API.Modules.Metrics.Application;

public enum MetricsCommandStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

public sealed record MetricsCommandResult<T>(
    MetricsCommandStatus Status,
    T? Value = default,
    string? ErrorCode = null,
    string? ErrorMessage = null);
