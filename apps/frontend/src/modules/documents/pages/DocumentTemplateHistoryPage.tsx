import { useMemo, useState } from "react";
import { Alert, Button, Card, Divider, Space, Table, Typography, Skeleton } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ArrowLeftOutlined } from "@ant-design/icons";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { useDocumentTemplateHistory } from "../hooks/useDocumentTemplates";
import type { DocumentTemplateHistoryItem } from "../types/documentTemplates";

const { Title, Paragraph, Text } = Typography;

type LocationState = {
  templateName?: string;
  from?: string;
};

export function DocumentTemplateHistoryPage() {
  const language = useI18nLanguage();
  const navigate = useNavigate();
  const { templateId } = useParams<{ templateId: string }>();
  const location = useLocation();
  const locationState = location.state as LocationState | null;
  const permissionState = usePermissions();
  const canReadHistory = permissionState.hasPermission(permissions.activityLogs.read);
  const tr = (key: string) => i18n.t(key, { lng: language });

  const [paging, setPaging] = useState({ page: 1, pageSize: 10 });
  const historyQuery = useDocumentTemplateHistory(templateId ?? null, paging, canReadHistory);

  const templateLabel = useMemo(
    () => locationState?.templateName ?? templateId ?? "-",
    [locationState?.templateName, templateId],
  );

  const historyItems = useMemo(() => {
    const items = historyQuery.data?.items ?? [];
    return [...items].sort((a, b) => new Date(b.occurredAt).getTime() - new Date(a.occurredAt).getTime());
  }, [historyQuery.data?.items]);

  const historyColumns: ColumnsType<DocumentTemplateHistoryItem> = [
    {
      title: tr("documents.templates.history.columns.occurred_at"),
      dataIndex: "occurredAt",
      key: "occurredAt",
      render: (value: string) => new Date(value).toLocaleString(language.startsWith("th") ? "th-TH" : "en-US"),
    },
    {
      title: tr("documents.templates.history.columns.action"),
      dataIndex: "eventType",
      key: "eventType",
      render: (value: string) => {
        const key = `documents.templates.history.actions.${value}`;
        return i18n.exists(key) ? tr(key) : value;
      },
    },
    {
      title: tr("documents.templates.history.columns.actor"),
      dataIndex: "actorDisplayName",
      key: "actor",
      render: (_: string | null, record) => record.actorDisplayName || record.actorEmail || record.actorUserId || "-",
    },
    {
      title: tr("documents.templates.history.columns.reason"),
      dataIndex: "reason",
      key: "reason",
      render: (value: string | null) => value ?? "-",
    },
    {
      title: tr("documents.templates.history.columns.summary"),
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
    <Card bordered={false} style={{ borderRadius: 16 }}>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <div>
          <Title level={2} style={{ margin: 0 }}>{tr("documents.templates.history_page.title")}</Title>
          <Text type="secondary">{templateLabel}</Text>
        </div>
        <Space>
          <Button
            icon={<ArrowLeftOutlined />}
            onClick={() =>
              navigate(locationState?.from ?? "/app/document-templates", {
                state: { templateName: templateLabel },
              })
            }
          >
            {tr("documents.templates.history_page.back_action")}
          </Button>
        </Space>
      </Space>

      <Paragraph type="secondary">{tr("documents.templates.history_page.description")}</Paragraph>

      <Divider />

      {!canReadHistory ? (
        <Alert type="info" showIcon message={tr("documents.read_only_title")} description={tr("documents.read_only_description")} style={{ marginBottom: 24 }} />
      ) : historyQuery.isLoading && historyItems.length === 0 ? (
        <Skeleton active paragraph={{ rows: 6 }} />
      ) : (
        <Table<DocumentTemplateHistoryItem>
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
          locale={{ emptyText: historyQuery.isError ? tr("documents.templates.history.empty") : tr("documents.templates.history.empty") }}
          expandable={{
            expandedRowRender: (record) => {
              const before = parseJson(record.beforeJson);
              const after = parseJson(record.afterJson);
              const metadata = parseJson(record.metadataJson);
              return (
                <Space direction="vertical" size={12} style={{ width: "100%" }}>
                  <div>
                    <Text strong>{tr("documents.templates.history.columns.before")}</Text>
                    <pre style={{ margin: "6px 0 0", whiteSpace: "pre-wrap" }}>
                      {before ? JSON.stringify(before, null, 2) : "-"}
                    </pre>
                  </div>
                  <div>
                    <Text strong>{tr("documents.templates.history.columns.after")}</Text>
                    <pre style={{ margin: "6px 0 0", whiteSpace: "pre-wrap" }}>
                      {after ? JSON.stringify(after, null, 2) : "-"}
                    </pre>
                  </div>
                  <div>
                    <Text strong>{tr("documents.templates.history.columns.metadata")}</Text>
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
  );
}
