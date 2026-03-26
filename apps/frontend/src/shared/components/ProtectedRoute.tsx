import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../../modules/auth";
import { CenteredLoader } from "./feedback/CenteredLoader";
import { SessionExpiredState } from "./SessionExpiredState";

export function ProtectedRoute() {
  const { authState, isAuthenticated, isReady } = useAuth();

  if (!isReady) {
    return <CenteredLoader />;
  }

  if (authState === "expired") {
    return <SessionExpiredState />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}
