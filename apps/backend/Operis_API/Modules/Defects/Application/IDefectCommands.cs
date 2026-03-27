using Operis_API.Modules.Defects.Contracts;

namespace Operis_API.Modules.Defects.Application;

public interface IDefectCommands
{
    Task<DefectCommandResult<DefectCommandResponse>> CreateDefectAsync(CreateDefectRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<DefectCommandResult<DefectDetailResponse>> UpdateDefectAsync(Guid defectId, UpdateDefectRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<DefectCommandResult<DefectDetailResponse>> ResolveDefectAsync(Guid defectId, ResolveDefectRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<DefectCommandResult<DefectDetailResponse>> CloseDefectAsync(Guid defectId, CloseDefectRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<DefectCommandResult<NonConformanceCommandResponse>> CreateNonConformanceAsync(CreateNonConformanceRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<DefectCommandResult<NonConformanceDetailResponse>> UpdateNonConformanceAsync(Guid nonConformanceId, UpdateNonConformanceRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<DefectCommandResult<NonConformanceDetailResponse>> CloseNonConformanceAsync(Guid nonConformanceId, CloseNonConformanceRequest request, string? actorUserId, CancellationToken cancellationToken);
}
