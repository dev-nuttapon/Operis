const allPermissions = [
  "users.read",
  "users.create",
  "users.update",
  "users.delete",
  "users.invite",
  "users.registrations.review",
  "master_data.read",
  "master_data.permanent_org.manage",
  "master_data.project_structures.manage",
  "projects.read",
  "projects.manage",
  "projects.roles.manage",
  "projects.members.manage",
  "projects.evidence.read",
  "projects.evidence.export",
  "projects.compliance.read",
  "projects.templates.manage",
  "activity_logs.read",
  "activity_logs.export",
  "audit_logs.read",
  "audit_logs.export",
  "documents.read",
  "workflows.read",
  "workflows.definitions.manage",
] as const;

export const permissions = {
  users: {
    read: "users.read",
    create: "users.create",
    update: "users.update",
    delete: "users.delete",
    invite: "users.invite",
    reviewRegistrations: "users.registrations.review",
  },
  masterData: {
    read: "master_data.read",
    managePermanentOrg: "master_data.permanent_org.manage",
    manageProjectStructures: "master_data.project_structures.manage",
  },
  projects: {
    read: "projects.read",
    manage: "projects.manage",
    manageRoles: "projects.roles.manage",
    manageMembers: "projects.members.manage",
    readEvidence: "projects.evidence.read",
    exportEvidence: "projects.evidence.export",
    readCompliance: "projects.compliance.read",
    manageTemplates: "projects.templates.manage",
  },
  activityLogs: {
    read: "activity_logs.read",
    export: "activity_logs.export",
  },
  auditLogs: {
    read: "audit_logs.read",
    export: "audit_logs.export",
  },
  documents: {
    read: "documents.read",
  },
  workflows: {
    read: "workflows.read",
    manageDefinitions: "workflows.definitions.manage",
  },
} as const;

export type Permission = (typeof allPermissions)[number];

const rolePermissionMap: Record<string, readonly Permission[]> = {
  "operis:super_admin": allPermissions,
  operis_super_admin: allPermissions,
  "operis:system_admin": [
    permissions.users.read,
    permissions.users.create,
    permissions.users.update,
    permissions.users.delete,
    permissions.users.invite,
    permissions.users.reviewRegistrations,
    permissions.masterData.read,
    permissions.masterData.managePermanentOrg,
    permissions.masterData.manageProjectStructures,
    permissions.projects.read,
    permissions.projects.manage,
    permissions.projects.manageRoles,
    permissions.projects.manageMembers,
    permissions.projects.readEvidence,
    permissions.projects.exportEvidence,
    permissions.projects.readCompliance,
    permissions.projects.manageTemplates,
    permissions.activityLogs.read,
    permissions.activityLogs.export,
    permissions.auditLogs.read,
    permissions.auditLogs.export,
    permissions.documents.read,
    permissions.workflows.read,
    permissions.workflows.manageDefinitions,
  ],
  operis_system_admin: [
    permissions.users.read,
    permissions.users.create,
    permissions.users.update,
    permissions.users.delete,
    permissions.users.invite,
    permissions.users.reviewRegistrations,
    permissions.masterData.read,
    permissions.masterData.managePermanentOrg,
    permissions.masterData.manageProjectStructures,
    permissions.projects.read,
    permissions.projects.manage,
    permissions.projects.manageRoles,
    permissions.projects.manageMembers,
    permissions.projects.readEvidence,
    permissions.projects.exportEvidence,
    permissions.projects.readCompliance,
    permissions.projects.manageTemplates,
    permissions.activityLogs.read,
    permissions.activityLogs.export,
    permissions.auditLogs.read,
    permissions.auditLogs.export,
    permissions.documents.read,
    permissions.workflows.read,
    permissions.workflows.manageDefinitions,
  ],
  "operis:audit_auditor": [
    permissions.activityLogs.read,
    permissions.activityLogs.export,
    permissions.auditLogs.read,
    permissions.auditLogs.export,
    permissions.projects.readEvidence,
    permissions.projects.readCompliance,
    permissions.documents.read,
    permissions.workflows.read,
  ],
  "operis:documents_owner": [permissions.documents.read],
  "operis:documents_reviewer": [permissions.documents.read],
  "operis:workflows_approver": [permissions.workflows.read],
  "operis:workflows_department_manager": [permissions.workflows.read],
  "operis:employee_viewer": [permissions.documents.read, permissions.workflows.read],
  "operis:ops_support": [permissions.activityLogs.read],
};

export function getPermissionsForRoles(roles: string[]): Permission[] {
  const resolved = new Set<Permission>();
  for (const role of roles) {
    for (const permission of rolePermissionMap[role] ?? []) {
      resolved.add(permission);
    }
  }

  return Array.from(resolved);
}
