import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { ProtectedRoute } from "../shared/components/ProtectedRoute";
import { MainLayout } from "../shared/components/layouts/MainLayout";
import { DocumentDashboardPage } from "../modules/documents/pages/DocumentDashboardPage";
import { LoginPage } from "../modules/auth";

export function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Root → login page */}
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="/login" element={<LoginPage />} />

        {/* Protected Dashboard Routes */}
        <Route path="/app" element={<ProtectedRoute />}>
          <Route element={<MainLayout />}>
            <Route index element={<Navigate to="documents" replace />} />
            <Route path="documents" element={<DocumentDashboardPage />} />
          </Route>
        </Route>

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
