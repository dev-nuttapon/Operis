namespace Operis_API.Modules.Knowledge.Application;

public enum KnowledgeCommandStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

public sealed record KnowledgeCommandResult<T>(KnowledgeCommandStatus Status, T? Value = default, string? ErrorCode = null, string? ErrorMessage = null);
