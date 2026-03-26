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
  ownerUserId: string;
  criticality: string;
  status: string;
  reviewDueAt: string | null;
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
  criticality?: string;
  status?: string;
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
