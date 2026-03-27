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

export interface OperationsListInput {
  scopeType?: string;
  scopeRef?: string;
  dependencyType?: string;
  supplierId?: string;
  supplierType?: string;
  ownerUserId?: string;
  agreementType?: string;
  criticality?: string;
  status?: string;
  reviewDueBefore?: string;
  effectiveToBefore?: string;
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
