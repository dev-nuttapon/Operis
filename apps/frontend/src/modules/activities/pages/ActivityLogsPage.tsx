import { useEffect, useMemo, useState } from "react";
import { Alert, App, Button, Card, DatePicker, Form, Input, Modal, Select, Space, Table, Tag, Typography, theme } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SortOrder, SorterResult } from "antd/es/table/interface";
import { EyeOutlined, SearchOutlined, HistoryOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useTranslation } from "react-i18next";
import { ApiError, getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useActivityLogs } from "../hooks/useActivityLogs";
import type { ActivityLogItem, ListActivityLogsInput } from "../types/activities";

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

export function ActivityLogsPage() {
  const { t, i18n } = useTranslation();
  const { token } = theme.useToken();
  const { notification } = App.useApp();
  const permissionState = usePermissions();
  const canReadActivityLogs = permissionState.hasPermission(permissions.activityLogs.read);
  const [form] = Form.useForm();
  const [filters, setFilters] = useState<ListActivityLogsInput>({ page: 1, pageSize: 10, sortBy: "occurredAt", sortOrder: "desc" });
  const [selectedActivity, setSelectedActivity] = useState<ActivityLogItem | null>(null);
  const activityLogsQuery = useActivityLogs(filters);

  useEffect(() => {
    if (activityLogsQuery.isError) {
      handleError(activityLogsQuery.error);
    }
  }, [activityLogsQuery.error, activityLogsQuery.isError]);

  const columns = useMemo<ColumnsType<ActivityLogItem>>(
    () => [
      {
        title: t("activity_logs.columns.occurred_at"),
        dataIndex: "occurredAt",
        sorter: true,
        render: (value: string) => formatDate(value, i18n.language),
      },
      {
        title: t("activity_logs.columns.module"),
        dataIndex: "module",
        sorter: true,
      },
      {
        title: t("activity_logs.columns.action"),
        dataIndex: "action",
        sorter: true,
      },
      {
        title: t("activity_logs.columns.entity"),
        key: "entity",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Text>{item.entityType}</Text>
            <Text type="secondary">{item.entityId || "-"}</Text>
          </Space>
        ),
      },
      {
        title: t("activity_logs.columns.actor"),
        key: "actor",
        render: (_, item) => item.actorEmail || item.actorDisplayName || item.actorUserId || "-",
      },
      {
        title: t("activity_logs.columns.status"),
        dataIndex: "status",
        sorter: true,
        render: (status: string) => (
          <Tag color={getStatusColor(status)}>{t(`activity_logs.status.${status}`, { defaultValue: status })}</Tag>
        ),
      },
      {
        title: t("activity_logs.columns.request"),
        key: "request",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Text>{item.httpMethod || "-"}</Text>
            <Text type="secondary">{item.requestPath || "-"}</Text>
          </Space>
        ),
      },
      {
        title: t("activity_logs.columns.actions"),
        key: "actions",
        render: (_, item) => (
          <Button icon={<EyeOutlined />} onClick={() => setSelectedActivity(item)}>
            {t("activity_logs.actions.view")}
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
        ? getApiErrorPresentation(error, t("activity_logs.notifications.load_failed_title"))
        : getApiErrorPresentation(error, t("activity_logs.notifications.load_failed_title"));

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
            <HistoryOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("activity_logs.title")}
            </Typography.Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("activity_logs.description")}
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canReadActivityLogs ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} style={{ marginBottom: 16 }} />
        ) : null}

        <Form form={form} layout="vertical" onFinish={handleSearch}>
          <Space wrap size={16} align="end" style={{ marginBottom: 16 }}>
            <Form.Item name="module" label={t("activity_logs.filters.module")}>
              <Input placeholder={t("activity_logs.placeholders.module")} />
            </Form.Item>
            <Form.Item name="action" label={t("activity_logs.filters.action")}>
              <Input placeholder={t("activity_logs.placeholders.action")} />
            </Form.Item>
            <Form.Item name="entityType" label={t("activity_logs.filters.entity_type")}>
              <Input placeholder={t("activity_logs.placeholders.entity_type")} />
            </Form.Item>
            <Form.Item name="entityId" label={t("activity_logs.filters.entity_id")}>
              <Input placeholder={t("activity_logs.placeholders.entity_id")} />
            </Form.Item>
            <Form.Item name="actor" label={t("activity_logs.filters.actor")}>
              <Input placeholder={t("activity_logs.placeholders.actor")} />
            </Form.Item>
            <Form.Item name="status" label={t("activity_logs.filters.status")}>
              <Select
                allowClear
                style={{ width: 160 }}
                options={[
                  { value: "success", label: t("activity_logs.status.success") },
                  { value: "failed", label: t("activity_logs.status.failed") },
                  { value: "denied", label: t("activity_logs.status.denied") },
                ]}
              />
            </Form.Item>
            <Form.Item name="range" label={t("activity_logs.filters.date_range")}>
              <RangePicker />
            </Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" icon={<SearchOutlined />}>
                {t("activity_logs.actions.search")}
              </Button>
              <Button onClick={handleReset}>{t("activity_logs.actions.reset")}</Button>
            </Space>
          </Space>
        </Form>

        <Table<ActivityLogItem>
          rowKey="id"
          columns={columns}
          dataSource={canReadActivityLogs ? (activityLogsQuery.data?.items ?? []) : []}
          loading={canReadActivityLogs ? activityLogsQuery.isLoading : false}
          pagination={{
            current: activityLogsQuery.data?.page ?? filters.page ?? 1,
            pageSize: activityLogsQuery.data?.pageSize ?? filters.pageSize ?? 10,
            total: activityLogsQuery.data?.total ?? 0,
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
            emptyText: activityLogsQuery.isError ? t("activity_logs.empty_error") : t("activity_logs.empty"),
          }}
          onChange={(pagination, _, sorter) => {
            const sort = sorter as SorterResult<ActivityLogItem>;
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
        open={selectedActivity !== null}
        title={t("activity_logs.detail.title")}
        onCancel={() => setSelectedActivity(null)}
        footer={[
          <Button key="close" onClick={() => setSelectedActivity(null)}>
            {t("activity_logs.actions.close")}
          </Button>,
        ]}
        width={1000}
        styles={{ body: { padding: "20px 0" } }}
      >
        {selectedActivity ? (
          <Space direction="vertical" size={24} style={{ width: "100%" }}>
            {/* Error Message Alert if failed */}
            {selectedActivity.status !== "success" && (selectedActivity.errorCode || selectedActivity.errorMessage) && (
              <Alert
                type="error"
                showIcon
                message={selectedActivity.errorCode || t("activity_logs.status.failed")}
                description={selectedActivity.errorMessage}
                style={{ margin: "0 24px" }}
              />
            )}

            <div style={{ padding: "0 24px" }}>
              <Typography.Title level={5} style={{ marginBottom: 16 }}>
                <HistoryOutlined style={{ marginRight: 8 }} />
                {t("activity_logs.detail.summary")}
              </Typography.Title>
              <Card size="small" variant="outlined" style={{ background: token.colorFillAlter }}>
                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "16px 32px" }}>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 12 }}>{t("activity_logs.columns.occurred_at")}</Text>
                    <Text strong>{formatDate(selectedActivity.occurredAt, i18n.language)}</Text>
                  </Space>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 12 }}>{t("activity_logs.columns.status")}</Text>
                    <Tag color={getStatusColor(selectedActivity.status)} style={{ margin: 0 }}>
                      {t(`activity_logs.status.${selectedActivity.status}`, { defaultValue: selectedActivity.status })}
                    </Tag>
                  </Space>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 12 }}>{t("activity_logs.columns.module")}</Text>
                    <Text>{selectedActivity.module}</Text>
                  </Space>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 12 }}>{t("activity_logs.columns.action")}</Text>
                    <Tag>{selectedActivity.action.toUpperCase()}</Tag>
                  </Space>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 12 }}>{t("activity_logs.columns.entity")}</Text>
                    <Text>{`${selectedActivity.entityType} (${selectedActivity.entityId || "-"})`}</Text>
                  </Space>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 12 }}>{t("activity_logs.columns.actor")}</Text>
                    <Text>{selectedActivity.actorEmail || selectedActivity.actorDisplayName || selectedActivity.actorUserId || "-"}</Text>
                  </Space>
                </div>
              </Card>
            </div>

            <div style={{ padding: "0 24px" }}>
              <Typography.Title level={5} style={{ marginBottom: 16 }}>
                <SearchOutlined style={{ marginRight: 8 }} />
                {t("activity_logs.detail.metadata")}
              </Typography.Title>
              <Card size="small" variant="outlined">
                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: "12px" }}>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 11 }}>{t("activity_logs.detail.http_method")}</Text>
                    <Text style={{ fontSize: 13 }}>{selectedActivity.httpMethod || "-"}</Text>
                  </Space>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 11 }}>{t("activity_logs.detail.status_code")}</Text>
                    <Text style={{ fontSize: 13 }}>{selectedActivity.statusCode || "-"}</Text>
                  </Space>
                  <Space direction="vertical" size={0}>
                    <Text type="secondary" style={{ fontSize: 11 }}>{t("activity_logs.detail.ip_address")}</Text>
                    <Text style={{ fontSize: 13 }}>{selectedActivity.ipAddress || "-"}</Text>
                  </Space>
                  <div style={{ gridColumn: "span 3" }}>
                    <Space direction="vertical" size={0} style={{ width: "100%" }}>
                      <Text type="secondary" style={{ fontSize: 11 }}>{t("activity_logs.detail.request_id")}</Text>
                      <Text copyable style={{ fontSize: 12, fontFamily: "monospace" }}>{selectedActivity.requestId || "-"}</Text>
                    </Space>
                  </div>
                  <div style={{ gridColumn: "span 3" }}>
                    <Space direction="vertical" size={0} style={{ width: "100%" }}>
                      <Text type="secondary" style={{ fontSize: 11 }}>{t("activity_logs.detail.trace_id")}</Text>
                      <Text copyable style={{ fontSize: 12, fontFamily: "monospace" }}>{selectedActivity.traceId || "-"}</Text>
                    </Space>
                  </div>
                  <div style={{ gridColumn: "span 3" }}>
                    <Space direction="vertical" size={0} style={{ width: "100%" }}>
                      <Text type="secondary" style={{ fontSize: 11 }}>{t("activity_logs.detail.user_agent")}</Text>
                      <Text style={{ fontSize: 12 }}>{selectedActivity.userAgent || "-"}</Text>
                    </Space>
                  </div>
                </div>
              </Card>
            </div>

            <div style={{ padding: "0 24px" }}>
              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "24px" }}>
                <div>
                  <Typography.Title level={5} style={{ marginBottom: 8 }}>{t("activity_logs.detail.before")}</Typography.Title>
                  <Card size="small" style={{ maxHeight: 300, overflow: "auto", background: token.colorFillTertiary, border: `1px solid ${token.colorBorderSecondary}` }}>
                    <pre style={{ margin: 0, fontSize: 12 }}>{prettyJson(selectedActivity.beforeJson)}</pre>
                  </Card>
                </div>
                <div>
                  <Typography.Title level={5} style={{ marginBottom: 8 }}>{t("activity_logs.detail.after")}</Typography.Title>
                  <Card size="small" style={{ maxHeight: 300, overflow: "auto", background: token.colorFillTertiary, border: `1px solid ${token.colorBorderSecondary}` }}>
                    <pre style={{ margin: 0, fontSize: 12 }}>{prettyJson(selectedActivity.afterJson)}</pre>
                  </Card>
                </div>
              </div>
            </div>

            {selectedActivity.changesJson && selectedActivity.changesJson !== "{}" && (
              <div style={{ padding: "0 24px" }}>
                <Typography.Title level={5} style={{ marginBottom: 8 }}>{t("activity_logs.detail.changes")}</Typography.Title>
                <Card size="small" style={{ background: token.colorWarningBg, border: `1px solid ${token.colorWarningBorder}` }}>
                  <pre style={{ margin: 0, fontSize: 12 }}>{prettyJson(selectedActivity.changesJson)}</pre>
                </Card>
              </div>
            )}

            {selectedActivity.metadataJson && selectedActivity.metadataJson !== "{}" && (
              <div style={{ padding: "0 24px" }}>
                <Typography.Title level={5} style={{ marginBottom: 8 }}>{t("activity_logs.detail.metadata")}</Typography.Title>
                <Card size="small" style={{ background: token.colorInfoBg, border: `1px solid ${token.colorInfoBorder}` }}>
                  <pre style={{ margin: 0, fontSize: 12 }}>{prettyJson(selectedActivity.metadataJson)}</pre>
                </Card>
              </div>
            )}
          </Space>
        ) : null}
      </Modal>
    </Space>
  );
}
