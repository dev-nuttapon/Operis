import { Card, Table, Typography, Space, Tag, Grid, Flex, Switch, Button, App, Drawer, Descriptions } from "antd";
import { BellOutlined } from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useNotifications } from "../hooks/useNotifications";
import { useNotificationActions } from "../hooks/useNotificationActions";
import { useNotificationDetail } from "../hooks/useNotificationDetail";
import type { NotificationListItem } from "../types/notifications";

export function NotificationsPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const [paging, setPaging] = useState({ page: 1, pageSize: 10 });
  const [unreadOnly, setUnreadOnly] = useState(false);
  const notificationsQuery = useNotifications({ ...paging, unreadOnly });
  const actions = useNotificationActions();
  const [selectedNotificationId, setSelectedNotificationId] = useState<string | null>(null);
  const detailQuery = useNotificationDetail(selectedNotificationId, Boolean(selectedNotificationId));

  const columns = useMemo<ColumnsType<NotificationListItem>>(
    () => [
      { title: t("notifications.columns.title"), dataIndex: "title", width: 220, ellipsis: true },
      { title: t("notifications.columns.description"), dataIndex: "description", width: 320, ellipsis: true },
      { title: t("notifications.columns.source"), dataIndex: "source", width: 160, ellipsis: true },
      {
        title: t("notifications.columns.status"),
        dataIndex: "status",
        width: 140,
        render: (value) => (
          <Tag color={value === "unread" ? "blue" : "default"}>
            {t(`notifications.status.${value}`)}
          </Tag>
        ),
      },
      {
        title: t("notifications.columns.created_at"),
        dataIndex: "createdAt",
        width: 180,
        render: (value) => (value ? new Date(value).toLocaleString() : "-"),
      },
      {
        title: t("notifications.columns.actions"),
        dataIndex: "actions",
        width: 140,
        render: (_, record) => (
          <Button
            size="small"
            type="link"
            disabled={record.status !== "unread" || actions.markReadMutation.isPending}
            onClick={async () => {
              try {
                await actions.markReadMutation.mutateAsync(record.id);
                notification.success({ message: t("notifications.messages.mark_read") });
              } catch {
                notification.error({ message: t("notifications.messages.mark_read_failed") });
              }
            }}
          >
            {t("notifications.actions.mark_read")}
          </Button>
        ),
      },
    ],
    [actions.markReadMutation, notification, t],
  );

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
            <BellOutlined />
          </div>
          <div style={{ width: "100%" }}>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("notifications.page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ marginTop: 4 }}>
              {t("notifications.page_description")}
            </Typography.Paragraph>
            <Flex gap={12} align="center" wrap="wrap">
              <Flex align="center" gap={8}>
                <Switch
                  checked={unreadOnly}
                  onChange={(checked) => {
                    setUnreadOnly(checked);
                    setPaging((current) => ({ ...current, page: 1 }));
                  }}
                />
                <Typography.Text>{t("notifications.actions.unread_only")}</Typography.Text>
              </Flex>
              <Button
                onClick={async () => {
                  try {
                    await actions.markAllReadMutation.mutateAsync();
                    notification.success({ message: t("notifications.messages.mark_all_read") });
                  } catch {
                    notification.error({ message: t("notifications.messages.mark_all_read_failed") });
                  }
                }}
                disabled={actions.markAllReadMutation.isPending}
              >
                {t("notifications.actions.mark_all_read")}
              </Button>
              <Button
                onClick={async () => {
                  try {
                    await actions.seedMutation.mutateAsync(8);
                    notification.success({ message: t("notifications.messages.seeded") });
                  } catch {
                    notification.error({ message: t("notifications.messages.seed_failed") });
                  }
                }}
                disabled={actions.seedMutation.isPending}
              >
                {t("notifications.actions.seed")}
              </Button>
            </Flex>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Table
          rowKey="id"
          dataSource={notificationsQuery.data?.items ?? []}
          loading={notificationsQuery.isLoading}
          columns={columns}
          locale={{ emptyText: t("notifications.empty") }}
          pagination={{
            current: notificationsQuery.data?.page ?? paging.page,
            pageSize: notificationsQuery.data?.pageSize ?? paging.pageSize,
            total: notificationsQuery.data?.total ?? 0,
            showSizeChanger: true,
            pageSizeOptions: [10, 25, 50, 100],
            onChange: (page, pageSize) =>
              setPaging((current) => ({
                page: pageSize === current.pageSize ? page : 1,
                pageSize,
              })),
          }}
          scroll={{ x: "max-content" }}
          size={isMobile ? "small" : "middle"}
          onRow={(record) => ({
            onClick: () => setSelectedNotificationId(record.id),
          })}
        />
      </Card>

      <Drawer
        open={Boolean(selectedNotificationId)}
        width={isMobile ? "100%" : 520}
        onClose={() => setSelectedNotificationId(null)}
        title={t("notifications.detail.title")}
      >
        {detailQuery.isLoading ? (
          <Typography.Text type="secondary">{t("notifications.detail.loading")}</Typography.Text>
        ) : detailQuery.data ? (
          <>
            <Descriptions
              column={1}
              size="small"
              items={[
                { label: t("notifications.columns.title"), children: detailQuery.data.title },
                { label: t("notifications.columns.source"), children: detailQuery.data.source },
                {
                  label: t("notifications.columns.status"),
                  children: t(`notifications.status.${detailQuery.data.status}`),
                },
                {
                  label: t("notifications.columns.created_at"),
                  children: new Date(detailQuery.data.createdAt).toLocaleString(),
                },
              ]}
            />
            <Typography.Paragraph style={{ marginTop: 16 }}>
              {detailQuery.data.description}
            </Typography.Paragraph>
            <Button
              type="primary"
              disabled={detailQuery.data.status !== "unread" || actions.markReadMutation.isPending}
              onClick={async () => {
                try {
                  await actions.markReadMutation.mutateAsync(detailQuery.data!.id);
                  notification.success({ message: t("notifications.messages.mark_read") });
                } catch {
                  notification.error({ message: t("notifications.messages.mark_read_failed") });
                }
              }}
            >
              {t("notifications.actions.mark_read")}
            </Button>
          </>
        ) : (
          <Typography.Text type="secondary">{t("notifications.detail.empty")}</Typography.Text>
        )}
      </Drawer>
    </Space>
  );
}
