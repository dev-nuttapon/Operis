import { lazy, Suspense } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { Flex, Spin } from "antd";
import { ProtectedRoute } from "../shared/components/ProtectedRoute";
import { MainLayout } from "../shared/components/layouts/MainLayout";

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
const LoginPage = lazy(() =>
  import("../modules/auth/pages/LoginPage").then((module) => ({ default: module.LoginPage }))
);
const AdminUsersPage = lazy(() =>
  import("../modules/users/pages/AdminUsersPage").then((module) => ({ default: module.AdminUsersPage }))
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
const ProjectRolesPage = lazy(() =>
  import("../modules/users/pages/ProjectRolesPage").then((module) => ({ default: module.ProjectRolesPage }))
);
const ProjectMembersPage = lazy(() =>
  import("../modules/users/pages/ProjectMembersPage").then((module) => ({ default: module.ProjectMembersPage }))
);
const ProjectOrgChartPage = lazy(() =>
  import("../modules/users/pages/ProjectOrgChartPage").then((module) => ({ default: module.ProjectOrgChartPage }))
);
const ProjectEvidencePage = lazy(() =>
  import("../modules/users/pages/ProjectEvidencePage").then((module) => ({ default: module.ProjectEvidencePage }))
);
const ProjectCompliancePage = lazy(() =>
  import("../modules/users/pages/ProjectCompliancePage").then((module) => ({ default: module.ProjectCompliancePage }))
);
const ProjectTypeTemplatesPage = lazy(() =>
  import("../modules/users/pages/ProjectTypeTemplatesPage").then((module) => ({ default: module.ProjectTypeTemplatesPage }))
);
const ProjectWorkspacePrototypePage = lazy(() =>
  import("../modules/users/pages/ProjectWorkspacePrototypePage").then((module) => ({ default: module.ProjectWorkspacePrototypePage }))
);
const ActivityLogsPage = lazy(() =>
  import("../modules/activities/pages/ActivityLogsPage").then((module) => ({ default: module.ActivityLogsPage }))
);
const AuditLogsPage = lazy(() =>
  import("../modules/audits/pages/AuditLogsPage").then((module) => ({ default: module.AuditLogsPage }))
);
const WorkflowDefinitionsPage = lazy(() =>
  import("../modules/workflows/pages/WorkflowDefinitionsPage").then((module) => ({ default: module.WorkflowDefinitionsPage }))
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
        <Routes>
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<PublicRegistrationPage />} />
          <Route path="/register/setup-password/:token" element={<RegistrationPasswordSetupPage />} />
          <Route path="/invite/:token" element={<InvitationAcceptPage />} />

          <Route path="/app" element={<ProtectedRoute />}>
            <Route element={<MainLayout />}>
              <Route index element={<Navigate to="documents" replace />} />
              <Route path="documents" element={<DocumentDashboardPage />} />
              <Route path="documents/templates" element={<DocumentTemplatesPage />} />
              <Route path="document-templates" element={<DocumentTemplatesPage />} />
              <Route path="document-templates/new" element={<DocumentTemplateCreatePage />} />
              <Route path="document-templates/:templateId/edit" element={<DocumentTemplateEditPage />} />
              <Route path="documents/upload" element={<DocumentUploadPage />} />
              <Route path="documents/:documentId/versions" element={<DocumentVersionsPage />} />
              <Route path="documents/:documentId/history" element={<DocumentHistoryPage />} />
              <Route path="documents/:documentId/versions/new" element={<DocumentVersionUploadPage />} />
              <Route path="projects" element={<ProjectsPage />} />
              <Route path="projects/new" element={<ProjectCreatePage />} />
              <Route path="workflows" element={<WorkflowDefinitionsPage />} />
              <Route path="admin/users" element={<AdminUsersPage />} />
              <Route path="admin/master" element={<Navigate to="/app/admin/master/divisions" replace />} />
              <Route path="admin/master/divisions" element={<AdminUsersPage />} />
              <Route path="admin/master/departments" element={<AdminUsersPage />} />
              <Route path="admin/master/positions" element={<AdminUsersPage />} />
              <Route path="admin/projects" element={<ProjectsPage />} />
              <Route path="admin/projects/new" element={<ProjectCreatePage />} />
              <Route path="admin/project-roles" element={<ProjectRolesPage />} />
              <Route path="admin/project-members" element={<ProjectMembersPage />} />
              <Route path="admin/project-org-chart" element={<ProjectOrgChartPage />} />
              <Route path="projects/:projectId/workspace" element={<ProjectWorkspacePrototypePage />} />
              <Route path="admin/project-evidence" element={<ProjectEvidencePage />} />
              <Route path="admin/project-compliance" element={<ProjectCompliancePage />} />
              <Route path="admin/project-type-templates" element={<ProjectTypeTemplatesPage />} />
              <Route path="admin/invitations" element={<AdminUsersPage />} />
              <Route path="admin/registrations" element={<AdminUsersPage />} />
              <Route path="admin/activity-logs" element={<ActivityLogsPage />} />
              <Route path="admin/audit-logs" element={<AuditLogsPage />} />
            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </Suspense>
    </BrowserRouter>
  );
}
