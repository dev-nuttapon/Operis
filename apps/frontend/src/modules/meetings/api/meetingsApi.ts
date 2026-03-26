import { apiRequest } from "../../../shared/lib/apiClient";
import type {
  DecisionDetail,
  DecisionFormInput,
  DecisionListInput,
  DecisionListResult,
  DecisionTransitionInput,
  DecisionUpdateInput,
  MeetingDetail,
  MeetingFormInput,
  MeetingListInput,
  MeetingListResult,
  MeetingMinutes,
  MeetingMinutesInput,
  MeetingUpdateInput,
} from "../types/meetings";

function toQuery<T extends object>(input?: T) {
  const params = new URLSearchParams();
  if (!input) {
    return "";
  }

  for (const [key, value] of Object.entries(input as Record<string, unknown>)) {
    if (value === undefined || value === null || value === "") {
      continue;
    }

    params.set(key, String(value));
  }

  const query = params.toString();
  return query ? `?${query}` : "";
}

export const listMeetings = (input?: MeetingListInput, signal?: AbortSignal) =>
  apiRequest<MeetingListResult>(`/api/v1/meetings${toQuery(input)}`, { signal });
export const getMeeting = (id: string, signal?: AbortSignal) =>
  apiRequest<MeetingDetail>(`/api/v1/meetings/${id}`, { signal });
export const createMeeting = (input: MeetingFormInput) =>
  apiRequest<MeetingDetail>("/api/v1/meetings", { method: "POST", body: input });
export const updateMeeting = (id: string, input: MeetingUpdateInput) =>
  apiRequest<MeetingDetail>(`/api/v1/meetings/${id}`, { method: "PUT", body: input });
export const approveMeeting = (id: string, input: DecisionTransitionInput) =>
  apiRequest<MeetingDetail>(`/api/v1/meetings/${id}/approve`, { method: "PUT", body: input });
export const getMeetingMinutes = (id: string, signal?: AbortSignal) =>
  apiRequest<MeetingMinutes>(`/api/v1/meetings/${id}/minutes`, { signal });
export const updateMeetingMinutes = (id: string, input: MeetingMinutesInput) =>
  apiRequest<MeetingMinutes>(`/api/v1/meetings/${id}/minutes`, { method: "PUT", body: input });

export const listDecisions = (input?: DecisionListInput, signal?: AbortSignal) =>
  apiRequest<DecisionListResult>(`/api/v1/decisions${toQuery(input)}`, { signal });
export const getDecision = (id: string, signal?: AbortSignal) =>
  apiRequest<DecisionDetail>(`/api/v1/decisions/${id}`, { signal });
export const createDecision = (input: DecisionFormInput) =>
  apiRequest<DecisionDetail>("/api/v1/decisions", { method: "POST", body: input });
export const updateDecision = (id: string, input: DecisionUpdateInput) =>
  apiRequest<DecisionDetail>(`/api/v1/decisions/${id}`, { method: "PUT", body: input });
export const approveDecision = (id: string, input: DecisionTransitionInput) =>
  apiRequest<DecisionDetail>(`/api/v1/decisions/${id}/approve`, { method: "PUT", body: input });
export const applyDecision = (id: string, input: DecisionTransitionInput) =>
  apiRequest<DecisionDetail>(`/api/v1/decisions/${id}/apply`, { method: "PUT", body: input });
