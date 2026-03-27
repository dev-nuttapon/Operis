import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  activateProcessAssetVersion,
  activateQaChecklist,
  applyTailoringRecord,
  approveProcessAssetVersion,
  approveProjectPlan,
  approveQaChecklist,
  approveTailoringRecord,
  archiveStakeholder,
  archiveTailoringRecord,
  baselineProjectPlan,
  createManagementReview,
  createPolicy,
  createPolicyAcknowledgement,
  createPolicyCampaign,
  createArchitectureRecord,
  getComplianceDashboard,
  getComplianceDrilldown,
  getManagementReview,
  createDesignReview,
  createIntegrationReview,
  createRaciMap,
  createProcessAsset,
  createProcessAssetVersion,
  createProjectPlan,
  createQaChecklist,
  createRetentionPolicy,
  createSlaRule,
  createStakeholder,
  createTailoringCriteria,
  createTailoringRecord,
  createTailoringReviewCycle,
  deprecateProcessAsset,
  deprecateQaChecklist,
  getArchitectureRecord,
  getProcessAsset,
  getProjectPlan,
  getQaChecklist,
  getTailoringReviewCycle,
  listApprovalEvidence,
  listArchitectureRecords,
  listManagementReviews,
  listPolicies,
  listPolicyAcknowledgements,
  listPolicyCampaigns,
  listDesignReviews,
  listIntegrationReviews,
  listProcessAssets,
  listProjectPlans,
  listQaChecklists,
  listRaciMaps,
  listRetentionPolicies,
  listSlaRules,
  listStakeholders,
  listTailoringCriteria,
  listTailoringRecords,
  listTailoringReviewCycles,
  listWorkflowOverrides,
  submitProcessAssetVersionReview,
  submitProjectPlanReview,
  submitTailoringRecord,
  supersedeProjectPlan,
  transitionTailoringReviewCycle,
  transitionManagementReview,
  transitionPolicy,
  transitionPolicyCampaign,
  updateComplianceDashboardPreferences,
  updateManagementReview,
  updatePolicy,
  updatePolicyCampaign,
  updateArchitectureRecord,
  updateDesignReview,
  updateIntegrationReview,
  updateRaciMap,
  updateProcessAsset,
  updateProcessAssetVersion,
  updateProjectPlan,
  updateQaChecklist,
  updateRetentionPolicy,
  updateSlaRule,
  updateStakeholder,
  updateTailoringCriteria,
  updateTailoringRecord,
  updateTailoringReviewCycle,
} from "../api/governanceApi";
import type {
  ArchitectureRecordFormInput,
  ComplianceDashboardInput,
  ComplianceDashboardPreferenceInput,
  DesignReviewFormInput,
  GovernanceListInput,
  IntegrationReviewFormInput,
  ManagementReviewFormInput,
  ManagementReviewTransitionInput,
  PolicyAcknowledgementInput,
  PolicyCampaignFormInput,
  PolicyCampaignTransitionInput,
  PolicyFormInput,
  PolicyTransitionInput,
  ProcessAssetFormInput,
  ProcessAssetVersionFormInput,
  ProjectPlanFormInput,
  QaChecklistFormInput,
  RaciMapFormInput,
  RetentionPolicyFormInput,
  SlaRuleFormInput,
  StakeholderFormInput,
  TailoringCriteriaFormInput,
  TailoringRecordFormInput,
  TailoringReviewCycleFormInput,
  TailoringReviewCycleUpdateInput,
  TailoringReviewTransitionInput,
} from "../types/governance";

export function useProcessAssets(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "process-assets", input],
    queryFn: ({ signal }) => listProcessAssets(input, signal),
    enabled,
  });
}

export function useProcessAsset(processAssetId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["governance", "process-asset", processAssetId],
    queryFn: ({ signal }) => (processAssetId ? getProcessAsset(processAssetId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(processAssetId),
  });
}

export function useQaChecklists(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "qa-checklists", input],
    queryFn: ({ signal }) => listQaChecklists(input, signal),
    enabled,
  });
}

export function useQaChecklist(qaChecklistId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["governance", "qa-checklist", qaChecklistId],
    queryFn: ({ signal }) => (qaChecklistId ? getQaChecklist(qaChecklistId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(qaChecklistId),
  });
}

export function useProjectPlans(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "project-plans", input],
    queryFn: ({ signal }) => listProjectPlans(input, signal),
    enabled,
  });
}

