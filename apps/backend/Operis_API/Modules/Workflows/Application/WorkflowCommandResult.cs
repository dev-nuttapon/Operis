namespace Operis_API.Modules.Workflows;

public enum WorkflowCommandStatus
{
    Success,
    ValidationError,
    Conflict
}

public sealed record WorkflowCommandResult(
    WorkflowCommandStatus Status,
    string? ErrorMessage = null,
    string? ErrorCode = null,
    WorkflowDefinitionContract? Response = null);
