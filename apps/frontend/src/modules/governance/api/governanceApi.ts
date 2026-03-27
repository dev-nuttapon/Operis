import { apiRequest } from "../../../shared/lib/apiClient";
import type {
  ApprovalEvidenceLog,
  ArchitectureRecord,
  ArchitectureRecordFormInput,
  ComplianceDashboard,
  ComplianceDashboardInput,
  ComplianceDashboardPreference,
  ComplianceDashboardPreferenceInput,
  ComplianceDrilldown,
  DesignReview,
  DesignReviewFormInput,
  GovernanceListInput,
  GovernanceListResult,
  GovernanceMutationResponse,
  IntegrationReview,
  IntegrationReviewFormInput,
  ManagementReviewDetail,
  ManagementReviewFormInput,
  ManagementReviewListItem,
  ManagementReviewTransitionInput,
  PolicyAcknowledgementInput,
  PolicyAcknowledgementItem,
  PolicyCampaignFormInput,
  PolicyCampaignItem,
  PolicyCampaignTransitionInput,
  PolicyFormInput,
  PolicyListItem,
  PolicyTransitionInput,
  ProcessAsset,
  ProcessAssetFormInput,
  ProcessAssetListItem,
  ProcessAssetVersionFormInput,
  ProjectPlan,
  ProjectPlanFormInput,
  ProjectPlanListItem,
  QaChecklist,
  QaChecklistFormInput,
  QaChecklistListItem,
  RaciMap,
  RaciMapFormInput,
  RetentionPolicy,
  RetentionPolicyFormInput,
  SlaRule,
  SlaRuleFormInput,
  Stakeholder,
  StakeholderFormInput,
  TailoringCriteria,
  TailoringCriteriaFormInput,
  TailoringRecord,
  TailoringRecordFormInput,
  TailoringRecordListItem,
  TailoringReviewCycle,
  TailoringReviewCycleFormInput,
  TailoringReviewCycleUpdateInput,
  TailoringReviewTransitionInput,
  WorkflowOverrideLog,
} from "../types/governance";

function toQuery(input?: GovernanceListInput) {
  const params = new URLSearchParams();
  if (input?.page) params.set("page", String(input.page));
  if (input?.pageSize) params.set("pageSize", String(input.pageSize));
  if (input?.search) params.set("search", input.search);
  if (input?.status) params.set("status", input.status);
  if (input?.ownerUserId) params.set("ownerUserId", input.ownerUserId);
  if (input?.projectId) params.set("projectId", input.projectId);
  if (input?.processCode) params.set("processCode", input.processCode);
  if (input?.entityType) params.set("entityType", input.entityType);
  if (input?.actorUserId) params.set("actorUserId", input.actorUserId);
  if (input?.outcome) params.set("outcome", input.outcome);
  if (input?.approvedFrom) params.set("approvedFrom", input.approvedFrom);
  if (input?.approvedTo) params.set("approvedTo", input.approvedTo);
  if (input?.requestedBy) params.set("requestedBy", input.requestedBy);
  if (input?.approvedBy) params.set("approvedBy", input.approvedBy);
  if (input?.occurredFrom) params.set("occurredFrom", input.occurredFrom);
  if (input?.occurredTo) params.set("occurredTo", input.occurredTo);
  if (input?.scopeType) params.set("scopeType", input.scopeType);
  if (input?.appliesTo) params.set("appliesTo", input.appliesTo);
  if (input?.architectureType) params.set("architectureType", input.architectureType);
  if (input?.architectureRecordId) params.set("architectureRecordId", input.architectureRecordId);
  if (input?.reviewType) params.set("reviewType", input.reviewType);
  if (input?.reviewedBy) params.set("reviewedBy", input.reviewedBy);
  if (input?.integrationType) params.set("integrationType", input.integrationType);
  if (input?.facilitatorUserId) params.set("facilitatorUserId", input.facilitatorUserId);
  if (input?.scheduledFrom) params.set("scheduledFrom", input.scheduledFrom);
  if (input?.scheduledTo) params.set("scheduledTo", input.scheduledTo);
  if (input?.policyId) params.set("policyId", input.policyId);
  if (input?.campaignId) params.set("campaignId", input.campaignId);
  if (input?.dueBefore) params.set("dueBefore", input.dueBefore);
  if (typeof input?.onlyOverdue === "boolean") params.set("onlyOverdue", String(input.onlyOverdue));
  const query = params.toString();
  return query ? `?${query}` : "";
}

