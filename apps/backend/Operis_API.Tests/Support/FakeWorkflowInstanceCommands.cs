using Operis_API.Modules.Workflows;

namespace Operis_API.Tests.Support;

internal sealed class FakeWorkflowInstanceCommands : IWorkflowInstanceCommands
{
    public Task<(bool Success, string? Error, string? ErrorCode, WorkflowInstanceDetailContract? Response)> CreateInstanceAsync(CreateWorkflowInstanceRequest request, string? actorUserId, string? actorDisplayName, string? actorEmail, CancellationToken cancellationToken) =>
        Task.FromResult<(bool Success, string? Error, string? ErrorCode, WorkflowInstanceDetailContract? Response)>((true, null, null, null));

    public Task<(bool Success, string? Error, string? ErrorCode, WorkflowInstanceDetailContract? Response, bool NotFound)> ApplyStepActionAsync(Guid workflowInstanceId, Guid workflowInstanceStepId, WorkflowStepActionRequest request, string? actorUserId, string? actorDisplayName, string? actorEmail, CancellationToken cancellationToken) =>
        Task.FromResult<(bool Success, string? Error, string? ErrorCode, WorkflowInstanceDetailContract? Response, bool NotFound)>((true, null, null, null, false));
}
