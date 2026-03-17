import { useMemo } from "react";
import { Alert, Button, Card, Divider, Space, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ArrowLeftOutlined } from "@ant-design/icons";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { useDocumentHistory } from "../hooks/useDocuments";
import type { DocumentHistoryItem } from "../api/documentsApi";

const { Title, Paragraph, Text } = Typography;

type LocationState = {
  documentName?: string;
};

export function DocumentHistoryPage() {
  const language = useI18nLanguage();
  const navigate = useNavigate();
  const { documentId } = useParams<{ documentId: string }>();
  const location = useLocation();
  const locationState = location.state as LocationState | null;
  const permissionState = usePermissions();
  const canReadHistory = permissionState.hasPermission(permissions.activityLogs.read);
  const tr = (key: string) => i18n.t(key, { lng: language });

  const historyQuery = useDocumentHistory(documentId ?? null, canReadHistory);

  const documentLabel = useMemo(() => locationState?.documentName ?? documentId ?? "-", [locationState?.documentName, documentId]);

  const historyItems = useMemo(() => {
    const items = historyQuery.data ?? [];
    return [...items].sort((a, b) => new Date(b.occurredAt).getTime() - new Date(a.occurredAt).getTime());
  }, [historyQuery.data]);

  const historyColumns: ColumnsType<DocumentHistoryItem> = [
    {
      title: tr("documents.history.columns.occurred_at"),
      dataIndex: "occurredAt",
      key: "occurredAt",
      render: (value: string) => new Date(value).toLocaleString(language.startsWith("th") ? "th-TH" : "en-US"),
    },
    {
      title: tr("documents.history.columns.action"),
      dataIndex: "eventType",
      key: "eventType",
      render: (value: string) => {
        const key = `documents.history.actions.${value}`;
        return i18n.exists(key) ? tr(key) : value;
      },
    },
    {
      title: tr("documents.history.columns.actor"),
      dataIndex: "actorDisplayName",
      key: "actor",
      render: (_: string | null, record) => record.actorDisplayName || record.actorEmail || record.actorUserId || "-",
    },
    {
      title: tr("documents.history.columns.status"),
      dataIndex: "status",
      key: "status",
      align: "center",
      render: (value: string) => {
        const normalized = value?.toLowerCase() ?? "";
        const labelKey = `documents.history.status.${normalized}`;
        const label = i18n.exists(labelKey) ? tr(labelKey) : value;
        const color = normalized === "success" ? "green" : normalized === "denied" ? "orange" : normalized === "failed" ? "red" : "default";
        return <Tag color={color}>{label}</Tag>;
      },
    },
    {
      title: tr("documents.history.columns.reason"),
      dataIndex: "reason",
      key: "reason",
      render: (value: string | null) => value ?? "-",
    },
  ];

  return (
    <Card bordered={false} style={{ borderRadius: 16 }}>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <div>
          <Title level={2} style={{ margin: 0 }}>{tr("documents.history_page.title")}</Title>
          <Text type="secondary">{documentLabel}</Text>
        </div>
        <Space>
          <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(`/app/documents/${documentId}/versions`, { state: { documentName: documentLabel } })}>
            {tr("documents.history_page.back_action")}
          </Button>
        </Space>
      </Space>

      <Paragraph type="secondary">{tr("documents.history_page.description")}</Paragraph>

      <Divider />

      {!canReadHistory ? (
        <Alert type="info" showIcon message={tr("documents.read_only_title")} description={tr("documents.read_only_description")} style={{ marginBottom: 24 }} />
      ) : (
        <Table<ActivityLogItem>
          rowKey="id"
          loading={historyQuery.isLoading}
          columns={historyColumns}
          dataSource={historyItems}
          pagination={false}
          scroll={{ x: "max-content" }}
          locale={{ emptyText: historyQuery.isError ? tr("documents.load_failed") : tr("documents.history.empty") }}
        />
      )}
    </Card>
  );
}