export const listProcessAssets = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<ProcessAssetListItem>>(`/api/v1/governance/process-assets${toQuery(input)}`, { signal });
export const getProcessAsset = (id: string, signal?: AbortSignal) =>
  apiRequest<ProcessAsset>(`/api/v1/governance/process-assets/${id}`, { signal });
export const createProcessAsset = (input: ProcessAssetFormInput) =>
  apiRequest<ProcessAsset>("/api/v1/governance/process-assets", { method: "POST", body: input });
export const updateProcessAsset = (id: string, input: ProcessAssetFormInput) =>
  apiRequest<ProcessAsset>(`/api/v1/governance/process-assets/${id}`, { method: "PUT", body: input });
export const createProcessAssetVersion = (id: string, input: ProcessAssetVersionFormInput) =>
  apiRequest<ProcessAsset>(`/api/v1/governance/process-assets/${id}/versions`, { method: "POST", body: input });
export const updateProcessAssetVersion = (processAssetId: string, versionId: string, input: ProcessAssetVersionFormInput) =>
  apiRequest<ProcessAsset>(`/api/v1/governance/process-assets/${processAssetId}/versions/${versionId}`, { method: "PUT", body: input });
export const submitProcessAssetVersionReview = (processAssetId: string, versionId: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/process-assets/${processAssetId}/versions/${versionId}/submit-review`, { method: "PUT" });
export const approveProcessAssetVersion = (processAssetId: string, versionId: string, changeSummary: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/process-assets/${processAssetId}/versions/${versionId}/approve`, { method: "PUT", body: { changeSummary } });
export const activateProcessAssetVersion = (processAssetId: string, versionId: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/process-assets/${processAssetId}/versions/${versionId}/activate`, { method: "PUT" });
export const deprecateProcessAsset = (processAssetId: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/process-assets/${processAssetId}/deprecate`, { method: "PUT" });

export const listQaChecklists = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<QaChecklistListItem>>(`/api/v1/governance/qa-checklists${toQuery(input)}`, { signal });
export const getQaChecklist = (id: string, signal?: AbortSignal) =>
  apiRequest<QaChecklist>(`/api/v1/governance/qa-checklists/${id}`, { signal });
export const createQaChecklist = (input: QaChecklistFormInput) =>
  apiRequest<QaChecklist>("/api/v1/governance/qa-checklists", { method: "POST", body: input });
export const updateQaChecklist = (id: string, input: QaChecklistFormInput) =>
  apiRequest<QaChecklist>(`/api/v1/governance/qa-checklists/${id}`, { method: "PUT", body: input });
