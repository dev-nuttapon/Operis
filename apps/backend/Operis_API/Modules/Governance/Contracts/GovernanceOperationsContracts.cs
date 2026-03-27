namespace Operis_API.Modules.Governance.Contracts;

public sealed record RaciMapListQuery(
    string? Search,
    string? Status,
    string? ProcessCode,
    int Page = 1,
    int PageSize = 25);

public sealed record ApprovalEvidenceListQuery(
    string? EntityType,
    string? ActorUserId,
    string? Outcome,
    DateOnly? ApprovedFrom,
    DateOnly? ApprovedTo,
    int Page = 1,
    int PageSize = 50);

public sealed record WorkflowOverrideListQuery(
    string? EntityType,
    string? RequestedBy,
    string? ApprovedBy,
    DateOnly? OccurredFrom,
    DateOnly? OccurredTo,
    int Page = 1,
    int PageSize = 50);

public sealed record SlaRuleListQuery(
    string? Search,
    string? Status,
    string? ScopeType,
    int Page = 1,
    int PageSize = 25);

public sealed record RetentionPolicyListQuery(
    string? Search,
    string? Status,
    string? AppliesTo,
    int Page = 1,
    int PageSize = 25);

public sealed record ArchitectureRecordListQuery(
    string? Search,
    string? Status,
    Guid? ProjectId,
    string? OwnerUserId,
    string? ArchitectureType,
    int Page = 1,
    int PageSize = 25);

public sealed record DesignReviewListQuery(
    string? Search,
    string? Status,
    Guid? ArchitectureRecordId,
    string? ReviewType,
    string? ReviewedBy,
    int Page = 1,
    int PageSize = 25);

public sealed record IntegrationReviewListQuery(
    string? Search,
    string? Status,
    string? IntegrationType,
    string? ReviewedBy,
    int Page = 1,
    int PageSize = 25);

public sealed record ComplianceDashboardQuery(
    Guid? ProjectId,
    string? ProcessArea,
    int? PeriodDays,
    bool? ShowOnlyAtRisk);

public sealed record ComplianceDashboardDrilldownQuery(
    Guid? ProjectId,
    string? ProcessArea,
    string? IssueType);

public sealed record ManagementReviewListQuery(
    string? Search,
    string? Status,
    Guid? ProjectId,
    string? FacilitatorUserId,
    DateOnly? ScheduledFrom,
    DateOnly? ScheduledTo,
    int Page = 1,
    int PageSize = 25);

public sealed record PolicyListQuery(
    string? Search,
    string? Status,
    int Page = 1,
    int PageSize = 25);

public sealed record PolicyCampaignListQuery(
    Guid? PolicyId,
    string? Status,
    DateOnly? DueBefore,
    int Page = 1,
    int PageSize = 25);

public sealed record PolicyAcknowledgementListQuery(
    Guid? PolicyId,
    Guid? CampaignId,
    string? UserId,
    string? Status,
    bool OnlyOverdue = false,
    int Page = 1,
    int PageSize = 25);

public sealed record UpdateComplianceDashboardPreferencesRequest(
    Guid? DefaultProjectId,
    string? DefaultProcessArea,
    int DefaultPeriodDays,
    bool DefaultShowOnlyAtRisk);

public sealed record ComplianceDashboardPreferenceResponse(
    Guid Id,
    string UserId,
    Guid? DefaultProjectId,
    string? DefaultProcessArea,
    int DefaultPeriodDays,
    bool DefaultShowOnlyAtRisk,
    DateTimeOffset UpdatedAt);

public sealed record ComplianceDashboardFiltersResponse(
    Guid? ProjectId,
    string? ProcessArea,
    int PeriodDays,
    bool ShowOnlyAtRisk);

public sealed record ComplianceDashboardSummaryResponse(
    int ProjectsInGoodStanding,
    int ProjectsWithMissingArtifacts,
    int OverdueApprovals,
    int StaleBaselines,
    int OpenCapa,
    int OpenAuditFindings,
    int OpenSecurityItems);

