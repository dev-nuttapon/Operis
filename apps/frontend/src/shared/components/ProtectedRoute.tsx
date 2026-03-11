import { Navigate, Outlet } from "react-router-dom";
import { Button, Flex, Typography } from "antd";
import { useAuth } from "../../modules/auth/hooks/useAuth";
import { login } from "../../modules/auth/services/keycloakAuth";

export function ProtectedRoute() {
  const { isAuthenticated, isReady } = useAuth();

  if (!isReady) {
    return null;
  }

  if (!isAuthenticated) {
    return (
      <Flex justify="center" align="center" style={{ minHeight: "100vh", flexDirection: "column", gap: 12 }}>
        <Typography.Text type="secondary">Please login to access this page</Typography.Text>
        <Button type="primary" onClick={() => void login("/app")}>Login</Button>
        <Navigate to="/login" replace />
      </Flex>
    );
  }

  return <Outlet />;
}
