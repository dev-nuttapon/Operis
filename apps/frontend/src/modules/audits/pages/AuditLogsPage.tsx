import { useEffect, useMemo, useState } from "react";
import { Alert, App, Button, Card, DatePicker, Form, Input, Modal, Select, Space, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SortOrder, SorterResult } from "antd/es/table/interface";
import { EyeOutlined, SearchOutlined, SafetyCertificateOutlined } from "@ant-design/icons";
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
              background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)",
              color: "#fff",
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
        width={960}
      >
        {selectedLog ? (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Card size="small" title={t("audit_logs.detail.summary")}>
              <Space direction="vertical" size={8}>
                <Text>{`${t("audit_logs.columns.module")}: ${selectedLog.module}`}</Text>
                <Text>{`${t("audit_logs.columns.action")}: ${selectedLog.action}`}</Text>
                <Text>{`${t("audit_logs.columns.entity")}: ${selectedLog.entityType} / ${selectedLog.entityId || "-"}`}</Text>
                <Text>{`${t("audit_logs.columns.actor")}: ${selectedLog.actorEmail || selectedLog.actorDisplayName || selectedLog.actorUserId || "-"}`}</Text>
                <Text>{`${t("audit_logs.columns.status")}: ${t(`audit_logs.status.${selectedLog.status}`, { defaultValue: selectedLog.status })}`}</Text>
              </Space>
            </Card>

            <Card size="small" title={t("audit_logs.detail.before")}>
              <Paragraph>
                <pre style={{ margin: 0, whiteSpace: "pre-wrap" }}>{prettyJson(selectedLog.beforeJson)}</pre>
              </Paragraph>
            </Card>

            <Card size="small" title={t("audit_logs.detail.after")}>
              <Paragraph>
                <pre style={{ margin: 0, whiteSpace: "pre-wrap" }}>{prettyJson(selectedLog.afterJson)}</pre>
              </Paragraph>
            </Card>

            <Card size="small" title={t("audit_logs.detail.changes")}>
              <Paragraph>
                <pre style={{ margin: 0, whiteSpace: "pre-wrap" }}>{prettyJson(selectedLog.changesJson)}</pre>
              </Paragraph>
            </Card>

            <Card size="small" title={t("audit_logs.detail.metadata")}>
              <Paragraph>
                <pre style={{ margin: 0, whiteSpace: "pre-wrap" }}>{prettyJson(selectedLog.metadataJson)}</pre>
              </Paragraph>
            </Card>
          </Space>
        ) : null}
      </Modal>
    </Space>
  );
}
