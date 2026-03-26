import { apiRequest } from "../../../shared/lib/apiClient";
import type {
  BaselineOverrideInput,
  BaselineRegistryDetail,
  BaselineRegistryFormInput,
  BaselineRegistryListResult,
  ChangeControlListInput,
  ChangeRequestDetail,
  ChangeRequestFormInput,
  ChangeRequestListResult,
  ChangeRequestUpdateInput,
  ConfigurationItem,
  ConfigurationItemFormInput,
  ConfigurationItemListResult,
  ConfigurationItemUpdateInput,
} from "../types/changeControl";

function toQuery(input?: ChangeControlListInput) {
  const params = new URLSearchParams();
  if (input?.page) params.set("page", String(input.page));
  if (input?.pageSize) params.set("pageSize", String(input.pageSize));
  if (input?.search) params.set("search", input.search);
  if (input?.status) params.set("status", input.status);
  if (input?.priority) params.set("priority", input.priority);
  if (input?.projectId) params.set("projectId", input.projectId);
  const query = params.toString();
  return query ? `?${query}` : "";
}

export const listChangeRequests = (input?: ChangeControlListInput, signal?: AbortSignal) =>
  apiRequest<ChangeRequestListResult>(`/api/v1/change-control/change-requests${toQuery(input)}`, { signal });
export const getChangeRequest = (id: string, signal?: AbortSignal) =>
  apiRequest<ChangeRequestDetail>(`/api/v1/change-control/change-requests/${id}`, { signal });
export const createChangeRequest = (input: ChangeRequestFormInput) =>
  apiRequest<ChangeRequestDetail>("/api/v1/change-control/change-requests", { method: "POST", body: input });
export const updateChangeRequest = (id: string, input: ChangeRequestUpdateInput) =>
  apiRequest<ChangeRequestDetail>(`/api/v1/change-control/change-requests/${id}`, { method: "PUT", body: input });
export const submitChangeRequest = (id: string) =>
  apiRequest<ChangeRequestDetail>(`/api/v1/change-control/change-requests/${id}/submit`, { method: "PUT" });
export const approveChangeRequest = (id: string, reason: string) =>
  apiRequest<ChangeRequestDetail>(`/api/v1/change-control/change-requests/${id}/approve`, { method: "PUT", body: { reason } });
export const rejectChangeRequest = (id: string, reason: string) =>
  apiRequest<ChangeRequestDetail>(`/api/v1/change-control/change-requests/${id}/reject`, { method: "PUT", body: { reason } });
export const implementChangeRequest = (id: string, summary: string) =>
  apiRequest<ChangeRequestDetail>(`/api/v1/change-control/change-requests/${id}/implement`, { method: "PUT", body: { summary } });
export const closeChangeRequest = (id: string, summary: string) =>
  apiRequest<ChangeRequestDetail>(`/api/v1/change-control/change-requests/${id}/close`, { method: "PUT", body: { summary } });

export const listConfigurationItems = (input?: ChangeControlListInput, signal?: AbortSignal) =>
  apiRequest<ConfigurationItemListResult>(`/api/v1/change-control/configuration-items${toQuery(input)}`, { signal });
export const getConfigurationItem = (id: string, signal?: AbortSignal) =>
  apiRequest<ConfigurationItem>(`/api/v1/change-control/configuration-items/${id}`, { signal });
export const createConfigurationItem = (input: ConfigurationItemFormInput) =>
  apiRequest<ConfigurationItem>("/api/v1/change-control/configuration-items", { method: "POST", body: input });
export const updateConfigurationItem = (id: string, input: ConfigurationItemUpdateInput) =>
  apiRequest<ConfigurationItem>(`/api/v1/change-control/configuration-items/${id}`, { method: "PUT", body: input });
export const approveConfigurationItem = (id: string) =>
  apiRequest<ConfigurationItem>(`/api/v1/change-control/configuration-items/${id}/approve`, { method: "PUT" });

export const listBaselineRegistry = (input?: ChangeControlListInput, signal?: AbortSignal) =>
  apiRequest<BaselineRegistryListResult>(`/api/v1/change-control/baseline-registry${toQuery(input)}`, { signal });
export const getBaselineRegistry = (id: string, signal?: AbortSignal) =>
  apiRequest<BaselineRegistryDetail>(`/api/v1/change-control/baseline-registry/${id}`, { signal });
export const createBaselineRegistry = (input: BaselineRegistryFormInput) =>
  apiRequest<BaselineRegistryDetail>("/api/v1/change-control/baseline-registry", { method: "POST", body: input });
export const approveBaselineRegistry = (id: string) =>
  apiRequest<BaselineRegistryDetail>(`/api/v1/change-control/baseline-registry/${id}/approve`, { method: "PUT" });
export const supersedeBaselineRegistry = (id: string, input: BaselineOverrideInput) =>
  apiRequest<BaselineRegistryDetail>(`/api/v1/change-control/baseline-registry/${id}/supersede`, { method: "PUT", body: input });
