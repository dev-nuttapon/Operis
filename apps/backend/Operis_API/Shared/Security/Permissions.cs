namespace Operis_API.Shared.Security;

public static class Permissions
{
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

    public static class Documents
    {
        public const string Read = "documents.read";
    }

    public static class Workflows
    {
        public const string Read = "workflows.read";
        public const string ManageDefinitions = "workflows.definitions.manage";
    }

    public static readonly IReadOnlyList<string> All =
    [
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
        AuditLogs.Read,
        AuditLogs.Export,
        Documents.Read,
        Workflows.Read,
        Workflows.ManageDefinitions
    ];
}
