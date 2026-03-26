import type { PaginatedResult, PaginationInput } from "../../../shared/types/pagination";

export type RequirementStatus = "draft" | "review" | "approved" | "baselined" | "superseded";
export type RequirementPriority = "low" | "medium" | "high" | "critical";

export interface RequirementListInput extends PaginationInput {
  search?: string;
  projectId?: string;
  priority?: string;
  status?: string;
  ownerUserId?: string;
  baselineStatus?: string;
  missingDownstreamLinks?: boolean;
}

export interface RequirementListItem {
  id: string;
  projectId: string;
  projectName: string;
  code: string;
  title: string;
  priority: string;
  ownerUserId: string;
  status: RequirementStatus;
  baselineStatus?: string | null;
  missingLinkCount: number;
  currentVersionId?: string | null;
  currentVersionNumber?: number | null;
  updatedAt: string;
}

export interface RequirementVersionItem {
  id: string;
  requirementId: string;
  versionNumber: number;
  businessReason: string;
  acceptanceCriteria: string;
  securityImpact?: string | null;
  performanceImpact?: string | null;
  status: string;
  createdAt: string;
}

export interface RequirementHistoryItem {
  id: string;
  eventType: string;
  summary?: string | null;
  reason?: string | null;
  actorUserId?: string | null;
  occurredAt: string;
}

export interface TraceabilityLinkItem {
  id: string;
  sourceType: string;
  sourceId: string;
  targetType: string;
  targetId: string;
  linkRule: string;
  status: string;
  createdBy: string;
  createdAt: string;
}

export interface RequirementDetail {
  id: string;
  projectId: string;
  projectName: string;
  code: string;
  title: string;
  description: string;
  priority: string;
  ownerUserId: string;
  status: RequirementStatus;
  currentVersionId?: string | null;
  versions: RequirementVersionItem[];
  traceabilityLinks: TraceabilityLinkItem[];
  history: RequirementHistoryItem[];
  createdAt: string;
  updatedAt: string;
}

export interface RequirementFormInput {
  projectId: string;
  code: string;
  title: string;
  description: string;
  priority: RequirementPriority;
  ownerUserId: string;
  businessReason: string;
  acceptanceCriteria: string;
  securityImpact?: string | null;
  performanceImpact?: string | null;
}

export interface RequirementUpdateInput extends Omit<RequirementFormInput, "projectId" | "code"> {}

export interface RequirementBaselineItem {
  id: string;
  projectId: string;
  projectName: string;
  baselineName: string;
  requirementIds: string[];
  status: string;
  approvedBy: string;
  approvedAt: string;
}

export interface RequirementBaselineInput {
  projectId: string;
  baselineName: string;
  requirementIds: string[];
  reason: string;
}

export interface TraceabilityListInput extends PaginationInput {
  projectId?: string;
  baselineStatus?: string;
  missingCoverage?: boolean;
}

export interface TraceabilityMatrixRow {
  requirementId: string;
  requirementCode: string;
  requirementTitle: string;
  projectId: string;
  projectName: string;
  requirementStatus: string;
  baselineStatus?: string | null;
  missingLinkCount: number;
  links: TraceabilityLinkItem[];
}

export interface TraceabilityLinkInput {
  sourceType: string;
  sourceId: string;
  targetType: string;
  targetId: string;
  linkRule: string;
}

export type RequirementListResult = PaginatedResult<RequirementListItem>;
export type RequirementBaselineListResult = PaginatedResult<RequirementBaselineItem>;
export type TraceabilityMatrixResult = PaginatedResult<TraceabilityMatrixRow>;
