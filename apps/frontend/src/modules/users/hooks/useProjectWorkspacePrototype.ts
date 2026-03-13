import { useMemo, useState } from "react";
import type {
  ProjectWorkspacePrototypeAuditEvent,
  ProjectWorkspacePrototypeComplianceCheck,
  ProjectWorkspacePrototypeEvidenceItem,
  ProjectWorkspacePrototypeMember,
  ProjectWorkspacePrototypeOrgNode,
  ProjectWorkspacePrototypeQuickAction,
  ProjectWorkspacePrototypeRole,
  ProjectWorkspacePrototypeSection,
  ProjectWorkspacePrototypeSummaryCard,
} from "../types/projectWorkspacePrototype";

export function useProjectWorkspacePrototype() {
  const [section, setSection] = useState<ProjectWorkspacePrototypeSection>("overview");

  const summaryCards: ProjectWorkspacePrototypeSummaryCard[] = useMemo(
    () => [
      {
        id: "readiness",
        tone: "blue",
        titleKey: "project_workspace_prototype.summary.readiness.title",
        value: "82%",
        helperKey: "project_workspace_prototype.summary.readiness.helper",
      },
      {
        id: "members",
        tone: "green",
        titleKey: "project_workspace_prototype.summary.members.title",
        value: "8",
        helperKey: "project_workspace_prototype.summary.members.helper",
      },
      {
        id: "gaps",
        tone: "gold",
        titleKey: "project_workspace_prototype.summary.gaps.title",
        value: "3",
        helperKey: "project_workspace_prototype.summary.gaps.helper",
      },
      {
        id: "evidence",
        tone: "red",
        titleKey: "project_workspace_prototype.summary.evidence.title",
        value: "2",
        helperKey: "project_workspace_prototype.summary.evidence.helper",
      },
    ],
    [],
  );

  const quickActions: ProjectWorkspacePrototypeQuickAction[] = useMemo(
    () => [
      {
        id: "missing-reviewer",
        labelKey: "project_workspace_prototype.quick_actions.missing_reviewer",
        targetSection: "roles",
      },
      {
        id: "org-chart",
        labelKey: "project_workspace_prototype.quick_actions.org_chart",
        targetSection: "orgChart",
      },
      {
        id: "evidence",
        labelKey: "project_workspace_prototype.quick_actions.evidence",
        targetSection: "evidence",
      },
      {
        id: "audit",
        labelKey: "project_workspace_prototype.quick_actions.audit",
        targetSection: "auditTrail",
      },
    ],
    [],
  );

  const teamMembers: ProjectWorkspacePrototypeMember[] = useMemo(
    () => [
      {
        id: "1",
        name: "Nuttapon Suksa-ard",
        email: "nuttapon@example.com",
        role: "Project Manager",
        primary: true,
        status: "Active",
        period: "01 Mar 2026 - 31 Dec 2026",
      },
      {
        id: "2",
        name: "Kanya Preechawat",
        email: "kanya@example.com",
        role: "QA Lead",
        reportsTo: "Nuttapon Suksa-ard",
        primary: true,
        status: "Active",
        period: "01 Mar 2026 - 31 Dec 2026",
      },
      {
        id: "3",
        name: "Somsak Chuenjit",
        email: "somsak@example.com",
        role: "Configuration Manager",
        reportsTo: "Nuttapon Suksa-ard",
        primary: true,
        status: "Active",
        period: "01 Mar 2026 - 31 Dec 2026",
      },
      {
        id: "4",
        name: "Ploy Daosri",
        email: "ploy@example.com",
        role: "Business Analyst",
        reportsTo: "Kanya Preechawat",
        primary: false,
        status: "Warning",
        period: "15 Mar 2026 - 31 Dec 2026",
      },
    ],
    [],
  );

  const roles: ProjectWorkspacePrototypeRole[] = useMemo(
    () => [
      {
        id: "1",
        name: "Project Manager",
        code: "PM",
        responsibility: "Own delivery, governance, and approval coordination.",
        authority: "Approve plan changes and assign project members.",
        review: false,
        approval: true,
        release: true,
        memberCount: 1,
      },
      {
        id: "2",
        name: "QA Lead",
        code: "QAL",
        responsibility: "Review process adherence and release readiness.",
        authority: "Review deliverables and raise compliance findings.",
        review: true,
        approval: false,
        release: false,
        memberCount: 1,
      },
      {
        id: "3",
        name: "Configuration Manager",
        code: "CM",
        responsibility: "Control baseline, controlled documents, and release records.",
        authority: "Release controlled documents and maintain baselines.",
        review: true,
        approval: false,
        release: true,
        memberCount: 1,
      },
      {
        id: "4",
        name: "Business Analyst",
        code: "BA",
        responsibility: "Own requirement elicitation and change clarification.",
        authority: "Prepare change impact notes and update requirement artifacts.",
        review: false,
        approval: false,
        release: false,
        memberCount: 1,
      },
    ],
    [],
  );

  const complianceChecks: ProjectWorkspacePrototypeComplianceCheck[] = useMemo(
    () => [
      {
        id: "owner",
        titleKey: "project_workspace_prototype.compliance.owner.title",
        detailKey: "project_workspace_prototype.compliance.owner.detail",
        severity: "passed",
        targetSection: "overview",
      },
      {
        id: "reviewer",
        titleKey: "project_workspace_prototype.compliance.reviewer.title",
        detailKey: "project_workspace_prototype.compliance.reviewer.detail",
        severity: "warning",
        targetSection: "roles",
      },
      {
        id: "org-chart",
        titleKey: "project_workspace_prototype.compliance.org_chart.title",
        detailKey: "project_workspace_prototype.compliance.org_chart.detail",
        severity: "passed",
        targetSection: "orgChart",
      },
      {
        id: "evidence",
        titleKey: "project_workspace_prototype.compliance.evidence.title",
        detailKey: "project_workspace_prototype.compliance.evidence.detail",
        severity: "failed",
        targetSection: "evidence",
      },
    ],
    [],
  );

  const evidenceItems: ProjectWorkspacePrototypeEvidenceItem[] = useMemo(
    () => [
      {
        id: "1",
        title: "Project Team Register",
        description: "Current team structure and role coverage for the project.",
        lastUpdated: "12 Mar 2026 14:40",
        format: "CSV / PDF",
      },
      {
        id: "2",
        title: "Role Responsibility Matrix",
        description: "Role authority, review points, and approval capability summary.",
        lastUpdated: "12 Mar 2026 13:10",
        format: "CSV",
      },
      {
        id: "3",
        title: "Assignment History Snapshot",
        description: "Historical changes of project assignments with reasons.",
        lastUpdated: "11 Mar 2026 09:20",
        format: "CSV / ZIP",
      },
    ],
    [],
  );

  const auditTrail: ProjectWorkspacePrototypeAuditEvent[] = useMemo(
    () => [
      {
        id: "1",
        timestamp: "12 Mar 2026 14:32",
        actor: "Nuttapon Suksa-ard",
        action: "Updated project role",
        target: "QA Lead",
        detail: "Enabled review responsibility for release readiness.",
      },
      {
        id: "2",
        timestamp: "12 Mar 2026 13:45",
        actor: "Kanya Preechawat",
        action: "Exported evidence",
        target: "Project Team Register",
        detail: "Downloaded current CSV package for internal review.",
      },
      {
        id: "3",
        timestamp: "11 Mar 2026 17:05",
        actor: "System Admin",
        action: "Assigned member",
        target: "Ploy Daosri",
        detail: "Assigned to Business Analyst role with reporting line to QA Lead.",
      },
    ],
    [],
  );

  const orgChart: ProjectWorkspacePrototypeOrgNode[] = useMemo(
    () => [
      {
        id: "1",
        label: "Nuttapon Suksa-ard",
        subtitle: "Project Manager",
        children: [
          {
            id: "2",
            label: "Kanya Preechawat",
            subtitle: "QA Lead",
            children: [
              {
                id: "4",
                label: "Ploy Daosri",
                subtitle: "Business Analyst",
              },
            ],
          },
          {
            id: "3",
            label: "Somsak Chuenjit",
            subtitle: "Configuration Manager",
          },
        ],
      },
    ],
    [],
  );

  return {
    section,
    setSection,
    summaryCards,
    quickActions,
    teamMembers,
    roles,
    complianceChecks,
    evidenceItems,
    auditTrail,
    orgChart,
  };
}
