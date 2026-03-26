namespace Operis_API.Modules.Operations.Application;

public enum OperationsCommandStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

public sealed record OperationsCommandResult<T>(OperationsCommandStatus Status, T? Value = default, string? ErrorMessage = null, string? ErrorCode = null);
