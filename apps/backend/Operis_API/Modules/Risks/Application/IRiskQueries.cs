using Operis_API.Modules.Risks.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Risks.Application;

public interface IRiskQueries
{
    Task<PagedResult<RiskListItemResponse>> ListRisksAsync(RiskListQuery query, CancellationToken cancellationToken);
    Task<RiskDetailResponse?> GetRiskAsync(Guid riskId, CancellationToken cancellationToken);
    Task<PagedResult<IssueListItemResponse>> ListIssuesAsync(IssueListQuery query, bool canReadSensitive, CancellationToken cancellationToken);
    Task<IssueDetailResponse?> GetIssueAsync(Guid issueId, bool canReadSensitive, CancellationToken cancellationToken);
}
