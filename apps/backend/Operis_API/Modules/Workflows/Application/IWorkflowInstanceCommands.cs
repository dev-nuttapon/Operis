namespace Operis_API.Modules.Workflows;

public interface IWorkflowInstanceCommands
{
    Task<(bool Success, string? Error, string? ErrorCode, WorkflowInstanceDetailContract? Response)> CreateInstanceAsync(
        CreateWorkflowInstanceRequest request,
        string? actorUserId,
        string? actorDisplayName,
        string? actorEmail,
        CancellationToken cancellationToken);

    Task<(bool Success, string? Error, string? ErrorCode, WorkflowInstanceDetailContract? Response, bool NotFound)> ApplyStepActionAsync(
        Guid workflowInstanceId,
        Guid workflowInstanceStepId,
        WorkflowStepActionRequest request,
        string? actorUserId,
        string? actorDisplayName,
        string? actorEmail,
        CancellationToken cancellationToken);
}
