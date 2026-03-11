import { Button, Flex, Typography, Spin } from "antd";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../modules/auth/hooks/useAuth";
import { login } from "../modules/auth/services/keycloakAuth";

export function AppEntryGate() {
  const { isReady, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  if (!isReady) {
    return (
      <Flex justify="center" align="center" style={{ minHeight: "100vh", flexDirection: "column", gap: 12 }}>
        <Spin size="large" />
        <Typography.Text type="secondary">Checking login...</Typography.Text>
      </Flex>
    );
  }

  if (isAuthenticated) {
    navigate("/app", { replace: true });
  }

  return (
    <Flex justify="center" align="center" style={{ minHeight: "100vh", flexDirection: "column", gap: 12 }}>
      <Typography.Title level={3} style={{ marginBottom: 0 }}>Operis</Typography.Title>
      <Typography.Text type="secondary">Please login to continue</Typography.Text>
      <Flex gap={12} style={{ marginTop: 16 }}>
        <Button type="primary" onClick={() => void login("/app")} disabled={!isReady}>
          Login
        </Button>
      </Flex>
    </Flex>
  );
}
