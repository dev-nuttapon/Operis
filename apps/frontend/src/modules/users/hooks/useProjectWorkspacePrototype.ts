import { useEffect, useMemo, useState } from "react";
import type {
  ProjectWorkspacePrototypeAuditEvent,
  ProjectWorkspacePrototypeApprovalStep,
  ProjectWorkspacePrototypeComplianceCheck,
  ProjectWorkspacePrototypeDataset,
  ProjectWorkspacePrototypeEvidenceItem,
  ProjectWorkspacePrototypeMember,
  ProjectWorkspacePrototypeOption,
  ProjectWorkspacePrototypeOrgNode,
  ProjectWorkspacePrototypeQuickAction,
  ProjectWorkspacePrototypeRoleDependency,
  ProjectWorkspacePrototypeRequiredDocument,
  ProjectWorkspacePrototypeRole,
  ProjectWorkspacePrototypeSection,
  ProjectWorkspacePrototypeSummaryCard,
} from "../types/projectWorkspacePrototype";

const templateOptions: ProjectWorkspacePrototypeOption[] = [
  {
    id: "software_delivery",
    labelKey: "project_workspace_prototype.templates.software_delivery.label",
    descriptionKey: "project_workspace_prototype.templates.software_delivery.description",
  },
  {
    id: "compliance_audit",
    labelKey: "project_workspace_prototype.templates.compliance_audit.label",
    descriptionKey: "project_workspace_prototype.templates.compliance_audit.description",
  },
  {
    id: "process_improvement",
    labelKey: "project_workspace_prototype.templates.process_improvement.label",
    descriptionKey: "project_workspace_prototype.templates.process_improvement.description",
  },
];

const scenarioOptionsByTemplate: Record<string, ProjectWorkspacePrototypeOption[]> = {
  software_delivery: [
    {
      id: "compact_team",
      labelKey: "project_workspace_prototype.scenarios.compact_team.label",
      descriptionKey: "project_workspace_prototype.scenarios.compact_team.description",
    },
    {
      id: "standard_team",
      labelKey: "project_workspace_prototype.scenarios.standard_team.label",
      descriptionKey: "project_workspace_prototype.scenarios.standard_team.description",
    },
    {
      id: "release_readiness",
      labelKey: "project_workspace_prototype.scenarios.release_readiness.label",
      descriptionKey: "project_workspace_prototype.scenarios.release_readiness.description",
    },
  ],
  compliance_audit: [
    {
      id: "audit_preparation",
      labelKey: "project_workspace_prototype.scenarios.audit_preparation.label",
      descriptionKey: "project_workspace_prototype.scenarios.audit_preparation.description",
    },
    {
      id: "corrective_action",
      labelKey: "project_workspace_prototype.scenarios.corrective_action.label",
      descriptionKey: "project_workspace_prototype.scenarios.corrective_action.description",
    },
  ],
  process_improvement: [
    {
      id: "process_rollout",
      labelKey: "project_workspace_prototype.scenarios.process_rollout.label",
      descriptionKey: "project_workspace_prototype.scenarios.process_rollout.description",
    },
    {
      id: "pilot_team",
      labelKey: "project_workspace_prototype.scenarios.pilot_team.label",
      descriptionKey: "project_workspace_prototype.scenarios.pilot_team.description",
    },
  ],
};

const quickActions: ProjectWorkspacePrototypeQuickAction[] = [
  {
    id: "role-gap",
    labelKey: "project_workspace_prototype.quick_actions.role_gap",
    targetSection: "roles",
  },
  {
    id: "org-chart",
    labelKey: "project_workspace_prototype.quick_actions.org_chart",
    targetSection: "orgChart",
  },
  {
    id: "compliance",
    labelKey: "project_workspace_prototype.quick_actions.compliance",
    targetSection: "compliance",
  },
  {
    id: "evidence",
    labelKey: "project_workspace_prototype.quick_actions.evidence",
    targetSection: "evidence",
  },
];

function card(
  id: string,
  tone: ProjectWorkspacePrototypeSummaryCard["tone"],
  titleKey: string,
  value: string,
  helperKey: string,
): ProjectWorkspacePrototypeSummaryCard {
  return { id, tone, titleKey, value, helperKey };
}

function member(
  id: string,
  name: string,
  email: string,
  roleCode: string,
  roleName: string,
  reportsTo: string | undefined,
  primary: boolean,
  status: ProjectWorkspacePrototypeMember["status"],
  period: string,
): ProjectWorkspacePrototypeMember {
  return { id, name, email, roleCode, roleName, reportsTo, primary, status, period };
}

function role(
  id: string,
  name: string,
  code: string,
  responsibility: string,
  authority: string,
  review: boolean,
  approval: boolean,
  release: boolean,
  memberCount: number,
): ProjectWorkspacePrototypeRole {
  return { id, name, code, responsibility, authority, review, approval, release, memberCount };
}

function compliance(
  id: string,
  titleKey: string,
  detailKey: string,
  severity: ProjectWorkspacePrototypeComplianceCheck["severity"],
  targetSection: ProjectWorkspacePrototypeSection,
): ProjectWorkspacePrototypeComplianceCheck {
  return { id, titleKey, detailKey, severity, targetSection };
}

function evidence(
  id: string,
  title: string,
  description: string,
  lastUpdated: string,
  format: string,
): ProjectWorkspacePrototypeEvidenceItem {
  return { id, title, description, lastUpdated, format };
}

function audit(
  id: string,
  timestamp: string,
  actor: string,
  action: string,
  target: string,
  detail: string,
): ProjectWorkspacePrototypeAuditEvent {
  return { id, timestamp, actor, action, target, detail };
}