public sealed record ComplianceProjectReadinessResponse(
    Guid ProjectId,
    string ProjectCode,
    string ProjectName,
    string ProjectStatus,
    string? ProjectPhase,
    int ReadinessScore,
    string ReadinessState,
    int MissingArtifactCount,
    int OverdueApprovalCount,
    int StaleBaselineCount,
    int OpenCapaCount,
    int OpenAuditFindingCount,
    int OpenSecurityItemCount);

public sealed record ComplianceProcessAreaResponse(
    string ProcessArea,
    string Label,
    int ProjectCount,
    int AtRiskProjectCount,
    int MissingArtifactCount,
    int OverdueApprovalCount,
    int StaleBaselineCount,
    int OpenCapaCount,
    int OpenAuditFindingCount,
    int OpenSecurityItemCount);

public sealed record ComplianceDashboardResponse(
    ComplianceDashboardSummaryResponse Summary,
    IReadOnlyList<ComplianceProjectReadinessResponse> Projects,
    IReadOnlyList<ComplianceProcessAreaResponse> ProcessAreas,
    DateTimeOffset GeneratedAt,
    ComplianceDashboardFiltersResponse Filters);

public sealed record ComplianceDrilldownRowResponse(
    string IssueType,
    string EntityType,
    string EntityId,
    string Title,
    string Module,
    string Route,
    string Status,
    string Scope,
    string? DueAt,
    string? Metadata);

public sealed record ComplianceDrilldownResponse(
    string IssueType,
    Guid? ProjectId,
    string? ProcessArea,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<ComplianceDrilldownRowResponse> Rows);

public sealed record ManagementReviewItemResponse(
    Guid Id,
    string ItemType,
    string Title,
    string? Summary,
    string? Decision,
    string? OwnerUserId,
    DateTimeOffset? DueAt,
    string Status,
    DateTimeOffset UpdatedAt);

public sealed record ManagementReviewActionResponse(
    Guid Id,
    string Title,
    string? Description,
    string OwnerUserId,
    DateTimeOffset? DueAt,
    string Status,
    bool IsMandatory,
    string? LinkedEntityType,
    string? LinkedEntityId,
    DateTimeOffset? ClosedAt,
    DateTimeOffset UpdatedAt);

