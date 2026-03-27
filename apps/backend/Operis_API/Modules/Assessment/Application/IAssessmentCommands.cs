using Operis_API.Modules.Assessment.Contracts;

namespace Operis_API.Modules.Assessment.Application;

public interface IAssessmentCommands
{
    Task<AssessmentCommandResult<AssessmentPackageDetailResponse>> CreatePackageAsync(CreateAssessmentPackageRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<AssessmentCommandResult<AssessmentPackageDetailResponse>> TransitionPackageAsync(Guid packageId, TransitionAssessmentPackageRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<AssessmentCommandResult<AssessmentPackageNoteResponse>> AddPackageNoteAsync(Guid packageId, CreateAssessmentNoteRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<AssessmentCommandResult<AssessmentFindingDetailResponse>> CreateFindingAsync(CreateAssessmentFindingRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<AssessmentCommandResult<AssessmentFindingDetailResponse>> TransitionFindingAsync(Guid findingId, TransitionAssessmentFindingRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<AssessmentCommandResult<ControlCatalogItemResponse>> CreateControlCatalogItemAsync(CreateControlCatalogItemRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<AssessmentCommandResult<ControlCatalogItemResponse>> UpdateControlCatalogItemAsync(Guid controlId, UpdateControlCatalogItemRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<AssessmentCommandResult<ControlMappingDetailResponse>> CreateControlMappingAsync(CreateControlMappingRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<AssessmentCommandResult<ControlMappingDetailResponse>> TransitionControlMappingAsync(Guid mappingId, TransitionControlMappingRequest request, string? actorUserId, CancellationToken cancellationToken);
}
