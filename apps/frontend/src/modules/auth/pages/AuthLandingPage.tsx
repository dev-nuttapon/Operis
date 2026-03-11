import { Button, Card, Divider, Space, Tag, Typography } from "antd";
import { appEnv } from "../../../shared/config/env";
import { useAuth } from "../hooks/useAuth";

export function AuthLandingPage() {
  const { isReady, isAuthenticated, login, logout } = useAuth();

  return (
    <div style={{ maxWidth: 960, margin: "48px auto", padding: "0 16px" }}>
      <Card title={appEnv.appName}>
        <Typography.Paragraph>
          Environment: <Typography.Text strong>{appEnv.mode}</Typography.Text>
        </Typography.Paragraph>
        <Divider />
        <Typography.Paragraph>
          API Base URL: <Typography.Text code>{appEnv.apiBaseUrl}</Typography.Text>
        </Typography.Paragraph>
        <Typography.Paragraph>
          Auth Base URL: <Typography.Text code>{appEnv.authBaseUrl}</Typography.Text>
        </Typography.Paragraph>
        <Typography.Paragraph>
          Keycloak Realm: <Typography.Text code>{appEnv.keycloakRealm}</Typography.Text>
        </Typography.Paragraph>
        <Typography.Paragraph>
          Keycloak Client: <Typography.Text code>{appEnv.keycloakClientId}</Typography.Text>
        </Typography.Paragraph>
        <Divider />
        <Space align="center">
          <Tag color={isAuthenticated ? "green" : "default"}>
            {isReady ? (isAuthenticated ? "Authenticated" : "Anonymous") : "Loading"}
          </Tag>
          {!isAuthenticated ? (
            <Button type="primary" onClick={() => void login()} disabled={!isReady}>
              Login with Keycloak
            </Button>
          ) : (
            <Button onClick={() => void logout()} disabled={!isReady}>
              Logout
            </Button>
          )}
        </Space>
      </Card>
    </div>
  );
}
