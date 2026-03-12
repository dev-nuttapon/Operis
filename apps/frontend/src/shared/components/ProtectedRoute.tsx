import { Navigate, Outlet } from "react-router-dom";
import { Flex, Spin } from "antd";
import { useAuth } from "../../modules/auth";

export function ProtectedRoute() {
  const { isAuthenticated, isReady } = useAuth();

  if (!isReady) {
    return (
      <Flex justify="center" align="center" style={{ minHeight: "100vh" }}>
        <Spin size="large" />
      </Flex>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}
