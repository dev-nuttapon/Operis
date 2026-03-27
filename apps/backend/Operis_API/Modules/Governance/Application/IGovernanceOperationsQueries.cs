using Operis_API.Modules.Governance.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Governance.Application;

public interface IGovernanceOperationsQueries
{
    Task<ComplianceDashboardResponse> GetComplianceDashboardAsync(ComplianceDashboardQuery query, string? actor, CancellationToken cancellationToken);
    Task<ComplianceDrilldownResponse> GetComplianceDrilldownAsync(ComplianceDashboardDrilldownQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ManagementReviewListItemResponse>> ListManagementReviewsAsync(ManagementReviewListQuery query, CancellationToken cancellationToken);
    Task<ManagementReviewDetailResponse?> GetManagementReviewAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<RaciMapResponse>> ListRaciMapsAsync(RaciMapListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ApprovalEvidenceLogResponse>> ListApprovalEvidenceAsync(ApprovalEvidenceListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<WorkflowOverrideLogResponse>> ListWorkflowOverridesAsync(WorkflowOverrideListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<SlaRuleResponse>> ListSlaRulesAsync(SlaRuleListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<RetentionPolicyResponse>> ListRetentionPoliciesAsync(RetentionPolicyListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<ArchitectureRecordResponse>> ListArchitectureRecordsAsync(ArchitectureRecordListQuery query, CancellationToken cancellationToken);
    Task<ArchitectureRecordResponse?> GetArchitectureRecordAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<DesignReviewResponse>> ListDesignReviewsAsync(DesignReviewListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<IntegrationReviewResponse>> ListIntegrationReviewsAsync(IntegrationReviewListQuery query, CancellationToken cancellationToken);
}
