using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Infrastructure;

namespace Operis_API.Shared.Security;

public sealed class PermissionMatrix(OperisDbContext? dbContext = null) : IPermissionMatrix
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> DefaultRolePermissions =
        new Dictionary<string, IReadOnlyList<string>>(Comparer)
        {
            ["operis:super_admin"] = Permissions.All,
            ["operis_super_admin"] = Permissions.All,
            ["operis:system_admin"] =
            [
                Permissions.Admin.PermissionMatrixRead,
                Permissions.Admin.PermissionMatrixApply,
                Permissions.Admin.SettingsRead,
                Permissions.Admin.SettingsManage,
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
                Permissions.AuditLogs.Manage,
                Permissions.Documents.Read,
                Permissions.Documents.Upload,
                Permissions.Documents.ManageVersions,
                Permissions.Documents.Publish,
                Permissions.Documents.DeleteDraft,
                Permissions.Documents.Deactivate,
                Permissions.Workflows.Read,
                Permissions.Workflows.ManageDefinitions,
                Permissions.Requirements.Read,
                Permissions.Requirements.Manage,
                Permissions.Requirements.Approve,
                Permissions.Requirements.Baseline,
                Permissions.Requirements.ManageTraceability,
                Permissions.ChangeControl.Read,
                Permissions.ChangeControl.Manage,
                Permissions.ChangeControl.Approve,
                Permissions.ChangeControl.ReadConfiguration,
                Permissions.ChangeControl.ManageConfiguration,
                Permissions.ChangeControl.ManageBaselines,
                Permissions.ChangeControl.ApproveBaselines,
                Permissions.ChangeControl.EmergencyOverride,
                Permissions.Risks.Read,
                Permissions.Risks.Manage,
                Permissions.Risks.ReadSensitive,
                Permissions.Meetings.Read,
                Permissions.Meetings.Manage,
                Permissions.Meetings.Approve,
                Permissions.Meetings.ReadRestricted,
                Permissions.Verification.Read,
                Permissions.Verification.Manage,
                Permissions.Verification.Approve,
                Permissions.Verification.SubmitUat,
                Permissions.Verification.Export,
                Permissions.Verification.ReadSensitiveEvidence,
                Permissions.Notifications.Read
            ],
            ["operis_system_admin"] =
            [
                Permissions.Admin.PermissionMatrixRead,
                Permissions.Admin.PermissionMatrixApply,
                Permissions.Admin.SettingsRead,
                Permissions.Admin.SettingsManage,
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
                Permissions.AuditLogs.Manage,
                Permissions.Documents.Read,
                Permissions.Workflows.Read,
                Permissions.Workflows.ManageDefinitions,
                Permissions.Requirements.Read,
                Permissions.ChangeControl.Read,
                Permissions.ChangeControl.ReadConfiguration,
                Permissions.Risks.Read,
                Permissions.Risks.ReadSensitive,
                Permissions.Meetings.Read,
                Permissions.Meetings.ReadRestricted,
                Permissions.Verification.Read,
                Permissions.Verification.Export,
                Permissions.Notifications.Read
            ],
            ["operis:audit_auditor"] =
            [
                Permissions.ActivityLogs.Read,
                Permissions.ActivityLogs.Export,
                Permissions.AuditLogs.Read,
                Permissions.AuditLogs.Export,
                Permissions.AuditLogs.Manage,
                Permissions.Projects.ReadEvidence,
                Permissions.Projects.ReadCompliance,
                Permissions.Documents.Read,
                Permissions.Workflows.Read,
                Permissions.Requirements.Read,
                Permissions.ChangeControl.Read,
                Permissions.ChangeControl.ReadConfiguration,
                Permissions.Risks.Read,
                Permissions.Risks.ReadSensitive,
                Permissions.Meetings.Read,
                Permissions.Meetings.ReadRestricted,
                Permissions.Verification.Read,
                Permissions.Verification.Export,
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
                Permissions.ChangeControl.ReadConfiguration,
                Permissions.ChangeControl.ManageConfiguration,
                Permissions.Risks.Read,
                Permissions.Meetings.Read,
                Permissions.Notifications.Read
            ],
            ["operis:documents_reviewer"] = [Permissions.Documents.Read, Permissions.Notifications.Read],
            ["operis:workflows_approver"] = [Permissions.Workflows.Read, Permissions.Notifications.Read],
            ["operis:workflows_department_manager"] = [Permissions.Workflows.Read, Permissions.Notifications.Read],
            ["operis:employee_viewer"] = [Permissions.Workflows.Read, Permissions.Notifications.Read],
            ["operis:ops_support"] = [Permissions.Admin.SettingsRead, Permissions.Notifications.Read, Permissions.ActivityLogs.Read],
            ["operis:compliance_admin"] =
            [
                Permissions.Governance.ProcessLibraryRead,
                Permissions.Governance.ProcessLibraryManage,
                Permissions.AuditLogs.Read,
                Permissions.AuditLogs.Export,
                Permissions.AuditLogs.Manage,
                Permissions.Governance.QaChecklistRead,
                Permissions.Governance.QaChecklistManage,
                Permissions.Governance.ProjectPlanRead,
                Permissions.Governance.ProjectPlanApprove,
                Permissions.Governance.StakeholderRead,
                Permissions.Governance.TailoringRead,
                Permissions.Governance.TailoringApprove,
                Permissions.Requirements.Read,
                Permissions.Requirements.Approve,
                Permissions.Requirements.Baseline,
                Permissions.ChangeControl.Read,
                Permissions.ChangeControl.Approve,
                Permissions.ChangeControl.ReadConfiguration,
                Permissions.ChangeControl.ApproveBaselines,
                Permissions.ChangeControl.EmergencyOverride,
                Permissions.Risks.Read,
                Permissions.Risks.Manage,
                Permissions.Risks.ReadSensitive,
                Permissions.Meetings.Read,
                Permissions.Meetings.Manage,
                Permissions.Meetings.Approve,
                Permissions.Meetings.ReadRestricted,
                Permissions.Verification.Read,
                Permissions.Verification.Manage,
                Permissions.Verification.Approve,
                Permissions.Verification.SubmitUat,
                Permissions.Verification.Export,
                Permissions.Verification.ReadSensitiveEvidence
            ],
            ["operis:pm"] =
            [
                Permissions.AuditLogs.Read,
                Permissions.AuditLogs.Export,
                Permissions.Governance.ProcessLibraryRead,
                Permissions.Governance.QaChecklistRead,
                Permissions.Governance.ProjectPlanRead,
                Permissions.Governance.ProjectPlanManage,
                Permissions.Governance.StakeholderRead,
                Permissions.Governance.StakeholderManage,
                Permissions.Governance.TailoringRead,
                Permissions.Governance.TailoringManage,
                Permissions.Requirements.Read,
                Permissions.Requirements.Manage,
                Permissions.Requirements.Baseline,
                Permissions.Requirements.ManageTraceability,
                Permissions.ChangeControl.Read,
                Permissions.ChangeControl.Manage,
                Permissions.ChangeControl.ReadConfiguration,
                Permissions.ChangeControl.ManageBaselines,
                Permissions.Risks.Read,
                Permissions.Risks.Manage,
                Permissions.Risks.ReadSensitive,
                Permissions.Meetings.Read,
                Permissions.Meetings.Manage,
                Permissions.Meetings.ReadRestricted,
                Permissions.Verification.Read,
                Permissions.Verification.SubmitUat
            ],
            ["operis:ba"] =
            [
                Permissions.Governance.ProcessLibraryRead,
                Permissions.Governance.QaChecklistRead,
                Permissions.Governance.StakeholderRead,
                Permissions.Requirements.Read,
                Permissions.Requirements.Manage,
                Permissions.Requirements.ManageTraceability,
                Permissions.ChangeControl.Read,
                Permissions.ChangeControl.Manage,
                Permissions.ChangeControl.ReadConfiguration,
                Permissions.Risks.Read,
                Permissions.Risks.Manage,
                Permissions.Meetings.Read,
                Permissions.Meetings.Manage,
                Permissions.Meetings.ReadRestricted,
                Permissions.Verification.Read
            ],
            ["operis:qa"] =
            [
                Permissions.Governance.QaChecklistRead,
                Permissions.Governance.ProjectPlanRead,
                Permissions.Requirements.Read,
                Permissions.ChangeControl.Read,
                Permissions.ChangeControl.ReadConfiguration,
                Permissions.Risks.Read,
                Permissions.Meetings.Read,
                Permissions.Verification.Read,
                Permissions.Verification.Manage,
                Permissions.Verification.Export,
                Permissions.Verification.ReadSensitiveEvidence
            ],
            ["operis:approver"] =
            [
                Permissions.AuditLogs.Read,
                Permissions.Governance.QaChecklistRead,
                Permissions.Governance.ProjectPlanRead,
                Permissions.Governance.ProjectPlanApprove,
                Permissions.Governance.TailoringRead,
                Permissions.Governance.TailoringApprove,
                Permissions.Requirements.Read,
                Permissions.Requirements.Approve,
                Permissions.Requirements.Baseline,
                Permissions.ChangeControl.Read,
                Permissions.ChangeControl.Approve,
                Permissions.ChangeControl.ReadConfiguration,
                Permissions.ChangeControl.ApproveBaselines,
                Permissions.Risks.Read,
                Permissions.Risks.ReadSensitive,
                Permissions.Meetings.Read,
                Permissions.Meetings.Approve,
                Permissions.Meetings.ReadRestricted,
                Permissions.Verification.Read,
                Permissions.Verification.Approve
            ],
            ["operis:requirements_manager"] =
            [
                Permissions.Requirements.Read,
                Permissions.Requirements.Manage,
                Permissions.Requirements.ManageTraceability
            ],
            ["operis:requirements_approver"] =
            [
                Permissions.Requirements.Read,
                Permissions.Requirements.Approve,
                Permissions.Requirements.Baseline
            ],
            ["operis:change_manager"] =
            [
                Permissions.ChangeControl.Read,
                Permissions.ChangeControl.Manage,
                Permissions.ChangeControl.ReadConfiguration,
                Permissions.ChangeControl.ManageBaselines
            ],
            ["operis:risk_manager"] =
            [
                Permissions.Risks.Read,
                Permissions.Risks.Manage,
                Permissions.Risks.ReadSensitive
            ],
            ["operis:risk_viewer"] =
            [
                Permissions.Risks.Read
            ],
            ["operis:meeting_manager"] =
            [
                Permissions.Meetings.Read,
                Permissions.Meetings.Manage,
                Permissions.Meetings.ReadRestricted
            ],
            ["operis:meeting_approver"] =
            [
                Permissions.Meetings.Read,
                Permissions.Meetings.Approve,
                Permissions.Meetings.ReadRestricted
            ],
            ["operis:meeting_viewer"] =
            [
                Permissions.Meetings.Read
            ],
            ["operis:verification_manager"] =
            [
                Permissions.Verification.Read,
                Permissions.Verification.Manage,
                Permissions.Verification.Export,
                Permissions.Verification.ReadSensitiveEvidence
            ],
            ["operis:verification_approver"] =
            [
                Permissions.Verification.Read,
                Permissions.Verification.Approve,
                Permissions.Verification.Export,
                Permissions.Verification.ReadSensitiveEvidence
            ],
            ["operis:verification_viewer"] =
            [
                Permissions.Verification.Read
            ],
            ["operis:uat_submitter"] =
            [
                Permissions.Verification.Read,
                Permissions.Verification.SubmitUat
            ],
            ["operis:configuration_controller"] =
            [
                Permissions.ChangeControl.ReadConfiguration,
                Permissions.ChangeControl.ManageConfiguration
            ]
        };

    public IReadOnlyList<string> GetPermissions(IEnumerable<string> roles)
    {
        var roleList = roles.Where(role => !string.IsNullOrWhiteSpace(role)).Distinct(Comparer).ToArray();
        var permissions = new HashSet<string>(Comparer);
        var overridesByRole = LoadOverrides(roleList);

        foreach (var role in roleList)
        {
            if (overridesByRole.TryGetValue(role, out var roleOverrides))
            {
                foreach (var permission in roleOverrides.Where(item => item.Value).Select(item => item.Key))
                {
                    permissions.Add(permission);
                }

                continue;
            }

            if (!DefaultRolePermissions.TryGetValue(role, out var mappedPermissions))
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

    private Dictionary<string, Dictionary<string, bool>> LoadOverrides(IReadOnlyCollection<string> roleList)
    {
        var overridesByRole = new Dictionary<string, Dictionary<string, bool>>(Comparer);
        if (dbContext is null || roleList.Count == 0)
        {
            return overridesByRole;
        }

        var entries = dbContext.Set<PermissionMatrixEntryEntity>()
            .AsNoTracking()
            .Where(x => roleList.Contains(x.RoleKeycloakName))
            .ToList();

        foreach (var group in entries.GroupBy(x => x.RoleKeycloakName, Comparer))
        {
            overridesByRole[group.Key] = group.ToDictionary(x => x.PermissionKey, x => x.IsGranted, Comparer);
        }

        return overridesByRole;
    }
}
