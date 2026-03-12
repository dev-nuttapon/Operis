import { Typography, Card, Button, Space, Divider, List, Tag } from "antd";
import { LogoutOutlined } from "@ant-design/icons";
import { useAuth } from "../../../modules/auth";
import { DocumentTestForm } from "../components/DocumentTestForm";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";
import { useDocumentDashboard } from "../hooks/useDocumentDashboard";

const { Title, Paragraph } = Typography;

export function DocumentDashboardPage() {
  const { logout, isAuthenticated } = useAuth();
  const language = useI18nLanguage();
  const { documentsQuery, latestDocuments, submittedData, setSubmittedData } = useDocumentDashboard();
  const tr = (key: string) => i18n.t(key, { lng: language });

  return (
    <Card bordered={false} style={{ borderRadius: 16 }}>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <Title level={2} style={{ margin: 0 }}>{tr("documents.page_title")}</Title>
        {isAuthenticated ? (
          <Button danger icon={<LogoutOutlined />} onClick={() => void logout()}>
            {tr("auth.logout_button")}
          </Button>
        ) : null}
      </Space>
      <Paragraph>
        {tr("documents.welcome")}
      </Paragraph>

      <Divider />

      <Title level={4} style={{ marginTop: 0 }}>
        Latest Documents
      </Title>
      <List
        loading={documentsQuery.isLoading}
        dataSource={latestDocuments}
        locale={{ emptyText: documentsQuery.isError ? "Unable to load documents." : "No documents yet." }}
        renderItem={(item) => (
          <List.Item>
            <Space style={{ width: "100%", justifyContent: "space-between" }}>
              <span>{item.fileName}</span>
              <Tag>{new Date(item.uploadedAt).toLocaleDateString(language.startsWith("th") ? "th-TH" : "en-US")}</Tag>
            </Space>
          </List.Item>
        )}
        style={{ marginBottom: 24 }}
      />

      <Title level={4} style={{ marginTop: 0 }}>
        {tr("documents.test_form_title")}
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
            {tr("documents.last_payload")}
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
