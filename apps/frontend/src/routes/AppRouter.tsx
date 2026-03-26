import { lazy, Suspense } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { Flex, Spin } from "antd";
import { ProtectedRoute } from "../shared/components/ProtectedRoute";
import { AuthorizedRoute } from "../shared/components/AuthorizedRoute";
import { MainLayout } from "../shared/components/layouts/MainLayout";
import { AppErrorBoundary } from "../shared/components/AppErrorBoundary";
import { permissions } from "../shared/authz/permissions";

const DocumentDashboardPage = lazy(() =>
  import("../modules/documents/pages/DocumentDashboardPage").then((module) => ({ default: module.DocumentDashboardPage }))
);
const DocumentUploadPage = lazy(() =>
  import("../modules/documents/pages/DocumentUploadPage").then((module) => ({ default: module.DocumentUploadPage }))
);
const DocumentVersionUploadPage = lazy(() =>
  import("../modules/documents/pages/DocumentVersionUploadPage").then((module) => ({ default: module.DocumentVersionUploadPage }))
);
const DocumentVersionsPage = lazy(() =>
  import("../modules/documents/pages/DocumentVersionsPage").then((module) => ({ default: module.DocumentVersionsPage }))
);
const DocumentTypeSetupPage = lazy(() =>
  import("../modules/documents/pages/DocumentTypeSetupPage").then((module) => ({ default: module.DocumentTypeSetupPage }))
);
const DocumentHistoryPage = lazy(() =>
  import("../modules/documents/pages/DocumentHistoryPage").then((module) => ({ default: module.DocumentHistoryPage }))
);
const DocumentTemplatesPage = lazy(() =>
  import("../modules/documents/pages/DocumentTemplatesPage").then((module) => ({ default: module.DocumentTemplatesPage }))
);
const DocumentTemplateCreatePage = lazy(() =>
  import("../modules/documents/pages/DocumentTemplateCreatePage").then((module) => ({ default: module.DocumentTemplateCreatePage }))
);
const DocumentTemplateEditPage = lazy(() =>
  import("../modules/documents/pages/DocumentTemplateEditPage").then((module) => ({ default: module.DocumentTemplateEditPage }))
);
const DocumentTemplateHistoryPage = lazy(() =>
  import("../modules/documents/pages/DocumentTemplateHistoryPage").then((module) => ({ default: module.DocumentTemplateHistoryPage }))
);
const LoginPage = lazy(() =>
  import("../modules/auth/pages/LoginPage").then((module) => ({ default: module.LoginPage }))
);
const AdminUsersPage = lazy(() =>
  import("../modules/users/pages/AdminUsersPage").then((module) => ({ default: module.AdminUsersPage }))
);
const AdminUserCreatePage = lazy(() =>
  import("../modules/users/pages/AdminUserCreatePage").then((module) => ({ default: module.AdminUserCreatePage }))
);
const AdminUserEditPage = lazy(() =>
  import("../modules/users/pages/AdminUserEditPage").then((module) => ({ default: module.AdminUserEditPage }))
);
const AdminSettingsPage = lazy(() =>
  import("../modules/users/pages/AdminSettingsPage").then((module) => ({ default: module.AdminSettingsPage }))
);
const PermissionMatrixPage = lazy(() =>
  import("../modules/users/pages/PermissionMatrixPage").then((module) => ({ default: module.PermissionMatrixPage }))
);
const ProcessLibraryPage = lazy(() =>
  import("../modules/governance/pages/ProcessLibraryPage").then((module) => ({ default: module.ProcessLibraryPage }))
);
const QaReviewChecklistPage = lazy(() =>
  import("../modules/governance/pages/QaReviewChecklistPage").then((module) => ({ default: module.QaReviewChecklistPage }))
);
const ProjectPlanPage = lazy(() =>
  import("../modules/governance/pages/ProjectPlanPage").then((module) => ({ default: module.ProjectPlanPage }))
);
const StakeholderRegisterPage = lazy(() =>
  import("../modules/governance/pages/StakeholderRegisterPage").then((module) => ({ default: module.StakeholderRegisterPage }))
);
const TailoringRecordPage = lazy(() =>
  import("../modules/governance/pages/TailoringRecordPage").then((module) => ({ default: module.TailoringRecordPage }))
);
const RequirementRegisterPage = lazy(() =>
  import("../modules/requirements/pages/RequirementRegisterPage").then((module) => ({ default: module.RequirementRegisterPage }))
);
const RequirementDetailPage = lazy(() =>
  import("../modules/requirements/pages/RequirementDetailPage").then((module) => ({ default: module.RequirementDetailPage }))
);
const RequirementBaselinesPage = lazy(() =>
  import("../modules/requirements/pages/RequirementBaselinesPage").then((module) => ({ default: module.RequirementBaselinesPage }))
);
const TraceabilityMatrixPage = lazy(() =>
  import("../modules/requirements/pages/TraceabilityMatrixPage").then((module) => ({ default: module.TraceabilityMatrixPage }))
);
const ChangeRequestRegisterPage = lazy(() =>
  import("../modules/change-control/pages/ChangeRequestRegisterPage").then((module) => ({ default: module.ChangeRequestRegisterPage }))
);
const ChangeRequestDetailPage = lazy(() =>
  import("../modules/change-control/pages/ChangeRequestDetailPage").then((module) => ({ default: module.ChangeRequestDetailPage }))
);
const ConfigurationItemsPage = lazy(() =>
  import("../modules/change-control/pages/ConfigurationItemsPage").then((module) => ({ default: module.ConfigurationItemsPage }))
);
const BaselineRegistryPage = lazy(() =>
  import("../modules/change-control/pages/BaselineRegistryPage").then((module) => ({ default: module.BaselineRegistryPage }))
);
const RiskRegisterPage = lazy(() =>
  import("../modules/risks/pages/RiskRegisterPage").then((module) => ({ default: module.RiskRegisterPage }))
);
const RiskDetailPage = lazy(() =>
  import("../modules/risks/pages/RiskDetailPage").then((module) => ({ default: module.RiskDetailPage }))
);
const IssueLogPage = lazy(() =>
  import("../modules/risks/pages/IssueLogPage").then((module) => ({ default: module.IssueLogPage }))
);
const IssueDetailPage = lazy(() =>
  import("../modules/risks/pages/IssueDetailPage").then((module) => ({ default: module.IssueDetailPage }))
);
const InvitationAcceptPage = lazy(() =>
  import("../modules/users/pages/InvitationAcceptPage").then((module) => ({ default: module.InvitationAcceptPage }))
);
const PublicRegistrationPage = lazy(() =>
  import("../modules/users/pages/PublicRegistrationPage").then((module) => ({ default: module.PublicRegistrationPage }))
);
const RegistrationPasswordSetupPage = lazy(() =>
  import("../modules/users/pages/RegistrationPasswordSetupPage").then((module) => ({ default: module.RegistrationPasswordSetupPage }))
);
const ProjectsPage = lazy(() =>
  import("../modules/users/pages/ProjectsPage").then((module) => ({ default: module.ProjectsPage }))
);
const ProjectCreatePage = lazy(() =>
  import("../modules/users/pages/ProjectCreatePage").then((module) => ({ default: module.ProjectCreatePage }))
);
const ProjectEditPage = lazy(() =>
  import("../modules/users/pages/ProjectEditPage").then((module) => ({ default: module.ProjectEditPage }))
);
const ProjectHistoryPage = lazy(() =>
  import("../modules/users/pages/ProjectHistoryPage").then((module) => ({ default: module.ProjectHistoryPage }))
);
const ProjectRolesPage = lazy(() =>
  import("../modules/users/pages/ProjectRolesPage").then((module) => ({ default: module.ProjectRolesPage }))
);
const ProjectRoleCreatePage = lazy(() =>
  import("../modules/users/pages/ProjectRoleCreatePage").then((module) => ({ default: module.ProjectRoleCreatePage }))
);
const ProjectRoleEditPage = lazy(() =>
  import("../modules/users/pages/ProjectRoleEditPage").then((module) => ({ default: module.ProjectRoleEditPage }))
);
const ProjectMembersPage = lazy(() =>
  import("../modules/users/pages/ProjectMembersPage").then((module) => ({ default: module.ProjectMembersPage }))
);
const ProjectMemberCreatePage = lazy(() =>
  import("../modules/users/pages/ProjectMemberCreatePage").then((module) => ({ default: module.ProjectMemberCreatePage }))
);
const ProjectMemberEditPage = lazy(() =>
  import("../modules/users/pages/ProjectMemberEditPage").then((module) => ({ default: module.ProjectMemberEditPage }))
);
const ProjectOrgChartPage = lazy(() =>
  import("../modules/users/pages/ProjectOrgChartPage").then((module) => ({ default: module.ProjectOrgChartPage }))
);
const ProjectWorkspacePrototypePage = lazy(() =>
  import("../modules/users/pages/ProjectWorkspacePrototypePage").then((module) => ({ default: module.ProjectWorkspacePrototypePage }))
);
const UserProfilePage = lazy(() =>
  import("../modules/users/pages/UserProfilePage").then((module) => ({ default: module.UserProfilePage }))
);
const ChangePasswordPage = lazy(() =>
  import("../modules/users/pages/ChangePasswordPage").then((module) => ({ default: module.ChangePasswordPage }))
);
const ActivityLogsPage = lazy(() =>
  import("../modules/activities/pages/ActivityLogsPage").then((module) => ({ default: module.ActivityLogsPage }))
);
const WorkflowDefinitionsPage = lazy(() =>
  import("../modules/workflows/pages/WorkflowDefinitionsPage").then((module) => ({ default: module.WorkflowDefinitionsPage }))
);
const WorkflowDefinitionCreatePage = lazy(() =>
  import("../modules/workflows/pages/WorkflowDefinitionCreatePage").then((module) => ({ default: module.WorkflowDefinitionCreatePage }))
);
const WorkflowDefinitionEditPage = lazy(() =>
  import("../modules/workflows/pages/WorkflowDefinitionEditPage").then((module) => ({ default: module.WorkflowDefinitionEditPage }))
);
const WorkflowTasksPage = lazy(() =>
  import("../modules/workflows/pages/WorkflowTasksPage").then((module) => ({ default: module.WorkflowTasksPage }))
);
const WorkflowTaskDetailPage = lazy(() =>
  import("../modules/workflows/pages/WorkflowTaskDetailPage").then((module) => ({ default: module.WorkflowTaskDetailPage }))
);
const WorkflowTaskUploadPage = lazy(() =>
  import("../modules/workflows/pages/WorkflowTaskUploadPage").then((module) => ({ default: module.WorkflowTaskUploadPage }))
);
const WorkspaceProjectsPage = lazy(() =>
  import("../modules/workflows/pages/WorkspaceProjectsPage").then((module) => ({ default: module.WorkspaceProjectsPage }))
);
const NotificationsPage = lazy(() =>
  import("../modules/notifications/pages/NotificationsPage").then((module) => ({ default: module.NotificationsPage }))
);

