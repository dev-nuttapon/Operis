export interface BusinessAuditEventItem {
  id: string;
  occurredAt: string;
  module: string;
  eventType: string;
  entityType: string;
  entityId: string | null;
  summary: string | null;
  reason: string | null;
  actorUserId: string | null;
  actorEmail: string | null;
  actorDisplayName: string | null;
  metadataJson: string | null;
}

export interface AuditEventItem {
  id: string;
  occurredAt: string;
  actorUserId: string | null;
  actorEmail: string | null;
  actorDisplayName: string | null;
  entityType: string;
  entityId: string | null;
  action: string;
  outcome: string;
  reason: string | null;
}

export interface ListAuditEventsInput {
  projectId?: string;
  entityType?: string;
  action?: string;
  actorUserId?: string;
  outcome?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

export interface AuditFindingItem {
  id: string;
  auditPlanId: string;
  auditPlanTitle: string;
  code: string;
  title: string;
  severity: string;
  status: string;
  ownerUserId: string;
  dueDate: string | null;
  resolutionSummary: string | null;
  updatedAt: string;
}

export interface AuditPlanListItem {
  id: string;
  projectId: string;
  projectName: string;
  title: string;
  scope: string;
  plannedAt: string;
  status: string;
  ownerUserId: string;
  openFindingCount: number;
  updatedAt: string;
}

export interface AuditPlanDetail {
  id: string;
  projectId: string;
  projectName: string;
  title: string;
  scope: string;
  criteria: string;
  plannedAt: string;
  status: string;
  ownerUserId: string;
  findings: AuditFindingItem[];
  history: BusinessAuditEventItem[];
  createdAt: string;
  updatedAt: string;
}

export interface AuditPlanListInput {
  projectId?: string;
  status?: string;
  ownerUserId?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateAuditPlanInput {
  projectId: string;
  title: string;
  scope: string;
  criteria: string;
  plannedAt: string;
  ownerUserId: string;
}

export interface UpdateAuditPlanInput {
  title: string;
  scope: string;
  criteria: string;
  plannedAt: string;
  status: string;
  ownerUserId: string;
}

export interface CreateAuditFindingInput {
  auditPlanId: string;
  code: string;
  title: string;
  description: string;
  severity: string;
  ownerUserId: string;
  dueDate?: string | null;
}

export interface UpdateAuditFindingInput {
  title: string;
  description: string;
  severity: string;
  status: string;
  ownerUserId: string;
  dueDate?: string | null;
  resolutionSummary?: string | null;
}

export interface CloseAuditFindingInput {
  resolutionSummary: string;
}

export interface EvidenceExportItem {
  id: string;
  requestedBy: string;
  scopeType: string;
  scopeRef: string;
  requestedAt: string;
  status: string;
  outputRef: string | null;
  failureReason: string | null;
}

export interface EvidenceExportDetail {
  id: string;
  requestedBy: string;
  scopeType: string;
  scopeRef: string;
  requestedAt: string;
  status: string;
  outputRef: string | null;
  from: string | null;
  to: string | null;
  includedArtifactTypes: string[];
  failureReason: string | null;
  history: BusinessAuditEventItem[];
}

export interface EvidenceExportListInput {
  scopeType?: string;
  status?: string;
  requestedBy?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateEvidenceExportInput {
  scopeType: string;
  scopeRef: string;
  from: string;
  to: string;
  includedArtifactTypes?: string[];
}

export interface EvidenceRuleListInput {
  search?: string;
  status?: string;
  processArea?: string;
  artifactType?: string;
  projectId?: string;
  page?: number;
  pageSize?: number;
}

export interface EvidenceRuleResultListInput {
  scopeType?: string;
  status?: string;
  processArea?: string;
  projectId?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateEvidenceRuleInput {
  ruleCode: string;
  title: string;
  processArea: string;
  artifactType: string;
  projectId?: string | null;
  status: string;
  expressionType: string;
  reason?: string | null;
}

export interface UpdateEvidenceRuleInput extends CreateEvidenceRuleInput {}

export interface EvaluateEvidenceRulesInput {
  projectId?: string | null;
  processArea?: string | null;
  scopeType?: string | null;
  scopeRef?: string | null;
}

export interface EvidenceRuleListItem {
  id: string;
  ruleCode: string;
  title: string;
  processArea: string;
  artifactType: string;
  projectId: string | null;
  status: string;
  expressionType: string;
  updatedAt: string;
}

export interface EvidenceRuleDetail {
  id: string;
  ruleCode: string;
  title: string;
  processArea: string;
  artifactType: string;
  projectId: string | null;
  status: string;
  expressionType: string;
  reason: string | null;
  createdAt: string;
  updatedAt: string;
  history: BusinessAuditEventItem[];
}

export interface EvidenceMissingItem {
  id: string;
  ruleId: string;
  projectId: string | null;
  projectCode: string | null;
  processArea: string;
  artifactType: string;
  reasonCode: string;
  title: string;
  module: string;
  route: string;
  scope: string;
  entityType: string | null;
  entityId: string | null;
  metadata: string | null;
  detectedAt: string;
}

export interface EvidenceRuleResultListItem {
  id: string;
  scopeType: string;
  scopeRef: string;
  projectId: string | null;
  projectCode: string | null;
  processArea: string | null;
  status: string;
  evaluatedRuleCount: number;
  missingItemCount: number;
  startedAt: string;
  completedAt: string;
}

export interface EvidenceRuleResultDetail extends EvidenceRuleResultListItem {
  missingItems: EvidenceMissingItem[];
  history: BusinessAuditEventItem[];
}
