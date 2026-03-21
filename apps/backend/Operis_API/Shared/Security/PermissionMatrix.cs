using System.Security.Claims;

namespace Operis_API.Shared.Security;

public sealed class PermissionMatrix : IPermissionMatrix
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> RolePermissions =
        new Dictionary<string, IReadOnlyList<string>>(Comparer)
        {
            ["operis:super_admin"] = Permissions.All,
            ["operis_super_admin"] = Permissions.All,
            ["operis:system_admin"] =
            [
                Permissions.Users.Read,
                Permissions.Users.Create,
                Permissions.Users.Update,
                Permissions.Users.Delete,
                Permissions.Users.Invite,
                Permissions.Users.ReviewRegistrations,
                Permissions.MasterData.Read,
                Permissions.MasterData.ManagePermanentOrg,
                Permissions.MasterData.ManageProjectStructures,
                Permissions.Projects.Read,
                Permissions.Projects.Manage,
                Permissions.Projects.ManageRoles,
                Permissions.Projects.ManageMembers,
                Permissions.Projects.ReadEvidence,
                Permissions.Projects.ExportEvidence,
                Permissions.Projects.ReadCompliance,
                Permissions.Projects.ManageTemplates,
                Permissions.ActivityLogs.Read,
                Permissions.ActivityLogs.Export,
                Permissions.AuditLogs.Read,
                Permissions.AuditLogs.Export,
                Permissions.Documents.Read,
                Permissions.Documents.Upload,
                Permissions.Documents.ManageVersions,
                Permissions.Documents.Publish,
                Permissions.Documents.DeleteDraft,
                Permissions.Documents.Deactivate,
                Permissions.Workflows.Read,
                Permissions.Workflows.ManageDefinitions,
                Permissions.Notifications.Read
            ],
            ["operis_system_admin"] =
            [
                Permissions.Users.Read,
                Permissions.Users.Create,
                Permissions.Users.Update,
                Permissions.Users.Delete,
                Permissions.Users.Invite,
                Permissions.Users.ReviewRegistrations,
                Permissions.MasterData.Read,
                Permissions.MasterData.ManagePermanentOrg,
                Permissions.MasterData.ManageProjectStructures,
                Permissions.Projects.Read,
                Permissions.Projects.Manage,
                Permissions.Projects.ManageRoles,
                Permissions.Projects.ManageMembers,
                Permissions.Projects.ReadEvidence,
                Permissions.Projects.ExportEvidence,
                Permissions.Projects.ReadCompliance,
                Permissions.Projects.ManageTemplates,
                Permissions.ActivityLogs.Read,
                Permissions.ActivityLogs.Export,
                Permissions.AuditLogs.Read,
                Permissions.AuditLogs.Export,
                Permissions.Documents.Read,
                Permissions.Workflows.Read,
                Permissions.Workflows.ManageDefinitions,
                Permissions.Notifications.Read
            ],
            ["operis:audit_auditor"] =
            [
                Permissions.ActivityLogs.Read,
                Permissions.ActivityLogs.Export,
                Permissions.AuditLogs.Read,
                Permissions.AuditLogs.Export,
                Permissions.Projects.ReadEvidence,
                Permissions.Projects.ReadCompliance,
                Permissions.Documents.Read,
                Permissions.Workflows.Read,
                Permissions.Notifications.Read
            ],
            ["operis:documents_owner"] =
            [
                Permissions.Documents.Read,
                Permissions.Documents.Upload,
                Permissions.Documents.ManageVersions,
                Permissions.Documents.Publish,
                Permissions.Documents.DeleteDraft,
                Permissions.Documents.Deactivate,
                Permissions.Notifications.Read
            ],
            ["operis:documents_reviewer"] = [Permissions.Documents.Read, Permissions.Notifications.Read],
            ["operis:workflows_approver"] = [Permissions.Workflows.Read, Permissions.Notifications.Read],
            ["operis:workflows_department_manager"] = [Permissions.Workflows.Read, Permissions.Notifications.Read],
            ["operis:employee_viewer"] = [Permissions.Workflows.Read, Permissions.Notifications.Read],
            ["operis:ops_support"] = [Permissions.ActivityLogs.Read, Permissions.Notifications.Read]
        };

    public IReadOnlyList<string> GetPermissions(IEnumerable<string> roles)
    {
        var permissions = new HashSet<string>(Comparer);
        foreach (var role in roles)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                continue;
            }

            if (!RolePermissions.TryGetValue(role, out var mappedPermissions))
            {
                continue;
            }

            foreach (var permission in mappedPermissions)
            {
                permissions.Add(permission);
            }
        }

        return permissions.ToArray();
    }

    public bool HasPermission(ClaimsPrincipal? principal, string permission)
    {
        if (principal is null || string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        var roles = principal.Claims
            .Where(claim => claim.Type == ClaimTypes.Role)
            .Select(claim => claim.Value);

        return GetPermissions(roles).Contains(permission, Comparer);
    }

    public bool HasAnyPermission(ClaimsPrincipal? principal, params string[] permissions)
    {
        var requested = permissions.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray();
        if (requested.Length == 0)
        {
            return false;
        }

        return requested.Any(permission => HasPermission(principal, permission));
    }
}
