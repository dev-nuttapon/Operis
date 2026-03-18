import { useEffect, useMemo, useRef, useState } from "react";
import { Alert, App, Card, Divider, Space, Table, Tag, Typography, Skeleton, theme } from "antd";
import type { ColumnsType } from "antd/es/table";
import { SafetyCertificateOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useSearchParams } from "react-router-dom";
import { ApiError, getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useAuditLogs } from "../hooks/useAuditLogs";
import type { BusinessAuditEventItem, ListAuditLogsInput } from "../types/audits";

const { Text, Paragraph, Title } = Typography;

function formatDate(value: string, language: string) {
  return new Intl.DateTimeFormat(language.startsWith("th") ? "th-TH" : "en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

export function AuditLogsPage() {
  const { t, i18n } = useTranslation();
  const { token } = theme.useToken();
  const { notification } = App.useApp();
  const [searchParams] = useSearchParams();
  const initializedRef = useRef(false);
  const permissionState = usePermissions();
  const canReadAuditLogs = permissionState.hasPermission(permissions.auditLogs.read);
  const [filters, setFilters] = useState<ListAuditLogsInput>({ page: 1, pageSize: 10, sortBy: "occurredAt", sortOrder: "desc" });
  const auditLogsQuery = useAuditLogs(filters);

  useEffect(() => {
    if (initializedRef.current) return;
    const entityType = searchParams.get("entityType") ?? undefined;
    const entityId = searchParams.get("entityId") ?? undefined;
    if (entityType || entityId) {
      setFilters((current) => ({
        ...current,
        entityType,
        entityId,
        page: 1,
        pageSize: current.pageSize ?? 10,
      }));
    }
    initializedRef.current = true;
  }, [searchParams]);

  useEffect(() => {
    if (auditLogsQuery.isError) {
      handleError(auditLogsQuery.error);
    }
  }, [auditLogsQuery.error, auditLogsQuery.isError]);

  const parseJson = (value: string | null) => {
    if (!value) return null;
    try {
      return JSON.parse(value);
    } catch {
      return value;
    }
  };

  const columns = useMemo<ColumnsType<BusinessAuditEventItem>>(
    () => [
      {
        title: t("audit_logs.columns.occurred_at"),
        dataIndex: "occurredAt",
        render: (value: string) => formatDate(value, i18n.language),
      },
      {
        title: t("audit_logs.columns.module"),
        dataIndex: "module",
      },
      {
        title: t("audit_logs.columns.event_type"),
        dataIndex: "eventType",
        render: (value: string) => <Tag>{value}</Tag>,
      },
      {
        title: t("audit_logs.columns.entity"),
        key: "entity",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Text>{item.entityType}</Text>
            <Text type="secondary">{item.entityId || "-"}</Text>
          </Space>
        ),
      },
      {
        title: t("audit_logs.columns.actor"),
        key: "actor",
        render: (_, item) => item.actorEmail || item.actorDisplayName || item.actorUserId || "-",
      },
      {
        title: t("audit_logs.columns.summary"),
        dataIndex: "summary",
        ellipsis: true,
        render: (value: string | null) => value || "-",
      },
      {
        title: t("audit_logs.columns.reason"),
        dataIndex: "reason",
        ellipsis: true,
        render: (value: string | null) => value || "-",
      },
    ],
    [i18n.language, t]
  );

  const handleError = (error: unknown) => {
    const presentation =
      error instanceof ApiError
        ? getApiErrorPresentation(error, t("audit_logs.notifications.load_failed_title"))
        : getApiErrorPresentation(error, t("audit_logs.notifications.load_failed_title"));

    notification.error({
      message: presentation.title,
      description: presentation.description,
    });
  };

  return (
    <Card bordered={false} style={{ borderRadius: 16 }}>
      <Space align="start" size={16} style={{ marginBottom: 12 }}>
        <div
          style={{
            width: 48,
            height: 48,
            borderRadius: 14,
            display: "grid",
            placeItems: "center",
            background: `linear-gradient(135deg, ${token.colorPrimary}, ${token.colorPrimaryActive})`,
            color: token.colorWhite,
          }}
        >
          <SafetyCertificateOutlined />
        </div>
        <div>
          <Title level={2} style={{ margin: 0 }}>
            {t("audit_logs.title")}
          </Title>
          <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
            {t("audit_logs.description")}
          </Paragraph>
        </div>
      </Space>

      <Divider />

      {!canReadAuditLogs ? (
        <Alert type="warning" showIcon message={t("errors.title_forbidden")} style={{ marginBottom: 16 }} />
      ) : null}

      {auditLogsQuery.isLoading && (auditLogsQuery.data?.items?.length ?? 0) === 0 ? (
        <Skeleton active paragraph={{ rows: 6 }} />
      ) : (
        <Table<BusinessAuditEventItem>
          rowKey="id"
          columns={columns}
          dataSource={canReadAuditLogs ? (auditLogsQuery.data?.items ?? []) : []}
          loading={canReadAuditLogs ? auditLogsQuery.isLoading : false}
          scroll={{ x: "max-content" }}
          pagination={{
            current: auditLogsQuery.data?.page ?? filters.page ?? 1,
            pageSize: auditLogsQuery.data?.pageSize ?? filters.pageSize ?? 10,
            total: auditLogsQuery.data?.total ?? 0,
            showSizeChanger: true,
            pageSizeOptions: [10, 25, 50, 100],
            onChange: (page, pageSize) =>
              setFilters((current) => ({
                ...current,
                page,
                pageSize,
              })),
          }}
          locale={{
            emptyText: auditLogsQuery.isError ? t("audit_logs.empty_error") : t("audit_logs.empty"),
          }}
          expandable={{
            expandedRowRender: (record) => {
              const metadata = parseJson(record.metadataJson);
              return (
                <Space direction="vertical" size={8} style={{ width: "100%" }}>
                  <Text strong>{t("audit_logs.columns.metadata")}</Text>
                  <pre style={{ margin: 0, whiteSpace: "pre-wrap" }}>
                    {metadata ? JSON.stringify(metadata, null, 2) : "-"}
                  </pre>
                </Space>
              );
            },
            rowExpandable: (record) => Boolean(record.metadataJson),
          }}
        />
      )}
    </Card>
  );
}
