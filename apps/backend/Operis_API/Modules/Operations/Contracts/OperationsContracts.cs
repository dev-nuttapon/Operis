namespace Operis_API.Modules.Operations.Contracts;

public sealed record CreateAccessReviewRequest(string ScopeType, string ScopeRef, string ReviewCycle, string? ReviewedBy);
public sealed record UpdateAccessReviewRequest(string ScopeType, string ScopeRef, string ReviewCycle, string? ReviewedBy, string Status);
public sealed record ApproveAccessReviewRequest(string Decision, string DecisionRationale);
public sealed record CreateSecurityReviewRequest(string ScopeType, string ScopeRef, string ControlsReviewed, string? FindingsSummary, string Status);
public sealed record UpdateSecurityReviewRequest(string ScopeType, string ScopeRef, string ControlsReviewed, string? FindingsSummary, string Status);
public sealed record CreateExternalDependencyRequest(string Name, string DependencyType, string OwnerUserId, string Criticality, DateTimeOffset? ReviewDueAt, string Status);
public sealed record UpdateExternalDependencyRequest(string Name, string DependencyType, string OwnerUserId, string Criticality, DateTimeOffset? ReviewDueAt, string Status);
public sealed record CreateConfigurationAuditRequest(string ScopeRef, DateTimeOffset PlannedAt, string Status, int FindingCount);

public sealed record AccessReviewResponse(Guid Id, string ScopeType, string ScopeRef, string ReviewCycle, string? ReviewedBy, string Status, string? Decision, string? DecisionRationale, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
public sealed record SecurityReviewResponse(Guid Id, string ScopeType, string ScopeRef, string ControlsReviewed, string? FindingsSummary, string Status, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
public sealed record ExternalDependencyResponse(Guid Id, string Name, string DependencyType, string OwnerUserId, string Criticality, string Status, DateTimeOffset? ReviewDueAt, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
public sealed record ConfigurationAuditResponse(Guid Id, string ScopeRef, DateTimeOffset PlannedAt, string Status, int FindingCount, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