function node(id: string, label: string, subtitle: string, children?: ProjectWorkspacePrototypeOrgNode[]): ProjectWorkspacePrototypeOrgNode {
  return { id, label, subtitle, children };
}

function requiredDocument(
  id: string,
  code: string,
  name: string,
  ownerRoleCode: string,
  stage: string,
  status: ProjectWorkspacePrototypeRequiredDocument["status"],
): ProjectWorkspacePrototypeRequiredDocument {
  return { id, code, name, ownerRoleCode, stage, status };
}

function approvalStep(
  id: string,
  order: number,
  roleCode: string,
  roleName: string,
  action: string,
  output: string,
): ProjectWorkspacePrototypeApprovalStep {
  return { id, order, roleCode, roleName, action, output };
}

function roleDependency(
  id: string,
  fromRoleCode: string,
  toRoleCode: string,
  relation: string,
  rationale: string,
): ProjectWorkspacePrototypeRoleDependency {
  return { id, fromRoleCode, toRoleCode, relation, rationale };
}

const datasets: Record<string, Record<string, ProjectWorkspacePrototypeDataset>> = {
  software_delivery: {
    compact_team: {
      summaryCards: [
        card("project-type", "blue", "project_workspace_prototype.summary.project_type.title", "Software Delivery", "project_workspace_prototype.summary.project_type.helper"),
        card("active-members", "green", "project_workspace_prototype.summary.active_members.title", "5", "project_workspace_prototype.summary.active_members.helper"),
        card("active-roles", "gold", "project_workspace_prototype.summary.active_roles.title", "5", "project_workspace_prototype.summary.active_roles.helper"),
        card("reporting-roots", "red", "project_workspace_prototype.summary.reporting_roots.title", "1", "project_workspace_prototype.summary.reporting_roots.helper"),
      ],
      teamMembers: [
        member("1", "Anan Techakul", "anan@example.com", "PM", "Project Manager", undefined, true, "Active", "01 Mar 2026 - 31 Dec 2026"),
        member("2", "Sirin Nanta", "sirin@example.com", "SWA", "Software Architect", "Anan Techakul", true, "Active", "01 Mar 2026 - 31 Dec 2026"),
        member("3", "Noppon Krailad", "noppon@example.com", "DEV", "Developer", "Sirin Nanta", true, "Active", "01 Mar 2026 - 31 Dec 2026"),
        member("4", "Pimchanok Deesri", "pimchanok@example.com", "SWT", "Software Tester", "Anan Techakul", false, "Active", "15 Mar 2026 - 31 Dec 2026"),
        member("5", "Tanan Kijprasert", "tanan@example.com", "CM", "Configuration Manager", "Anan Techakul", false, "Warning", "20 Mar 2026 - 31 Dec 2026"),
      ],
      roles: [
        role("1", "Project Manager", "PM", "Own scope, plan, and stakeholder alignment.", "Approve team changes and major schedule decisions.", false, true, false, 1),
        role("2", "Software Architect", "SWA", "Own architecture baseline and technical review.", "Review technical deviations and solution design.", true, false, false, 1),
        role("3", "Software Tester", "SWT", "Plan and execute verification and validation.", "Review quality evidence and release readiness inputs.", true, false, false, 1),
        role("4", "Configuration Manager", "CM", "Control baselines and release records.", "Release controlled items and preserve configuration integrity.", true, false, true, 1),
        role("5", "Developer", "DEV", "Implement approved scope and update working artifacts.", "Prepare implementation evidence and technical updates.", false, false, false, 1),
      ],
      complianceChecks: [
        compliance("owner", "project_workspace_prototype.compliance.owner.title", "project_workspace_prototype.compliance.owner.detail", "passed", "overview"),
        compliance("role-gap", "project_workspace_prototype.compliance.role_gap.title", "project_workspace_prototype.compliance.role_gap.detail", "warning", "roles"),
        compliance("org-chart", "project_workspace_prototype.compliance.org_chart.title", "project_workspace_prototype.compliance.org_chart.detail", "passed", "orgChart"),
        compliance("evidence", "project_workspace_prototype.compliance.evidence.title", "project_workspace_prototype.compliance.evidence.detail", "warning", "evidence"),
      ],
      evidenceItems: [
        evidence("1", "Architecture Review Package", "Architecture decision records, review notes, and impact log.", "12 Mar 2026 14:40", "PDF / ZIP"),
        evidence("2", "Verification Evidence Pack", "Test summary, defect register, and traceability notes.", "12 Mar 2026 13:10", "CSV / PDF"),
      ],
      auditTrail: [
        audit("1", "12 Mar 2026 14:32", "Anan Techakul", "Updated role coverage", "SWA", "Added technical review accountability for release checkpoint."),
        audit("2", "12 Mar 2026 10:12", "Pimchanok Deesri", "Prepared verification pack", "Verification Evidence Pack", "Uploaded draft package for internal review."),
      ],
      orgChart: [
        node("1", "Anan Techakul", "PM", [
          node("2", "Sirin Nanta", "SWA", [node("3", "Noppon Krailad", "DEV")]),
          node("4", "Pimchanok Deesri", "SWT"),
          node("5", "Tanan Kijprasert", "CM"),
        ]),
      ],
    },
    standard_team: {
      summaryCards: [
        card("project-type", "blue", "project_workspace_prototype.summary.project_type.title", "Software Delivery", "project_workspace_prototype.summary.project_type.helper"),
        card("active-members", "green", "project_workspace_prototype.summary.active_members.title", "8", "project_workspace_prototype.summary.active_members.helper"),
        card("active-roles", "gold", "project_workspace_prototype.summary.active_roles.title", "8", "project_workspace_prototype.summary.active_roles.helper"),
        card("reporting-roots", "red", "project_workspace_prototype.summary.reporting_roots.title", "1", "project_workspace_prototype.summary.reporting_roots.helper"),
      ],
      teamMembers: [
        member("1", "Anan Techakul", "anan@example.com", "PM", "Project Manager", undefined, true, "Active", "01 Mar 2026 - 31 Dec 2026"),
        member("2", "Sirin Nanta", "sirin@example.com", "SWA", "Software Architect", "Anan Techakul", true, "Active", "01 Mar 2026 - 31 Dec 2026"),
        member("3", "Noppon Krailad", "noppon@example.com", "TL", "Team Lead", "Anan Techakul", true, "Active", "01 Mar 2026 - 31 Dec 2026"),
        member("4", "Pimchanok Deesri", "pimchanok@example.com", "SWT", "Software Tester", "Noppon Krailad", false, "Active", "15 Mar 2026 - 31 Dec 2026"),
        member("5", "Tanan Kijprasert", "tanan@example.com", "CM", "Configuration Manager", "Anan Techakul", false, "Active", "20 Mar 2026 - 31 Dec 2026"),
        member("6", "Kanya Rattanakul", "kanya@example.com", "QA", "Quality Assurance", "Anan Techakul", false, "Active", "10 Mar 2026 - 31 Dec 2026"),
        member("7", "Preecha Montree", "preecha@example.com", "BA", "Business Analyst", "Anan Techakul", false, "Active", "01 Mar 2026 - 30 Sep 2026"),
        member("8", "Metha Chansiri", "metha@example.com", "DEV", "Developer", "Noppon Krailad", false, "Active", "15 Mar 2026 - 31 Dec 2026"),
      ],
      roles: [
        role("1", "Project Manager", "PM", "Lead delivery, budget, and stakeholder direction.", "Approve key project decisions.", false, true, false, 1),
        role("2", "Software Architect", "SWA", "Own design standards and technical review baseline.", "Review architecture changes.", true, false, false, 1),
        role("3", "Team Lead", "TL", "Coordinate delivery team and sprint execution.", "Approve working allocation within team.", false, false, false, 1),
        role("4", "Software Tester", "SWT", "Own test execution and defect reporting.", "Review validation evidence.", true, false, false, 1),
        role("5", "Configuration Manager", "CM", "Own baseline and release control.", "Release controlled items.", true, false, true, 1),
        role("6", "Quality Assurance", "QA", "Monitor process adherence and internal quality checks.", "Review process evidence and deviations.", true, false, false, 1),
        role("7", "Business Analyst", "BA", "Manage requirement clarification and impact notes.", "Prepare requirement evidence.", false, false, false, 1),
        role("8", "Developer", "DEV", "Implement approved scope and technical changes.", "Prepare implementation evidence.", false, false, false, 1),
      ],
      complianceChecks: [
        compliance("owner", "project_workspace_prototype.compliance.owner.title", "project_workspace_prototype.compliance.owner.detail", "passed", "overview"),
        compliance("role-gap", "project_workspace_prototype.compliance.role_gap.title", "project_workspace_prototype.compliance.role_gap.detail", "passed", "roles"),
        compliance("org-chart", "project_workspace_prototype.compliance.org_chart.title", "project_workspace_prototype.compliance.org_chart.detail", "passed", "orgChart"),
        compliance("evidence", "project_workspace_prototype.compliance.evidence.title", "project_workspace_prototype.compliance.evidence.detail", "warning", "evidence"),
      ],
      evidenceItems: [
        evidence("1", "Team Responsibility Matrix", "RACI and role matrix for current delivery cycle.", "13 Mar 2026 09:00", "XLSX / PDF"),
        evidence("2", "Release Control Package", "Baseline report, release checklist, and approval memo.", "13 Mar 2026 11:45", "ZIP / PDF"),
      ],
      auditTrail: [
        audit("1", "13 Mar 2026 08:45", "Kanya Rattanakul", "Completed QA readiness review", "Iteration 3", "Marked process review for iteration 3 as complete."),
        audit("2", "12 Mar 2026 16:20", "Tanan Kijprasert", "Released controlled baseline", "Baseline B3", "Published controlled baseline for release readiness review."),
      ],
      orgChart: [
        node("1", "Anan Techakul", "PM", [
          node("2", "Sirin Nanta", "SWA"),
          node("3", "Noppon Krailad", "TL", [
            node("4", "Pimchanok Deesri", "SWT"),
            node("8", "Metha Chansiri", "DEV"),
          ]),
          node("5", "Tanan Kijprasert", "CM"),
          node("6", "Kanya Rattanakul", "QA"),
          node("7", "Preecha Montree", "BA"),
        ]),
      ],
    },
    release_readiness: {
      summaryCards: [
        card("project-type", "blue", "project_workspace_prototype.summary.project_type.title", "Software Delivery", "project_workspace_prototype.summary.project_type.helper"),
        card("active-members", "green", "project_workspace_prototype.summary.active_members.title", "6", "project_workspace_prototype.summary.active_members.helper"),
        card("active-roles", "gold", "project_workspace_prototype.summary.active_roles.title", "7", "project_workspace_prototype.summary.active_roles.helper"),
        card("reporting-roots", "red", "project_workspace_prototype.summary.reporting_roots.title", "1", "project_workspace_prototype.summary.reporting_roots.helper"),
      ],
      teamMembers: [
        member("1", "Anan Techakul", "anan@example.com", "PM", "Project Manager", undefined, true, "Active", "01 Mar 2026 - 31 Dec 2026"),
        member("2", "Sirin Nanta", "sirin@example.com", "SWA", "Software Architect", "Anan Techakul", true, "Active", "01 Mar 2026 - 31 Dec 2026"),
        member("3", "Tanan Kijprasert", "tanan@example.com", "CM", "Configuration Manager", "Anan Techakul", true, "Active", "01 Mar 2026 - 31 Dec 2026"),
        member("4", "Kanya Rattanakul", "kanya@example.com", "QA", "Quality Assurance", "Anan Techakul", false, "Active", "01 Mar 2026 - 31 Dec 2026"),
        member("5", "Pimchanok Deesri", "pimchanok@example.com", "RVW", "Reviewer", "Anan Techakul", false, "Warning", "10 Mar 2026 - 31 Dec 2026"),
        member("6", "Nattawat Sila", "nattawat@example.com", "APR", "Approver", "Anan Techakul", false, "Warning", "10 Mar 2026 - 31 Dec 2026"),
      ],
      roles: [
        role("1", "Project Manager", "PM", "Coordinate release planning and stakeholder checkpoints.", "Approve release gate decisions.", false, true, false, 1),
        role("2", "Software Architect", "SWA", "Review architecture impact on release baseline.", "Review technical readiness evidence.", true, false, false, 1),
        role("3", "Configuration Manager", "CM", "Maintain configuration items and release integrity.", "Release controlled baseline and release pack.", true, false, true, 1),
        role("4", "Quality Assurance", "QA", "Check process adherence before release.", "Review process compliance evidence.", true, false, false, 1),
        role("5", "Reviewer", "RVW", "Review evidence completeness and quality gaps.", "Review final deliverable pack.", true, false, false, 1),
        role("6", "Approver", "APR", "Authorize release package for controlled use.", "Approve final release package.", false, true, false, 1),
        role("7", "Software Tester", "SWT", "Verify release candidate behavior.", "Review test exit criteria evidence.", true, false, false, 0),
      ],
      complianceChecks: [
        compliance("owner", "project_workspace_prototype.compliance.owner.title", "project_workspace_prototype.compliance.owner.detail", "passed", "overview"),
        compliance("role-gap", "project_workspace_prototype.compliance.role_gap.title", "project_workspace_prototype.compliance.role_gap.detail", "failed", "roles"),
        compliance("org-chart", "project_workspace_prototype.compliance.org_chart.title", "project_workspace_prototype.compliance.org_chart.detail", "passed", "orgChart"),
        compliance("evidence", "project_workspace_prototype.compliance.evidence.title", "project_workspace_prototype.compliance.evidence.detail", "warning", "evidence"),
      ],
      evidenceItems: [
        evidence("1", "Release Gate Pack", "Gate checklist, role sign-offs, and controlled release note.", "13 Mar 2026 12:15", "PDF / XLSX"),
        evidence("2", "Baseline Snapshot", "Frozen release baseline with traceability notes.", "13 Mar 2026 11:00", "ZIP"),
      ],
      requiredDocuments: [
        requiredDocument("1", "RGP", "Release Gate Pack", "PM", "Release", "Ready"),
        requiredDocument("2", "BLS", "Baseline Snapshot", "CM", "Release", "Ready"),
        requiredDocument("3", "TEX", "Test Exit Summary", "SWT", "Verify", "Missing"),
        requiredDocument("4", "FAM", "Final Approval Memo", "APR", "Release", "Draft"),
      ],
      auditTrail: [
        audit("1", "13 Mar 2026 12:20", "Nattawat Sila", "Requested missing test role coverage", "Release Gate Pack", "Flagged absence of an active SWT assignment before final release."),
        audit("2", "13 Mar 2026 11:10", "Tanan Kijprasert", "Published baseline snapshot", "Baseline B4", "Locked current baseline for release review."),
      ],
      orgChart: [
        node("1", "Anan Techakul", "PM", [
          node("2", "Sirin Nanta", "SWA"),
          node("3", "Tanan Kijprasert", "CM"),
          node("4", "Kanya Rattanakul", "QA"),
          node("5", "Pimchanok Deesri", "RVW"),
          node("6", "Nattawat Sila", "APR"),
        ]),
      ],
    },
  },
  compliance_audit: {
    audit_preparation: {
      summaryCards: [
        card("project-type", "blue", "project_workspace_prototype.summary.project_type.title", "Compliance Audit", "project_workspace_prototype.summary.project_type.helper"),
        card("active-members", "green", "project_workspace_prototype.summary.active_members.title", "4", "project_workspace_prototype.summary.active_members.helper"),
        card("active-roles", "gold", "project_workspace_prototype.summary.active_roles.title", "4", "project_workspace_prototype.summary.active_roles.helper"),
        card("reporting-roots", "red", "project_workspace_prototype.summary.reporting_roots.title", "1", "project_workspace_prototype.summary.reporting_roots.helper"),
      ],
      teamMembers: [
        member("1", "Mali Sutham", "mali@example.com", "PM", "Audit Lead", undefined, true, "Active", "01 Apr 2026 - 30 Sep 2026"),
        member("2", "Kanya Rattanakul", "kanya@example.com", "QA", "Quality Assurance", "Mali Sutham", true, "Active", "01 Apr 2026 - 30 Sep 2026"),
        member("3", "Tanan Kijprasert", "tanan@example.com", "CM", "Configuration Manager", "Mali Sutham", false, "Active", "01 Apr 2026 - 30 Sep 2026"),
        member("4", "Preecha Montree", "preecha@example.com", "APR", "Approver", "Mali Sutham", false, "Warning", "15 Apr 2026 - 30 Sep 2026"),
      ],
      roles: [
        role("1", "Audit Lead", "PM", "Lead audit preparation scope and evidence schedule.", "Approve readiness decisions and escalation.", false, true, false, 1),
        role("2", "Quality Assurance", "QA", "Perform compliance verification and process review.", "Review conformance evidence.", true, false, false, 1),
        role("3", "Configuration Manager", "CM", "Control controlled documents and baseline evidence.", "Release approved controlled evidence.", true, false, true, 1),
        role("4", "Approver", "APR", "Provide formal sign-off for audit readiness package.", "Approve final readiness package.", false, true, false, 1),
      ],
      complianceChecks: [
        compliance("owner", "project_workspace_prototype.compliance.owner.title", "project_workspace_prototype.compliance.owner.detail", "passed", "overview"),
        compliance("role-gap", "project_workspace_prototype.compliance.role_gap.title", "project_workspace_prototype.compliance.role_gap.detail", "warning", "roles"),
        compliance("org-chart", "project_workspace_prototype.compliance.org_chart.title", "project_workspace_prototype.compliance.org_chart.detail", "passed", "orgChart"),
        compliance("evidence", "project_workspace_prototype.compliance.evidence.title", "project_workspace_prototype.compliance.evidence.detail", "failed", "evidence"),
      ],
      evidenceItems: [
        evidence("1", "Audit Readiness Checklist", "Role-based readiness checklist for audit cycle.", "14 Mar 2026 09:20", "XLSX / PDF"),
        evidence("2", "Controlled Evidence Index", "Controlled document index for audit pack.", "14 Mar 2026 10:40", "CSV"),
      ],
      requiredDocuments: [
        requiredDocument("1", "ARP", "Audit Readiness Plan", "PM", "Prepare", "Ready"),
        requiredDocument("2", "CEI", "Controlled Evidence Index", "CM", "Collect Evidence", "Ready"),
        requiredDocument("3", "SOM", "Sign-off Matrix", "APR", "Review", "Draft"),
        requiredDocument("4", "FND", "Open Findings Register", "QA", "Close Findings", "Missing"),
      ],
      auditTrail: [
        audit("1", "14 Mar 2026 11:15", "Mali Sutham", "Requested missing sign-off", "Audit Readiness Checklist", "Raised action for missing approval sign-off before audit package freeze."),
        audit("2", "14 Mar 2026 10:45", "Tanan Kijprasert", "Updated evidence index", "Controlled Evidence Index", "Added revised controlled records for latest process update."),
      ],
      orgChart: [
        node("1", "Mali Sutham", "PM", [
          node("2", "Kanya Rattanakul", "QA"),
          node("3", "Tanan Kijprasert", "CM"),
          node("4", "Preecha Montree", "APR"),
        ]),
      ],
    },
    corrective_action: {
      summaryCards: [
        card("project-type", "blue", "project_workspace_prototype.summary.project_type.title", "Compliance Audit", "project_workspace_prototype.summary.project_type.helper"),
        card("active-members", "green", "project_workspace_prototype.summary.active_members.title", "5", "project_workspace_prototype.summary.active_members.helper"),
        card("active-roles", "gold", "project_workspace_prototype.summary.active_roles.title", "5", "project_workspace_prototype.summary.active_roles.helper"),
        card("reporting-roots", "red", "project_workspace_prototype.summary.reporting_roots.title", "1", "project_workspace_prototype.summary.reporting_roots.helper"),
      ],
      teamMembers: [
        member("1", "Mali Sutham", "mali@example.com", "PM", "Corrective Action Lead", undefined, true, "Active", "01 Apr 2026 - 31 Aug 2026"),
        member("2", "Kanya Rattanakul", "kanya@example.com", "QA", "Quality Assurance", "Mali Sutham", true, "Active", "01 Apr 2026 - 31 Aug 2026"),
        member("3", "Preecha Montree", "preecha@example.com", "BA", "Business Analyst", "Mali Sutham", false, "Active", "01 Apr 2026 - 31 Jul 2026"),
        member("4", "Noppon Krailad", "noppon@example.com", "DEV", "Process Implementer", "Mali Sutham", false, "Active", "10 Apr 2026 - 31 Aug 2026"),
        member("5", "Tanan Kijprasert", "tanan@example.com", "CM", "Configuration Manager", "Mali Sutham", false, "Active", "01 Apr 2026 - 31 Aug 2026"),
      ],
      roles: [
        role("1", "Corrective Action Lead", "PM", "Own CAPA plan and closure tracking.", "Approve corrective action closure.", false, true, false, 1),
        role("2", "Quality Assurance", "QA", "Verify corrective action effectiveness.", "Review closure evidence.", true, false, false, 1),
        role("3", "Business Analyst", "BA", "Capture process gaps and remediation scope.", "Prepare gap analysis evidence.", false, false, false, 1),
        role("4", "Process Implementer", "DEV", "Implement agreed process changes.", "Prepare implementation record.", false, false, false, 1),
        role("5", "Configuration Manager", "CM", "Control revised process assets and baselines.", "Release approved revised assets.", true, false, true, 1),
      ],
      complianceChecks: [
        compliance("owner", "project_workspace_prototype.compliance.owner.title", "project_workspace_prototype.compliance.owner.detail", "passed", "overview"),
        compliance("role-gap", "project_workspace_prototype.compliance.role_gap.title", "project_workspace_prototype.compliance.role_gap.detail", "passed", "roles"),
        compliance("org-chart", "project_workspace_prototype.compliance.org_chart.title", "project_workspace_prototype.compliance.org_chart.detail", "passed", "orgChart"),
        compliance("evidence", "project_workspace_prototype.compliance.evidence.title", "project_workspace_prototype.compliance.evidence.detail", "warning", "evidence"),
      ],
      evidenceItems: [
        evidence("1", "Corrective Action Register", "Action owners, dates, and closure evidence.", "14 Mar 2026 15:20", "CSV / PDF"),
        evidence("2", "Updated Process Asset Pack", "Revised SOP, checklist, and implementation notes.", "14 Mar 2026 16:10", "ZIP / PDF"),
      ],
      requiredDocuments: [
        requiredDocument("1", "CAR", "Corrective Action Register", "PM", "Close Findings", "Ready"),
        requiredDocument("2", "GAP", "Gap Analysis Summary", "BA", "Prepare", "Ready"),
        requiredDocument("3", "VER", "Verification Closure Record", "QA", "Review", "Draft"),
        requiredDocument("4", "RPA", "Released Process Assets", "CM", "Close Findings", "Missing"),
      ],
      auditTrail: [
        audit("1", "14 Mar 2026 15:40", "Mali Sutham", "Approved action plan update", "Corrective Action Register", "Accepted revised target dates and remediation owners."),
        audit("2", "14 Mar 2026 16:15", "Tanan Kijprasert", "Released revised process asset pack", "Process Asset Pack", "Published updated controlled process assets after QA review."),
      ],
      orgChart: [
        node("1", "Mali Sutham", "PM", [
          node("2", "Kanya Rattanakul", "QA"),
          node("3", "Preecha Montree", "BA"),
          node("4", "Noppon Krailad", "DEV"),
          node("5", "Tanan Kijprasert", "CM"),
        ]),
      ],
    },
  },
  process_improvement: {
    process_rollout: {
      summaryCards: [
        card("project-type", "blue", "project_workspace_prototype.summary.project_type.title", "Process Improvement", "project_workspace_prototype.summary.project_type.helper"),
        card("active-members", "green", "project_workspace_prototype.summary.active_members.title", "6", "project_workspace_prototype.summary.active_members.helper"),
        card("active-roles", "gold", "project_workspace_prototype.summary.active_roles.title", "6", "project_workspace_prototype.summary.active_roles.helper"),
        card("reporting-roots", "red", "project_workspace_prototype.summary.reporting_roots.title", "1", "project_workspace_prototype.summary.reporting_roots.helper"),
      ],
      teamMembers: [
        member("1", "Metha Chansiri", "metha@example.com", "PM", "Improvement Lead", undefined, true, "Active", "01 May 2026 - 31 Dec 2026"),
        member("2", "Preecha Montree", "preecha@example.com", "BA", "Business Analyst", "Metha Chansiri", true, "Active", "01 May 2026 - 31 Dec 2026"),
        member("3", "Kanya Rattanakul", "kanya@example.com", "QA", "Quality Assurance", "Metha Chansiri", false, "Active", "01 May 2026 - 31 Dec 2026"),
        member("4", "Tanan Kijprasert", "tanan@example.com", "CM", "Configuration Manager", "Metha Chansiri", false, "Active", "01 May 2026 - 31 Dec 2026"),
        member("5", "Nattawat Sila", "nattawat@example.com", "RVW", "Reviewer", "Metha Chansiri", false, "Warning", "15 May 2026 - 31 Dec 2026"),
        member("6", "Sirin Nanta", "sirin@example.com", "APR", "Approver", "Metha Chansiri", false, "Active", "15 May 2026 - 31 Dec 2026"),
      ],
      roles: [
        role("1", "Improvement Lead", "PM", "Own rollout plan and adoption checkpoints.", "Approve rollout planning decisions.", false, true, false, 1),
        role("2", "Business Analyst", "BA", "Capture process needs and change impact.", "Prepare rollout analysis evidence.", false, false, false, 1),
        role("3", "Quality Assurance", "QA", "Review process compliance after rollout.", "Review adherence evidence.", true, false, false, 1),
        role("4", "Configuration Manager", "CM", "Control revised process assets and packages.", "Release approved process assets.", true, false, true, 1),
        role("5", "Reviewer", "RVW", "Review rollout readiness and evidence quality.", "Review pilot evidence pack.", true, false, false, 1),
        role("6", "Approver", "APR", "Approve organization-wide rollout decision.", "Approve release of updated process.", false, true, false, 1),
      ],
      complianceChecks: [
        compliance("owner", "project_workspace_prototype.compliance.owner.title", "project_workspace_prototype.compliance.owner.detail", "passed", "overview"),
        compliance("role-gap", "project_workspace_prototype.compliance.role_gap.title", "project_workspace_prototype.compliance.role_gap.detail", "warning", "roles"),
        compliance("org-chart", "project_workspace_prototype.compliance.org_chart.title", "project_workspace_prototype.compliance.org_chart.detail", "passed", "orgChart"),
        compliance("evidence", "project_workspace_prototype.compliance.evidence.title", "project_workspace_prototype.compliance.evidence.detail", "warning", "evidence"),
      ],
      evidenceItems: [
        evidence("1", "Rollout Readiness Checklist", "Readiness review for process rollout decision.", "15 Mar 2026 10:30", "XLSX / PDF"),
        evidence("2", "Updated Process Release Pack", "Controlled process assets prepared for rollout.", "15 Mar 2026 11:50", "ZIP / PDF"),
      ],
      auditTrail: [
        audit("1", "15 Mar 2026 12:15", "Sirin Nanta", "Approved rollout review pack", "Rollout Readiness Checklist", "Authorized final readiness review for rollout gate."),
        audit("2", "15 Mar 2026 11:55", "Tanan Kijprasert", "Prepared process release pack", "Updated Process Release Pack", "Locked revised process assets for controlled rollout."),
      ],
      orgChart: [
        node("1", "Metha Chansiri", "PM", [
          node("2", "Preecha Montree", "BA"),
          node("3", "Kanya Rattanakul", "QA"),
          node("4", "Tanan Kijprasert", "CM"),
          node("5", "Nattawat Sila", "RVW"),
          node("6", "Sirin Nanta", "APR"),
        ]),
      ],
    },
    pilot_team: {
      summaryCards: [
        card("project-type", "blue", "project_workspace_prototype.summary.project_type.title", "Process Improvement", "project_workspace_prototype.summary.project_type.helper"),
        card("active-members", "green", "project_workspace_prototype.summary.active_members.title", "4", "project_workspace_prototype.summary.active_members.helper"),
        card("active-roles", "gold", "project_workspace_prototype.summary.active_roles.title", "4", "project_workspace_prototype.summary.active_roles.helper"),
        card("reporting-roots", "red", "project_workspace_prototype.summary.reporting_roots.title", "1", "project_workspace_prototype.summary.reporting_roots.helper"),
      ],
      teamMembers: [
        member("1", "Metha Chansiri", "metha@example.com", "PM", "Pilot Lead", undefined, true, "Active", "01 May 2026 - 31 Aug 2026"),
        member("2", "Preecha Montree", "preecha@example.com", "BA", "Business Analyst", "Metha Chansiri", true, "Active", "01 May 2026 - 31 Aug 2026"),
        member("3", "Kanya Rattanakul", "kanya@example.com", "QA", "Quality Assurance", "Metha Chansiri", false, "Active", "01 May 2026 - 31 Aug 2026"),
        member("4", "Tanan Kijprasert", "tanan@example.com", "CM", "Configuration Manager", "Metha Chansiri", false, "Warning", "01 May 2026 - 31 Aug 2026"),
      ],
      roles: [
        role("1", "Pilot Lead", "PM", "Coordinate pilot execution and adoption feedback.", "Approve pilot checkpoints.", false, true, false, 1),
        role("2", "Business Analyst", "BA", "Capture process adoption feedback and issues.", "Prepare pilot findings evidence.", false, false, false, 1),
        role("3", "Quality Assurance", "QA", "Review pilot compliance behavior.", "Review pilot evidence.", true, false, false, 1),
        role("4", "Configuration Manager", "CM", "Control pilot version of process assets.", "Release controlled pilot artifacts.", true, false, true, 1),
      ],
      complianceChecks: [
        compliance("owner", "project_workspace_prototype.compliance.owner.title", "project_workspace_prototype.compliance.owner.detail", "passed", "overview"),
        compliance("role-gap", "project_workspace_prototype.compliance.role_gap.title", "project_workspace_prototype.compliance.role_gap.detail", "warning", "roles"),
        compliance("org-chart", "project_workspace_prototype.compliance.org_chart.title", "project_workspace_prototype.compliance.org_chart.detail", "passed", "orgChart"),
        compliance("evidence", "project_workspace_prototype.compliance.evidence.title", "project_workspace_prototype.compliance.evidence.detail", "warning", "evidence"),
      ],
      evidenceItems: [
        evidence("1", "Pilot Findings Register", "Findings, actions, and owners from pilot execution.", "15 Mar 2026 15:10", "CSV / PDF"),
        evidence("2", "Pilot Asset Pack", "Pilot-specific controlled assets and notes.", "15 Mar 2026 16:25", "ZIP"),
      ],
      auditTrail: [
        audit("1", "15 Mar 2026 15:20", "Preecha Montree", "Updated pilot findings", "Pilot Findings Register", "Captured latest pilot adoption issues and actions."),
        audit("2", "15 Mar 2026 16:30", "Tanan Kijprasert", "Released pilot asset pack", "Pilot Asset Pack", "Published pilot package for review."),
      ],
      orgChart: [
        node("1", "Metha Chansiri", "PM", [
          node("2", "Preecha Montree", "BA"),
          node("3", "Kanya Rattanakul", "QA"),
          node("4", "Tanan Kijprasert", "CM"),
        ]),
      ],
    },
  },
};

