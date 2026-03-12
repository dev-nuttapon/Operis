import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../../modules/auth";
import { CenteredLoader } from "./feedback/CenteredLoader";

export function ProtectedRoute() {
  const { isAuthenticated, isReady } = useAuth();

  if (!isReady) {
    return <CenteredLoader />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}
