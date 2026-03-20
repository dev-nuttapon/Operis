import { useEffect, useMemo, useState } from "react";
import { Alert, App, Button, Card, Form, Input, Modal, Space, Table, Typography, Skeleton, Flex, Grid } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { DeleteOutlined, EditOutlined, PlusOutlined, SolutionOutlined } from "@ant-design/icons";
import { useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { toApiSortOrder } from "../utils/adminUsersPresentation";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import type { ProjectRole } from "../types/users";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";
import { ActionMenu } from "../../../shared/components/ActionMenu";

export function ProjectRolesPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const navigate = useNavigate();
  const location = useLocation();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canManageProjectRoles = permissionState.hasPermission(permissions.projects.manageRoles);
  const [paging, setPaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    sortBy: "displayOrder",
    sortOrder: "asc" as "asc" | "desc",
  });
  const [searchInput, setSearchInput] = useState("");
  const [deleteTarget, setDeleteTarget] = useState<ProjectRole | null>(null);
  const [deleteForm] = Form.useForm();

  const debouncedSearch = useDebouncedValue(searchInput, 300);

  useEffect(() => {
    setPaging((current) => ({ ...current, page: 1, search: debouncedSearch }));
  }, [debouncedSearch, setPaging]);

  const { projectRolesQuery, deleteProjectRoleMutation } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 1 },
    projectRoles: { ...paging, search: debouncedSearch },
    projectAssignments: null,
  });

  const handleError = (fallbackTitle: string, error: unknown) => {
    const presentation = getApiErrorPresentation(error, fallbackTitle);
    notification.error({ message: presentation.title, description: presentation.description });
  };

  const columns = useMemo<ColumnsType<ProjectRole>>(
    () => [
      {
        title: t("project_roles.columns.name"),
        dataIndex: "name",
        sorter: true,
      },
      {
        title: t("project_roles.columns.code"),
        dataIndex: "code",
        render: (value: string | null) => value ?? "-",
      },
      {
        title: t("project_roles.columns.display_order"),
        dataIndex: "displayOrder",
        sorter: true,
      },
      {
        title: t("admin_users.columns.actions"),
        key: "actions",
        render: (_, record) =>
          canManageProjectRoles ? (
            <ActionMenu
              items={[
                {
                  key: "edit",
                  icon: <EditOutlined />,
                  label: t("common.actions.edit"),
                  onClick: () =>
                    navigate(`/app/projects/roles/${record.id}/edit`, {
                      state: { from: `${location.pathname}${location.search}` },
                    }),
                },
                {
                  key: "delete",
                  icon: <DeleteOutlined />,
                  label: t("common.actions.delete"),
                  danger: true,
                  onClick: () => {
                    setDeleteTarget(record);
                    deleteForm.resetFields();
                  },
                },
              ]}
            />
          ) : null,
      },
    ],
    [canManageProjectRoles, deleteForm, location.pathname, location.search, navigate, t],
  );

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)", color: "#fff" }}>
            <SolutionOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("project_roles.page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("project_roles.page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Space direction="vertical" size={16} style={{ width: "100%" }}>
          {!canManageProjectRoles ? (
            <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
          ) : (
            <>
              <Flex
                gap={12}
                wrap={!isMobile}
                vertical={isMobile}
                align={isMobile ? "stretch" : "center"}
                justify="space-between"
                style={{ width: "100%", marginBottom: 4 }}
              >
                <Input.Search
                  allowClear
                  placeholder={t("project_roles.search_placeholder")}
                  value={searchInput}
                  onChange={(event) => setSearchInput(event.target.value)}
                  onSearch={(value) => setSearchInput(value)}
                  style={{ width: isMobile ? "100%" : undefined, maxWidth: isMobile ? undefined : 360 }}
                />
                <Flex gap={8} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"}>
                  {canManageProjectRoles ? (
                    <Button
                      type="primary"
                      icon={<PlusOutlined />}
                      size="large"
                      onClick={() => {
                        navigate("/app/projects/roles/new", {
                          state: { from: `${location.pathname}${location.search}` },
                        });
                      }}
                      block={isMobile}
                    >
                      {t("project_roles.create_action")}
                    </Button>
                  ) : null}
                </Flex>
              </Flex>

              {projectRolesQuery.isLoading && (projectRolesQuery.data?.items?.length ?? 0) === 0 ? (
                <Skeleton active paragraph={{ rows: 6 }} />
              ) : (
                <Table
                  rowKey="id"
                  columns={columns}
                  dataSource={projectRolesQuery.data?.items ?? []}
                  loading={projectRolesQuery.isLoading}
                  scroll={{ x: "max-content" }}
                  pagination={{
                    current: projectRolesQuery.data?.page ?? paging.page,
                    pageSize: projectRolesQuery.data?.pageSize ?? paging.pageSize,
                    total: projectRolesQuery.data?.total ?? 0,
                    showSizeChanger: true,
                    pageSizeOptions: [10, 25, 50, 100],
                  }}
                  onChange={(nextPagination, _, sorter) => {
                    const resolvedSorter = sorter as SorterResult<ProjectRole>;
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
        </Space>
      </Card>

      <Modal
        title={deleteTarget ? t("project_roles.delete_modal_title_with_name", { name: deleteTarget.name }) : t("project_roles.delete_modal_title")}
        open={deleteTarget !== null && canManageProjectRoles}
        onCancel={() => {
          setDeleteTarget(null);
          deleteForm.resetFields();
        }}
        onOk={() => {
          deleteForm.validateFields().then((values) => {
            if (!deleteTarget) {
              return;
            }
            deleteProjectRoleMutation.mutate(
              { id: deleteTarget.id, input: { reason: values.reason } },
              {
                onSuccess: () => {
                  setDeleteTarget(null);
                  deleteForm.resetFields();
                  notification.success({ message: t("project_roles.messages.deleted", { name: deleteTarget.name }) });
                },
                onError: (error) => handleError(t("project_roles.messages.delete_failed"), error),
              },
            );
          }).catch(() => undefined);
        }}
        okButtonProps={{ danger: true }}
        confirmLoading={deleteProjectRoleMutation.isPending}
      >
        <Form form={deleteForm} layout="vertical">
          <Typography.Paragraph type="secondary">{t("project_roles.delete_description")}</Typography.Paragraph>
          <Form.Item name="reason" label={t("admin_users.fields.reason")} rules={[{ required: true, message: t("admin_users.validation.reason_required") }]}> 
            <Input.TextArea rows={4} placeholder={t("project_roles.placeholders.delete_reason")} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
