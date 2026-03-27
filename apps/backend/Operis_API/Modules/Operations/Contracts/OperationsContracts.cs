namespace Operis_API.Modules.Operations.Contracts;

public sealed record CreateAccessReviewRequest(string ScopeType, string ScopeRef, string ReviewCycle, string? ReviewedBy);
public sealed record UpdateAccessReviewRequest(string ScopeType, string ScopeRef, string ReviewCycle, string? ReviewedBy, string Status);
public sealed record ApproveAccessReviewRequest(string Decision, string DecisionRationale);
public sealed record CreateSecurityReviewRequest(string ScopeType, string ScopeRef, string ControlsReviewed, string? FindingsSummary, string Status);
public sealed record UpdateSecurityReviewRequest(string ScopeType, string ScopeRef, string ControlsReviewed, string? FindingsSummary, string Status);
public sealed record CreateExternalDependencyRequest(string Name, string DependencyType, Guid? SupplierId, string OwnerUserId, string Criticality, DateTimeOffset? ReviewDueAt, string Status);
public sealed record UpdateExternalDependencyRequest(string Name, string DependencyType, Guid? SupplierId, string OwnerUserId, string Criticality, DateTimeOffset? ReviewDueAt, string Status);
public sealed record CreateConfigurationAuditRequest(string ScopeRef, DateTimeOffset PlannedAt, string Status, int FindingCount);
public sealed record CreateSupplierRequest(string Name, string SupplierType, string OwnerUserId, string Criticality, DateTimeOffset? ReviewDueAt, string Status);
public sealed record UpdateSupplierRequest(string Name, string SupplierType, string OwnerUserId, string Criticality, DateTimeOffset? ReviewDueAt, string Status);
public sealed record CreateSupplierAgreementRequest(Guid SupplierId, string AgreementType, DateOnly? EffectiveFrom, DateOnly? EffectiveTo, string? SlaTerms, string? EvidenceRef, string Status);
public sealed record UpdateSupplierAgreementRequest(Guid SupplierId, string AgreementType, DateOnly? EffectiveFrom, DateOnly? EffectiveTo, string? SlaTerms, string? EvidenceRef, string Status);
public sealed record CreateAccessRecertificationRequest(string ScopeType, string ScopeRef, DateTimeOffset PlannedAt, string ReviewOwnerUserId, IReadOnlyList<string>? SubjectUserIds, string? ExceptionNotes);
public sealed record UpdateAccessRecertificationRequest(string ScopeType, string ScopeRef, DateTimeOffset PlannedAt, string ReviewOwnerUserId, string Status, IReadOnlyList<string>? SubjectUserIds, string? ExceptionNotes);
public sealed record AddAccessRecertificationDecisionRequest(string SubjectUserId, string Decision, string? Reason);

public sealed record AccessReviewResponse(Guid Id, string ScopeType, string ScopeRef, string ReviewCycle, string? ReviewedBy, string Status, string? Decision, string? DecisionRationale, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
public sealed record SecurityReviewResponse(Guid Id, string ScopeType, string ScopeRef, string ControlsReviewed, string? FindingsSummary, string Status, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
public sealed record ExternalDependencyResponse(Guid Id, string Name, string DependencyType, Guid? SupplierId, string? SupplierName, string OwnerUserId, string Criticality, string Status, DateTimeOffset? ReviewDueAt, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
public sealed record ConfigurationAuditResponse(Guid Id, string ScopeRef, DateTimeOffset PlannedAt, string Status, int FindingCount, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
public sealed record SupplierResponse(Guid Id, string Name, string SupplierType, string OwnerUserId, string Criticality, string Status, DateTimeOffset? ReviewDueAt, int ActiveAgreementCount, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
public sealed record SupplierAgreementResponse(Guid Id, Guid SupplierId, string SupplierName, string AgreementType, DateOnly EffectiveFrom, DateOnly? EffectiveTo, string SlaTerms, string EvidenceRef, string Status, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
public sealed record AccessRecertificationDecisionResponse(Guid Id, Guid ScheduleId, string SubjectUserId, string Decision, string Reason, string DecidedBy, DateTimeOffset DecidedAt);
public sealed record AccessRecertificationResponse(Guid Id, string ScopeType, string ScopeRef, DateTimeOffset PlannedAt, string ReviewOwnerUserId, string Status, IReadOnlyList<string> SubjectUserIds, IReadOnlyList<AccessRecertificationDecisionResponse> Decisions, string? ExceptionNotes, int CompletedCount, int PendingCount, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt, DateTimeOffset? CompletedAt);
