import { useState } from "react";
import { Typography, Card, Button, Space, Divider } from "antd";
import { LogoutOutlined } from "@ant-design/icons";
import { useAuth } from "../../../modules/auth/hooks/useAuth";
import { DocumentTestForm } from "../components/DocumentTestForm";
import type { DocumentFormValues } from "../types/documentForm";
import i18n from "../../../shared/i18n/config";

const { Title, Paragraph } = Typography;

export function DocumentDashboardPage() {
  const { logout, isAuthenticated } = useAuth();
  const [submittedData, setSubmittedData] = useState<DocumentFormValues | null>(null);
  const tr = (key: string, fallback: string) => i18n.t(key, { defaultValue: fallback });

  return (
    <Card bordered={false} style={{ borderRadius: 16 }}>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <Title level={2} style={{ margin: 0 }}>{tr("documents.page_title", "Document Dashboard")}</Title>
        {isAuthenticated ? (
          <Button danger icon={<LogoutOutlined />} onClick={() => void logout()}>
            {tr("auth.logout_button", "Logout")}
          </Button>
        ) : null}
      </Space>
      <Paragraph>
        {tr("documents.welcome", "Welcome to the Paperless Document System.")}
      </Paragraph>

      <Divider />

      <Title level={4} style={{ marginTop: 0 }}>
        {tr("documents.test_form_title", "Documents Test Form")}
      </Title>
      <DocumentTestForm
        onSubmit={(values) => {
          setSubmittedData(values);
        }}
      />

      {submittedData ? (
        <>
          <Divider />
          <Title level={5} style={{ marginTop: 0 }}>
            {tr("documents.last_payload", "Last Submitted Payload")}
          </Title>
          <pre
            style={{
              margin: 0,
              padding: 12,
              borderRadius: 8,
              background: "rgba(15, 23, 42, 0.06)",
              overflowX: "auto",
            }}
          >
            {JSON.stringify(submittedData, null, 2)}
          </pre>
        </>
      ) : null}
    </Card>
  );
}
