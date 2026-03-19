import { useMemo, useState } from "react";
import { Alert, Button, Card, Space, Table, Typography, Skeleton, Grid } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ArrowLeftOutlined, FolderOpenOutlined } from "@ant-design/icons";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { useProjectHistory } from "../hooks/useProjectHistory";
import type { ProjectHistoryItem } from "../types/users";

const { Title, Paragraph, Text } = Typography;

type LocationState = {
  projectName?: string;
  from?: string;
};

export function ProjectHistoryPage() {
  const language = useI18nLanguage();
  const navigate = useNavigate();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const { projectId } = useParams<{ projectId: string }>();
  const location = useLocation();
  const locationState = location.state as LocationState | null;
  const permissionState = usePermissions();
  const canReadHistory = permissionState.hasPermission(permissions.activityLogs.read);
  const tr = (key: string) => i18n.t(key, { lng: language });

  const [paging, setPaging] = useState({ page: 1, pageSize: 10 });
  const historyQuery = useProjectHistory(projectId ?? null, paging, canReadHistory);

  const projectLabel = useMemo(() => locationState?.projectName ?? projectId ?? "-", [locationState?.projectName, projectId]);

  const historyItems = useMemo(() => {
    const items = historyQuery.data?.items ?? [];
    return [...items].sort((a, b) => new Date(b.occurredAt).getTime() - new Date(a.occurredAt).getTime());
  }, [historyQuery.data?.items]);

  const historyColumns: ColumnsType<ProjectHistoryItem> = [
    {
      title: tr("projects.history.columns.occurred_at"),
      dataIndex: "occurredAt",
      key: "occurredAt",
      render: (value: string) => new Date(value).toLocaleString(language.startsWith("th") ? "th-TH" : "en-US"),
    },
    {
      title: tr("projects.history.columns.action"),
      dataIndex: "eventType",
      key: "eventType",
      render: (value: string) => {
        const key = `projects.history.actions.${value}`;
        return i18n.exists(key) ? tr(key) : value;
      },
    },
    {
      title: tr("projects.history.columns.actor"),
      dataIndex: "actorDisplayName",
      key: "actor",
      render: (_: string | null, record) => record.actorDisplayName || record.actorEmail || record.actorUserId || "-",
    },
    {
      title: tr("projects.history.columns.reason"),
      dataIndex: "reason",
      key: "reason",
      render: (value: string | null) => value ?? "-",
    },
    {
      title: tr("projects.history.columns.summary"),
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
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(locationState?.from ?? "/app/projects")} block={isMobile}>
          {tr("projects.history_page.back_action")}
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
            <FolderOpenOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>
              {tr("projects.history_page.title")}
            </Title>
            <Text type="secondary">{projectLabel}</Text>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {tr("projects.history_page.description")}
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Title level={4} style={{ margin: "0 0 16px" }}>
          {tr("projects.history_page.list_title")}
        </Title>
        {!canReadHistory ? (
          <Alert type="info" showIcon message={tr("documents.read_only_title")} description={tr("documents.read_only_description")} style={{ marginBottom: 24 }} />
        ) : historyQuery.isLoading && historyItems.length === 0 ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : (
          <Table<ProjectHistoryItem>
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
            locale={{ emptyText: historyQuery.isError ? tr("projects.history.load_failed") : tr("projects.history.empty") }}
            expandable={{
              expandedRowRender: (record) => {
                const before = parseJson(record.beforeJson);
                const after = parseJson(record.afterJson);
                const metadata = parseJson(record.metadataJson);
                return (
                  <Space direction="vertical" size={12} style={{ width: "100%" }}>
                    <div>
                      <Text strong>{tr("projects.history.columns.before")}</Text>
                      <pre style={{ margin: "6px 0 0", whiteSpace: "pre-wrap" }}>
                        {before ? JSON.stringify(before, null, 2) : "-"}
                      </pre>
                    </div>
                    <div>
                      <Text strong>{tr("projects.history.columns.after")}</Text>
                      <pre style={{ margin: "6px 0 0", whiteSpace: "pre-wrap" }}>
                        {after ? JSON.stringify(after, null, 2) : "-"}
                      </pre>
                    </div>
                    <div>
                      <Text strong>{tr("projects.history.columns.metadata")}</Text>
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
