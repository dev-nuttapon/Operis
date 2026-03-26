using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Risks.Contracts;

public sealed record RiskListItemResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    int Probability,
    int Impact,
    string OwnerUserId,
    string Status,
    DateTimeOffset? NextReviewAt,
    DateTimeOffset UpdatedAt);

public sealed record RiskReviewItemResponse(
    Guid Id,
    Guid RiskId,
    string ReviewedBy,
    DateTimeOffset ReviewedAt,
    string Decision,
    string? Notes);

public sealed record RiskHistoryItem(
    Guid Id,
    string EventType,
    string? Summary,
    string? Reason,
    string? ActorUserId,
    DateTimeOffset OccurredAt);

public sealed record RiskDetailResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    string Description,
    int Probability,
    int Impact,
    string OwnerUserId,
    string? MitigationPlan,
    string? Cause,
    string? Effect,
    string? ContingencyPlan,
    string Status,
    DateTimeOffset? NextReviewAt,
    IReadOnlyList<RiskReviewItemResponse> Reviews,
    IReadOnlyList<RiskHistoryItem> History,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record IssueListItemResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    string Severity,
    string OwnerUserId,
    DateOnly? DueDate,
    string Status,
    int OpenActionCount,
    bool IsSensitive,
    DateTimeOffset UpdatedAt);

public sealed record IssueActionResponse(
    Guid Id,
    Guid IssueId,
    string ActionDescription,
    string AssignedTo,
    DateOnly? DueDate,
    string Status,
    string? VerificationNote,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record IssueDetailResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    string? Description,
    string OwnerUserId,
    DateOnly? DueDate,
    string Status,
    string Severity,
    string? RootIssue,
    string? Dependencies,
    string? ResolutionSummary,
    bool IsSensitive,
    string? SensitiveContext,
    IReadOnlyList<IssueActionResponse> Actions,
    IReadOnlyList<RiskHistoryItem> History,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateRiskRequest(
    Guid ProjectId,
    string Code,
    string Title,
    string Description,
    int Probability,
    int Impact,
    string OwnerUserId,
    string? MitigationPlan,
    string? Cause,
    string? Effect,
    string? ContingencyPlan,
    DateTimeOffset? NextReviewAt);

public sealed record UpdateRiskRequest(
    string Title,
    string Description,
    int Probability,
    int Impact,
    string OwnerUserId,
    string? MitigationPlan,
    string? Cause,
    string? Effect,
    string? ContingencyPlan,
    DateTimeOffset? NextReviewAt);

public sealed record RiskTransitionRequest(
    string? Notes,
    string? MitigationPlan,
    DateTimeOffset? NextReviewAt);

public sealed record CreateIssueRequest(
    Guid ProjectId,
    string Code,
    string Title,
    string Description,
    string OwnerUserId,
    DateOnly? DueDate,
    string Severity,
    string? RootIssue,
    string? Dependencies,
    bool IsSensitive,
    string? SensitiveContext);

public sealed record UpdateIssueRequest(
    string Title,
    string Description,
    string OwnerUserId,
    DateOnly? DueDate,
    string Severity,
    string? RootIssue,
    string? Dependencies,
    string? ResolutionSummary,
    bool IsSensitive,
    string? SensitiveContext,
    string? Status);

public sealed record CreateIssueActionRequest(
    string ActionDescription,
    string AssignedTo,
    DateOnly? DueDate);

public sealed record UpdateIssueActionRequest(
    string ActionDescription,
    string AssignedTo,
    DateOnly? DueDate,
    string Status,
    string? VerificationNote);

public sealed record IssueResolutionRequest(string? ResolutionSummary);

public sealed record RiskListQuery(
    string? Search,
    Guid? ProjectId,
    string? Status,
    string? OwnerUserId,
    int? RiskLevel,
    DateTimeOffset? NextReviewBefore,
    int Page = 1,
    int PageSize = 25);

public sealed record IssueListQuery(
    string? Search,
    Guid? ProjectId,
    string? Status,
    string? OwnerUserId,
    string? Severity,
    DateOnly? DueBefore,
    DateOnly? DueAfter,
    int Page = 1,
    int PageSize = 25);
