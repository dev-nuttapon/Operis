import { apiRequest } from "../../../shared/lib/apiClient";
import type { PaginatedResult } from "../../../shared/types/pagination";
import type {
  AccessRecertification,
  AccessReview,
  AddAccessRecertificationDecisionInput,
  ApproveAccessReviewInput,
  ConfigurationAudit,
  CreateAccessRecertificationInput,
  CreateAccessReviewInput,
  CreateConfigurationAuditInput,
  CreateExternalDependencyInput,
  CreateSecurityReviewInput,
  CreateSupplierAgreementInput,
  CreateSupplierInput,
  ExternalDependency,
  OperationsListInput,
  SecurityReview,
  Supplier,
  SupplierAgreement,
  UpdateAccessRecertificationInput,
  UpdateAccessReviewInput,
  UpdateExternalDependencyInput,
  UpdateSupplierAgreementInput,
  UpdateSupplierInput,
  UpdateSecurityReviewInput,
} from "../types/operations";

function toQuery(input?: OperationsListInput) {
  const params = new URLSearchParams();
  if (input?.scopeType) params.set("scopeType", input.scopeType);
  if (input?.scopeRef) params.set("scopeRef", input.scopeRef);
  if (input?.reviewOwnerUserId) params.set("reviewOwnerUserId", input.reviewOwnerUserId);
  if (input?.dependencyType) params.set("dependencyType", input.dependencyType);
  if (input?.supplierId) params.set("supplierId", input.supplierId);
  if (input?.supplierType) params.set("supplierType", input.supplierType);
  if (input?.ownerUserId) params.set("ownerUserId", input.ownerUserId);
  if (input?.agreementType) params.set("agreementType", input.agreementType);
  if (input?.criticality) params.set("criticality", input.criticality);
  if (input?.status) params.set("status", input.status);
  if (input?.plannedBefore) params.set("plannedBefore", input.plannedBefore);
  if (input?.reviewDueBefore) params.set("reviewDueBefore", input.reviewDueBefore);
  if (input?.effectiveToBefore) params.set("effectiveToBefore", input.effectiveToBefore);
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

export const listAccessRecertifications = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<AccessRecertification>>(`/api/v1/access-recertifications${toQuery(input)}`, { signal });

export const getAccessRecertification = (id: string, signal?: AbortSignal) =>
  apiRequest<AccessRecertification>(`/api/v1/access-recertifications/${id}`, { signal });

export const listExternalDependencies = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<ExternalDependency>>(`/api/v1/external-dependencies${toQuery(input)}`, { signal });

export const listConfigurationAudits = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<ConfigurationAudit>>(`/api/v1/configuration-audits${toQuery(input)}`, { signal });

export const listSuppliers = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<Supplier>>(`/api/v1/suppliers${toQuery(input)}`, { signal });

export const getSupplier = (id: string, signal?: AbortSignal) =>
  apiRequest<Supplier>(`/api/v1/suppliers/${id}`, { signal });

export const listSupplierAgreements = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<SupplierAgreement>>(`/api/v1/supplier-agreements${toQuery(input)}`, { signal });

export const createAccessReview = (input: CreateAccessReviewInput) =>
  apiRequest<AccessReview>("/api/v1/access-reviews", { method: "POST", body: input });

export const updateAccessReview = (id: string, input: UpdateAccessReviewInput) =>
  apiRequest<AccessReview>(`/api/v1/access-reviews/${id}`, { method: "PUT", body: input });

export const approveAccessReview = (id: string, input: ApproveAccessReviewInput) =>
  apiRequest<AccessReview>(`/api/v1/access-reviews/${id}/approve`, { method: "PUT", body: input });

export const createAccessRecertification = (input: CreateAccessRecertificationInput) =>
  apiRequest<AccessRecertification>("/api/v1/access-recertifications", { method: "POST", body: input });

export const updateAccessRecertification = (id: string, input: UpdateAccessRecertificationInput) =>
  apiRequest<AccessRecertification>(`/api/v1/access-recertifications/${id}`, { method: "PUT", body: input });

export const addAccessRecertificationDecision = (id: string, input: AddAccessRecertificationDecisionInput) =>
  apiRequest(`/api/v1/access-recertifications/${id}/decisions`, { method: "POST", body: input });

export const completeAccessRecertification = (id: string) =>
  apiRequest<AccessRecertification>(`/api/v1/access-recertifications/${id}/complete`, { method: "PUT" });

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

export const createSupplier = (input: CreateSupplierInput) =>
  apiRequest<Supplier>("/api/v1/suppliers", { method: "POST", body: input });

export const updateSupplier = (id: string, input: UpdateSupplierInput) =>
  apiRequest<Supplier>(`/api/v1/suppliers/${id}`, { method: "PUT", body: input });

export const createSupplierAgreement = (input: CreateSupplierAgreementInput) =>
  apiRequest<SupplierAgreement>("/api/v1/supplier-agreements", { method: "POST", body: input });

export const updateSupplierAgreement = (id: string, input: UpdateSupplierAgreementInput) =>
  apiRequest<SupplierAgreement>(`/api/v1/supplier-agreements/${id}`, { method: "PUT", body: input });
