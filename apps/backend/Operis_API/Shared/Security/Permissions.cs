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

    public static class Governance
    {
        public const string ProcessLibraryRead = "governance.process_library.read";
        public const string ProcessLibraryManage = "governance.process_library.manage";
        public const string QaChecklistRead = "governance.qa_checklist.read";
        public const string QaChecklistManage = "governance.qa_checklist.manage";
        public const string ProjectPlanRead = "governance.project_plan.read";
        public const string ProjectPlanManage = "governance.project_plan.manage";
        public const string ProjectPlanApprove = "governance.project_plan.approve";
        public const string StakeholderRead = "governance.stakeholder.read";
        public const string StakeholderManage = "governance.stakeholder.manage";
        public const string TailoringRead = "governance.tailoring.read";
        public const string TailoringManage = "governance.tailoring.manage";
        public const string TailoringApprove = "governance.tailoring.approve";
    }

    public static class Requirements
    {
        public const string Read = "requirements.read";
        public const string Manage = "requirements.manage";
        public const string Approve = "requirements.approve";
        public const string Baseline = "requirements.baseline";
        public const string ManageTraceability = "requirements.traceability.manage";
    }

    public static class ChangeControl
    {
        public const string Read = "change_control.read";
        public const string Manage = "change_control.manage";
        public const string Approve = "change_control.approve";
        public const string ManageConfiguration = "change_control.configuration.manage";
        public const string ReadConfiguration = "change_control.configuration.read";
        public const string ManageBaselines = "change_control.baselines.manage";
        public const string ApproveBaselines = "change_control.baselines.approve";
        public const string EmergencyOverride = "change_control.emergency_override";
    }

    public static class Risks
    {
        public const string Read = "risks.read";
        public const string Manage = "risks.manage";
        public const string ReadSensitive = "risks.sensitive.read";
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
        Governance.ProcessLibraryRead,
        Governance.ProcessLibraryManage,
        Governance.QaChecklistRead,
        Governance.QaChecklistManage,
        Governance.ProjectPlanRead,
        Governance.ProjectPlanManage,
        Governance.ProjectPlanApprove,
        Governance.StakeholderRead,
        Governance.StakeholderManage,
        Governance.TailoringRead,
        Governance.TailoringManage,
        Governance.TailoringApprove,
        Requirements.Read,
        Requirements.Manage,
        Requirements.Approve,
        Requirements.Baseline,
        Requirements.ManageTraceability,
        ChangeControl.Read,
        ChangeControl.Manage,
        ChangeControl.Approve,
        ChangeControl.ReadConfiguration,
        ChangeControl.ManageConfiguration,
        ChangeControl.ManageBaselines,
        ChangeControl.ApproveBaselines,
        ChangeControl.EmergencyOverride,
        Risks.Read,
        Risks.Manage,
        Risks.ReadSensitive,
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
            Governance.ProcessLibraryRead => "Read Process Library",
            Governance.ProcessLibraryManage => "Manage Process Library",
            Governance.QaChecklistRead => "Read QA Checklists",
            Governance.QaChecklistManage => "Manage QA Checklists",
            Governance.ProjectPlanRead => "Read Project Plans",
            Governance.ProjectPlanManage => "Manage Project Plans",
            Governance.ProjectPlanApprove => "Approve Project Plans",
            Governance.StakeholderRead => "Read Stakeholders",
            Governance.StakeholderManage => "Manage Stakeholders",
            Governance.TailoringRead => "Read Tailoring Records",
            Governance.TailoringManage => "Manage Tailoring Records",
            Governance.TailoringApprove => "Approve Tailoring Records",
            Requirements.Read => "Read Requirements",
            Requirements.Manage => "Manage Requirements",
            Requirements.Approve => "Approve Requirements",
            Requirements.Baseline => "Baseline Requirements",
            Requirements.ManageTraceability => "Manage Requirement Traceability",
            ChangeControl.Read => "Read Change Requests",
            ChangeControl.Manage => "Manage Change Requests",
            ChangeControl.Approve => "Approve Change Requests",
            ChangeControl.ReadConfiguration => "Read Configuration Items",
            ChangeControl.ManageConfiguration => "Manage Configuration Items",
            ChangeControl.ManageBaselines => "Manage Baselines",
            ChangeControl.ApproveBaselines => "Approve Baselines",
            ChangeControl.EmergencyOverride => "Emergency Override Change Control",
            Risks.Read => "Read Risks and Issues",
            Risks.Manage => "Manage Risks and Issues",
            Risks.ReadSensitive => "Read Sensitive Issues",
            Notifications.Read => "Read Notifications",
            _ => permission
        };
}
