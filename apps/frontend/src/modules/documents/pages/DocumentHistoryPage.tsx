import { useMemo, useState } from "react";
import { Alert, Button, Card, Space, Table, Typography, Skeleton } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ArrowLeftOutlined, FileTextOutlined } from "@ant-design/icons";
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
  from?: string;
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

  const [paging, setPaging] = useState({ page: 1, pageSize: 10 });
  const historyQuery = useDocumentHistory(documentId ?? null, paging, canReadHistory);

  const documentLabel = useMemo(() => locationState?.documentName ?? documentId ?? "-", [locationState?.documentName, documentId]);

  const historyItems = useMemo(() => {
    const items = historyQuery.data?.items ?? [];
    return [...items].sort((a, b) => new Date(b.occurredAt).getTime() - new Date(a.occurredAt).getTime());
  }, [historyQuery.data?.items]);

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
      title: tr("documents.history.columns.reason"),
      dataIndex: "reason",
      key: "reason",
      render: (value: string | null) => value ?? "-",
    },
    {
      title: tr("documents.history.columns.summary"),
      dataIndex: "summary",
      key: "summary",
      render: (value: string | null) => value ?? "-",
    },
  ];

  const parseJson = (value: string | null) => {
    if (!value) {
      return null;
    }
    try {
      return JSON.parse(value);
    } catch {
      return value;
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button
          icon={<ArrowLeftOutlined />}
          onClick={() =>
            navigate(locationState?.from ?? `/app/documents/${documentId}/versions`, {
              state: { documentName: documentLabel },
            })
          }
        >
          {tr("documents.history_page.back_action")}
        </Button>
      </Space>

      <Card variant="borderless">
        <Space align="start" size={16}>
          <div
            style={{
              width: 48,
              height: 48,
              borderRadius: 14,
              display: "grid",
              placeItems: "center",
              background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)",
              color: "#fff",
            }}
          >
            <FileTextOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>
              {tr("documents.history_page.title")}
            </Title>
            <Text type="secondary">{documentLabel}</Text>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {tr("documents.history_page.description")}
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Title level={4} style={{ margin: "0 0 16px" }}>
          {tr("documents.history_page.list_title")}
        </Title>
        {!canReadHistory ? (
          <Alert type="info" showIcon message={tr("documents.read_only_title")} description={tr("documents.read_only_description")} style={{ marginBottom: 24 }} />
        ) : historyQuery.isLoading && historyItems.length === 0 ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : (
          <Table<DocumentHistoryItem>
            rowKey="id"
            loading={historyQuery.isLoading}
            columns={historyColumns}
            dataSource={historyItems}
            pagination={{
              current: historyQuery.data?.page ?? paging.page,
              pageSize: historyQuery.data?.pageSize ?? paging.pageSize,
              total: historyQuery.data?.total ?? 0,
              showSizeChanger: true,
              pageSizeOptions: [10, 25, 50, 100],
              onChange: (page, pageSize) => setPaging({ page, pageSize }),
            }}
            scroll={{ x: "max-content" }}
            locale={{ emptyText: historyQuery.isError ? tr("documents.load_failed") : tr("documents.history.empty") }}
            expandable={{
              expandedRowRender: (record) => {
                const before = parseJson(record.beforeJson);
                const after = parseJson(record.afterJson);
                const metadata = parseJson(record.metadataJson);
                return (
                  <Space direction="vertical" size={12} style={{ width: "100%" }}>
                    <div>
                      <Text strong>{tr("documents.history.columns.before")}</Text>
                      <pre style={{ margin: "6px 0 0", whiteSpace: "pre-wrap" }}>
                        {before ? JSON.stringify(before, null, 2) : "-"}
                      </pre>
                    </div>
                    <div>
                      <Text strong>{tr("documents.history.columns.after")}</Text>
                      <pre style={{ margin: "6px 0 0", whiteSpace: "pre-wrap" }}>
                        {after ? JSON.stringify(after, null, 2) : "-"}
                      </pre>
                    </div>
                    <div>
                      <Text strong>{tr("documents.history.columns.metadata")}</Text>
                      <pre style={{ margin: "6px 0 0", whiteSpace: "pre-wrap" }}>
                        {metadata ? JSON.stringify(metadata, null, 2) : "-"}
                      </pre>
                    </div>
                  </Space>
                );
              },
              rowExpandable: (record) => Boolean(record.beforeJson || record.afterJson || record.metadataJson),
            }}
          />
        )}
      </Card>
    </Space>
  );
}