export const approveQaChecklist = (id: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/qa-checklists/${id}/approve`, { method: "PUT" });
export const activateQaChecklist = (id: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/qa-checklists/${id}/activate`, { method: "PUT" });
export const deprecateQaChecklist = (id: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/qa-checklists/${id}/deprecate`, { method: "PUT" });

export const listProjectPlans = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<ProjectPlanListItem>>(`/api/v1/governance/project-plans${toQuery(input)}`, { signal });
export const getProjectPlan = (id: string, signal?: AbortSignal) =>
  apiRequest<ProjectPlan>(`/api/v1/governance/project-plans/${id}`, { signal });
export const createProjectPlan = (input: ProjectPlanFormInput) =>
  apiRequest<ProjectPlan>("/api/v1/governance/project-plans", { method: "POST", body: input });
export const updateProjectPlan = (id: string, input: ProjectPlanFormInput) =>
  apiRequest<ProjectPlan>(`/api/v1/governance/project-plans/${id}`, { method: "PUT", body: input });
export const submitProjectPlanReview = (id: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/project-plans/${id}/submit-review`, { method: "PUT" });
export const approveProjectPlan = (id: string, reason: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/project-plans/${id}/approve`, { method: "PUT", body: { reason } });
export const baselineProjectPlan = (id: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/project-plans/${id}/baseline`, { method: "PUT" });
export const supersedeProjectPlan = (id: string, reason: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/project-plans/${id}/supersede`, { method: "PUT", body: { reason } });

export const listStakeholders = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<Stakeholder>>(`/api/v1/governance/stakeholders${toQuery(input)}`, { signal });
export const getStakeholder = (id: string, signal?: AbortSignal) =>
  apiRequest<Stakeholder>(`/api/v1/governance/stakeholders/${id}`, { signal });
export const createStakeholder = (input: StakeholderFormInput) =>
  apiRequest<Stakeholder>("/api/v1/governance/stakeholders", { method: "POST", body: input });
export const updateStakeholder = (id: string, input: StakeholderFormInput) =>
  apiRequest<Stakeholder>(`/api/v1/governance/stakeholders/${id}`, { method: "PUT", body: input });
export const archiveStakeholder = (id: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/stakeholders/${id}/archive`, { method: "PUT" });

export const listTailoringRecords = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<TailoringRecordListItem>>(`/api/v1/governance/tailoring-records${toQuery(input)}`, { signal });
export const getTailoringRecord = (id: string, signal?: AbortSignal) =>
  apiRequest<TailoringRecord>(`/api/v1/governance/tailoring-records/${id}`, { signal });
export const createTailoringRecord = (input: TailoringRecordFormInput) =>
  apiRequest<TailoringRecord>("/api/v1/governance/tailoring-records", { method: "POST", body: input });
export const updateTailoringRecord = (id: string, input: TailoringRecordFormInput) =>
  apiRequest<TailoringRecord>(`/api/v1/governance/tailoring-records/${id}`, { method: "PUT", body: input });
export const submitTailoringRecord = (id: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/tailoring-records/${id}/submit`, { method: "PUT" });
export const approveTailoringRecord = (id: string, decision: "approved" | "rejected", reason: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/tailoring-records/${id}/approve`, { method: "PUT", body: { decision, reason } });
export const applyTailoringRecord = (id: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/tailoring-records/${id}/apply`, { method: "PUT" });
export const archiveTailoringRecord = (id: string) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/tailoring-records/${id}/archive`, { method: "PUT" });
export const listTailoringCriteria = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<TailoringCriteria>>(`/api/v1/governance/tailoring-criteria${toQuery(input)}`, { signal });
export const createTailoringCriteria = (input: TailoringCriteriaFormInput) =>
  apiRequest<TailoringCriteria>("/api/v1/governance/tailoring-criteria", { method: "POST", body: input });
export const updateTailoringCriteria = (id: string, input: TailoringCriteriaFormInput) =>
  apiRequest<TailoringCriteria>(`/api/v1/governance/tailoring-criteria/${id}`, { method: "PUT", body: input });
export const listTailoringReviewCycles = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<TailoringReviewCycle>>(`/api/v1/governance/tailoring-reviews${toQuery(input)}`, { signal });
export const getTailoringReviewCycle = (id: string, signal?: AbortSignal) =>
  apiRequest<TailoringReviewCycle>(`/api/v1/governance/tailoring-reviews/${id}`, { signal });
export const createTailoringReviewCycle = (input: TailoringReviewCycleFormInput) =>
  apiRequest<TailoringReviewCycle>("/api/v1/governance/tailoring-reviews", { method: "POST", body: input });
export const updateTailoringReviewCycle = (id: string, input: TailoringReviewCycleUpdateInput) =>
  apiRequest<TailoringReviewCycle>(`/api/v1/governance/tailoring-reviews/${id}`, { method: "PUT", body: input });
export const transitionTailoringReviewCycle = (id: string, input: TailoringReviewTransitionInput) =>
  apiRequest<GovernanceMutationResponse>(`/api/v1/governance/tailoring-reviews/${id}/transition`, { method: "POST", body: input });

