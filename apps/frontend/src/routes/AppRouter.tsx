import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { ProtectedRoute } from "../shared/components/ProtectedRoute";
import { MainLayout } from "../shared/components/layouts/MainLayout";
import { DocumentDashboardPage } from "../modules/documents/pages/DocumentDashboardPage";
import { LoginPage } from "../modules/auth";
import { AdminUsersPage, InvitationAcceptPage } from "../modules/users";

export function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Root → login page */}
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/invite/:token" element={<InvitationAcceptPage />} />

        {/* Protected Dashboard Routes */}
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
          </Route>
        </Route>

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
