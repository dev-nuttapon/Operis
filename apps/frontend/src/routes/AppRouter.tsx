import { lazy, Suspense } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { Flex, Spin } from "antd";
import { ProtectedRoute } from "../shared/components/ProtectedRoute";
import { MainLayout } from "../shared/components/layouts/MainLayout";
import { AppErrorBoundary } from "../shared/components/AppErrorBoundary";

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
                <Route path="documents" element={<DocumentDashboardPage />} />
                <Route path="documents/templates" element={<DocumentTemplatesPage />} />
                <Route path="document-templates" element={<DocumentTemplatesPage />} />
                <Route path="document-templates/new" element={<DocumentTemplateCreatePage />} />
                <Route path="document-templates/:templateId/edit" element={<DocumentTemplateEditPage />} />
                <Route path="document-templates/:templateId/history" element={<DocumentTemplateHistoryPage />} />
                <Route path="documents/upload" element={<DocumentUploadPage />} />
                <Route path="documents/:documentId/versions" element={<DocumentVersionsPage />} />
                <Route path="documents/:documentId/history" element={<DocumentHistoryPage />} />
                <Route path="documents/:documentId/versions/new" element={<DocumentVersionUploadPage />} />
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
              <Route path="notifications" element={<NotificationsPage />} />
              <Route path="admin/users" element={<AdminUsersPage />} />
              <Route path="admin/users/new" element={<AdminUserCreatePage />} />
              <Route path="admin/users/:userId/edit" element={<AdminUserEditPage />} />
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
