using Operis_API.Modules.Releases.Contracts;

namespace Operis_API.Modules.Releases.Application;

public interface IReleaseCommands
{
    Task<ReleaseCommandResult<ReleaseCommandResponse>> CreateReleaseAsync(CreateReleaseRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ReleaseCommandResult<ReleaseDetailResponse>> UpdateReleaseAsync(Guid releaseId, UpdateReleaseRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ReleaseCommandResult<ReleaseDetailResponse>> ApproveReleaseAsync(Guid releaseId, ApproveReleaseRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ReleaseCommandResult<ReleaseDetailResponse>> ExecuteReleaseAsync(Guid releaseId, ExecuteReleaseRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ReleaseCommandResult<DeploymentChecklistItem>> CreateDeploymentChecklistAsync(CreateDeploymentChecklistRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ReleaseCommandResult<DeploymentChecklistItem>> UpdateDeploymentChecklistAsync(Guid checklistId, UpdateDeploymentChecklistRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ReleaseCommandResult<ReleaseNoteItem>> CreateReleaseNoteAsync(CreateReleaseNoteRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ReleaseCommandResult<ReleaseNotePublishResponse>> PublishReleaseNoteAsync(Guid releaseNoteId, string? actorUserId, CancellationToken cancellationToken);
}
