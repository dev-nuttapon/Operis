using Operis_API.Modules.Risks.Contracts;

namespace Operis_API.Modules.Risks.Application;

public interface IRiskCommands
{
    Task<RiskCommandResult<RiskDetailResponse>> CreateRiskAsync(CreateRiskRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RiskCommandResult<RiskDetailResponse>> UpdateRiskAsync(Guid riskId, UpdateRiskRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RiskCommandResult<RiskDetailResponse>> AssessRiskAsync(Guid riskId, RiskTransitionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RiskCommandResult<RiskDetailResponse>> MitigateRiskAsync(Guid riskId, RiskTransitionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RiskCommandResult<RiskDetailResponse>> CloseRiskAsync(Guid riskId, RiskTransitionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RiskCommandResult<IssueDetailResponse>> CreateIssueAsync(CreateIssueRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RiskCommandResult<IssueDetailResponse>> UpdateIssueAsync(Guid issueId, UpdateIssueRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RiskCommandResult<IssueDetailResponse>> CreateIssueActionAsync(Guid issueId, CreateIssueActionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RiskCommandResult<IssueDetailResponse>> UpdateIssueActionAsync(Guid issueId, Guid actionId, UpdateIssueActionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RiskCommandResult<IssueDetailResponse>> ResolveIssueAsync(Guid issueId, IssueResolutionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<RiskCommandResult<IssueDetailResponse>> CloseIssueAsync(Guid issueId, IssueResolutionRequest request, string? actorUserId, CancellationToken cancellationToken);
}
