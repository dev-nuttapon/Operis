using Operis_API.Modules.Requirements.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Requirements.Application;

public interface IRequirementQueries
{
    Task<PagedResult<RequirementListItem>> ListRequirementsAsync(RequirementListQuery query, CancellationToken cancellationToken);
    Task<RequirementDetailResponse?> GetRequirementAsync(Guid requirementId, CancellationToken cancellationToken);
    Task<PagedResult<RequirementBaselineItem>> ListBaselinesAsync(RequirementBaselineListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<TraceabilityMatrixRow>> ListTraceabilityAsync(TraceabilityListQuery query, CancellationToken cancellationToken);
}
