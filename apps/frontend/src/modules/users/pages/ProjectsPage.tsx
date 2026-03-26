import { useEffect, useMemo, useState } from "react";
import { App, Button, Card, Form, Input, Modal, Space, Table, Tag, Typography, Skeleton, Flex, Grid } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { DeleteOutlined, EditOutlined, EyeOutlined, FolderOpenOutlined, HistoryOutlined, PlusOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useLocation, useNavigate } from "react-router-dom";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { formatDate, toApiSortOrder } from "../utils/adminUsersPresentation";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import type { ProjectListItem } from "../types/users";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";
import { ActionMenu } from "../../../shared/components/ActionMenu";

export function ProjectsPage() {
  const { t, i18n } = useTranslation();
  const { notification } = App.useApp();
  const navigate = useNavigate();
  const location = useLocation();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canReadProjects = permissionState.hasPermission(permissions.projects.read);
  const canManageProjects = permissionState.hasPermission(permissions.projects.manage);
  const canReadActivityLogs = permissionState.hasPermission(permissions.activityLogs.read);
  const isMyProjectsPage = location.pathname === "/app/projects";
  const canViewProjectList = canReadProjects || isMyProjectsPage;
  const projectStatusLabel = useMemo<Record<string, string>>(
    () => ({
      planned: t("projects.options.status.planned"),
      active: t("projects.options.status.active"),
      onhold: t("projects.options.status.on_hold"),
      completed: t("projects.options.status.completed"),
      cancelled: t("projects.options.status.cancelled"),
    }),
    [t],
  );
  const [paging, setPaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    sortBy: "createdAt",
    sortOrder: "desc" as "asc" | "desc",
  });
  const [searchInput, setSearchInput] = useState("");
  const [deleteTarget, setDeleteTarget] = useState<ProjectListItem | null>(null);
  const [deleteForm] = Form.useForm();

  const debouncedSearch = useDebouncedValue(searchInput, 300);

  useEffect(() => {
    setPaging((current) => ({ ...current, page: 1, search: debouncedSearch }));
  }, [debouncedSearch, setPaging]);
  const { projectsQuery, deleteProjectMutation } = useProjectAdmin({
    projectsEnabled: canViewProjectList,
    projects: { ...paging, search: debouncedSearch, assignedOnly: isMyProjectsPage && !canReadProjects },
    projectRoles: { page: 1, pageSize: 10 },
    projectAssignments: null,
  });
  const projectData = projectsQuery.data as { items?: ProjectListItem[]; page?: number; pageSize?: number; total?: number } | undefined;

  const handleError = (fallbackTitle: string, error: unknown) => {
    const presentation = getApiErrorPresentation(error, fallbackTitle);
    notification.error({ message: presentation.title, description: presentation.description });
  };

  const columns = useMemo<ColumnsType<ProjectListItem>>(
    () => [
      { title: t("projects.columns.code"), dataIndex: "code", sorter: true },
      { title: t("projects.columns.name"), dataIndex: "name", sorter: true },
      { title: t("projects.columns.project_type"), dataIndex: "projectType", sorter: true },
      { title: t("projects.columns.phase"), dataIndex: "phase", sorter: true, render: (value: string | null) => value ?? "-" },
      { title: t("projects.columns.owner"), dataIndex: "ownerDisplayName", render: (value: string | null) => value ?? "-" },
      {
        title: t("projects.columns.status"),
        dataIndex: "status",
        sorter: true,
        render: (value: string) => (
          <Tag color={value === "active" ? "green" : value === "completed" ? "blue" : value === "cancelled" ? "red" : "gold"}>
            {projectStatusLabel[value] ?? value}
          </Tag>
        ),
      },
      {
        title: t("projects.columns.planned_start_at"),
        dataIndex: "plannedStartAt",
        sorter: true,
        render: (value: string | null) => formatDate(value, i18n.language),
      },
      {
        title: t("projects.columns.end_at"),
        dataIndex: "endAt",
        render: (value: string | null) => formatDate(value, i18n.language),
      },
      {
        title: t("admin_users.columns.actions"),
        key: "actions",
        render: (_, record) => {
          const items = [
            {
              key: "detail",
              icon: <EyeOutlined />,
              label: t("common.actions.detail"),
              onClick: () => navigate(`/app/projects/${record.id}/workspace`),
            },
            {
              key: "edit",
              icon: <EditOutlined />,
              label: t("common.actions.edit"),
              disabled: !canManageProjects,
              onClick: () => navigate(`/app/projects/${record.id}/edit`, { state: { from: `${location.pathname}${location.search}` } }),
            },
            {
              key: "history",
              icon: <HistoryOutlined />,
              label: t("common.actions.history"),
              disabled: !canReadActivityLogs,
              onClick: () =>
                navigate(`/app/projects/${record.id}/history`, {
                  state: { projectName: record.name, from: `${location.pathname}${location.search}` },
                }),
            },
            {
              key: "delete",
              icon: <DeleteOutlined />,
              label: t("common.actions.delete"),
              danger: true,
              disabled: !canManageProjects,
              onClick: () => {
                setDeleteTarget(record);
                deleteForm.resetFields();
              },
            },
          ];

          return <ActionMenu items={items} />;
        },
      },
    ],
    [canManageProjects, canReadActivityLogs, deleteForm, i18n.language, navigate, projectStatusLabel, t],
  );

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)", color: "#fff" }}>
            <FolderOpenOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {isMyProjectsPage ? t("projects.my_page_title") : t("projects.page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {isMyProjectsPage ? t("projects.my_page_description") : t("projects.page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canViewProjectList ? (
          <Typography.Text type="secondary">{t("errors.title_forbidden")}</Typography.Text>
        ) : (
          <>
            <Flex
              gap={12}
              wrap={!isMobile}
              vertical={isMobile}
              align={isMobile ? "stretch" : "center"}
              justify="space-between"
              style={{ width: "100%", marginBottom: 16 }}
            >
              <Input.Search
                allowClear
                placeholder={t("projects.search_placeholder")}
                value={searchInput}
                onChange={(event) => setSearchInput(event.target.value)}
                onSearch={(value) => setSearchInput(value)}
                style={{ width: isMobile ? "100%" : undefined, maxWidth: isMobile ? undefined : 360 }}
              />
              {canManageProjects ? (
                <Button
                  type="primary"
                  icon={<PlusOutlined />}
                  size="large"
                  onClick={() => navigate("/app/projects/new", { state: { from: location.pathname } })}
                  block={isMobile}
                >
                  {t("projects.create_action")}
                </Button>
              ) : null}
            </Flex>

            {projectsQuery.isLoading && (Array.isArray(projectData?.items) ? projectData.items.length : 0) === 0 ? (
              <Skeleton active paragraph={{ rows: 6 }} />
            ) : (
              <Table
                rowKey="id"
                columns={columns}
                dataSource={Array.isArray(projectData?.items) ? projectData.items : []}
                loading={projectsQuery.isLoading}
                scroll={{ x: "max-content" }}
                pagination={{
                  current: projectData?.page ?? paging.page,
                  pageSize: projectData?.pageSize ?? paging.pageSize,
                  total: projectData?.total ?? 0,
                  showSizeChanger: true,
                  pageSizeOptions: [10, 25, 50, 100],
                }}
                onChange={(nextPagination, _, sorter) => {
                  const resolvedSorter = sorter as SorterResult<ProjectListItem>;
                  setPaging((current) => ({
                    ...current,
                    page: nextPagination.current ?? current.page,
                    pageSize: nextPagination.pageSize ?? current.pageSize,
                    sortBy: typeof resolvedSorter.field === "string" ? resolvedSorter.field : current.sortBy,
                    sortOrder: toApiSortOrder(resolvedSorter.order) ?? current.sortOrder,
                  }));
                }}
              />
            )}
          </>
        )}
      </Card>

      <Modal
        title={deleteTarget ? t("projects.delete_modal_title_with_name", { name: deleteTarget.name }) : t("projects.delete_modal_title")}
        open={deleteTarget !== null && canManageProjects}
        onCancel={() => {
          setDeleteTarget(null);
          deleteForm.resetFields();
        }}
        onOk={() => {
          deleteForm.validateFields().then((values) => {
            if (!deleteTarget) {
              return;
            }
            deleteProjectMutation.mutate(
              { id: deleteTarget.id, input: { reason: values.reason } },
              {
                onSuccess: () => {
                  setDeleteTarget(null);
                  deleteForm.resetFields();
                  notification.success({ message: t("projects.messages.deleted", { name: deleteTarget.name }) });
                },
                onError: (error) => handleError(t("projects.messages.delete_failed"), error),
              },
            );
          }).catch(() => undefined);
        }}
        okButtonProps={{ danger: true }}
        confirmLoading={deleteProjectMutation.isPending}
      >
        <Form form={deleteForm} layout="vertical">
          <Typography.Paragraph type="secondary">{t("projects.delete_description")}</Typography.Paragraph>
          <Form.Item name="reason" label={t("admin_users.fields.reason")} rules={[{ required: true, message: t("admin_users.validation.reason_required") }]}>
            <Input.TextArea rows={4} placeholder={t("projects.placeholders.delete_reason")} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
