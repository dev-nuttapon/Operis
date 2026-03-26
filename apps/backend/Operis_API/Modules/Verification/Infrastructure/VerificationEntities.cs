namespace Operis_API.Modules.Verification.Infrastructure;

public sealed record TestPlanEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ScopeSummary { get; init; } = string.Empty;
    public string OwnerUserId { get; init; } = string.Empty;
    public string Status { get; init; } = "draft";
    public string? EntryCriteria { get; init; }
    public string? ExitCriteria { get; init; }
    public string LinkedRequirementIdsJson { get; init; } = "[]";
    public string? ApprovalReason { get; init; }
    public string? ApprovedBy { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public DateTimeOffset? BaselinedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record TestCaseEntity
{
    public Guid Id { get; init; }
    public Guid TestPlanId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Preconditions { get; init; }
    public string StepsJson { get; init; } = "[]";
    public string ExpectedResult { get; init; } = string.Empty;
    public Guid? RequirementId { get; init; }
    public string Status { get; init; } = "draft";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record TestExecutionEntity
{
    public Guid Id { get; init; }
    public Guid TestCaseId { get; init; }
    public string ExecutedBy { get; init; } = string.Empty;
    public DateTimeOffset ExecutedAt { get; init; }
    public string Result { get; init; } = string.Empty;
    public string? EvidenceRef { get; init; }
    public string? Notes { get; init; }
    public bool IsSensitiveEvidence { get; init; }
    public string? EvidenceClassification { get; init; }
}

public sealed record UatSignoffEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string? ReleaseId { get; init; }
    public string ScopeSummary { get; init; } = string.Empty;
    public string? SubmittedBy { get; init; }
    public DateTimeOffset? SubmittedAt { get; init; }
    public string? ApprovedBy { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public string Status { get; init; } = "draft";
    public string? DecisionReason { get; init; }
    public string EvidenceRefsJson { get; init; } = "[]";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
