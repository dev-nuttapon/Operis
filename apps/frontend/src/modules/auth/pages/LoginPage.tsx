import { Flex, Typography, Spin } from "antd";
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
      return;
    }
    if (isReady && !isAuthenticated) {
      void login("/app");
    }
  }, [isReady, isAuthenticated, navigate]);

  return (
    <Flex justify="center" align="center" style={{ minHeight: "100vh", flexDirection: "column", gap: 12 }}>
      <Typography.Title level={3} style={{ marginBottom: 0 }}>Operis</Typography.Title>
      <Spin size="large" />
      <Typography.Text type="secondary">Checking login...</Typography.Text>
    </Flex>
  );
}
