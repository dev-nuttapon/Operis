import { apiRequest } from "../../../shared/lib/apiClient";
import type {
  ApproveReleaseInput,
  DeploymentChecklistFormInput,
  DeploymentChecklistListResult,
  DeploymentChecklistUpdateInput,
  ExecuteReleaseInput,
  ReleaseCommandResponse,
  ReleaseDetail,
  ReleaseFormInput,
  ReleaseListInput,
  ReleaseListResult,
  ReleaseNoteFormInput,
  ReleaseNotesListResult,
  ReleaseUpdateInput,
} from "../types/releases";

function toQuery(input?: ReleaseListInput) {
  const params = new URLSearchParams();
  if (input?.page) params.set("page", String(input.page));
  if (input?.pageSize) params.set("pageSize", String(input.pageSize));
  if (input?.search) params.set("search", input.search);
  if (input?.status) params.set("status", input.status);
  if (input?.projectId) params.set("projectId", input.projectId);
  if (input?.releaseId) params.set("releaseId", input.releaseId);
  const query = params.toString();
  return query ? `?${query}` : "";
}

export const listReleases = (input?: ReleaseListInput, signal?: AbortSignal) =>
  apiRequest<ReleaseListResult>(`/api/v1/releases${toQuery(input)}`, { signal });
export const getRelease = (id: string, signal?: AbortSignal) =>
  apiRequest<ReleaseDetail>(`/api/v1/releases/${id}`, { signal });
export const createRelease = (input: ReleaseFormInput) =>
  apiRequest<ReleaseCommandResponse>("/api/v1/releases", { method: "POST", body: input });
export const updateRelease = (id: string, input: ReleaseUpdateInput) =>
  apiRequest<ReleaseDetail>(`/api/v1/releases/${id}`, { method: "PUT", body: input });
export const approveRelease = (id: string, input: ApproveReleaseInput) =>
  apiRequest<ReleaseDetail>(`/api/v1/releases/${id}/approve`, { method: "PUT", body: input });
export const executeRelease = (id: string, input: ExecuteReleaseInput) =>
  apiRequest<ReleaseDetail>(`/api/v1/releases/${id}/release`, { method: "PUT", body: input });

export const listDeploymentChecklists = (input?: ReleaseListInput, signal?: AbortSignal) =>
  apiRequest<DeploymentChecklistListResult>(`/api/v1/deployment-checklists${toQuery(input)}`, { signal });
export const createDeploymentChecklist = (input: DeploymentChecklistFormInput) =>
  apiRequest("/api/v1/deployment-checklists", { method: "POST", body: input });
export const updateDeploymentChecklist = (id: string, input: DeploymentChecklistUpdateInput) =>
  apiRequest(`/api/v1/deployment-checklists/${id}`, { method: "PUT", body: input });

export const listReleaseNotes = (input?: ReleaseListInput, signal?: AbortSignal) =>
  apiRequest<ReleaseNotesListResult>(`/api/v1/release-notes${toQuery(input)}`, { signal });
export const createReleaseNote = (input: ReleaseNoteFormInput) =>
  apiRequest("/api/v1/release-notes", { method: "POST", body: input });
export const publishReleaseNote = (id: string) =>
  apiRequest(`/api/v1/release-notes/${id}/publish`, { method: "PUT" });
