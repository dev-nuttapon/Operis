import { apiRequest } from "../../../shared/lib/apiClient";
import type { PaginatedResult } from "../../../shared/types/pagination";
import type {
  AccessReview,
  ApproveAccessReviewInput,
  ConfigurationAudit,
  CreateAccessReviewInput,
  CreateConfigurationAuditInput,
  CreateExternalDependencyInput,
  CreateSecurityReviewInput,
  ExternalDependency,
  OperationsListInput,
  SecurityReview,
  UpdateAccessReviewInput,
  UpdateExternalDependencyInput,
  UpdateSecurityReviewInput,
} from "../types/operations";

function toQuery(input?: OperationsListInput) {
  const params = new URLSearchParams();
  if (input?.scopeType) params.set("scopeType", input.scopeType);
  if (input?.scopeRef) params.set("scopeRef", input.scopeRef);
  if (input?.dependencyType) params.set("dependencyType", input.dependencyType);
  if (input?.criticality) params.set("criticality", input.criticality);
  if (input?.status) params.set("status", input.status);
  if (input?.search) params.set("search", input.search);
  if (input?.sortBy) params.set("sortBy", input.sortBy);
  if (input?.sortOrder) params.set("sortOrder", input.sortOrder);
  if (input?.page) params.set("page", String(input.page));
  if (input?.pageSize) params.set("pageSize", String(input.pageSize));
  const query = params.toString();
  return query ? `?${query}` : "";
}

export const listAccessReviews = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<AccessReview>>(`/api/v1/access-reviews${toQuery(input)}`, { signal });

export const listSecurityReviews = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<SecurityReview>>(`/api/v1/security-reviews${toQuery(input)}`, { signal });

export const listExternalDependencies = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<ExternalDependency>>(`/api/v1/external-dependencies${toQuery(input)}`, { signal });

export const listConfigurationAudits = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<ConfigurationAudit>>(`/api/v1/configuration-audits${toQuery(input)}`, { signal });

export const createAccessReview = (input: CreateAccessReviewInput) =>
  apiRequest<AccessReview>("/api/v1/access-reviews", { method: "POST", body: input });

export const updateAccessReview = (id: string, input: UpdateAccessReviewInput) =>
  apiRequest<AccessReview>(`/api/v1/access-reviews/${id}`, { method: "PUT", body: input });

export const approveAccessReview = (id: string, input: ApproveAccessReviewInput) =>
  apiRequest<AccessReview>(`/api/v1/access-reviews/${id}/approve`, { method: "PUT", body: input });

export const createSecurityReview = (input: CreateSecurityReviewInput) =>
  apiRequest<SecurityReview>("/api/v1/security-reviews", { method: "POST", body: input });

export const updateSecurityReview = (id: string, input: UpdateSecurityReviewInput) =>
  apiRequest<SecurityReview>(`/api/v1/security-reviews/${id}`, { method: "PUT", body: input });

export const createExternalDependency = (input: CreateExternalDependencyInput) =>
  apiRequest<ExternalDependency>("/api/v1/external-dependencies", { method: "POST", body: input });

export const updateExternalDependency = (id: string, input: UpdateExternalDependencyInput) =>
  apiRequest<ExternalDependency>(`/api/v1/external-dependencies/${id}`, { method: "PUT", body: input });

export const createConfigurationAudit = (input: CreateConfigurationAuditInput) =>
  apiRequest<ConfigurationAudit>("/api/v1/configuration-audits", { method: "POST", body: input });
