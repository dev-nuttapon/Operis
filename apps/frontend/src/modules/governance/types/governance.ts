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
  architectureType?: string;
  architectureRecordId?: string;
  reviewType?: string;
  reviewedBy?: string;
  integrationType?: string;
  facilitatorUserId?: string;
  scheduledFrom?: string;
  scheduledTo?: string;
  policyId?: string;
  campaignId?: string;
  dueBefore?: string;
  onlyOverdue?: boolean;
}

export interface ComplianceDashboardInput {
  projectId?: string;
  processArea?: string;
  periodDays?: number;
  showOnlyAtRisk?: boolean;
}

export interface ComplianceDashboardPreferenceInput {
  defaultProjectId?: string;
  defaultProcessArea?: string;
  defaultPeriodDays: number;
  defaultShowOnlyAtRisk: boolean;
}

export interface ComplianceDashboardFilters {
  projectId?: string | null;
  processArea?: string | null;
  periodDays: number;
  showOnlyAtRisk: boolean;
}

export interface ComplianceDashboardSummary {
  projectsInGoodStanding: number;
  projectsWithMissingArtifacts: number;
  overdueApprovals: number;
  staleBaselines: number;
  openCapa: number;
  openAuditFindings: number;
  openSecurityItems: number;
}

export interface ComplianceProjectReadiness {
  projectId: string;
  projectCode: string;
  projectName: string;
  projectStatus: string;
  projectPhase?: string | null;
  readinessScore: number;
  readinessState: string;
  missingArtifactCount: number;
  overdueApprovalCount: number;
  staleBaselineCount: number;
  openCapaCount: number;
  openAuditFindingCount: number;
  openSecurityItemCount: number;
}

export interface ComplianceProcessArea {
  processArea: string;
  label: string;
  projectCount: number;
  atRiskProjectCount: number;
  missingArtifactCount: number;
  overdueApprovalCount: number;
  staleBaselineCount: number;
  openCapaCount: number;
  openAuditFindingCount: number;
  openSecurityItemCount: number;
}

export interface ComplianceDashboard {
  summary: ComplianceDashboardSummary;
  projects: ComplianceProjectReadiness[];
  processAreas: ComplianceProcessArea[];
  generatedAt: string;
  filters: ComplianceDashboardFilters;
}

export interface ComplianceDrilldownRow {
  issueType: string;
  entityType: string;
  entityId: string;
  title: string;
  module: string;
  route: string;
  status: string;
  scope: string;
  dueAt?: string | null;
  metadata?: string | null;
}

export interface ComplianceDrilldown {
  issueType: string;
  projectId?: string | null;
  processArea?: string | null;
  generatedAt: string;
  rows: ComplianceDrilldownRow[];
}

export interface ComplianceDashboardPreference {
  id: string;
  userId: string;
  defaultProjectId?: string | null;
  defaultProcessArea?: string | null;
  defaultPeriodDays: number;
  defaultShowOnlyAtRisk: boolean;
  updatedAt: string;
}

export interface ManagementReviewItemInput {
  itemType: string;
  title: string;
  summary?: string | null;
  decision?: string | null;
  ownerUserId?: string | null;
  dueAt?: string | null;
  status?: string | null;
}

export interface ManagementReviewActionInput {
  title: string;
  description?: string | null;
  ownerUserId: string;
  dueAt?: string | null;
  status?: string | null;
  isMandatory: boolean;
  linkedEntityType?: string | null;
  linkedEntityId?: string | null;
}

export interface ManagementReviewListItem {
  id: string;
  projectId?: string | null;
  projectName?: string | null;
  reviewCode: string;
  title: string;
  reviewPeriod: string;
  scheduledAt: string;
  facilitatorUserId: string;
  status: string;
  openActionCount: number;
  updatedAt: string;
}

export interface ManagementReviewItem {
  id: string;
  itemType: string;
  title: string;
  summary?: string | null;
  decision?: string | null;
  ownerUserId?: string | null;
  dueAt?: string | null;
  status: string;
  updatedAt: string;
}

export interface ManagementReviewAction {
  id: string;
  title: string;
  description?: string | null;
  ownerUserId: string;
  dueAt?: string | null;
  status: string;
  isMandatory: boolean;
  linkedEntityType?: string | null;
  linkedEntityId?: string | null;
  closedAt?: string | null;
  updatedAt: string;
}

