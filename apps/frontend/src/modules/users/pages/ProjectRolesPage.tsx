import { useEffect, useMemo, useState } from "react";
import { Alert, App, Button, Card, Form, Input, Modal, Select, Space, Table, Typography, Skeleton, Flex, Grid } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { DeleteOutlined, EditOutlined, PlusOutlined, SolutionOutlined } from "@ant-design/icons";
import { useLocation, useNavigate, useSearchParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { toApiSortOrder } from "../utils/adminUsersPresentation";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import { useProjectOptions } from "../hooks/useProjectOptions";
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
  const [searchParams] = useSearchParams();
  const permissionState = usePermissions();
  const canReadProjects = permissionState.hasPermission(permissions.projects.read);
  const canManageProjectRoles = permissionState.hasPermission(permissions.projects.manageRoles);
  const [selectedProjectId, setSelectedProjectId] = useState<string>();
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

  useEffect(() => {
    const projectId = searchParams.get("projectId") ?? undefined;
    if (projectId) {
      setSelectedProjectId(projectId);
    }
  }, [searchParams]);
  const { projectRolesQuery, deleteProjectRoleMutation } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 1 },
    projectRoles: { ...paging, search: debouncedSearch, projectId: selectedProjectId },
    projectAssignments: null,
  });
  const projectOptionsState = useProjectOptions({ enabled: canReadProjects });

  const handleError = (fallbackTitle: string, error: unknown) => {
    const presentation = getApiErrorPresentation(error, fallbackTitle);
    notification.error({ message: presentation.title, description: presentation.description });
  };

  const projectOptions = projectOptionsState.options;
  const selectedProjectName = selectedProjectId ? projectOptionsState.itemsById.get(selectedProjectId)?.name ?? null : null;

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
        title: t("project_roles.columns.review_role"),
        dataIndex: "isReviewRole",
        render: (value: boolean) => (value ? t("common.actions.yes") : t("common.actions.no")),
      },
      {
        title: t("project_roles.columns.approval_role"),
        dataIndex: "isApprovalRole",
        render: (value: boolean) => (value ? t("common.actions.yes") : t("common.actions.no")),
      },
      {
        title: t("project_roles.columns.document_permissions"),
        key: "documentPermissions",
        render: (_, record) => {
          const labels = [
            record.canCreateDocuments ? t("project_roles.permissions.create") : null,
            record.canReviewDocuments ? t("project_roles.permissions.review") : null,
            record.canApproveDocuments ? t("project_roles.permissions.approve") : null,
            record.canReleaseDocuments ? t("project_roles.permissions.release") : null,
          ].filter(Boolean) as string[];

          return labels.length > 0 ? labels.join(", ") : "-";
        },
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
                    navigate(`/app/admin/project-roles/${record.id}/edit?projectId=${record.projectId ?? selectedProjectId ?? ""}`, {
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
    [canManageProjectRoles, deleteForm, location.pathname, location.search, navigate, selectedProjectId, t],
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
          <Select
            allowClear
            showSearch
            filterOption={false}
            placeholder={t("project_roles.select_project_placeholder")}
            options={projectOptions}
            value={selectedProjectId}
            onSearch={projectOptionsState.onSearch}
            loading={projectOptionsState.loading}
            onChange={(value) => {
              setSelectedProjectId(value);
              setPaging((current) => ({ ...current, page: 1 }));
            }}
            dropdownRender={(menu) => (
              <>
                {menu}
                {projectOptionsState.hasMore ? (
                  <div style={{ padding: 8 }}>
                    <button
                      type="button"
                      onMouseDown={(event) => event.preventDefault()}
                      onClick={() => projectOptionsState.onLoadMore()}
                      style={{
                        width: "100%",
                        border: "none",
                        background: "transparent",
                        color: "#1677ff",
                        cursor: "pointer",
                        padding: 4,
                      }}
                    >
                      {t("projects.load_more_projects")}
                    </button>
                  </div>
                ) : null}
              </>
            )}
          />

          {!canReadProjects ? (
            <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
          ) : !selectedProjectId ? (
            <Alert type="info" showIcon message={t("project_roles.select_project_message")} />
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
                        navigate(`/app/admin/project-roles/new?projectId=${selectedProjectId}`, {
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

              {canReadProjects && projectRolesQuery.isLoading && (projectRolesQuery.data?.items?.length ?? 0) === 0 ? (
                <Skeleton active paragraph={{ rows: 6 }} />
              ) : (
                <Table
                  rowKey="id"
                  columns={columns}
                  dataSource={canReadProjects ? (projectRolesQuery.data?.items ?? []) : []}
                  loading={canReadProjects ? projectRolesQuery.isLoading : false}
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
          <Typography.Paragraph type="secondary">{t("project_roles.delete_description", { project: selectedProjectName ?? "-" })}</Typography.Paragraph>
          <Form.Item name="reason" label={t("admin_users.fields.reason")} rules={[{ required: true, message: t("admin_users.validation.reason_required") }]}> 
            <Input.TextArea rows={4} placeholder={t("project_roles.placeholders.delete_reason")} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