const defaultTemplateId = templateOptions[0]!.id;

const governanceByTemplate: Record<
  string,
  {
    requiredDocuments: ProjectWorkspacePrototypeRequiredDocument[];
    approvalSteps: ProjectWorkspacePrototypeApprovalStep[];
    roleDependencies: ProjectWorkspacePrototypeRoleDependency[];
  }
> = {
  software_delivery: {
    requiredDocuments: [
      requiredDocument("1", "SDP", "Software Development Plan", "PM", "Plan", "Ready"),
      requiredDocument("2", "SAD", "Software Architecture Description", "SWA", "Build", "Draft"),
      requiredDocument("3", "SVR", "Software Verification Report", "SWT", "Verify", "Ready"),
      requiredDocument("4", "REL", "Release Package", "CM", "Release", "Missing"),
    ],
    approvalSteps: [
      approvalStep("1", 1, "SWA", "Software Architect", "Technical Review", "Reviewed architecture baseline"),
      approvalStep("2", 2, "SWT", "Software Tester", "Verification Review", "Validated quality evidence"),
      approvalStep("3", 3, "APR", "Approver", "Final Approval", "Approved release decision"),
      approvalStep("4", 4, "CM", "Configuration Manager", "Controlled Release", "Released controlled package"),
    ],
    roleDependencies: [
      roleDependency("1", "SWA", "DEV", "Design direction", "Implementation should align with the architecture baseline set by SWA."),
      roleDependency("2", "SWT", "SWA", "Feedback loop", "Verification findings should feed back into architecture and technical decisions."),
      roleDependency("3", "CM", "APR", "Release gate", "CM should release only after APR has approved the package."),
    ],
  },
  compliance_audit: {
    requiredDocuments: [
      requiredDocument("1", "ARP", "Audit Readiness Plan", "PM", "Prepare", "Ready"),
      requiredDocument("2", "CEI", "Controlled Evidence Index", "CM", "Collect Evidence", "Ready"),
      requiredDocument("3", "QAR", "Quality Assurance Review Record", "QA", "Review", "Draft"),
      requiredDocument("4", "CAR", "Corrective Action Register", "PM", "Close Findings", "Missing"),
    ],
    approvalSteps: [
      approvalStep("1", 1, "QA", "Quality Assurance", "Evidence Review", "Reviewed conformance evidence"),
      approvalStep("2", 2, "RVW", "Reviewer", "Readiness Review", "Confirmed audit pack readiness"),
      approvalStep("3", 3, "APR", "Approver", "Formal Approval", "Approved audit submission pack"),
      approvalStep("4", 4, "CM", "Configuration Manager", "Controlled Publish", "Published controlled audit package"),
    ],
    roleDependencies: [
      roleDependency("1", "QA", "CM", "Controlled evidence", "QA review depends on CM maintaining controlled evidence records."),
      roleDependency("2", "RVW", "QA", "Challenge review", "Reviewer should challenge QA readiness before formal approval."),
      roleDependency("3", "APR", "RVW", "Formal sign-off", "APR should approve only after reviewer confirmation is complete."),
    ],
  },
  process_improvement: {
    requiredDocuments: [
      requiredDocument("1", "PAP", "Process Assessment Pack", "BA", "Assess", "Ready"),
      requiredDocument("2", "PID", "Process Improvement Design", "PM", "Design", "Draft"),
      requiredDocument("3", "PIL", "Pilot Findings Register", "QA", "Pilot", "Ready"),
      requiredDocument("4", "RLP", "Rollout Package", "CM", "Rollout", "Missing"),
    ],
    approvalSteps: [
      approvalStep("1", 1, "BA", "Business Analyst", "Impact Review", "Reviewed improvement scope"),
      approvalStep("2", 2, "QA", "Quality Assurance", "Process Review", "Reviewed pilot evidence"),
      approvalStep("3", 3, "APR", "Approver", "Rollout Approval", "Approved process rollout"),
      approvalStep("4", 4, "CM", "Configuration Manager", "Controlled Release", "Released updated process assets"),
    ],
    roleDependencies: [
      roleDependency("1", "BA", "PM", "Scope shaping", "BA findings should shape PM rollout decisions."),
      roleDependency("2", "QA", "BA", "Validation loop", "QA validates whether the designed process solves the identified gap."),
      roleDependency("3", "CM", "APR", "Controlled rollout", "CM should release revised assets only after APR approves rollout."),
    ],
  },
};

