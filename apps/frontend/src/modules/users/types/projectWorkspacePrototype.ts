export type ProjectWorkspacePrototypeSection =
  | "overview"
  | "team"
  | "orgChart"
  | "roles"
  | "workflow"
  | "compliance"
  | "evidence"
  | "auditTrail";

export interface ProjectWorkspacePrototypeSummaryCard {
  id: string;
  tone: "blue" | "green" | "gold" | "red";
  titleKey: string;
  value: string;
  helperKey: string;
}

export interface ProjectWorkspacePrototypeMember {
  id: string;
  name: string;
  email: string;
  roleCode: string;
  roleName: string;
  reportsTo?: string;
  primary: boolean;
  status: string;
  period: string;
}

export interface ProjectWorkspacePrototypeRole {
  id: string;
  name: string;
  code: string;
  responsibility: string;
  authority: string;
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
