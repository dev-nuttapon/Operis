import { useEffect, useMemo, useState } from "react";
import { Alert, App, Button, Card, DatePicker, Form, Input, Modal, Select, Space, Table, Tag, Typography, theme } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SortOrder, SorterResult } from "antd/es/table/interface";
import { EyeOutlined, SearchOutlined, SafetyCertificateOutlined, HistoryOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useTranslation } from "react-i18next";
import { ApiError, getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useAuditLogs } from "../hooks/useAuditLogs";
import type { AuditLogItem, ListAuditLogsInput } from "../types/audits";

const { RangePicker } = DatePicker;
const { Text, Paragraph } = Typography;

function formatDate(value: string, language: string) {
  return new Intl.DateTimeFormat(language.startsWith("th") ? "th-TH" : "en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function prettyJson(value: string | null) {
  if (!value) {
    return "-";
  }

  try {
    return JSON.stringify(JSON.parse(value), null, 2);
  } catch {
    return value;
  }
}

function getStatusColor(status: string) {
  if (status === "success") {
    return "green";
  }

  if (status === "failed") {
    return "red";
  }

  if (status === "denied") {
    return "gold";
  }

  return "default";
}

function toApiSortOrder(order?: SortOrder): "asc" | "desc" | undefined {
  if (order === "ascend") return "asc";
  if (order === "descend") return "desc";
  return undefined;
}

export function AuditLogsPage() {
  const { t, i18n } = useTranslation();
  const { token } = theme.useToken();
  const { notification } = App.useApp();
  const permissionState = usePermissions();
  const canReadAuditLogs = permissionState.hasPermission(permissions.auditLogs.read);
  const [form] = Form.useForm();
  const [filters, setFilters] = useState<ListAuditLogsInput>({ page: 1, pageSize: 10, sortBy: "occurredAt", sortOrder: "desc" });
  const [selectedLog, setSelectedLog] = useState<AuditLogItem | null>(null);
  const auditLogsQuery = useAuditLogs(filters);

  useEffect(() => {
    if (auditLogsQuery.isError) {
      handleError(auditLogsQuery.error);
    }
  }, [auditLogsQuery.error, auditLogsQuery.isError]);

  const columns = useMemo<ColumnsType<AuditLogItem>>(
    () => [
      {
        title: t("audit_logs.columns.occurred_at"),
        dataIndex: "occurredAt",
        sorter: true,
        render: (value: string) => formatDate(value, i18n.language),
      },
      {
        title: t("audit_logs.columns.module"),
        dataIndex: "module",
        sorter: true,
      },
      {
        title: t("audit_logs.columns.action"),
        dataIndex: "action",
        sorter: true,
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
        title: t("audit_logs.columns.status"),
        dataIndex: "status",
        sorter: true,
        render: (status: string) => (
          <Tag color={getStatusColor(status)}>{t(`audit_logs.status.${status}`, { defaultValue: status })}</Tag>
        ),
      },
      {
        title: t("audit_logs.columns.request"),
        key: "request",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Text>{item.httpMethod || "-"}</Text>
            <Text type="secondary">{item.requestPath || "-"}</Text>
          </Space>
        ),
      },
      {
        title: t("audit_logs.columns.actions"),
        key: "actions",
        render: (_, item) => (
          <Button icon={<EyeOutlined />} onClick={() => setSelectedLog(item)}>
            {t("audit_logs.actions.view")}
          </Button>
        ),
      },
    ],
    [i18n.language, t]
  );

  const handleSearch = (values: {
    module?: string;
    action?: string;
    entityType?: string;
    entityId?: string;
    actor?: string;
    status?: string;
    range?: [dayjs.Dayjs, dayjs.Dayjs];
  }) => {
    setFilters({
      module: values.module || undefined,
      action: values.action || undefined,
      entityType: values.entityType || undefined,
      entityId: values.entityId || undefined,
      actor: values.actor || undefined,
      status: values.status || undefined,
      from: values.range?.[0]?.startOf("day").toISOString(),
      to: values.range?.[1]?.endOf("day").toISOString(),
      page: 1,
      pageSize: filters.pageSize ?? 10,
    });
  };

  const handleReset = () => {
    form.resetFields();
    setFilters({ page: 1, pageSize: 10, sortBy: "occurredAt", sortOrder: "desc" });
  };

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
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
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
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("audit_logs.title")}
            </Typography.Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("audit_logs.description")}
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canReadAuditLogs ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} style={{ marginBottom: 16 }} />
        ) : null}

        <Form form={form} layout="vertical" onFinish={handleSearch}>
          <Space wrap size={16} align="end" style={{ marginBottom: 16 }}>
            <Form.Item name="module" label={t("audit_logs.filters.module")}>
              <Input placeholder={t("audit_logs.placeholders.module")} />
            </Form.Item>
            <Form.Item name="action" label={t("audit_logs.filters.action")}>
              <Input placeholder={t("audit_logs.placeholders.action")} />
            </Form.Item>
            <Form.Item name="entityType" label={t("audit_logs.filters.entity_type")}>
              <Input placeholder={t("audit_logs.placeholders.entity_type")} />
            </Form.Item>
            <Form.Item name="entityId" label={t("audit_logs.filters.entity_id")}>
              <Input placeholder={t("audit_logs.placeholders.entity_id")} />
            </Form.Item>
            <Form.Item name="actor" label={t("audit_logs.filters.actor")}>
              <Input placeholder={t("audit_logs.placeholders.actor")} />
            </Form.Item>
            <Form.Item name="status" label={t("audit_logs.filters.status")}>
              <Select
                allowClear
                style={{ width: 160 }}
                options={[
                  { value: "success", label: t("audit_logs.status.success") },
                  { value: "failed", label: t("audit_logs.status.failed") },
                  { value: "denied", label: t("audit_logs.status.denied") },
                ]}
              />
            </Form.Item>
            <Form.Item name="range" label={t("audit_logs.filters.date_range")}>
              <RangePicker />
            </Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" icon={<SearchOutlined />}>
                {t("audit_logs.actions.search")}
              </Button>
              <Button onClick={handleReset}>{t("audit_logs.actions.reset")}</Button>
            </Space>
          </Space>
        </Form>

        <Table<AuditLogItem>
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
          onChange={(pagination, _, sorter) => {
            const sort = sorter as SorterResult<AuditLogItem>;
            setFilters((current) => ({
              ...current,
              page: pagination.current ?? current.page,
              pageSize: pagination.pageSize ?? current.pageSize,
              sortBy: typeof sort.field === "string" ? sort.field : current.sortBy,
              sortOrder: toApiSortOrder(sort.order) ?? current.sortOrder,
            }));
          }}
        />
      </Card>

      <Modal
        open={selectedLog !== null}
        title={t("audit_logs.detail.title")}
        onCancel={() => setSelectedLog(null)}
        footer={[
          <Button key="close" onClick={() => setSelectedLog(null)}>
            {t("audit_logs.actions.close")}
          </Button>,
        ]}
        width={1000}
        styles={{ body: { padding: "20px 0" } }}
      >
        {selectedLog ? (
          <Space direction="vertical" size={24} style={{ width: "100%" }}>
            {/* Error Message Alert if failed */}
            {selectedLog.status !== "success" && (selectedLog.errorCode || selectedLog.errorMessage) && (
              <Alert
                type="error"
                showIcon
                message={selectedLog.errorCode || t("audit_logs.status.failed")}
                description={selectedLog.errorMessage}
                style={{ margin: "0 24px" }}
              />
            )}

            <div style={{ padding: "0 24px" }}>
              <Typography.Title level={5} style={{ marginBottom: 16 }}>
                <HistoryOutlined style={{ marginRight: 8 }} />
                {t("audit_logs.detail.summary")}
              </Typography.Title>
              <Card size="small" variant="outlined" style={{ background: token.colorFillAlter }}>
                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "16px 32px" }}>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 12 }}>{t("audit_logs.columns.occurred_at")}</Text>
                    <Text strong>{formatDate(selectedLog.occurredAt, i18n.language)}</Text>
                  </Space>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 12 }}>{t("audit_logs.columns.status")}</Text>
                    <Tag color={getStatusColor(selectedLog.status)} style={{ margin: 0 }}>
                      {t(`audit_logs.status.${selectedLog.status}`, { defaultValue: selectedLog.status })}
                    </Tag>
                  </Space>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 12 }}>{t("audit_logs.columns.module")}</Text>
                    <Text>{selectedLog.module}</Text>
                  </Space>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 12 }}>{t("audit_logs.columns.action")}</Text>
                    <Tag>{selectedLog.action.toUpperCase()}</Tag>
                  </Space>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 12 }}>{t("audit_logs.columns.entity")}</Text>
                    <Text>{`${selectedLog.entityType} (${selectedLog.entityId || "-"})`}</Text>
                  </Space>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 12 }}>{t("audit_logs.columns.actor")}</Text>
                    <Text>{selectedLog.actorEmail || selectedLog.actorDisplayName || selectedLog.actorUserId || "-"}</Text>
                  </Space>
                </div>
              </Card>
            </div>

            <div style={{ padding: "0 24px" }}>
              <Typography.Title level={5} style={{ marginBottom: 16 }}>
                <SearchOutlined style={{ marginRight: 8 }} />
                {t("audit_logs.detail.metadata")}
              </Typography.Title>
              <Card size="small" variant="outlined">
                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: "12px" }}>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 11 }}>{t("audit_logs.detail.http_method")}</Text>
                    <Text style={{ fontSize: 13 }}>{selectedLog.httpMethod || "-"}</Text>
                  </Space>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 11 }}>{t("audit_logs.detail.status_code")}</Text>
                    <Text style={{ fontSize: 13 }}>{selectedLog.statusCode || "-"}</Text>
                  </Space>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 11 }}>{t("audit_logs.detail.ip_address")}</Text>
                    <Text style={{ fontSize: 13 }}>{selectedLog.ipAddress || "-"}</Text>
                  </Space>
                  <div style={{ gridColumn: "span 3" }}>
                    <Space direction="vertical" size={0} style={{ width: "100%" }}>
                      <Text type="secondary" style={{ fontSize: 11 }}>{t("audit_logs.detail.request_id")}</Text>
                      <Text copyable style={{ fontSize: 12, fontFamily: "monospace" }}>{selectedLog.requestId || "-"}</Text>
                    </Space>
                  </div>
                  <div style={{ gridColumn: "span 3" }}>
                    <Space direction="vertical" size={0} style={{ width: "100%" }}>
                      <Text type="secondary" style={{ fontSize: 11 }}>{t("audit_logs.detail.trace_id")}</Text>
                      <Text copyable style={{ fontSize: 12, fontFamily: "monospace" }}>{selectedLog.traceId || "-"}</Text>
                    </Space>
                  </div>
                  <div style={{ gridColumn: "span 3" }}>
                    <Space direction="vertical" size={0} style={{ width: "100%" }}>
                      <Text type="secondary" style={{ fontSize: 11 }}>{t("audit_logs.detail.user_agent")}</Text>
                      <Text style={{ fontSize: 12 }}>{selectedLog.userAgent || "-"}</Text>
                    </Space>
                  </div>
                </div>
              </Card>
            </div>

            <div style={{ padding: "0 24px" }}>
              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "24px" }}>
                <div>
                  <Typography.Title level={5} style={{ marginBottom: 8 }}>{t("audit_logs.detail.before")}</Typography.Title>
                  <Card size="small" style={{ maxHeight: 300, overflow: "auto", background: token.colorFillTertiary, border: `1px solid ${token.colorBorderSecondary}` }}>
                    <pre style={{ margin: 0, fontSize: 12 }}>{prettyJson(selectedLog.beforeJson)}</pre>
                  </Card>
                </div>
                <div>
                  <Typography.Title level={5} style={{ marginBottom: 8 }}>{t("audit_logs.detail.after")}</Typography.Title>
                  <Card size="small" style={{ maxHeight: 300, overflow: "auto", background: token.colorFillTertiary, border: `1px solid ${token.colorBorderSecondary}` }}>
                    <pre style={{ margin: 0, fontSize: 12 }}>{prettyJson(selectedLog.afterJson)}</pre>
                  </Card>
                </div>
              </div>
            </div>

            {selectedLog.changesJson && selectedLog.changesJson !== "{}" && (
              <div style={{ padding: "0 24px" }}>
                <Typography.Title level={5} style={{ marginBottom: 8 }}>{t("audit_logs.detail.changes")}</Typography.Title>
                <Card size="small" style={{ background: token.colorWarningBg, border: `1px solid ${token.colorWarningBorder}` }}>
                  <pre style={{ margin: 0, fontSize: 12 }}>{prettyJson(selectedLog.changesJson)}</pre>
                </Card>
              </div>
            )}

            {selectedLog.metadataJson && selectedLog.metadataJson !== "{}" && (
              <div style={{ padding: "0 24px" }}>
                <Typography.Title level={5} style={{ marginBottom: 8 }}>{t("audit_logs.detail.metadata")}</Typography.Title>
                <Card size="small" style={{ background: token.colorInfoBg, border: `1px solid ${token.colorInfoBorder}` }}>
                  <pre style={{ margin: 0, fontSize: 12 }}>{prettyJson(selectedLog.metadataJson)}</pre>
                </Card>
              </div>
            )}
          </Space>
        ) : null}
      </Modal>
    </Space>
  );
}
