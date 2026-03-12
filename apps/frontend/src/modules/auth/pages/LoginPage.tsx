import { Alert, Flex, Typography, Spin, Button, Space } from "antd";
import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { login } from "../services/keycloakAuth";
import { useAuth } from "../hooks/useAuth";

export function LoginPage() {
  const { t } = useTranslation();
  const { isReady, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [loginError, setLoginError] = useState<string | null>(null);
  const loginStartedRef = useRef(false);

  useEffect(() => {
    if (isReady && isAuthenticated) {
      navigate("/app", { replace: true });
      return;
    }
    if (isReady && !isAuthenticated && !loginStartedRef.current) {
      loginStartedRef.current = true;
      void login("/app").catch((err: unknown) => {
        const message = err instanceof Error ? err.message : t("auth.login_failed");
        setLoginError(message);
        loginStartedRef.current = false;
      });
    }
  }, [isReady, isAuthenticated, navigate, t]);

  return (
    <Flex justify="center" align="center" style={{ minHeight: "100vh", flexDirection: "column", gap: 12 }}>
      <Typography.Title level={3} style={{ marginBottom: 0 }}>{t("common.application_name")}</Typography.Title>
      <Spin size="large" />
      <Typography.Text type="secondary">{t("auth.checking_login")}</Typography.Text>
      {loginError ? (
        <Flex vertical gap={8} style={{ maxWidth: 420 }}>
          <Alert
            type="error"
            showIcon
            message={t("auth.sso_login_failed")}
            description={loginError}
          />
          <Button
            type="primary"
            onClick={() => {
              setLoginError(null);
              loginStartedRef.current = false;
              void login("/app");
            }}
          >
            {t("auth.retry_login")}
          </Button>
        </Flex>
      ) : (
        <Space direction="vertical" size={8} align="center">
          <Button ghost onClick={() => navigate("/register")}>
            {t("public_registration.open_registration")}
          </Button>
        </Space>
      )}
    </Flex>
  );
}
