import { apiRequest } from "../../../shared/lib/apiClient";
import type { PaginatedResult } from "../../../shared/types/pagination";
import type {
  AccessRecertification,
  AccessReview,
  AddAccessRecertificationDecisionInput,
  ApproveAccessReviewInput,
  BackupEvidence,
  ClassificationPolicy,
  ConfigurationAudit,
  CreateBackupEvidenceInput,
  CreateAccessRecertificationInput,
  CreateAccessReviewInput,
  CreateClassificationPolicyInput,
  CreateConfigurationAuditInput,
  CreateDrDrillInput,
  CreateExternalDependencyInput,
  CreateLegalHoldInput,
  CreatePrivilegedAccessEventInput,
  CreateRestoreVerificationInput,
  CreateSecretRotationInput,
  CreateSecurityIncidentInput,
  CreateSecurityReviewInput,
  CreateSupplierAgreementInput,
  CreateSupplierInput,
  CreateVulnerabilityInput,
  DrDrill,
  ExternalDependency,
  LegalHold,
  OperationsListInput,
  PrivilegedAccessEvent,
  ReleaseLegalHoldInput,
  RestoreVerification,
  SecretRotation,
  SecurityIncident,
  SecurityReview,
  Supplier,
  SupplierAgreement,
  UpdateDrDrillInput,
  UpdateClassificationPolicyInput,
  UpdateAccessRecertificationInput,
  UpdateAccessReviewInput,
  UpdateExternalDependencyInput,
  UpdatePrivilegedAccessEventInput,
  UpdateSecretRotationInput,
  UpdateSecurityIncidentInput,
  UpdateSupplierAgreementInput,
  UpdateSupplierInput,
  UpdateSecurityReviewInput,
  UpdateVulnerabilityInput,
  VulnerabilityRecord,
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
  if (input?.severity) params.set("severity", input.severity);
  if (input?.status) params.set("status", input.status);
  if (input?.plannedBefore) params.set("plannedBefore", input.plannedBefore);
  if (input?.reviewDueBefore) params.set("reviewDueBefore", input.reviewDueBefore);
  if (input?.effectiveToBefore) params.set("effectiveToBefore", input.effectiveToBefore);
  if (input?.verifiedBy) params.set("verifiedBy", input.verifiedBy);
  if (input?.requestedBy) params.set("requestedBy", input.requestedBy);
  if (input?.approvedBy) params.set("approvedBy", input.approvedBy);
  if (input?.usedBy) params.set("usedBy", input.usedBy);
  if (input?.classificationLevel) params.set("classificationLevel", input.classificationLevel);
  if (input?.backupScope) params.set("backupScope", input.backupScope);
  if (input?.backupEvidenceId) params.set("backupEvidenceId", input.backupEvidenceId);
  if (input?.scope) params.set("scope", input.scope);
  if (input?.projectId) params.set("projectId", input.projectId);
  if (input?.executedAfter) params.set("executedAfter", input.executedAfter);
  if (input?.plannedAfter) params.set("plannedAfter", input.plannedAfter);
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

export const listSecurityIncidents = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<SecurityIncident>>(`/api/v1/security-incidents${toQuery(input)}`, { signal });

export const getSecurityIncident = (id: string, signal?: AbortSignal) =>
  apiRequest<SecurityIncident>(`/api/v1/security-incidents/${id}`, { signal });

export const listVulnerabilities = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<VulnerabilityRecord>>(`/api/v1/vulnerabilities${toQuery(input)}`, { signal });

export const listSecretRotations = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<SecretRotation>>(`/api/v1/secret-rotations${toQuery(input)}`, { signal });

export const listPrivilegedAccessEvents = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<PrivilegedAccessEvent>>(`/api/v1/privileged-access-events${toQuery(input)}`, { signal });

export const listClassificationPolicies = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<ClassificationPolicy>>(`/api/v1/classification-policies${toQuery(input)}`, { signal });

export const listExternalDependencies = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<ExternalDependency>>(`/api/v1/external-dependencies${toQuery(input)}`, { signal });

export const listConfigurationAudits = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<ConfigurationAudit>>(`/api/v1/configuration-audits${toQuery(input)}`, { signal });

export const listBackupEvidence = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<BackupEvidence>>(`/api/v1/backup-evidence${toQuery(input)}`, { signal });

export const listRestoreVerifications = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<RestoreVerification>>(`/api/v1/restore-verifications${toQuery(input)}`, { signal });

export const listDrDrills = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<DrDrill>>(`/api/v1/dr-drills${toQuery(input)}`, { signal });

export const listLegalHolds = (input?: OperationsListInput, signal?: AbortSignal) =>
  apiRequest<PaginatedResult<LegalHold>>(`/api/v1/legal-holds${toQuery(input)}`, { signal });

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

export const createSecurityIncident = (input: CreateSecurityIncidentInput) =>
  apiRequest<SecurityIncident>("/api/v1/security-incidents", { method: "POST", body: input });

export const updateSecurityIncident = (id: string, input: UpdateSecurityIncidentInput) =>
  apiRequest<SecurityIncident>(`/api/v1/security-incidents/${id}`, { method: "PUT", body: input });

export const createVulnerability = (input: CreateVulnerabilityInput) =>
  apiRequest<VulnerabilityRecord>("/api/v1/vulnerabilities", { method: "POST", body: input });

export const updateVulnerability = (id: string, input: UpdateVulnerabilityInput) =>
  apiRequest<VulnerabilityRecord>(`/api/v1/vulnerabilities/${id}`, { method: "PUT", body: input });

export const createSecretRotation = (input: CreateSecretRotationInput) =>
  apiRequest<SecretRotation>("/api/v1/secret-rotations", { method: "POST", body: input });

export const updateSecretRotation = (id: string, input: UpdateSecretRotationInput) =>
  apiRequest<SecretRotation>(`/api/v1/secret-rotations/${id}`, { method: "PUT", body: input });

export const createPrivilegedAccessEvent = (input: CreatePrivilegedAccessEventInput) =>
  apiRequest<PrivilegedAccessEvent>("/api/v1/privileged-access-events", { method: "POST", body: input });

export const updatePrivilegedAccessEvent = (id: string, input: UpdatePrivilegedAccessEventInput) =>
  apiRequest<PrivilegedAccessEvent>(`/api/v1/privileged-access-events/${id}`, { method: "PUT", body: input });

export const createClassificationPolicy = (input: CreateClassificationPolicyInput) =>
  apiRequest<ClassificationPolicy>("/api/v1/classification-policies", { method: "POST", body: input });

export const updateClassificationPolicy = (id: string, input: UpdateClassificationPolicyInput) =>
  apiRequest<ClassificationPolicy>(`/api/v1/classification-policies/${id}`, { method: "PUT", body: input });

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

export const createBackupEvidence = (input: CreateBackupEvidenceInput) =>
  apiRequest<BackupEvidence>("/api/v1/backup-evidence", { method: "POST", body: input });

export const createRestoreVerification = (input: CreateRestoreVerificationInput) =>
  apiRequest<RestoreVerification>("/api/v1/restore-verifications", { method: "POST", body: input });

export const createDrDrill = (input: CreateDrDrillInput) =>
  apiRequest<DrDrill>("/api/v1/dr-drills", { method: "POST", body: input });

export const updateDrDrill = (id: string, input: UpdateDrDrillInput) =>
  apiRequest<DrDrill>(`/api/v1/dr-drills/${id}`, { method: "PUT", body: input });

export const createLegalHold = (input: CreateLegalHoldInput) =>
  apiRequest<LegalHold>("/api/v1/legal-holds", { method: "POST", body: input });

export const releaseLegalHold = (id: string, input: ReleaseLegalHoldInput) =>
  apiRequest<LegalHold>(`/api/v1/legal-holds/${id}/release`, { method: "PUT", body: input });

export const createSupplier = (input: CreateSupplierInput) =>
  apiRequest<Supplier>("/api/v1/suppliers", { method: "POST", body: input });

export const updateSupplier = (id: string, input: UpdateSupplierInput) =>
  apiRequest<Supplier>(`/api/v1/suppliers/${id}`, { method: "PUT", body: input });

export const createSupplierAgreement = (input: CreateSupplierAgreementInput) =>
  apiRequest<SupplierAgreement>("/api/v1/supplier-agreements", { method: "POST", body: input });

export const updateSupplierAgreement = (id: string, input: UpdateSupplierAgreementInput) =>
  apiRequest<SupplierAgreement>(`/api/v1/supplier-agreements/${id}`, { method: "PUT", body: input });