export function useProjectPlan(projectPlanId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["governance", "project-plan", projectPlanId],
    queryFn: ({ signal }) => (projectPlanId ? getProjectPlan(projectPlanId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(projectPlanId),
  });
}

export function useStakeholders(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "stakeholders", input],
    queryFn: ({ signal }) => listStakeholders(input, signal),
    enabled,
  });
}

export function useTailoringRecords(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "tailoring-records", input],
    queryFn: ({ signal }) => listTailoringRecords(input, signal),
    enabled,
  });
}

export function useTailoringCriteria(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "tailoring-criteria", input],
    queryFn: ({ signal }) => listTailoringCriteria(input, signal),
    enabled,
  });
}

export function useTailoringReviewCycles(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "tailoring-review-cycles", input],
    queryFn: ({ signal }) => listTailoringReviewCycles(input, signal),
    enabled,
  });
}

export function useTailoringReviewCycle(id: string | null, enabled = true) {
  return useQuery({
    queryKey: ["governance", "tailoring-review-cycle", id],
    queryFn: ({ signal }) => (id ? getTailoringReviewCycle(id, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(id),
  });
}

export function useRaciMaps(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "raci-maps", input],
    queryFn: ({ signal }) => listRaciMaps(input, signal),
    enabled,
  });
}

export function useComplianceDashboard(input?: ComplianceDashboardInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "compliance-dashboard", input],
    queryFn: ({ signal }) => getComplianceDashboard(input, signal),
    enabled,
  });
}

export function useComplianceDrilldown(input: { issueType: string; projectId?: string; processArea?: string } | null, enabled = true) {
  return useQuery({
    queryKey: ["governance", "compliance-dashboard", "drilldown", input],
    queryFn: ({ signal }) => (input ? getComplianceDrilldown(input, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(input),
  });
}

export function useManagementReviews(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "management-reviews", input],
    queryFn: ({ signal }) => listManagementReviews(input, signal),
    enabled,
  });
}

export function useManagementReview(id: string | null, enabled = true) {
  return useQuery({
    queryKey: ["governance", "management-review", id],
    queryFn: ({ signal }) => (id ? getManagementReview(id, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(id),
  });
}

export function usePolicies(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "policies", input],
    queryFn: ({ signal }) => listPolicies(input, signal),
    enabled,
  });
}

export function usePolicyCampaigns(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "policy-campaigns", input],
    queryFn: ({ signal }) => listPolicyCampaigns(input, signal),
    enabled,
  });
}

export function usePolicyAcknowledgements(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "policy-acknowledgements", input],
    queryFn: ({ signal }) => listPolicyAcknowledgements(input, signal),
    enabled,
  });
}

export function useApprovalEvidence(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "approval-evidence", input],
    queryFn: ({ signal }) => listApprovalEvidence(input, signal),
    enabled,
  });
}

export function useWorkflowOverrides(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "workflow-overrides", input],
    queryFn: ({ signal }) => listWorkflowOverrides(input, signal),
    enabled,
  });
}

export function useSlaRules(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "sla-rules", input],
    queryFn: ({ signal }) => listSlaRules(input, signal),
    enabled,
  });
}

export function useRetentionPolicies(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "retention-policies", input],
    queryFn: ({ signal }) => listRetentionPolicies(input, signal),
    enabled,
  });
}

export function useArchitectureRecords(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "architecture-records", input],
    queryFn: ({ signal }) => listArchitectureRecords(input, signal),
    enabled,
  });
}

export function useArchitectureRecord(id: string | null, enabled = true) {
  return useQuery({
    queryKey: ["governance", "architecture-record", id],
    queryFn: ({ signal }) => (id ? getArchitectureRecord(id, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(id),
  });
}

export function useDesignReviews(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "design-reviews", input],
    queryFn: ({ signal }) => listDesignReviews(input, signal),
    enabled,
  });
}

export function useIntegrationReviews(input?: GovernanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["governance", "integration-reviews", input],
    queryFn: ({ signal }) => listIntegrationReviews(input, signal),
    enabled,
  });
}

function useInvalidateGovernance() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: ["governance"] });
  };
}

export function useCreateProcessAsset() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: ProcessAssetFormInput) => createProcessAsset(input),
    onSuccess: invalidate,
  });
}

export function useCreateArchitectureRecord() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: ArchitectureRecordFormInput) => createArchitectureRecord(input),
    onSuccess: invalidate,
  });
}