export interface ManagementReviewDetail {
  id: string;
  projectId?: string | null;
  projectName?: string | null;
  reviewCode: string;
  title: string;
  reviewPeriod: string;
  scheduledAt: string;
  facilitatorUserId: string;
  status: string;
  agendaSummary?: string | null;
  minutesSummary?: string | null;
  decisionSummary?: string | null;
  escalationEntityType?: string | null;
  escalationEntityId?: string | null;
  closedBy?: string | null;
  closedAt?: string | null;
  items: ManagementReviewItem[];
  actions: ManagementReviewAction[];
  history: WorkflowOverrideLog[];
  createdAt: string;
  updatedAt: string;
}

export interface ManagementReviewFormInput {
  projectId?: string | null;
  reviewCode: string;
  title: string;
  reviewPeriod: string;
  scheduledAt: string;
  facilitatorUserId: string;
  agendaSummary?: string | null;
  minutesSummary?: string | null;
  decisionSummary?: string | null;
  escalationEntityType?: string | null;
  escalationEntityId?: string | null;
  items?: ManagementReviewItemInput[];
  actions?: ManagementReviewActionInput[];
}

export interface ManagementReviewTransitionInput {
  targetStatus: string;
  reason?: string | null;
}

export interface PolicyListItem {
  id: string;
  policyCode: string;
  title: string;
  summary?: string | null;
  effectiveDate: string;
  requiresAttestation: boolean;
  status: string;
  approvedAt?: string | null;
  approvedBy?: string | null;
  publishedAt?: string | null;
  retiredAt?: string | null;
  campaignCount: number;
  openCampaignCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface PolicyCampaignItem {
  id: string;
  policyId: string;
  policyTitle: string;
  campaignCode: string;
  title: string;
  targetScopeType: string;
  targetScopeRef: string;
  dueAt: string;
  status: string;
  targetUserCount: number;
  acknowledgedCount: number;
  overdueCount: number;
  launchedAt?: string | null;
  launchedBy?: string | null;
  closedAt?: string | null;
  closedBy?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface PolicyAcknowledgementItem {
  id: string;
  policyId: string;
  policyTitle: string;
  policyCampaignId: string;
  campaignTitle: string;
  userId: string;
  status: string;
  isOverdue: boolean;
  requiresAttestation: boolean;
  dueAt: string;
  acknowledgedAt?: string | null;
  attestationText?: string | null;
  updatedAt: string;
}

export interface PolicyFormInput {
  policyCode: string;
  title: string;
  summary?: string | null;
  effectiveDate: string;
  requiresAttestation: boolean;
}

export interface PolicyTransitionInput {
  targetStatus: string;
  reason?: string | null;
}

export interface PolicyCampaignFormInput {
  policyId: string;
  campaignCode: string;
  title: string;
  targetScopeType: string;
  targetScopeRef: string;
  dueAt: string;
}

export interface PolicyCampaignTransitionInput {
  targetStatus: string;
  reason?: string | null;
}

export interface PolicyAcknowledgementInput {
  policyCampaignId: string;
  attestationText?: string | null;
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

export interface ArchitectureRecord {
  id: string;
  projectId: string;
  projectName?: string | null;
  title: string;
  architectureType: string;
  ownerUserId: string;
  status: string;
  currentVersionId?: string | null;
  summary?: string | null;
  securityImpact?: string | null;
  evidenceRef?: string | null;
  approvedBy?: string | null;
  approvedAt?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface ArchitectureRecordFormInput {
  projectId: string;
  title: string;
  architectureType: string;
  ownerUserId: string;
  status: string;
  currentVersionId?: string | null;
  summary?: string | null;
  securityImpact?: string | null;
  evidenceRef?: string | null;
}

export interface DesignReview {
  id: string;
  architectureRecordId: string;
  architectureTitle?: string | null;
  reviewType: string;
  reviewedBy?: string | null;
  status: string;
  decisionReason?: string | null;
  designSummary?: string | null;
  concerns?: string | null;
  evidenceRef?: string | null;
  decidedAt?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface DesignReviewFormInput {
  architectureRecordId: string;
  reviewType: string;
  reviewedBy?: string | null;
  status: string;
  decisionReason?: string | null;
  designSummary?: string | null;
  concerns?: string | null;
  evidenceRef?: string | null;
}

export interface IntegrationReview {
  id: string;
  scopeRef: string;
  integrationType: string;
  reviewedBy?: string | null;
  status: string;
  decisionReason?: string | null;
  risks?: string | null;
  dependencyImpact?: string | null;
  evidenceRef?: string | null;
  decidedAt?: string | null;
  appliedAt?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface IntegrationReviewFormInput {
  scopeRef: string;
  integrationType: string;
  reviewedBy?: string | null;
  status: string;
  decisionReason?: string | null;
  risks?: string | null;
  dependencyImpact?: string | null;
  evidenceRef?: string | null;
}
