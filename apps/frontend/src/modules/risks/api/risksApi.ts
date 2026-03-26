import { apiRequest } from "../../../shared/lib/apiClient";
import type {
  IssueActionInput,
  IssueActionUpdateInput,
  IssueDetail,
  IssueFormInput,
  IssueListInput,
  IssueListResult,
  IssueResolutionInput,
  IssueUpdateInput,
  RiskDetail,
  RiskFormInput,
  RiskListInput,
  RiskListResult,
  RiskTransitionInput,
  RiskUpdateInput,
} from "../types/risks";

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

export const listRisks = (input?: RiskListInput, signal?: AbortSignal) =>
  apiRequest<RiskListResult>(`/api/v1/risks${toQuery(input)}`, { signal });
export const getRisk = (id: string, signal?: AbortSignal) =>
  apiRequest<RiskDetail>(`/api/v1/risks/${id}`, { signal });
export const createRisk = (input: RiskFormInput) =>
  apiRequest<RiskDetail>("/api/v1/risks", { method: "POST", body: input });
export const updateRisk = (id: string, input: RiskUpdateInput) =>
  apiRequest<RiskDetail>(`/api/v1/risks/${id}`, { method: "PUT", body: input });
export const assessRisk = (id: string, input: RiskTransitionInput) =>
  apiRequest<RiskDetail>(`/api/v1/risks/${id}/assess`, { method: "PUT", body: input });
export const mitigateRisk = (id: string, input: RiskTransitionInput) =>
  apiRequest<RiskDetail>(`/api/v1/risks/${id}/mitigate`, { method: "PUT", body: input });
export const closeRisk = (id: string, input: RiskTransitionInput) =>
  apiRequest<RiskDetail>(`/api/v1/risks/${id}/close`, { method: "PUT", body: input });

export const listIssues = (input?: IssueListInput, signal?: AbortSignal) =>
  apiRequest<IssueListResult>(`/api/v1/issues${toQuery(input)}`, { signal });
export const getIssue = (id: string, signal?: AbortSignal) =>
  apiRequest<IssueDetail>(`/api/v1/issues/${id}`, { signal });
export const createIssue = (input: IssueFormInput) =>
  apiRequest<IssueDetail>("/api/v1/issues", { method: "POST", body: input });
export const updateIssue = (id: string, input: IssueUpdateInput) =>
  apiRequest<IssueDetail>(`/api/v1/issues/${id}`, { method: "PUT", body: input });
export const createIssueAction = (issueId: string, input: IssueActionInput) =>
  apiRequest<IssueDetail>(`/api/v1/issues/${issueId}/actions`, { method: "POST", body: input });
export const updateIssueAction = (issueId: string, actionId: string, input: IssueActionUpdateInput) =>
  apiRequest<IssueDetail>(`/api/v1/issues/${issueId}/actions/${actionId}`, { method: "PUT", body: input });
export const resolveIssue = (id: string, input: IssueResolutionInput) =>
  apiRequest<IssueDetail>(`/api/v1/issues/${id}/resolve`, { method: "PUT", body: input });
export const closeIssue = (id: string, input: IssueResolutionInput) =>
  apiRequest<IssueDetail>(`/api/v1/issues/${id}/close`, { method: "PUT", body: input });
