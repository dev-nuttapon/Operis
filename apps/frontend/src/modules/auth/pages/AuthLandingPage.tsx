import { Button, Card, Divider, Space, Tag, Typography, Select, Flex } from "antd";
import { useTranslation } from "react-i18next";
import { appEnv } from "../../../shared/config/env";
import { useAuth } from "../hooks/useAuth";
import { useThemeStore, ThemeMode } from "../../../shared/store/useThemeStore";
import { useEffect } from "react";

const { Title, Paragraph, Text } = Typography;

export function AuthLandingPage() {
  const { isAuthenticated, isReady, login, logout } = useAuth();
  const { theme, setTheme } = useThemeStore();
  const { t, i18n } = useTranslation();

  // Keep html data-theme sync for global CSS conditional
  useEffect(() => {
     let actualTheme = theme;
     if (theme === 'system') {
        const isDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
        actualTheme = isDark ? 'dark' : 'light';
     }
     document.documentElement.setAttribute('data-theme', actualTheme);
  }, [theme]);


  const handleLanguageChange = (val: string) => {
    i18n.changeLanguage(val);
  };

  const currentLang = i18n.language.startsWith('th') ? 'th' : 'en';

  return (
    <div style={{ minHeight: "100vh", display: "flex", flexDirection: "column" }}>
      {/* Header controls */}
      <Flex justify="flex-end" align="center" style={{ padding: "24px 32px" }} gap="middle">
        <Space>
          <span style={{ color: "rgba(15, 23, 42, 0.72)", fontSize: 13, fontWeight: 600 }}>
            {t("common.language")}
          </span>
          <Select 
            value={currentLang} 
            onChange={handleLanguageChange}
            variant="borderless"
            options={[
              { value: 'en', label: t('common.language_en') },
              { value: 'th', label: t('common.language_th') }
            ]}
          />
        </Space>
        
        <Select 
          value={theme}
          onChange={(v: ThemeMode) => setTheme(v)}
          variant="borderless"
          options={[
            { value: 'light', label: t('common.theme.light') },
            { value: 'dark', label: t('common.theme.dark') },
            { value: 'system', label: t('common.theme.system') }
          ]}
        />
      </Flex>

      {/* Main Content */}
      <Flex flex={1} justify="center" align="center" style={{ padding: "0 16px" }}>
        <Card 
          className="glass-panel"
          style={{ 
            maxWidth: 600, 
            width: "100%", 
            borderRadius: 24, 
            textAlign: "center",
            padding: "24px 0"
          }}
          variant="borderless"
        >
          <Title level={2} style={{ marginBottom: 8, fontWeight: 700 }}>
            {t("auth.welcome_title")}
          </Title>
          <Paragraph type="secondary" style={{ fontSize: 16, marginBottom: 32 }}>
            {t("auth.welcome_subtitle")}
          </Paragraph>

          <Space orientation="vertical" size="large" style={{ width: '100%', padding: '0 32px' }}>
            
            {/* Status and Action block */}
            <div style={{ padding: "24px", background: "rgba(0,0,0,0.02)", borderRadius: 16 }}>
              <Space orientation="vertical" size="middle" style={{ width: "100%" }}>
                <Tag 
                  color={isAuthenticated ? "success" : "default"}
                  style={{ padding: "4px 12px", borderRadius: 12, fontSize: 14 }}
                >
                  {isAuthenticated ? t('auth.status_authenticated') : t('auth.status_anonymous')}
                </Tag>

                {!isAuthenticated ? (
                  <Button 
                    type="primary" 
                    size="large" 
                    onClick={() => void login()} 
                    disabled={!isReady}
                    style={{ width: "100%", height: 48, borderRadius: 24, fontWeight: 600 }}
                  >
                    {t('auth.login_button')}
                  </Button>
                ) : (
                  <Button 
                    size="large"
                    danger
                    onClick={() => void logout()} 
                    disabled={!isReady}
                    style={{ width: "100%", height: 48, borderRadius: 24, fontWeight: 600 }}
                  >
                    {t('auth.logout_button')}
                  </Button>
                )}
              </Space>
            </div>

            <Divider dashed style={{ borderColor: 'rgba(150,150,150,0.3)' }} />

            {/* Diagnostic Information */}
            <div style={{ textAlign: "left", opacity: 0.85 }}>
              <Paragraph style={{ margin: 0, padding: "4px 0" }}>
                <Text type="secondary">{t('auth.env_label')}: </Text>
                <Text strong>{appEnv.mode}</Text>
              </Paragraph>
              <Paragraph style={{ margin: 0, padding: "4px 0" }}>
                <Text type="secondary">{t('auth.api_url_label')}: </Text>
                <Text code>{appEnv.apiBaseUrl}</Text>
              </Paragraph>
              <Paragraph style={{ margin: 0, padding: "4px 0" }}>
                <Text type="secondary">{t('auth.auth_url_label')}: </Text>
                <Text code>{appEnv.authBaseUrl}</Text>
              </Paragraph>
              <Paragraph style={{ margin: 0, padding: "4px 0" }}>
                <Text type="secondary">{t('auth.realm_label')}: </Text>
                <Text code>{appEnv.keycloakRealm}</Text>
              </Paragraph>
              <Paragraph style={{ margin: 0, padding: "4px 0" }}>
                <Text type="secondary">{t('auth.client_label')}: </Text>
                <Text code>{appEnv.keycloakClientId}</Text>
              </Paragraph>
            </div>

          </Space>
        </Card>
      </Flex>
    </div>
  );
}