export function useUpdateComplianceDashboardPreferences() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: ComplianceDashboardPreferenceInput) => updateComplianceDashboardPreferences(input),
    onSuccess: invalidate,
  });
}

export function useCreateManagementReview() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: ManagementReviewFormInput) => createManagementReview(input),
    onSuccess: invalidate,
  });
}

export function useCreatePolicy() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: PolicyFormInput) => createPolicy(input),
    onSuccess: invalidate,
  });
}

export function useUpdatePolicy() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: PolicyFormInput }) => updatePolicy(id, input),
    onSuccess: invalidate,
  });
}

export function useTransitionPolicy() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: PolicyTransitionInput }) => transitionPolicy(id, input),
    onSuccess: invalidate,
  });
}

export function useCreatePolicyCampaign() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: PolicyCampaignFormInput) => createPolicyCampaign(input),
    onSuccess: invalidate,
  });
}

export function useUpdatePolicyCampaign() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: PolicyCampaignFormInput }) => updatePolicyCampaign(id, input),
    onSuccess: invalidate,
  });
}

export function useTransitionPolicyCampaign() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: PolicyCampaignTransitionInput }) => transitionPolicyCampaign(id, input),
    onSuccess: invalidate,
  });
}

export function useCreatePolicyAcknowledgement() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: PolicyAcknowledgementInput) => createPolicyAcknowledgement(input),
    onSuccess: invalidate,
  });
}

export function useUpdateManagementReview() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: ManagementReviewFormInput }) => updateManagementReview(id, input),
    onSuccess: invalidate,
  });
}

export function useTransitionManagementReview() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: ManagementReviewTransitionInput }) => transitionManagementReview(id, input),
    onSuccess: invalidate,
  });
}

export function useUpdateArchitectureRecord() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: ArchitectureRecordFormInput }) => updateArchitectureRecord(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateDesignReview() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: DesignReviewFormInput) => createDesignReview(input),
    onSuccess: invalidate,
  });
}

export function useUpdateDesignReview() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: DesignReviewFormInput }) => updateDesignReview(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateIntegrationReview() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: IntegrationReviewFormInput) => createIntegrationReview(input),
    onSuccess: invalidate,
  });
}

export function useUpdateIntegrationReview() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: IntegrationReviewFormInput }) => updateIntegrationReview(id, input),
    onSuccess: invalidate,
  });
}

export function useUpdateProcessAsset() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: ProcessAssetFormInput }) => updateProcessAsset(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateProcessAssetVersion() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: ProcessAssetVersionFormInput }) => createProcessAssetVersion(id, input),
    onSuccess: invalidate,
  });
}

export function useUpdateProcessAssetVersion() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ processAssetId, versionId, input }: { processAssetId: string; versionId: string; input: ProcessAssetVersionFormInput }) =>
      updateProcessAssetVersion(processAssetId, versionId, input),
    onSuccess: invalidate,
  });
}

export function useProcessAssetActions() {
  const invalidate = useInvalidateGovernance();
  return {
    submitReview: useMutation({
      mutationFn: ({ processAssetId, versionId }: { processAssetId: string; versionId: string }) => submitProcessAssetVersionReview(processAssetId, versionId),
      onSuccess: invalidate,
    }),
    approve: useMutation({
      mutationFn: ({ processAssetId, versionId, changeSummary }: { processAssetId: string; versionId: string; changeSummary: string }) =>
        approveProcessAssetVersion(processAssetId, versionId, changeSummary),
      onSuccess: invalidate,
    }),
    activate: useMutation({
      mutationFn: ({ processAssetId, versionId }: { processAssetId: string; versionId: string }) => activateProcessAssetVersion(processAssetId, versionId),
      onSuccess: invalidate,
    }),
    deprecate: useMutation({
      mutationFn: (processAssetId: string) => deprecateProcessAsset(processAssetId),
      onSuccess: invalidate,
    }),
  };
}

export function useCreateQaChecklist() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: QaChecklistFormInput) => createQaChecklist(input),
    onSuccess: invalidate,
  });
}

export function useUpdateQaChecklist() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: QaChecklistFormInput }) => updateQaChecklist(id, input),
    onSuccess: invalidate,
  });
}

