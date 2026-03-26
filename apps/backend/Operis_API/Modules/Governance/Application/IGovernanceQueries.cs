using Operis_API.Modules.Governance.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Governance.Application;

public interface IGovernanceQueries
{
    Task<PagedResult<ProcessAssetListItemResponse>> ListProcessAssetsAsync(GovernanceListQuery query, CancellationToken cancellationToken);
    Task<ProcessAssetResponse?> GetProcessAssetAsync(Guid processAssetId, CancellationToken cancellationToken);
    Task<PagedResult<QaChecklistListItemResponse>> ListQaChecklistsAsync(GovernanceListQuery query, CancellationToken cancellationToken);
    Task<QaChecklistResponse?> GetQaChecklistAsync(Guid qaChecklistId, CancellationToken cancellationToken);
    Task<PagedResult<ProjectPlanListItemResponse>> ListProjectPlansAsync(GovernanceListQuery query, CancellationToken cancellationToken);
    Task<ProjectPlanResponse?> GetProjectPlanAsync(Guid projectPlanId, CancellationToken cancellationToken);
    Task<PagedResult<StakeholderResponse>> ListStakeholdersAsync(GovernanceListQuery query, CancellationToken cancellationToken);
    Task<StakeholderResponse?> GetStakeholderAsync(Guid stakeholderId, CancellationToken cancellationToken);
    Task<PagedResult<TailoringRecordListItemResponse>> ListTailoringRecordsAsync(GovernanceListQuery query, CancellationToken cancellationToken);
    Task<TailoringRecordResponse?> GetTailoringRecordAsync(Guid tailoringRecordId, CancellationToken cancellationToken);
}
