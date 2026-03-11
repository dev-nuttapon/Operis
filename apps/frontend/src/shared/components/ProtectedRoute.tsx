import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../../modules/auth/hooks/useAuth";
import { Spin, Flex } from "antd";

export function ProtectedRoute() {
  const { isReady, isAuthenticated } = useAuth();

  if (!isReady) {
    return (
      <Flex justify="center" align="center" style={{ minHeight: "100vh" }}>
        <Spin size="large" />
      </Flex>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}
