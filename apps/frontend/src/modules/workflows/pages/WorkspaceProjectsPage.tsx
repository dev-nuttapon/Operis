import { Button, Card, Grid, Space, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { FolderOpenOutlined } from "@ant-design/icons";
import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { useProjectList } from "../../users";
import type { ProjectListItem } from "../../users/types/users";

export function WorkspaceProjectsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canManageProjects = permissionState.hasPermission(permissions.projects.manage);
  const canReadProjects = permissionState.hasPermission(permissions.projects.read);
  const canViewProjects = canManageProjects || canReadProjects;
  const [paging, setPaging] = useState({ page: 1, pageSize: 10 });

  const projectsQuery = useProjectList(
    { page: paging.page, pageSize: paging.pageSize, assignedOnly: !canManageProjects },
    canViewProjects,
  );

  const statusLabel = useMemo<Record<string, string>>(
    () => ({
      planned: t("projects.options.status.planned"),
      active: t("projects.options.status.active"),
      onhold: t("projects.options.status.on_hold"),
      completed: t("projects.options.status.completed"),
      cancelled: t("projects.options.status.cancelled"),
    }),
    [t],
  );

  const columns = useMemo<ColumnsType<ProjectListItem>>(
    () => [
      { title: t("workflow_tasks.projects.columns.code"), dataIndex: "code", width: 140 },
      { title: t("workflow_tasks.projects.columns.name"), dataIndex: "name", ellipsis: true },
      { title: t("workflow_tasks.projects.columns.owner"), dataIndex: "ownerDisplayName", render: (value) => value ?? "-" },
      {
        title: t("workflow_tasks.projects.columns.status"),
        dataIndex: "status",
        render: (value: string) => (
          <Tag color={value === "active" ? "green" : value === "completed" ? "blue" : value === "cancelled" ? "red" : "gold"}>
            {statusLabel[value] ?? value}
          </Tag>
        ),
      },
      {
        title: t("workflow_tasks.projects.columns.actions"),
        key: "actions",
        align: "center",
        render: (_, record) => (
          <Button
            type="primary"
            onClick={() => navigate(`/app/workspace/${record.id}`)}
            size={isMobile ? "middle" : "small"}
          >
            {t("workflow_tasks.projects.actions.work")}
          </Button>
        ),
      },
    ],
    [isMobile, navigate, statusLabel, t],
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
            <FolderOpenOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("workflow_tasks.projects.title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ marginTop: 4 }}>
              {t("workflow_tasks.projects.description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canViewProjects ? (
          <Typography.Text type="secondary">{t("errors.title_forbidden")}</Typography.Text>
        ) : (
          <Table
            rowKey="id"
            dataSource={projectsQuery.data?.items ?? []}
            loading={projectsQuery.isLoading}
            columns={columns}
            locale={{ emptyText: t("workflow_tasks.projects.empty") }}
            pagination={{
              current: projectsQuery.data?.page ?? paging.page,
              pageSize: projectsQuery.data?.pageSize ?? paging.pageSize,
              total: projectsQuery.data?.total ?? 0,
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
          />
        )}
      </Card>
    </Space>
  );
}
