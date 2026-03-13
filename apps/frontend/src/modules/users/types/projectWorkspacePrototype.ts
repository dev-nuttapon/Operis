export type ProjectWorkspacePrototypeSection =
  | "overview"
  | "team"
  | "orgChart"
  | "roles"
  | "compliance"
  | "evidence"
  | "auditTrail";

export interface ProjectWorkspacePrototypeOption {
  id: string;
  labelKey: string;
  descriptionKey: string;
}

export interface ProjectWorkspacePrototypeSummaryCard {
  id: string;
  tone: "blue" | "green" | "gold" | "red";
  titleKey: string;
  value: string;
  helperKey: string;
}

export interface ProjectWorkspacePrototypeQuickAction {
  id: string;
  labelKey: string;
  targetSection: ProjectWorkspacePrototypeSection;
}

export interface ProjectWorkspacePrototypeMember {
  id: string;
  name: string;
  email: string;
  roleCode: string;
  roleName: string;
  reportsTo?: string;
  primary: boolean;
  status: "Active" | "Warning";
  period: string;
}

export interface ProjectWorkspacePrototypeRole {
  id: string;
  name: string;
  code: string;
  responsibility: string;
  authority: string;
  review: boolean;
  approval: boolean;
  release: boolean;
  memberCount: number;
}

export interface ProjectWorkspacePrototypeComplianceCheck {
  id: string;
  titleKey: string;
  detailKey: string;
  severity: "passed" | "warning" | "failed";
  targetSection: ProjectWorkspacePrototypeSection;
}

export interface ProjectWorkspacePrototypeEvidenceItem {
  id: string;
  title: string;
  description: string;
  lastUpdated: string;
  format: string;
}

export interface ProjectWorkspacePrototypeAuditEvent {
  id: string;
  timestamp: string;
  actor: string;
  action: string;
  target: string;
  detail: string;
}

export interface ProjectWorkspacePrototypeOrgNode {
  id: string;
  label: string;
  subtitle: string;
  children?: ProjectWorkspacePrototypeOrgNode[];
}

export interface ProjectWorkspacePrototypeRequiredDocument {
  id: string;
  code: string;
  name: string;
  ownerRoleCode: string;
  stage: string;
  status: "Ready" | "Missing" | "Draft";
}

export interface ProjectWorkspacePrototypeApprovalStep {
  id: string;
  order: number;
  roleCode: string;
  roleName: string;
  action: string;
  output: string;
}

export interface ProjectWorkspacePrototypeRoleDependency {
  id: string;
  fromRoleCode: string;
  toRoleCode: string;
  relation: string;
  rationale: string;
}

export interface ProjectWorkspacePrototypeDataset {
  summaryCards: ProjectWorkspacePrototypeSummaryCard[];
  teamMembers: ProjectWorkspacePrototypeMember[];
  roles: ProjectWorkspacePrototypeRole[];
  complianceChecks: ProjectWorkspacePrototypeComplianceCheck[];
  evidenceItems: ProjectWorkspacePrototypeEvidenceItem[];
  auditTrail: ProjectWorkspacePrototypeAuditEvent[];
  orgChart: ProjectWorkspacePrototypeOrgNode[];
  requiredDocuments?: ProjectWorkspacePrototypeRequiredDocument[];
  approvalSteps?: ProjectWorkspacePrototypeApprovalStep[];
  roleDependencies?: ProjectWorkspacePrototypeRoleDependency[];
}
