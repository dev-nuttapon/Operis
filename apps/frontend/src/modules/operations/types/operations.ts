export interface AccessReview {
  id: string;
  scopeType: string;
  scopeRef: string;
  reviewCycle: string;
  reviewedBy: string | null;
  status: string;
  decision: string | null;
  decisionRationale: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface SecurityReview {
  id: string;
  scopeType: string;
  scopeRef: string;
  controlsReviewed: string;
  findingsSummary: string | null;
  status: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface ExternalDependency {
  id: string;
  name: string;
  dependencyType: string;
  supplierId: string | null;
  supplierName: string | null;
  ownerUserId: string;
  criticality: string;
  status: string;
  reviewDueAt: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface Supplier {
  id: string;
  name: string;
  supplierType: string;
  ownerUserId: string;
  criticality: string;
  status: string;
  reviewDueAt: string | null;
  activeAgreementCount: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface SupplierAgreement {
  id: string;
  supplierId: string;
  supplierName: string;
  agreementType: string;
  effectiveFrom: string;
  effectiveTo: string | null;
  slaTerms: string;
  evidenceRef: string;
  status: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface ConfigurationAudit {
  id: string;
  scopeRef: string;
  plannedAt: string;
  status: string;
  findingCount: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface AccessRecertificationDecision {
  id: string;
  scheduleId: string;
  subjectUserId: string;
  decision: string;
  reason: string;
  decidedBy: string;
  decidedAt: string;
}

export interface AccessRecertification {
  id: string;
  scopeType: string;
  scopeRef: string;
  plannedAt: string;
  reviewOwnerUserId: string;
  status: string;
  subjectUserIds: string[];
  decisions: AccessRecertificationDecision[];
  exceptionNotes: string | null;
  completedCount: number;
  pendingCount: number;
  createdAt: string;
  updatedAt: string | null;
  completedAt: string | null;
}

export interface SecurityIncident {
  id: string;
  projectId: string | null;
  projectName: string | null;
  code: string;
  title: string;
  severity: string;
  reportedAt: string;
  ownerUserId: string;
  status: string;
  resolutionSummary: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface VulnerabilityRecord {
  id: string;
  assetRef: string;
  title: string;
  severity: string;
  identifiedAt: string;
  patchDueAt: string | null;
  ownerUserId: string;
  status: string;
  verificationSummary: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface SecretRotation {
  id: string;
  secretScope: string;
  plannedAt: string;
  rotatedAt: string | null;
  verifiedBy: string | null;
  verifiedAt: string | null;
  status: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface PrivilegedAccessEvent {
  id: string;
  requestedBy: string;
  approvedBy: string | null;
  usedBy: string | null;
  requestedAt: string;
  approvedAt: string | null;
  usedAt: string | null;
  reviewedAt: string | null;
  status: string;
  reason: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface ClassificationPolicy {
  id: string;
  policyCode: string;
  classificationLevel: string;
  scope: string;
  status: string;
  handlingRule: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface OperationsListInput {
  scopeType?: string;
  scopeRef?: string;
  reviewOwnerUserId?: string;
  dependencyType?: string;
  supplierId?: string;
  supplierType?: string;
  ownerUserId?: string;
  agreementType?: string;
  criticality?: string;
  severity?: string;
  status?: string;
  plannedBefore?: string;
  reviewDueBefore?: string;
  effectiveToBefore?: string;
  verifiedBy?: string;
  requestedBy?: string;
  approvedBy?: string;
  usedBy?: string;
  classificationLevel?: string;
  scope?: string;
  projectId?: string;
  search?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
  page?: number;
  pageSize?: number;
}

export interface CreateAccessReviewInput {
  scopeType: string;
  scopeRef: string;
  reviewCycle: string;
  reviewedBy?: string | null;
}

export interface UpdateAccessReviewInput extends CreateAccessReviewInput {
  status: string;
}

export interface ApproveAccessReviewInput {
  decision: string;
  decisionRationale: string;
}

export interface CreateSecurityReviewInput {
  scopeType: string;
  scopeRef: string;
  controlsReviewed: string;
  findingsSummary?: string | null;
  status: string;
}

export interface UpdateSecurityReviewInput extends CreateSecurityReviewInput {}

export interface CreateAccessRecertificationInput {
  scopeType: string;
  scopeRef: string;
  plannedAt: string;
  reviewOwnerUserId: string;
  subjectUserIds?: string[] | null;
  exceptionNotes?: string | null;
}

export interface UpdateAccessRecertificationInput extends CreateAccessRecertificationInput {
  status: string;
}

export interface AddAccessRecertificationDecisionInput {
  subjectUserId: string;
  decision: string;
  reason?: string | null;
}

export interface CreateExternalDependencyInput {
  name: string;
  dependencyType: string;
  supplierId?: string | null;
  ownerUserId: string;
  criticality: string;
  reviewDueAt?: string | null;
  status: string;
}

export interface UpdateExternalDependencyInput extends CreateExternalDependencyInput {}

export interface CreateConfigurationAuditInput {
  scopeRef: string;
  plannedAt: string;
  status: string;
  findingCount: number;
}

export interface CreateSupplierInput {
  name: string;
  supplierType: string;
  ownerUserId: string;
  criticality: string;
  reviewDueAt?: string | null;
  status: string;
}

export interface UpdateSupplierInput extends CreateSupplierInput {}

export interface CreateSupplierAgreementInput {
  supplierId: string;
  agreementType: string;
  effectiveFrom: string;
  effectiveTo?: string | null;
  slaTerms?: string | null;
  evidenceRef: string;
  status: string;
}

export interface UpdateSupplierAgreementInput extends CreateSupplierAgreementInput {}

export interface CreateSecurityIncidentInput {
  projectId?: string | null;
  code: string;
  title: string;
  severity: string;
  reportedAt: string;
  ownerUserId: string;
  status: string;
  resolutionSummary?: string | null;
}

export interface UpdateSecurityIncidentInput extends CreateSecurityIncidentInput {}

export interface CreateVulnerabilityInput {
  assetRef: string;
  title: string;
  severity: string;
  identifiedAt: string;
  patchDueAt?: string | null;
  ownerUserId: string;
  status: string;
  verificationSummary?: string | null;
}

export interface UpdateVulnerabilityInput extends CreateVulnerabilityInput {}

export interface CreateSecretRotationInput {
  secretScope: string;
  plannedAt: string;
  rotatedAt?: string | null;
  verifiedBy?: string | null;
  verifiedAt?: string | null;
  status: string;
}

export interface UpdateSecretRotationInput extends CreateSecretRotationInput {}

export interface CreatePrivilegedAccessEventInput {
  requestedBy: string;
  approvedBy?: string | null;
  usedBy?: string | null;
  requestedAt: string;
  approvedAt?: string | null;
  usedAt?: string | null;
  reviewedAt?: string | null;
  status: string;
  reason: string;
}

export interface UpdatePrivilegedAccessEventInput extends CreatePrivilegedAccessEventInput {}

export interface CreateClassificationPolicyInput {
  policyCode: string;
  classificationLevel: string;
  scope: string;
  status: string;
  handlingRule?: string | null;
}

export interface UpdateClassificationPolicyInput extends CreateClassificationPolicyInput {}
