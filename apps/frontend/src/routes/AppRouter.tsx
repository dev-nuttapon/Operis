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
const AuditLogsPage = lazy(() =>
  import("../modules/audits/pages/AuditLogsPage").then((module) => ({ default: module.AuditLogsPage }))
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
              <Route path="admin/users" element={<AdminUsersPage />} />
              <Route path="admin/master" element={<Navigate to="/app/admin/master/departments" replace />} />
              <Route path="admin/master/departments" element={<AdminUsersPage />} />
              <Route path="admin/master/job-titles" element={<AdminUsersPage />} />
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
