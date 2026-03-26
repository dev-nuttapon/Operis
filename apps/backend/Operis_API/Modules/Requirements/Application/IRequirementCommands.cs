using Operis_API.Modules.Requirements.Contracts;

namespace Operis_API.Modules.Requirements.Application;

public interface IRequirementCommands
{
    Task<RequirementCommandResult<RequirementDetailResponse>> CreateRequirementAsync(CreateRequirementRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RequirementCommandResult<RequirementDetailResponse>> UpdateRequirementAsync(Guid requirementId, UpdateRequirementRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RequirementCommandResult<RequirementDetailResponse>> SubmitRequirementAsync(Guid requirementId, string? actorUserId, CancellationToken cancellationToken);
    Task<RequirementCommandResult<RequirementDetailResponse>> ApproveRequirementAsync(Guid requirementId, RequirementDecisionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RequirementCommandResult<RequirementDetailResponse>> BaselineRequirementAsync(Guid requirementId, string? actorUserId, CancellationToken cancellationToken);
    Task<RequirementCommandResult<RequirementDetailResponse>> SupersedeRequirementAsync(Guid requirementId, RequirementDecisionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RequirementCommandResult<RequirementBaselineItem>> CreateBaselineAsync(CreateRequirementBaselineRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RequirementCommandResult<TraceabilityLinkItem>> CreateTraceabilityLinkAsync(CreateTraceabilityLinkRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RequirementCommandResult<bool>> DeleteTraceabilityLinkAsync(Guid linkId, string? actorUserId, CancellationToken cancellationToken);
}
