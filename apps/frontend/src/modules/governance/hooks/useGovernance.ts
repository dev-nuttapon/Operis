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
  createProcessAsset,
  createProcessAssetVersion,
  createProjectPlan,
  createQaChecklist,
  createStakeholder,
  createTailoringRecord,
  deprecateProcessAsset,
  deprecateQaChecklist,
  getProcessAsset,
  getProjectPlan,
  getQaChecklist,
  listProcessAssets,
  listProjectPlans,
  listQaChecklists,
  listStakeholders,
  listTailoringRecords,
  submitProcessAssetVersionReview,
  submitProjectPlanReview,
  submitTailoringRecord,
  supersedeProjectPlan,
  updateProcessAsset,
  updateProcessAssetVersion,
  updateProjectPlan,
  updateQaChecklist,
  updateStakeholder,
  updateTailoringRecord,
} from "../api/governanceApi";
import type {
  GovernanceListInput,
  ProcessAssetFormInput,
  ProcessAssetVersionFormInput,
  ProjectPlanFormInput,
  QaChecklistFormInput,
  StakeholderFormInput,
  TailoringRecordFormInput,
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
