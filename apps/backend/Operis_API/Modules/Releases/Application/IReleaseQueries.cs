using Operis_API.Modules.Releases.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Releases.Application;

public interface IReleaseQueries
{
    Task<PagedResult<ReleaseListItem>> ListReleasesAsync(ReleaseListQuery query, CancellationToken cancellationToken);
    Task<ReleaseDetailResponse?> GetReleaseAsync(Guid releaseId, CancellationToken cancellationToken);
    Task<PagedResult<DeploymentChecklistItem>> ListDeploymentChecklistsAsync(DeploymentChecklistListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ReleaseNoteItem>> ListReleaseNotesAsync(ReleaseNoteListQuery query, CancellationToken cancellationToken);
}