export const listRaciMaps = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<RaciMap>>(`/api/v1/governance/raci-maps${toQuery(input)}`, { signal });
export const createRaciMap = (input: RaciMapFormInput) =>
  apiRequest<RaciMap>("/api/v1/governance/raci-maps", { method: "POST", body: input });
export const updateRaciMap = (id: string, input: RaciMapFormInput) =>
  apiRequest<RaciMap>(`/api/v1/governance/raci-maps/${id}`, { method: "PUT", body: input });

export const listApprovalEvidence = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<ApprovalEvidenceLog>>(`/api/v1/governance/approval-evidence${toQuery(input)}`, { signal });

export const listWorkflowOverrides = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<WorkflowOverrideLog>>(`/api/v1/governance/workflow-overrides${toQuery(input)}`, { signal });

export const listSlaRules = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<SlaRule>>(`/api/v1/governance/sla-rules${toQuery(input)}`, { signal });
export const createSlaRule = (input: SlaRuleFormInput) =>
  apiRequest<SlaRule>("/api/v1/governance/sla-rules", { method: "POST", body: input });
export const updateSlaRule = (id: string, input: SlaRuleFormInput) =>
  apiRequest<SlaRule>(`/api/v1/governance/sla-rules/${id}`, { method: "PUT", body: input });

export const listRetentionPolicies = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<RetentionPolicy>>(`/api/v1/governance/retention-policies${toQuery(input)}`, { signal });
export const createRetentionPolicy = (input: RetentionPolicyFormInput) =>
  apiRequest<RetentionPolicy>("/api/v1/governance/retention-policies", { method: "POST", body: input });
export const updateRetentionPolicy = (id: string, input: RetentionPolicyFormInput) =>
  apiRequest<RetentionPolicy>(`/api/v1/governance/retention-policies/${id}`, { method: "PUT", body: input });

export const listArchitectureRecords = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<ArchitectureRecord>>(`/api/v1/governance/architecture-records${toQuery(input)}`, { signal });
export const getArchitectureRecord = (id: string, signal?: AbortSignal) =>
  apiRequest<ArchitectureRecord>(`/api/v1/governance/architecture-records/${id}`, { signal });
export const createArchitectureRecord = (input: ArchitectureRecordFormInput) =>
  apiRequest<ArchitectureRecord>("/api/v1/governance/architecture-records", { method: "POST", body: input });
export const updateArchitectureRecord = (id: string, input: ArchitectureRecordFormInput) =>
  apiRequest<ArchitectureRecord>(`/api/v1/governance/architecture-records/${id}`, { method: "PUT", body: input });

export const listDesignReviews = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<DesignReview>>(`/api/v1/governance/design-reviews${toQuery(input)}`, { signal });
export const createDesignReview = (input: DesignReviewFormInput) =>
  apiRequest<DesignReview>("/api/v1/governance/design-reviews", { method: "POST", body: input });
export const updateDesignReview = (id: string, input: DesignReviewFormInput) =>
  apiRequest<DesignReview>(`/api/v1/governance/design-reviews/${id}`, { method: "PUT", body: input });

export const listIntegrationReviews = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<IntegrationReview>>(`/api/v1/governance/integration-reviews${toQuery(input)}`, { signal });
export const createIntegrationReview = (input: IntegrationReviewFormInput) =>
  apiRequest<IntegrationReview>("/api/v1/governance/integration-reviews", { method: "POST", body: input });
export const updateIntegrationReview = (id: string, input: IntegrationReviewFormInput) =>
  apiRequest<IntegrationReview>(`/api/v1/governance/integration-reviews/${id}`, { method: "PUT", body: input });

