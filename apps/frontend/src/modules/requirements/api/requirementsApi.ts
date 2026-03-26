import { apiRequest } from "../../../shared/lib/apiClient";
import type {
  RequirementBaselineInput,
  RequirementBaselineItem,
  RequirementBaselineListResult,
  RequirementDetail,
  RequirementFormInput,
  RequirementListInput,
  RequirementListResult,
  RequirementUpdateInput,
  TraceabilityLinkInput,
  TraceabilityLinkItem,
  TraceabilityListInput,
  TraceabilityMatrixResult,
} from "../types/requirements";

function appendPaging(params: URLSearchParams, page?: number, pageSize?: number) {
  if (page) params.set("page", String(page));
  if (pageSize) params.set("pageSize", String(pageSize));
}

function toRequirementQuery(input?: RequirementListInput) {
  const params = new URLSearchParams();
  appendPaging(params, input?.page, input?.pageSize);
  if (input?.search) params.set("search", input.search);
  if (input?.projectId) params.set("projectId", input.projectId);
  if (input?.priority) params.set("priority", input.priority);
  if (input?.status) params.set("status", input.status);
  if (input?.ownerUserId) params.set("ownerUserId", input.ownerUserId);
  if (input?.baselineStatus) params.set("baselineStatus", input.baselineStatus);
  if (typeof input?.missingDownstreamLinks === "boolean") {
    params.set("missingDownstreamLinks", String(input.missingDownstreamLinks));
  }
  const query = params.toString();
  return query ? `?${query}` : "";
}

function toBaselineQuery(projectId?: string, status?: string, page = 1, pageSize = 10) {
  const params = new URLSearchParams();
  appendPaging(params, page, pageSize);
  if (projectId) params.set("projectId", projectId);
  if (status) params.set("status", status);
  const query = params.toString();
  return query ? `?${query}` : "";
}

function toTraceabilityQuery(input?: TraceabilityListInput) {
  const params = new URLSearchParams();
  appendPaging(params, input?.page, input?.pageSize);
  if (input?.projectId) params.set("projectId", input.projectId);
  if (input?.baselineStatus) params.set("baselineStatus", input.baselineStatus);
  if (typeof input?.missingCoverage === "boolean") {
    params.set("missingCoverage", String(input.missingCoverage));
  }
  const query = params.toString();
  return query ? `?${query}` : "";
}

export const listRequirements = (input?: RequirementListInput, signal?: AbortSignal) =>
  apiRequest<RequirementListResult>(`/api/v1/requirements/${toRequirementQuery(input)}`, { signal });

export const getRequirement = (id: string, signal?: AbortSignal) =>
  apiRequest<RequirementDetail>(`/api/v1/requirements/${id}`, { signal });

export const createRequirement = (input: RequirementFormInput) =>
  apiRequest<RequirementDetail>("/api/v1/requirements/", { method: "POST", body: input });

export const updateRequirement = (id: string, input: RequirementUpdateInput) =>
  apiRequest<RequirementDetail>(`/api/v1/requirements/${id}`, { method: "PUT", body: input });

export const submitRequirement = (id: string) =>
  apiRequest<RequirementDetail>(`/api/v1/requirements/${id}/submit`, { method: "PUT" });

export const approveRequirement = (id: string, reason: string) =>
  apiRequest<RequirementDetail>(`/api/v1/requirements/${id}/approve`, { method: "PUT", body: { reason } });

export const baselineRequirement = (id: string) =>
  apiRequest<RequirementDetail>(`/api/v1/requirements/${id}/baseline`, { method: "PUT" });

export const supersedeRequirement = (id: string, reason: string) =>
  apiRequest<RequirementDetail>(`/api/v1/requirements/${id}/supersede`, { method: "PUT", body: { reason } });

export const listRequirementBaselines = (projectId?: string, status?: string, page = 1, pageSize = 10, signal?: AbortSignal) =>
  apiRequest<RequirementBaselineListResult>(`/api/v1/requirements/baselines${toBaselineQuery(projectId, status, page, pageSize)}`, { signal });

export const createRequirementBaseline = (input: RequirementBaselineInput) =>
  apiRequest<RequirementBaselineItem>("/api/v1/requirements/baselines", { method: "POST", body: input });

export const listTraceabilityMatrix = (input?: TraceabilityListInput, signal?: AbortSignal) =>
  apiRequest<TraceabilityMatrixResult>(`/api/v1/requirements/traceability${toTraceabilityQuery(input)}`, { signal });

export const createTraceabilityLink = (input: TraceabilityLinkInput) =>
  apiRequest<TraceabilityLinkItem>("/api/v1/requirements/traceability-links", { method: "POST", body: input });

export const deleteTraceabilityLink = (linkId: string) =>
  apiRequest<void>(`/api/v1/requirements/traceability-links/${linkId}`, { method: "DELETE" });