function RouteFallback() {
  return (
    <Flex align="center" justify="center" style={{ minHeight: "100vh" }}>
      <Spin size="large" />
    </Flex>
  );
}

export function AppRouter() {
  return (
    <BrowserRouter>
      <Suspense fallback={<RouteFallback />}>
        <AppErrorBoundary>
          <Routes>
            <Route path="/" element={<Navigate to="/login" replace />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<PublicRegistrationPage />} />
            <Route path="/register/setup-password/:token" element={<RegistrationPasswordSetupPage />} />
            <Route path="/invite/:token" element={<InvitationAcceptPage />} />

            <Route path="/app" element={<ProtectedRoute />}>
              <Route element={<MainLayout />}>
                <Route index element={<Navigate to="documents" replace />} />
                <Route path="profile" element={<UserProfilePage />} />
                <Route path="change-password" element={<ChangePasswordPage />} />
                <Route path="documents" element={<AuthorizedRoute anyOf={[permissions.documents.read, permissions.documents.upload, permissions.documents.manageVersions, permissions.documents.publish]}><DocumentDashboardPage /></AuthorizedRoute>} />
                <Route path="documents/types" element={<AuthorizedRoute anyOf={[permissions.documents.read, permissions.documents.deactivate]}><DocumentTypeSetupPage /></AuthorizedRoute>} />
                <Route path="documents/templates" element={<AuthorizedRoute anyOf={[permissions.documents.read, permissions.documents.upload]}><DocumentTemplatesPage /></AuthorizedRoute>} />
                <Route path="document-templates" element={<DocumentTemplatesPage />} />
                <Route path="document-templates/new" element={<DocumentTemplateCreatePage />} />
                <Route path="document-templates/:templateId/edit" element={<DocumentTemplateEditPage />} />
                <Route path="document-templates/:templateId/history" element={<DocumentTemplateHistoryPage />} />
                <Route path="documents/new" element={<AuthorizedRoute permission={permissions.documents.upload}><DocumentUploadPage /></AuthorizedRoute>} />
                <Route path="documents/:documentId" element={<AuthorizedRoute anyOf={[permissions.documents.read, permissions.documents.manageVersions, permissions.documents.publish]}><DocumentVersionsPage /></AuthorizedRoute>} />
                <Route path="documents/:documentId/versions" element={<AuthorizedRoute anyOf={[permissions.documents.read, permissions.documents.manageVersions, permissions.documents.publish]}><DocumentVersionsPage /></AuthorizedRoute>} />
                <Route path="documents/:documentId/history" element={<AuthorizedRoute permission={permissions.activityLogs.read}><DocumentHistoryPage /></AuthorizedRoute>} />
                <Route path="documents/:documentId/versions/new" element={<AuthorizedRoute permission={permissions.documents.manageVersions}><DocumentVersionUploadPage /></AuthorizedRoute>} />
                <Route path="projects" element={<ProjectsPage />} />
                <Route path="projects/new" element={<ProjectCreatePage />} />
                <Route path="projects/:projectId/edit" element={<ProjectEditPage />} />
                <Route path="projects/:projectId/history" element={<ProjectHistoryPage />} />
              <Route path="steps" element={<WorkflowDefinitionsPage />} />
              <Route path="steps/new" element={<WorkflowDefinitionCreatePage />} />
              <Route path="steps/:workflowDefinitionId/edit" element={<WorkflowDefinitionEditPage />} />
              <Route path="workspace" element={<WorkspaceProjectsPage />} />
              <Route path="workspace/:projectId" element={<WorkflowTasksPage />} />
              <Route path="workspace/:projectId/tasks/:workflowInstanceStepId" element={<WorkflowTaskDetailPage />} />
              <Route
                path="workspace/:projectId/tasks/:workflowInstanceStepId/upload"
                element={<WorkflowTaskUploadPage />}
              />
              <Route path="notifications" element={<NotificationsPage />} />
              <Route path="admin/users" element={<AuthorizedRoute permission={permissions.users.read}><AdminUsersPage /></AuthorizedRoute>} />
              <Route path="admin/users/new" element={<AuthorizedRoute permission={permissions.users.create}><AdminUserCreatePage /></AuthorizedRoute>} />
              <Route path="admin/users/:userId/edit" element={<AuthorizedRoute permission={permissions.users.update}><AdminUserEditPage /></AuthorizedRoute>} />
              <Route path="admin/permissions" element={<AuthorizedRoute permission={permissions.admin.permissionMatrixRead}><PermissionMatrixPage /></AuthorizedRoute>} />
              <Route path="admin/settings" element={<AuthorizedRoute anyOf={[permissions.admin.settingsRead, permissions.admin.settingsManage]}><AdminSettingsPage /></AuthorizedRoute>} />
              <Route path="process-library" element={<AuthorizedRoute anyOf={[permissions.governance.processLibraryRead, permissions.governance.processLibraryManage]}><ProcessLibraryPage /></AuthorizedRoute>} />
              <Route path="qa-review-checklists" element={<AuthorizedRoute anyOf={[permissions.governance.qaChecklistRead, permissions.governance.qaChecklistManage]}><QaReviewChecklistPage /></AuthorizedRoute>} />
              <Route path="project-plans" element={<AuthorizedRoute anyOf={[permissions.governance.projectPlanRead, permissions.governance.projectPlanManage, permissions.governance.projectPlanApprove]}><ProjectPlanPage /></AuthorizedRoute>} />
              <Route path="stakeholders" element={<AuthorizedRoute anyOf={[permissions.governance.stakeholderRead, permissions.governance.stakeholderManage]}><StakeholderRegisterPage /></AuthorizedRoute>} />
              <Route path="tailoring-records" element={<AuthorizedRoute anyOf={[permissions.governance.tailoringRead, permissions.governance.tailoringManage, permissions.governance.tailoringApprove]}><TailoringRecordPage /></AuthorizedRoute>} />
              <Route path="requirements" element={<AuthorizedRoute anyOf={[permissions.requirements.read, permissions.requirements.manage, permissions.requirements.approve, permissions.requirements.baseline]}><RequirementRegisterPage /></AuthorizedRoute>} />
              <Route path="requirements/:requirementId" element={<AuthorizedRoute anyOf={[permissions.requirements.read, permissions.requirements.manage, permissions.requirements.approve, permissions.requirements.baseline]}><RequirementDetailPage /></AuthorizedRoute>} />
              <Route path="requirements/baselines" element={<AuthorizedRoute anyOf={[permissions.requirements.read, permissions.requirements.baseline]}><RequirementBaselinesPage /></AuthorizedRoute>} />
              <Route path="requirements/traceability" element={<AuthorizedRoute permission={permissions.requirements.read}><TraceabilityMatrixPage /></AuthorizedRoute>} />
              <Route path="change-control/change-requests" element={<AuthorizedRoute anyOf={[permissions.changeControl.read, permissions.changeControl.manage, permissions.changeControl.approve]}><ChangeRequestRegisterPage /></AuthorizedRoute>} />
              <Route path="change-control/change-requests/:changeRequestId" element={<AuthorizedRoute anyOf={[permissions.changeControl.read, permissions.changeControl.manage, permissions.changeControl.approve]}><ChangeRequestDetailPage /></AuthorizedRoute>} />
              <Route path="change-control/configuration-items" element={<AuthorizedRoute anyOf={[permissions.changeControl.readConfiguration, permissions.changeControl.manageConfiguration]}><ConfigurationItemsPage /></AuthorizedRoute>} />
              <Route path="change-control/baseline-registry" element={<AuthorizedRoute anyOf={[permissions.changeControl.manageBaselines, permissions.changeControl.approveBaselines, permissions.changeControl.readConfiguration]}><BaselineRegistryPage /></AuthorizedRoute>} />
              <Route path="risks" element={<AuthorizedRoute anyOf={[permissions.risks.read, permissions.risks.manage]}><RiskRegisterPage /></AuthorizedRoute>} />
              <Route path="risks/:riskId" element={<AuthorizedRoute anyOf={[permissions.risks.read, permissions.risks.manage]}><RiskDetailPage /></AuthorizedRoute>} />
              <Route path="issues" element={<AuthorizedRoute anyOf={[permissions.risks.read, permissions.risks.manage]}><IssueLogPage /></AuthorizedRoute>} />
              <Route path="issues/:issueId" element={<AuthorizedRoute anyOf={[permissions.risks.read, permissions.risks.manage]}><IssueDetailPage /></AuthorizedRoute>} />
              <Route path="admin/master" element={<Navigate to="/app/admin/master/divisions" replace />} />
                <Route path="admin/master/divisions" element={<AdminUsersPage />} />
                <Route path="admin/master/departments" element={<AdminUsersPage />} />
                <Route path="admin/master/positions" element={<AdminUsersPage />} />
                <Route path="projects/roles" element={<ProjectRolesPage />} />
                <Route path="projects/roles/new" element={<ProjectRoleCreatePage />} />
                <Route path="projects/roles/:projectRoleId/edit" element={<ProjectRoleEditPage />} />
                <Route path="admin/project-members" element={<ProjectMembersPage />} />
                <Route path="admin/project-members/new" element={<ProjectMemberCreatePage />} />
                <Route path="admin/project-members/:assignmentId/edit" element={<ProjectMemberEditPage />} />
                <Route path="admin/project-org-chart" element={<ProjectOrgChartPage />} />
                <Route path="projects/:projectId/workspace" element={<ProjectWorkspacePrototypePage />} />
                <Route path="admin/invitations" element={<AdminUsersPage />} />
                <Route path="admin/registrations" element={<AdminUsersPage />} />
                <Route path="admin/activity-logs" element={<ActivityLogsPage />} />
              </Route>
            </Route>

            <Route path="*" element={<Navigate to="/login" replace />} />
          </Routes>
        </AppErrorBoundary>
      </Suspense>
    </BrowserRouter>
  );
}