function getScenarioOptions(templateId: string) {
  return scenarioOptionsByTemplate[templateId] ?? [];
}

function getDefaultScenarioId(templateId: string) {
  return getScenarioOptions(templateId)[0]?.id ?? "";
}

export function useProjectWorkspacePrototype(initialTemplateId?: string) {
  const [section, setSection] = useState<ProjectWorkspacePrototypeSection>("overview");
  const [templateId, setTemplateId] = useState(initialTemplateId ?? defaultTemplateId);
  const [scenarioId, setScenarioId] = useState(getDefaultScenarioId(initialTemplateId ?? defaultTemplateId));

  const scenarioOptions = useMemo(() => getScenarioOptions(templateId), [templateId]);

  useEffect(() => {
    const available = getScenarioOptions(templateId);
    if (!available.some((option) => option.id === scenarioId)) {
      setScenarioId(getDefaultScenarioId(templateId));
    }
  }, [scenarioId, templateId]);

  const dataset = useMemo(() => {
    const templateDatasets = datasets[templateId] ?? datasets[defaultTemplateId];
    return templateDatasets?.[scenarioId] ?? templateDatasets?.[getDefaultScenarioId(templateId)] ?? datasets[defaultTemplateId]![getDefaultScenarioId(defaultTemplateId)]!;
  }, [scenarioId, templateId]);

  const governance = useMemo(
    () => governanceByTemplate[templateId] ?? governanceByTemplate[defaultTemplateId]!,
    [templateId],
  );

  return {
    section,
    setSection,
    templateId,
    setTemplateId,
    scenarioId,
    setScenarioId,
    templateOptions,
    scenarioOptions,
    quickActions,
    summaryCards: dataset.summaryCards,
    teamMembers: dataset.teamMembers,
    roles: dataset.roles,
    complianceChecks: dataset.complianceChecks,
    evidenceItems: dataset.evidenceItems,
    auditTrail: dataset.auditTrail,
    orgChart: dataset.orgChart,
    requiredDocuments: dataset.requiredDocuments ?? governance.requiredDocuments,
    approvalSteps: dataset.approvalSteps ?? governance.approvalSteps,
    roleDependencies: dataset.roleDependencies ?? governance.roleDependencies,
  };
}
