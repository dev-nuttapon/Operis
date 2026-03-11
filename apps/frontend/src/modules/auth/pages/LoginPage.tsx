import { Button, Flex, Typography, Spin } from "antd";
import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { login } from "../services/keycloakAuth";
import { useAuth } from "../hooks/useAuth";

export function LoginPage() {
  const { isReady, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (isReady && isAuthenticated) {
      navigate("/app", { replace: true });
    }
  }, [isReady, isAuthenticated, navigate]);

  if (!isReady) {
    return (
      <Flex justify="center" align="center" style={{ minHeight: "100vh", flexDirection: "column", gap: 12 }}>
        <Spin size="large" />
        <Typography.Text type="secondary">Checking login...</Typography.Text>
      </Flex>
    );
  }

  return (
    <Flex justify="center" align="center" style={{ minHeight: "100vh", flexDirection: "column", gap: 12 }}>
      <Typography.Title level={3} style={{ marginBottom: 0 }}>Operis</Typography.Title>
      <Typography.Text type="secondary">Please login to continue</Typography.Text>
      <Button type="primary" onClick={() => void login("/app")}>
        Login
      </Button>
    </Flex>
  );
}
