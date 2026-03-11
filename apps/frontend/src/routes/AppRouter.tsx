import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthLandingPage } from "../modules/auth";
import { ProtectedRoute } from "../shared/components/ProtectedRoute";
import { MainLayout } from "../shared/components/layouts/MainLayout";
import { DocumentDashboardPage } from "../modules/documents/pages/DocumentDashboardPage";

export function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public Route */}
        <Route path="/" element={<AuthLandingPage />} />

        {/* Protected Dashboard Routes */}
        <Route path="/app" element={<ProtectedRoute />}>
          <Route element={<MainLayout />}>
            <Route index element={<Navigate to="documents" replace />} />
            <Route path="documents" element={<DocumentDashboardPage />} />
          </Route>
        </Route>

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
