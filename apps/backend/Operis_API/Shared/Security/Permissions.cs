namespace Operis_API.Shared.Security;

public static class Permissions
{
    public static class Admin
    {
        public const string PermissionMatrixRead = "admin.permission_matrix.read";
        public const string PermissionMatrixApply = "admin.permission_matrix.apply";
        public const string SettingsRead = "admin.settings.read";
        public const string SettingsManage = "admin.settings.manage";
    }

    public static class Users
    {
        public const string Read = "users.read";
        public const string Create = "users.create";
        public const string Update = "users.update";
        public const string Delete = "users.delete";
        public const string Invite = "users.invite";
        public const string ReviewRegistrations = "users.registrations.review";
    }

    public static class MasterData
    {
        public const string Read = "master_data.read";
        public const string ManagePermanentOrg = "master_data.permanent_org.manage";
        public const string ManageProjectStructures = "master_data.project_structures.manage";
    }

    public static class Projects
    {
        public const string Read = "projects.read";
        public const string Manage = "projects.manage";
        public const string ManageRoles = "projects.roles.manage";
        public const string ManageMembers = "projects.members.manage";
        public const string ReadEvidence = "projects.evidence.read";
        public const string ExportEvidence = "projects.evidence.export";
        public const string ReadCompliance = "projects.compliance.read";
        public const string ManageTemplates = "projects.templates.manage";
    }

    public static class AuditLogs
    {
        public const string Read = "audit_logs.read";
        public const string Export = "audit_logs.export";
    }

    public static class ActivityLogs
    {
        public const string Read = "activity_logs.read";
        public const string Export = "activity_logs.export";
    }

    public static class Documents
    {
        public const string Read = "documents.read";
        public const string Upload = "documents.upload";
        public const string ManageVersions = "documents.versions.manage";
        public const string Publish = "documents.publish";
        public const string DeleteDraft = "documents.draft.delete";
        public const string Deactivate = "documents.deactivate";
    }

    public static class Workflows
    {
        public const string Read = "workflows.read";
        public const string ManageDefinitions = "workflows.definitions.manage";
    }

    public static class Notifications
    {
        public const string Read = "notifications.read";
    }

    public static readonly IReadOnlyList<string> All =
    [
        Admin.PermissionMatrixRead,
        Admin.PermissionMatrixApply,
        Admin.SettingsRead,
        Admin.SettingsManage,
        Users.Read,
        Users.Create,
        Users.Update,
        Users.Delete,
        Users.Invite,
        Users.ReviewRegistrations,
        MasterData.Read,
        MasterData.ManagePermanentOrg,
        MasterData.ManageProjectStructures,
        Projects.Read,
        Projects.Manage,
        Projects.ManageRoles,
        Projects.ManageMembers,
        Projects.ReadEvidence,
        Projects.ExportEvidence,
        Projects.ReadCompliance,
        Projects.ManageTemplates,
        ActivityLogs.Read,
        ActivityLogs.Export,
        AuditLogs.Read,
        AuditLogs.Export,
        Documents.Read,
        Documents.Upload,
        Documents.ManageVersions,
        Documents.Publish,
        Documents.DeleteDraft,
        Documents.Deactivate,
        Workflows.Read,
        Workflows.ManageDefinitions,
        Notifications.Read
    ];

    public static string GetDisplayName(string permission) =>
        permission switch
        {
            Admin.PermissionMatrixRead => "Read Permission Matrix",
            Admin.PermissionMatrixApply => "Apply Permission Matrix",
            Admin.SettingsRead => "Read System Settings",
            Admin.SettingsManage => "Manage System Settings",
            Users.Read => "Read Users",
            Users.Create => "Create Users",
            Users.Update => "Update Users",
            Users.Delete => "Delete Users",
            Users.Invite => "Invite Users",
            Users.ReviewRegistrations => "Review Registrations",
            MasterData.Read => "Read Master Data",
            MasterData.ManagePermanentOrg => "Manage Permanent Org",
            MasterData.ManageProjectStructures => "Manage Project Structures",
            Projects.Read => "Read Projects",
            Projects.Manage => "Manage Projects",
            Projects.ManageRoles => "Manage Project Roles",
            Projects.ManageMembers => "Manage Project Members",
            Projects.ReadEvidence => "Read Project Evidence",
            Projects.ExportEvidence => "Export Project Evidence",
            Projects.ReadCompliance => "Read Project Compliance",
            Projects.ManageTemplates => "Manage Project Templates",
            ActivityLogs.Read => "Read Activity Logs",
            ActivityLogs.Export => "Export Activity Logs",
            AuditLogs.Read => "Read Audit Logs",
            AuditLogs.Export => "Export Audit Logs",
            Documents.Read => "Read Documents",
            Documents.Upload => "Upload Documents",
            Documents.ManageVersions => "Manage Document Versions",
            Documents.Publish => "Publish Documents",
            Documents.DeleteDraft => "Delete Document Drafts",
            Documents.Deactivate => "Deactivate Documents",
            Workflows.Read => "Read Workflows",
            Workflows.ManageDefinitions => "Manage Workflow Definitions",
            Notifications.Read => "Read Notifications",
            _ => permission
        };
}
