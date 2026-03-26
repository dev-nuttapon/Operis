import { apiRequest } from "../../../shared/lib/apiClient";
import type {
  GovernanceListInput,
  GovernanceListResult,
  GovernanceMutationResponse,
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
  Stakeholder,
  StakeholderFormInput,
  TailoringRecord,
  TailoringRecordFormInput,
  TailoringRecordListItem,
} from "../types/governance";

function toQuery(input?: GovernanceListInput) {
  const params = new URLSearchParams();
  if (input?.page) params.set("page", String(input.page));
  if (input?.pageSize) params.set("pageSize", String(input.pageSize));
  if (input?.search) params.set("search", input.search);
  if (input?.status) params.set("status", input.status);
  if (input?.ownerUserId) params.set("ownerUserId", input.ownerUserId);
  if (input?.projectId) params.set("projectId", input.projectId);
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
