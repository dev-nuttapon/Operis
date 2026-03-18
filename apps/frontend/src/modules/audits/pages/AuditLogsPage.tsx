import { useEffect, useMemo, useRef, useState } from "react";
import { Alert, App, Button, Card, DatePicker, Form, Input, Space, Table, Tag, Typography, theme, Skeleton } from "antd";
import type { ColumnsType } from "antd/es/table";
import { SearchOutlined, SafetyCertificateOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useTranslation } from "react-i18next";
import { useSearchParams } from "react-router-dom";
import { ApiError, getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";
import { useAuditLogs } from "../hooks/useAuditLogs";
import type { BusinessAuditEventItem, ListAuditLogsInput } from "../types/audits";

const { RangePicker } = DatePicker;
const { Text, Paragraph } = Typography;

function formatDate(value: string, language: string) {
  return new Intl.DateTimeFormat(language.startsWith("th") ? "th-TH" : "en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function areFiltersEqual(a: ListAuditLogsInput, b: ListAuditLogsInput) {
  return (
    a.module === b.module &&
    a.eventType === b.eventType &&
    a.entityType === b.entityType &&
    a.entityId === b.entityId &&
    a.actor === b.actor &&
    a.from === b.from &&
    a.to === b.to &&
    a.page === b.page &&
    a.pageSize === b.pageSize &&
    a.sortBy === b.sortBy &&
    a.sortOrder === b.sortOrder
  );
}

export function AuditLogsPage() {
  const { t, i18n } = useTranslation();
  const { token } = theme.useToken();
  const { notification } = App.useApp();
  const [searchParams] = useSearchParams();
  const initializedRef = useRef(false);
  const permissionState = usePermissions();
  const canReadAuditLogs = permissionState.hasPermission(permissions.auditLogs.read);
  const [form] = Form.useForm();
  const [filters, setFilters] = useState<ListAuditLogsInput>({ page: 1, pageSize: 10, sortBy: "occurredAt", sortOrder: "desc" });
  const auditLogsQuery = useAuditLogs(filters);
  const watchedModule = Form.useWatch("module", form);
  const watchedEventType = Form.useWatch("eventType", form);
  const watchedEntityType = Form.useWatch("entityType", form);
  const watchedEntityId = Form.useWatch("entityId", form);
  const watchedActor = Form.useWatch("actor", form);
  const watchedRange = Form.useWatch("range", form);

  const debouncedFilters = useDebouncedValue(
    useMemo(
      () => ({
        module: watchedModule,
        eventType: watchedEventType,
        entityType: watchedEntityType,
        entityId: watchedEntityId,
        actor: watchedActor,
        range: watchedRange,
      }),
      [watchedActor, watchedEntityId, watchedEntityType, watchedEventType, watchedModule, watchedRange],
    ),
    400,
  );

  useEffect(() => {
    if (initializedRef.current) return;
    const entityType = searchParams.get("entityType") ?? undefined;
    const entityId = searchParams.get("entityId") ?? undefined;
    if (entityType || entityId) {
      form.setFieldsValue({ entityType, entityId });
      setFilters((current) => ({
        ...current,
        entityType,
        entityId,
        page: 1,
        pageSize: current.pageSize ?? 10,
      }));
    }
    initializedRef.current = true;
  }, [form, searchParams]);

  useEffect(() => {
    if (auditLogsQuery.isError) {
      handleError(auditLogsQuery.error);
    }
  }, [auditLogsQuery.error, auditLogsQuery.isError]);

  useEffect(() => {
    setFilters((current) => {
      const nextFilters: ListAuditLogsInput = {
        ...current,
        module: debouncedFilters.module || undefined,
        eventType: debouncedFilters.eventType || undefined,
        entityType: debouncedFilters.entityType || undefined,
        entityId: debouncedFilters.entityId || undefined,
        actor: debouncedFilters.actor || undefined,
        from: debouncedFilters.range?.[0]?.startOf("day").toISOString(),
        to: debouncedFilters.range?.[1]?.endOf("day").toISOString(),
        page: 1,
        pageSize: current.pageSize ?? 10,
      };

      if (areFiltersEqual(current, nextFilters)) {
        return current;
      }
      return nextFilters;
    });
  }, [debouncedFilters]);

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

  const handleSearch = (values: {
    module?: string;
    eventType?: string;
    entityType?: string;
    entityId?: string;
    actor?: string;
    range?: [dayjs.Dayjs, dayjs.Dayjs];
  }) => {
    setFilters({
      module: values.module || undefined,
      eventType: values.eventType || undefined,
      entityType: values.entityType || undefined,
      entityId: values.entityId || undefined,
      actor: values.actor || undefined,
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
            <Form.Item name="eventType" label={t("audit_logs.filters.event_type")}>
              <Input placeholder={t("audit_logs.placeholders.event_type")} />
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
          />
        )}
      </Card>
    </Space>
  );
}
