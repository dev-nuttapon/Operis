import { apiRequest } from "../../../shared/lib/apiClient";
import type {
  CloseDefectInput,
  CloseNonConformanceInput,
  DefectDetail,
  DefectFormInput,
  DefectListInput,
  DefectListResult,
  DefectUpdateInput,
  NonConformanceDetail,
  NonConformanceFormInput,
  NonConformanceListInput,
  NonConformanceListResult,
  NonConformanceUpdateInput,
  ResolveDefectInput,
} from "../types/defects";

function toQuery<T extends object>(input?: T) {
  const params = new URLSearchParams();
  Object.entries(input ?? {}).forEach(([key, value]) => {
    if (value !== undefined && value !== "") params.set(key, String(value));
  });
  const query = params.toString();
  return query ? `?${query}` : "";
}

export const listDefects = (input?: DefectListInput, signal?: AbortSignal) =>
  apiRequest<DefectListResult>(`/api/v1/defects${toQuery(input)}`, { signal });
export const getDefect = (id: string, signal?: AbortSignal) =>
  apiRequest<DefectDetail>(`/api/v1/defects/${id}`, { signal });
export const createDefect = (input: DefectFormInput) =>
  apiRequest("/api/v1/defects", { method: "POST", body: input });
export const updateDefect = (id: string, input: DefectUpdateInput) =>
  apiRequest<DefectDetail>(`/api/v1/defects/${id}`, { method: "PUT", body: input });
export const resolveDefect = (id: string, input: ResolveDefectInput) =>
  apiRequest<DefectDetail>(`/api/v1/defects/${id}/resolve`, { method: "PUT", body: input });
export const closeDefect = (id: string, input: CloseDefectInput) =>
  apiRequest<DefectDetail>(`/api/v1/defects/${id}/close`, { method: "PUT", body: input });

export const listNonConformances = (input?: NonConformanceListInput, signal?: AbortSignal) =>
  apiRequest<NonConformanceListResult>(`/api/v1/non-conformances${toQuery(input)}`, { signal });
export const getNonConformance = (id: string, signal?: AbortSignal) =>
  apiRequest<NonConformanceDetail>(`/api/v1/non-conformances/${id}`, { signal });
export const createNonConformance = (input: NonConformanceFormInput) =>
  apiRequest("/api/v1/non-conformances", { method: "POST", body: input });
export const updateNonConformance = (id: string, input: NonConformanceUpdateInput) =>
  apiRequest<NonConformanceDetail>(`/api/v1/non-conformances/${id}`, { method: "PUT", body: input });
export const closeNonConformance = (id: string, input: CloseNonConformanceInput) =>
  apiRequest<NonConformanceDetail>(`/api/v1/non-conformances/${id}/close`, { method: "PUT", body: input });