export const getComplianceDashboard = (input?: ComplianceDashboardInput, signal?: AbortSignal) => {
  const params = new URLSearchParams();
  if (input?.projectId) params.set("projectId", input.projectId);
  if (input?.processArea) params.set("processArea", input.processArea);
  if (input?.periodDays) params.set("periodDays", String(input.periodDays));
  if (typeof input?.showOnlyAtRisk === "boolean") params.set("showOnlyAtRisk", String(input.showOnlyAtRisk));
  const query = params.toString();
  return apiRequest<ComplianceDashboard>(`/api/v1/governance/compliance-dashboard${query ? `?${query}` : ""}`, { signal });
};

export const getComplianceDrilldown = (
  input: { issueType: string; projectId?: string; processArea?: string },
  signal?: AbortSignal,
) => {
  const params = new URLSearchParams();
  params.set("issueType", input.issueType);
  if (input.projectId) params.set("projectId", input.projectId);
  if (input.processArea) params.set("processArea", input.processArea);
  return apiRequest<ComplianceDrilldown>(`/api/v1/governance/compliance-dashboard/drilldown?${params.toString()}`, { signal });
};

export const updateComplianceDashboardPreferences = (input: ComplianceDashboardPreferenceInput) =>
  apiRequest<ComplianceDashboardPreference>("/api/v1/governance/compliance-dashboard/preferences", { method: "PUT", body: input });

export const listManagementReviews = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<ManagementReviewListItem>>(`/api/v1/governance/management-reviews${toQuery(input)}`, { signal });
export const getManagementReview = (id: string, signal?: AbortSignal) =>
  apiRequest<ManagementReviewDetail>(`/api/v1/governance/management-reviews/${id}`, { signal });
export const createManagementReview = (input: ManagementReviewFormInput) =>
  apiRequest<ManagementReviewDetail>("/api/v1/governance/management-reviews", { method: "POST", body: input });
export const updateManagementReview = (id: string, input: ManagementReviewFormInput) =>
  apiRequest<ManagementReviewDetail>(`/api/v1/governance/management-reviews/${id}`, { method: "PUT", body: input });
export const transitionManagementReview = (id: string, input: ManagementReviewTransitionInput) =>
  apiRequest<ManagementReviewDetail>(`/api/v1/governance/management-reviews/${id}/transition`, { method: "POST", body: input });

export const listPolicies = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<PolicyListItem>>(`/api/v1/governance/policies${toQuery(input)}`, { signal });
export const createPolicy = (input: PolicyFormInput) =>
  apiRequest<PolicyListItem>("/api/v1/governance/policies", { method: "POST", body: input });
export const updatePolicy = (id: string, input: PolicyFormInput) =>
  apiRequest<PolicyListItem>(`/api/v1/governance/policies/${id}`, { method: "PUT", body: input });
export const transitionPolicy = (id: string, input: PolicyTransitionInput) =>
  apiRequest<PolicyListItem>(`/api/v1/governance/policies/${id}/transition`, { method: "POST", body: input });

export const listPolicyCampaigns = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<PolicyCampaignItem>>(`/api/v1/governance/policy-campaigns${toQuery(input)}`, { signal });
export const createPolicyCampaign = (input: PolicyCampaignFormInput) =>
  apiRequest<PolicyCampaignItem>("/api/v1/governance/policy-campaigns", { method: "POST", body: input });
export const updatePolicyCampaign = (id: string, input: PolicyCampaignFormInput) =>
  apiRequest<PolicyCampaignItem>(`/api/v1/governance/policy-campaigns/${id}`, { method: "PUT", body: input });
export const transitionPolicyCampaign = (id: string, input: PolicyCampaignTransitionInput) =>
  apiRequest<PolicyCampaignItem>(`/api/v1/governance/policy-campaigns/${id}/transition`, { method: "POST", body: input });

export const listPolicyAcknowledgements = (input?: GovernanceListInput, signal?: AbortSignal) =>
  apiRequest<GovernanceListResult<PolicyAcknowledgementItem>>(`/api/v1/governance/policy-acknowledgements${toQuery(input)}`, { signal });
export const createPolicyAcknowledgement = (input: PolicyAcknowledgementInput) =>
  apiRequest<PolicyAcknowledgementItem>("/api/v1/governance/policy-acknowledgements", { method: "POST", body: input });
