import { apiRequest } from "../../../shared/lib/apiClient";
import type { PaginatedResult } from "../../../shared/types/pagination";
import type { CreateWaiverInput, TransitionWaiverInput, UpdateWaiverInput, WaiverDetail, WaiverListInput, WaiverListItem } from "../types/exceptions";

function toQuery(input: Record<string, string | number | boolean | undefined | null>) {
  const params = new URLSearchParams();
  Object.entries(input).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      params.set(key, String(value));
    }
  });

  const query = params.toString();
  return query ? `?${query}` : "";
}

export function listWaivers(input: WaiverListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<WaiverListItem>>(`/api/v1/exceptions/waivers${toQuery(input as Record<string, string | number | boolean | undefined | null>)}`, { signal });
}

export function getWaiver(waiverId: string, signal?: AbortSignal) {
  return apiRequest<WaiverDetail>(`/api/v1/exceptions/waivers/${waiverId}`, { signal });
}

export function createWaiver(input: CreateWaiverInput) {
  return apiRequest<WaiverDetail>("/api/v1/exceptions/waivers", { method: "POST", body: input });
}

export function updateWaiver(waiverId: string, input: UpdateWaiverInput) {
  return apiRequest<WaiverDetail>(`/api/v1/exceptions/waivers/${waiverId}`, { method: "PUT", body: input });
}

export function transitionWaiver(waiverId: string, input: TransitionWaiverInput) {
  return apiRequest<WaiverDetail>(`/api/v1/exceptions/waivers/${waiverId}/transition`, { method: "POST", body: input });
}
