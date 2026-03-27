using Operis_API.Modules.Governance.Contracts;

namespace Operis_API.Modules.Governance.Application;

public interface IGovernanceOperationsCommands
{
    Task<GovernanceCommandResult<RaciMapResponse>> CreateRaciMapAsync(CreateRaciMapRequest request, string? actor, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<RaciMapResponse>> UpdateRaciMapAsync(Guid id, UpdateRaciMapRequest request, string? actor, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<SlaRuleResponse>> CreateSlaRuleAsync(CreateSlaRuleRequest request, string? actor, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<SlaRuleResponse>> UpdateSlaRuleAsync(Guid id, UpdateSlaRuleRequest request, string? actor, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<RetentionPolicyResponse>> CreateRetentionPolicyAsync(CreateRetentionPolicyRequest request, string? actor, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<RetentionPolicyResponse>> UpdateRetentionPolicyAsync(Guid id, UpdateRetentionPolicyRequest request, string? actor, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<ArchitectureRecordResponse>> CreateArchitectureRecordAsync(CreateArchitectureRecordRequest request, string? actor, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<ArchitectureRecordResponse>> UpdateArchitectureRecordAsync(Guid id, UpdateArchitectureRecordRequest request, string? actor, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<DesignReviewResponse>> CreateDesignReviewAsync(CreateDesignReviewRequest request, string? actor, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<DesignReviewResponse>> UpdateDesignReviewAsync(Guid id, UpdateDesignReviewRequest request, string? actor, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<IntegrationReviewResponse>> CreateIntegrationReviewAsync(CreateIntegrationReviewRequest request, string? actor, CancellationToken cancellationToken);
    Task<GovernanceCommandResult<IntegrationReviewResponse>> UpdateIntegrationReviewAsync(Guid id, UpdateIntegrationReviewRequest request, string? actor, CancellationToken cancellationToken);
}
