using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Verification.Contracts;

public sealed record TestPlanListItemResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    string OwnerUserId,
    string Status,
    string CoverageStatus,
    int LinkedRequirementCount,
    int CoveredRequirementCount,
    DateTimeOffset UpdatedAt);

public sealed record TestCaseListItemResponse(
    Guid Id,
    Guid TestPlanId,
    string TestPlanCode,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    string Status,
    Guid? RequirementId,
    string? RequirementCode,
    string? LatestResult,
    DateTimeOffset? LatestExecutedAt,
    DateTimeOffset UpdatedAt);

public sealed record TestExecutionListItemResponse(
    Guid Id,
    Guid TestCaseId,
    string TestCaseCode,
    string ExecutedBy,
    DateTimeOffset ExecutedAt,
    string Result,
    string? EvidenceRef,
    bool IsSensitiveEvidence,
    string? EvidenceClassification,
    string? Notes);

public sealed record VerificationHistoryItem(
    Guid Id,
    string EventType,
    string? Summary,
    string? Reason,
    string? ActorUserId,
    DateTimeOffset OccurredAt);

public sealed record TestPlanDetailResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    string ScopeSummary,
    string OwnerUserId,
    string Status,
    string? EntryCriteria,
    string? ExitCriteria,
    IReadOnlyList<Guid> LinkedRequirementIds,
    string CoverageStatus,
    IReadOnlyList<TestCaseListItemResponse> TestCases,
    IReadOnlyList<VerificationHistoryItem> History,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TestCaseDetailResponse(
    Guid Id,
    Guid TestPlanId,
    string TestPlanCode,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    string? Preconditions,
    IReadOnlyList<string> Steps,
    string ExpectedResult,
    Guid? RequirementId,
    string? RequirementCode,
    string Status,
    string? LatestResult,
    IReadOnlyList<TestExecutionListItemResponse> Executions,
    IReadOnlyList<VerificationHistoryItem> History,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TestExecutionCreateResponse(
    Guid Id,
    DateTimeOffset ExecutedAt,
    string Result);

public sealed record ExecutionExportResponse(
    string Status,
    int Count,
    IReadOnlyList<TestExecutionListItemResponse> Items,
    string? Message);

public sealed record UatSignoffListItemResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string? ReleaseId,
    string Status,
    string? SubmittedBy,
    string? ApprovedBy,
    int EvidenceCount,
    DateTimeOffset UpdatedAt);

public sealed record UatSignoffDetailResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string? ReleaseId,
    string ScopeSummary,
    string? SubmittedBy,
    DateTimeOffset? SubmittedAt,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    string Status,
    string? DecisionReason,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<VerificationHistoryItem> History,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateTestPlanRequest(
    Guid ProjectId,
    string Code,
    string Title,
    string ScopeSummary,
    string OwnerUserId,
    string? EntryCriteria,
    string? ExitCriteria,
    IReadOnlyList<Guid>? LinkedRequirementIds);

public sealed record UpdateTestPlanRequest(
    string Title,
    string ScopeSummary,
    string OwnerUserId,
    string? EntryCriteria,
    string? ExitCriteria,
    IReadOnlyList<Guid>? LinkedRequirementIds,
    string? Status);

public sealed record VerificationDecisionRequest(string? Reason);

public sealed record CreateTestCaseRequest(
    Guid TestPlanId,
    string Code,
    string Title,
    string? Preconditions,
    IReadOnlyList<string>? Steps,
    string ExpectedResult,
    Guid? RequirementId,
    string? Status);

public sealed record UpdateTestCaseRequest(
    string Title,
    string? Preconditions,
    IReadOnlyList<string>? Steps,
    string ExpectedResult,
    Guid? RequirementId,
    string? Status);

public sealed record CreateTestExecutionRequest(
    Guid TestCaseId,
    string Result,
    string? EvidenceRef,
    string? Notes,
    bool IsSensitiveEvidence,
    string? EvidenceClassification);

public sealed record ExecutionExportRequest(
    Guid? TestCaseId,
    string? Result,
    string? ExecutedBy,
    DateTimeOffset? From,
    DateTimeOffset? To);

public sealed record CreateUatSignoffRequest(
    Guid ProjectId,
    string? ReleaseId,
    string ScopeSummary,
    IReadOnlyList<string>? EvidenceRefs,
    string? DecisionReason);

public sealed record UpdateUatSignoffRequest(
    string? ReleaseId,
    string ScopeSummary,
    IReadOnlyList<string>? EvidenceRefs,
    string? DecisionReason);

public sealed record TestPlanListQuery(
    string? Search,
    Guid? ProjectId,
    string? Status,
    string? OwnerUserId,
    string? CoverageStatus,
    int Page = 1,
    int PageSize = 25);

public sealed record TestCaseListQuery(
    string? Search,
    Guid? TestPlanId,
    Guid? RequirementId,
    string? Status,
    string? LatestResult,
    int Page = 1,
    int PageSize = 25);

public sealed record TestExecutionListQuery(
    Guid? TestCaseId,
    string? Result,
    string? ExecutedBy,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page = 1,
    int PageSize = 25);

public sealed record UatSignoffListQuery(
    Guid? ProjectId,
    string? Status,
    string? SubmittedBy,
    int Page = 1,
    int PageSize = 25);
