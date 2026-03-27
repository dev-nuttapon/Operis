using Operis_API.Modules.Governance.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Governance.Application;

public interface IGovernanceOperationsQueries
{
    Task<PagedResult<RaciMapResponse>> ListRaciMapsAsync(RaciMapListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ApprovalEvidenceLogResponse>> ListApprovalEvidenceAsync(ApprovalEvidenceListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<WorkflowOverrideLogResponse>> ListWorkflowOverridesAsync(WorkflowOverrideListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<SlaRuleResponse>> ListSlaRulesAsync(SlaRuleListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<RetentionPolicyResponse>> ListRetentionPoliciesAsync(RetentionPolicyListQuery query, CancellationToken cancellationToken);
}