public sealed record PolicyResponse(
    Guid Id,
    string PolicyCode,
    string Title,
    string? Summary,
    DateTimeOffset EffectiveDate,
    bool RequiresAttestation,
    string Status,
    DateTimeOffset? ApprovedAt,
    string? ApprovedBy,
    DateTimeOffset? PublishedAt,
    DateTimeOffset? RetiredAt,
    int CampaignCount,
    int OpenCampaignCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record PolicyCampaignResponse(
    Guid Id,
    Guid PolicyId,
    string PolicyTitle,
    string CampaignCode,
    string Title,
    string TargetScopeType,
    string TargetScopeRef,
    DateTimeOffset DueAt,
    string Status,
    int TargetUserCount,
    int AcknowledgedCount,
    int OverdueCount,
    DateTimeOffset? LaunchedAt,
    string? LaunchedBy,
    DateTimeOffset? ClosedAt,
    string? ClosedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record PolicyAcknowledgementResponse(
    Guid Id,
    Guid PolicyId,
    string PolicyTitle,
    Guid PolicyCampaignId,
    string CampaignTitle,
    string UserId,
    string Status,
    bool IsOverdue,
    bool RequiresAttestation,
    DateTimeOffset DueAt,
    DateTimeOffset? AcknowledgedAt,
    string? AttestationText,
    DateTimeOffset UpdatedAt);

public sealed record ManagementReviewListItemResponse(
    Guid Id,
    Guid? ProjectId,
    string? ProjectName,
    string ReviewCode,
    string Title,
    string ReviewPeriod,
    DateTimeOffset ScheduledAt,
    string FacilitatorUserId,
    string Status,
    int OpenActionCount,
    DateTimeOffset UpdatedAt);

public sealed record ManagementReviewDetailResponse(
    Guid Id,
    Guid? ProjectId,
    string? ProjectName,
    string ReviewCode,
    string Title,
    string ReviewPeriod,
    DateTimeOffset ScheduledAt,
    string FacilitatorUserId,
    string Status,
    string? AgendaSummary,
    string? MinutesSummary,
    string? DecisionSummary,
    string? EscalationEntityType,
    string? EscalationEntityId,
    string? ClosedBy,
    DateTimeOffset? ClosedAt,
    IReadOnlyList<ManagementReviewItemResponse> Items,
    IReadOnlyList<ManagementReviewActionResponse> Actions,
    IReadOnlyList<WorkflowOverrideLogResponse> History,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ManagementReviewItemInput(
    string ItemType,
    string Title,
    string? Summary,
    string? Decision,
    string? OwnerUserId,
    DateTimeOffset? DueAt,
    string? Status);

public sealed record ManagementReviewActionInput(
    string Title,
    string? Description,
    string OwnerUserId,
    DateTimeOffset? DueAt,
    string? Status,
    bool IsMandatory,
    string? LinkedEntityType,
    string? LinkedEntityId);

public sealed record CreateManagementReviewRequest(
    Guid? ProjectId,
    string ReviewCode,
    string Title,
    string ReviewPeriod,
    DateTimeOffset ScheduledAt,
    string FacilitatorUserId,
    string? AgendaSummary,
    string? MinutesSummary,
    string? DecisionSummary,
    string? EscalationEntityType,
    string? EscalationEntityId,
    IReadOnlyList<ManagementReviewItemInput>? Items,
    IReadOnlyList<ManagementReviewActionInput>? Actions);

public sealed record UpdateManagementReviewRequest(
    Guid? ProjectId,
    string ReviewCode,
    string Title,
    string ReviewPeriod,
    DateTimeOffset ScheduledAt,
    string FacilitatorUserId,
    string? AgendaSummary,
    string? MinutesSummary,
    string? DecisionSummary,
    string? EscalationEntityType,
    string? EscalationEntityId,
    IReadOnlyList<ManagementReviewItemInput>? Items,
    IReadOnlyList<ManagementReviewActionInput>? Actions);

public sealed record TransitionManagementReviewRequest(
    string TargetStatus,
    string? Reason);

public sealed record CreatePolicyRequest(
    string PolicyCode,
    string Title,
    string? Summary,
    DateTimeOffset EffectiveDate,
    bool RequiresAttestation);

public sealed record UpdatePolicyRequest(
    string PolicyCode,
    string Title,
    string? Summary,
    DateTimeOffset EffectiveDate,
    bool RequiresAttestation);

public sealed record TransitionPolicyRequest(
    string TargetStatus,
    string? Reason);

public sealed record CreatePolicyCampaignRequest(
    Guid PolicyId,
    string CampaignCode,
    string Title,
    string TargetScopeType,
    string TargetScopeRef,
    DateTimeOffset DueAt);

public sealed record UpdatePolicyCampaignRequest(
    string CampaignCode,
    string Title,
    string TargetScopeType,
    string TargetScopeRef,
    DateTimeOffset DueAt);

public sealed record TransitionPolicyCampaignRequest(
    string TargetStatus,
    string? Reason);

public sealed record CreatePolicyAcknowledgementRequest(
    Guid PolicyCampaignId,
    string? AttestationText);

public sealed record RaciMapResponse(
    Guid Id,
    string ProcessCode,
    string RoleName,
    string ResponsibilityType,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateRaciMapRequest(
    string ProcessCode,
    string RoleName,
    string ResponsibilityType,
    string Status,
    string? Reason);

public sealed record UpdateRaciMapRequest(
    string ProcessCode,
    string RoleName,
    string ResponsibilityType,
    string Status,
    string? Reason);

public sealed record ApprovalEvidenceLogResponse(
    Guid Id,
    string EntityType,
    string EntityId,
    string ApproverUserId,
    DateTimeOffset ApprovedAt,
    string Reason,
    string Outcome);

public sealed record WorkflowOverrideLogResponse(
    Guid Id,
    string EntityType,
    string EntityId,
    string RequestedBy,
    string ApprovedBy,
    string Reason,
    DateTimeOffset OccurredAt);

public sealed record SlaRuleResponse(
    Guid Id,
    string ScopeType,
    string ScopeRef,
    int TargetDurationHours,
    string EscalationPolicyId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateSlaRuleRequest(
    string ScopeType,
    string ScopeRef,
    int TargetDurationHours,
    string EscalationPolicyId,
    string Status,
    string? Reason);

public sealed record UpdateSlaRuleRequest(
    string ScopeType,
    string ScopeRef,
    int TargetDurationHours,
    string EscalationPolicyId,
    string Status,
    string? Reason);

public sealed record RetentionPolicyResponse(
    Guid Id,
    string PolicyCode,
    string AppliesTo,
    int RetentionPeriodDays,
    string? ArchiveRule,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateRetentionPolicyRequest(
    string PolicyCode,
    string AppliesTo,
    int RetentionPeriodDays,
    string? ArchiveRule,
    string Status,
    string? Reason);

public sealed record UpdateRetentionPolicyRequest(
    string PolicyCode,
    string AppliesTo,
    int RetentionPeriodDays,
    string? ArchiveRule,
    string Status,
    string? Reason);

public sealed record ArchitectureRecordResponse(
    Guid Id,
    Guid ProjectId,
    string? ProjectName,
    string Title,
    string ArchitectureType,
    string OwnerUserId,
    string Status,
    string? CurrentVersionId,
    string? Summary,
    string? SecurityImpact,
    string? EvidenceRef,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateArchitectureRecordRequest(
    Guid ProjectId,
    string Title,
    string ArchitectureType,
    string OwnerUserId,
    string Status,
    string? CurrentVersionId,
    string? Summary,
    string? SecurityImpact,
    string? EvidenceRef);

public sealed record UpdateArchitectureRecordRequest(
    Guid ProjectId,
    string Title,
    string ArchitectureType,
    string OwnerUserId,
    string Status,
    string? CurrentVersionId,
    string? Summary,
    string? SecurityImpact,
    string? EvidenceRef);

public sealed record DesignReviewResponse(
    Guid Id,
    Guid ArchitectureRecordId,
    string? ArchitectureTitle,
    string ReviewType,
    string? ReviewedBy,
    string Status,
    string? DecisionReason,
    string? DesignSummary,
    string? Concerns,
    string? EvidenceRef,
    DateTimeOffset? DecidedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateDesignReviewRequest(
    Guid ArchitectureRecordId,
    string ReviewType,
    string? ReviewedBy,
    string Status,
    string? DecisionReason,
    string? DesignSummary,
    string? Concerns,
    string? EvidenceRef);

public sealed record UpdateDesignReviewRequest(
    Guid ArchitectureRecordId,
    string ReviewType,
    string? ReviewedBy,
    string Status,
    string? DecisionReason,
    string? DesignSummary,
    string? Concerns,
    string? EvidenceRef);

public sealed record IntegrationReviewResponse(
    Guid Id,
    string ScopeRef,
    string IntegrationType,
    string? ReviewedBy,
    string Status,
    string? DecisionReason,
    string? Risks,
    string? DependencyImpact,
    string? EvidenceRef,
    DateTimeOffset? DecidedAt,
    DateTimeOffset? AppliedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateIntegrationReviewRequest(
    string ScopeRef,
    string IntegrationType,
    string? ReviewedBy,
    string Status,
    string? DecisionReason,
    string? Risks,
    string? DependencyImpact,
    string? EvidenceRef);

public sealed record UpdateIntegrationReviewRequest(
    string ScopeRef,
    string IntegrationType,
    string? ReviewedBy,
    string Status,
    string? DecisionReason,
    string? Risks,
    string? DependencyImpact,
    string? EvidenceRef);
