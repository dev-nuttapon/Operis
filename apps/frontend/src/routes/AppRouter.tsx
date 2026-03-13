import { lazy, Suspense } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { Flex, Spin } from "antd";
import { ProtectedRoute } from "../shared/components/ProtectedRoute";
import { MainLayout } from "../shared/components/layouts/MainLayout";

const DocumentDashboardPage = lazy(() =>
  import("../modules/documents/pages/DocumentDashboardPage").then((module) => ({ default: module.DocumentDashboardPage }))
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
              <Route path="workflows" element={<WorkflowDefinitionsPage />} />
              <Route path="admin/users" element={<AdminUsersPage />} />
              <Route path="admin/master" element={<Navigate to="/app/admin/master/divisions" replace />} />
              <Route path="admin/master/divisions" element={<AdminUsersPage />} />
              <Route path="admin/master/departments" element={<AdminUsersPage />} />
              <Route path="admin/master/positions" element={<AdminUsersPage />} />
              <Route path="admin/projects" element={<ProjectsPage />} />
              <Route path="admin/project-roles" element={<ProjectRolesPage />} />
              <Route path="admin/project-members" element={<ProjectMembersPage />} />
              <Route path="admin/project-org-chart" element={<ProjectOrgChartPage />} />
              <Route path="admin/project-evidence" element={<ProjectEvidencePage />} />
              <Route path="admin/project-compliance" element={<ProjectCompliancePage />} />
              <Route path="admin/project-type-templates" element={<ProjectTypeTemplatesPage />} />
              <Route path="admin/invitations" element={<AdminUsersPage />} />
              <Route path="admin/registrations" element={<AdminUsersPage />} />
              <Route path="admin/audit-logs" element={<AuditLogsPage />} />
            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </Suspense>
    </BrowserRouter>
  );
}