export function useQaChecklistActions() {
  const invalidate = useInvalidateGovernance();
  return {
    approve: useMutation({ mutationFn: (id: string) => approveQaChecklist(id), onSuccess: invalidate }),
    activate: useMutation({ mutationFn: (id: string) => activateQaChecklist(id), onSuccess: invalidate }),
    deprecate: useMutation({ mutationFn: (id: string) => deprecateQaChecklist(id), onSuccess: invalidate }),
  };
}

export function useCreateProjectPlan() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: ProjectPlanFormInput) => createProjectPlan(input),
    onSuccess: invalidate,
  });
}

export function useUpdateProjectPlan() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: ProjectPlanFormInput }) => updateProjectPlan(id, input),
    onSuccess: invalidate,
  });
}

export function useProjectPlanActions() {
  const invalidate = useInvalidateGovernance();
  return {
    submitReview: useMutation({ mutationFn: (id: string) => submitProjectPlanReview(id), onSuccess: invalidate }),
    approve: useMutation({ mutationFn: ({ id, reason }: { id: string; reason: string }) => approveProjectPlan(id, reason), onSuccess: invalidate }),
    baseline: useMutation({ mutationFn: (id: string) => baselineProjectPlan(id), onSuccess: invalidate }),
    supersede: useMutation({ mutationFn: ({ id, reason }: { id: string; reason: string }) => supersedeProjectPlan(id, reason), onSuccess: invalidate }),
  };
}

export function useCreateStakeholder() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: StakeholderFormInput) => createStakeholder(input),
    onSuccess: invalidate,
  });
}

export function useUpdateStakeholder() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: StakeholderFormInput }) => updateStakeholder(id, input),
    onSuccess: invalidate,
  });
}

export function useArchiveStakeholder() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (id: string) => archiveStakeholder(id),
    onSuccess: invalidate,
  });
}

export function useCreateRaciMap() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: RaciMapFormInput) => createRaciMap(input),
    onSuccess: invalidate,
  });
}

export function useUpdateRaciMap() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: RaciMapFormInput }) => updateRaciMap(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateSlaRule() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: SlaRuleFormInput) => createSlaRule(input),
    onSuccess: invalidate,
  });
}

export function useUpdateSlaRule() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: SlaRuleFormInput }) => updateSlaRule(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateRetentionPolicy() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: RetentionPolicyFormInput) => createRetentionPolicy(input),
    onSuccess: invalidate,
  });
}

export function useUpdateRetentionPolicy() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: RetentionPolicyFormInput }) => updateRetentionPolicy(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateTailoringRecord() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: TailoringRecordFormInput) => createTailoringRecord(input),
    onSuccess: invalidate,
  });
}

export function useUpdateTailoringRecord() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: TailoringRecordFormInput }) => updateTailoringRecord(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateTailoringCriteria() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: TailoringCriteriaFormInput) => createTailoringCriteria(input),
    onSuccess: invalidate,
  });
}

export function useUpdateTailoringCriteria() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: TailoringCriteriaFormInput }) => updateTailoringCriteria(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateTailoringReviewCycle() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: (input: TailoringReviewCycleFormInput) => createTailoringReviewCycle(input),
    onSuccess: invalidate,
  });
}

export function useUpdateTailoringReviewCycle() {
  const invalidate = useInvalidateGovernance();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: TailoringReviewCycleUpdateInput }) => updateTailoringReviewCycle(id, input),
    onSuccess: invalidate,
  });
}

export function useTailoringReviewCycleActions() {
  const invalidate = useInvalidateGovernance();
  return {
    transition: useMutation({
      mutationFn: ({ id, input }: { id: string; input: TailoringReviewTransitionInput }) => transitionTailoringReviewCycle(id, input),
      onSuccess: invalidate,
    }),
  };
}

export function useTailoringActions() {
  const invalidate = useInvalidateGovernance();
  return {
    submit: useMutation({ mutationFn: (id: string) => submitTailoringRecord(id), onSuccess: invalidate }),
    approve: useMutation({
      mutationFn: ({ id, decision, reason }: { id: string; decision: "approved" | "rejected"; reason: string }) =>
        approveTailoringRecord(id, decision, reason),
      onSuccess: invalidate,
    }),
    apply: useMutation({ mutationFn: (id: string) => applyTailoringRecord(id), onSuccess: invalidate }),
    archive: useMutation({ mutationFn: (id: string) => archiveTailoringRecord(id), onSuccess: invalidate }),
  };
}
