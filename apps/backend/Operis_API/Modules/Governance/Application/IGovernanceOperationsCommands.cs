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
}
