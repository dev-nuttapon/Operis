using Operis_API.Modules.Assessment.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Assessment.Application;

public interface IAssessmentQueries
{
    Task<PagedResult<AssessmentPackageListItemResponse>> ListPackagesAsync(AssessmentPackageListQuery query, CancellationToken cancellationToken);
    Task<AssessmentPackageDetailResponse?> GetPackageAsync(Guid packageId, CancellationToken cancellationToken);
    Task<PagedResult<AssessmentFindingListItemResponse>> ListFindingsAsync(AssessmentFindingListQuery query, CancellationToken cancellationToken);
    Task<AssessmentFindingDetailResponse?> GetFindingAsync(Guid findingId, CancellationToken cancellationToken);
    Task<PagedResult<ControlCatalogItemResponse>> ListControlCatalogAsync(ControlCatalogListQuery query, CancellationToken cancellationToken);
    Task<ControlCatalogItemResponse?> GetControlCatalogItemAsync(Guid controlId, CancellationToken cancellationToken);
    Task<PagedResult<ControlMappingDetailResponse>> ListControlMappingsAsync(ControlMappingListQuery query, CancellationToken cancellationToken);
    Task<ControlMappingDetailResponse?> GetControlMappingAsync(Guid mappingId, CancellationToken cancellationToken);
    Task<PagedResult<ControlCoverageItemResponse>> ListControlCoverageAsync(ControlCoverageListQuery query, string? actorUserId, CancellationToken cancellationToken);
}
