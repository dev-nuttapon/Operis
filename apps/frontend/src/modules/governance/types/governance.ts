import type { PaginatedResult, PaginationInput } from "../../../shared/types/pagination";

export type GovernanceStatus =
  | "draft"
  | "reviewed"
  | "approved"
  | "active"
  | "deprecated"
  | "review"
  | "baseline"
  | "superseded"
  | "submitted"
  | "rejected"
  | "applied"
  | "archived";

export interface GovernanceListInput extends PaginationInput {
  search?: string;
  status?: string;
  ownerUserId?: string;
  projectId?: string;
  processCode?: string;
  entityType?: string;
  actorUserId?: string;
  outcome?: string;
  approvedFrom?: string;
  approvedTo?: string;
  requestedBy?: string;
  approvedBy?: string;
  occurredFrom?: string;
  occurredTo?: string;
  scopeType?: string;
  appliesTo?: string;
}

export interface ProcessAssetVersionSummary {
  id: string;
  versionNumber: number;
  title: string;
  status: string;
  changeSummary?: string | null;
  approvedBy?: string | null;
  approvedAt?: string | null;
  updatedAt: string;
}

export interface ProcessAssetListItem {
  id: string;
  code: string;
  name: string;
  category: string;
  status: string;
  ownerUserId: string;
  currentVersion?: ProcessAssetVersionSummary | null;
  effectiveFrom?: string | null;
  effectiveTo?: string | null;
  updatedAt: string;
}

export interface ProcessAssetVersionDetail extends ProcessAssetVersionSummary {
  summary: string;
  contentRef?: string | null;
  createdAt: string;
}

export interface ProcessAsset {
  id: string;
  code: string;
  name: string;
  category: string;
  status: string;
  ownerUserId: string;
  effectiveFrom?: string | null;
  effectiveTo?: string | null;
  currentVersionId?: string | null;
  versions: ProcessAssetVersionDetail[];
  createdAt: string;
  updatedAt: string;
}

export interface ProcessAssetFormInput {
  code: string;
  name: string;
  category: string;
  ownerUserId: string;
  effectiveFrom?: string | null;
  effectiveTo?: string | null;
  initialVersionTitle?: string;
  initialVersionSummary?: string;
  initialContentRef?: string | null;
}

export interface ProcessAssetVersionFormInput {
  title: string;
  summary: string;
  contentRef?: string | null;
  changeSummary?: string | null;
}

export interface QaChecklistItem {
  itemText: string;
  mandatory: boolean;
  applicablePhase: string;
  evidenceRule: string;
}

export interface QaChecklistListItem {
  id: string;
  code: string;
  name: string;
  scope: string;
  status: string;
  ownerUserId: string;
  updatedAt: string;
}

export interface QaChecklist extends QaChecklistListItem {
  items: QaChecklistItem[];
  createdAt: string;
}

export interface QaChecklistFormInput {
  code: string;
  name: string;
  scope: string;
  ownerUserId: string;
  items: QaChecklistItem[];
}

export interface ProjectPlanListItem {
  id: string;
  projectId: string;
  projectName: string;
  name: string;
  lifecycleModel: string;
  status: string;
  ownerUserId: string;
  startDate: string;
  targetEndDate: string;
  updatedAt: string;
}

export interface ProjectPlan {
  id: string;
  projectId: string;
  name: string;
  scopeSummary: string;
  lifecycleModel: string;
  startDate: string;
  targetEndDate: string;
  ownerUserId: string;
  status: string;
  milestones: string[];
  roles: string[];
  riskApproach: string;
  qualityApproach: string;
  approvalReason?: string | null;
  approvedBy?: string | null;
  approvedAt?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface ProjectPlanFormInput {
  projectId: string;
  name: string;
  scopeSummary: string;
  lifecycleModel: string;
  startDate: string;
  targetEndDate: string;
  ownerUserId: string;
  milestones: string[];
  roles: string[];
  riskApproach: string;
  qualityApproach: string;
}

export interface Stakeholder {
  id: string;
  projectId: string;
  projectName: string;
  name: string;
  roleName: string;
  influenceLevel: string;
  contactChannel: string;
  status: string;
  createdAt: string;
  updatedAt: string;
}

export interface StakeholderFormInput {
  projectId: string;
  name: string;
  roleName: string;
  influenceLevel: string;
  contactChannel: string;
  status?: string;
}

export interface TailoringRecordListItem {
  id: string;
  projectId: string;
  projectName: string;
  requestedChange: string;
  status: string;
  requesterUserId: string;
  approverUserId?: string | null;
  updatedAt: string;
}

export interface TailoringRecord extends TailoringRecordListItem {
  reason: string;
  impactSummary: string;
  approvedAt?: string | null;
  impactedProcessAssetId?: string | null;
  approvalRationale?: string | null;
  createdAt: string;
}

export interface TailoringRecordFormInput {
  projectId: string;
  requesterUserId: string;
  requestedChange: string;
  reason: string;
  impactSummary: string;
  impactedProcessAssetId?: string | null;
}

export interface GovernanceMutationResponse {
  id: string;
  status: string;
  updatedAt: string;
  approvedBy?: string | null;
  approvedAt?: string | null;
}

export type GovernanceListResult<T> = PaginatedResult<T>;

export interface RaciMap {
  id: string;
  processCode: string;
  roleName: string;
  responsibilityType: "R" | "A" | "C" | "I";
  status: string;
  createdAt: string;
  updatedAt: string;
}

export interface RaciMapFormInput {
  processCode: string;
  roleName: string;
  responsibilityType: "R" | "A" | "C" | "I";
  status: string;
  reason?: string | null;
}

export interface ApprovalEvidenceLog {
  id: string;
  entityType: string;
  entityId: string;
  approverUserId: string;
  approvedAt: string;
  reason: string;
  outcome: string;
}

export interface WorkflowOverrideLog {
  id: string;
  entityType: string;
  entityId: string;
  requestedBy: string;
  approvedBy: string;
  reason: string;
  occurredAt: string;
}

export interface SlaRule {
  id: string;
  scopeType: string;
  scopeRef: string;
  targetDurationHours: number;
  escalationPolicyId: string;
  status: string;
  createdAt: string;
  updatedAt: string;
}

export interface SlaRuleFormInput {
  scopeType: string;
  scopeRef: string;
  targetDurationHours: number;
  escalationPolicyId: string;
  status: string;
  reason?: string | null;
}

export interface RetentionPolicy {
  id: string;
  policyCode: string;
  appliesTo: string;
  retentionPeriodDays: number;
  archiveRule?: string | null;
  status: string;
  createdAt: string;
  updatedAt: string;
}

export interface RetentionPolicyFormInput {
  policyCode: string;
  appliesTo: string;
  retentionPeriodDays: number;
  archiveRule?: string | null;
  status: string;
  reason?: string | null;
}
