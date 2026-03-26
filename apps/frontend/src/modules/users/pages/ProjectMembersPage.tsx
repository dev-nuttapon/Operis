import { useEffect, useMemo, useState } from "react";
import { Alert, App, Button, Card, Input, Select, Space, Table, Tag, Typography, Skeleton, Flex, Grid, Modal, Form } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { DeleteOutlined, EditOutlined, PlusOutlined, ShareAltOutlined } from "@ant-design/icons";
import { useLocation, useNavigate, useSearchParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { formatDate, toApiSortOrder } from "../utils/adminUsersPresentation";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import { useProjectOptions } from "../hooks/useProjectOptions";
import type { ProjectAssignment } from "../types/users";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";
import { ActionMenu } from "../../../shared/components/ActionMenu";

export function ProjectMembersPage() {
  const { t, i18n } = useTranslation();
  const { notification } = App.useApp();
  const navigate = useNavigate();
  const location = useLocation();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const [searchParams] = useSearchParams();
  const permissionState = usePermissions();
  const canReadProjects = permissionState.hasPermission(permissions.projects.read);
  const canManageProjectMembers = permissionState.hasPermission(permissions.projects.manageMembers);
  const [selectedProjectId, setSelectedProjectId] = useState<string>();
  const [paging, setPaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    sortBy: "createdAt",
    sortOrder: "desc" as "asc" | "desc",
  });
  const [searchInput, setSearchInput] = useState("");
  const [deleteTarget, setDeleteTarget] = useState<ProjectAssignment | null>(null);
  const [deleteForm] = Form.useForm<{ reason: string }>();

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
  const {
    projectAssignmentsQuery,
    deleteProjectAssignmentMutation,
  } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 1 },
    projectRoles: { page: 1, pageSize: 1 },
    projectAssignments: selectedProjectId ? { projectId: selectedProjectId, ...paging, search: debouncedSearch } : null,
  });
  const assignmentsData = projectAssignmentsQuery.data as { items?: ProjectAssignment[]; page?: number; pageSize?: number; total?: number } | undefined;
  const projectOptionsState = useProjectOptions({ enabled: canReadProjects });
  const handleError = (fallbackTitle: string, error: unknown) => {
    const presentation = getApiErrorPresentation(error, fallbackTitle);
    notification.error({ message: presentation.title, description: presentation.description });
  };

  const projectOptions = projectOptionsState.options;

  const columns = useMemo<ColumnsType<ProjectAssignment>>(
    () => [
      {
        title: t("project_members.columns.member"),
        dataIndex: "userDisplayName",
        render: (_, record) => record.userDisplayName ?? record.userEmail ?? record.userId,
      },
      {
        title: t("project_members.columns.project_role"),
        dataIndex: "projectRoleName",
      },
      {
        title: t("project_members.columns.reports_to"),
        dataIndex: "reportsToDisplayName",
        render: (_, record) => (record.reportsToUserId ? record.reportsToDisplayName ?? record.reportsToUserId : "-"),
      },
      {
        title: t("project_members.columns.primary"),
        dataIndex: "isPrimary",
        render: (value: boolean) => (value ? t("common.actions.yes") : t("common.actions.no")),
      },
      {
        title: t("project_members.columns.status"),
        dataIndex: "status",
        render: (value: string) => <Tag color={value === "Active" ? "green" : value === "Removed" ? "red" : "gold"}>{value}</Tag>,
      },
      {
        title: t("project_members.columns.start_at"),
        dataIndex: "startAt",
        sorter: true,
        render: (value: string) => formatDate(value, i18n.language),
      },
      {
        title: t("project_members.columns.end_at"),
        dataIndex: "endAt",
        render: (value: string | null) => formatDate(value, i18n.language),
      },
      {
        title: t("admin_users.columns.actions"),
        key: "actions",
        render: (_, record) =>
          canManageProjectMembers ? (
            <ActionMenu
              items={[
                {
                  key: "edit",
                  icon: <EditOutlined />,
                  label: t("common.actions.edit"),
                  onClick: () =>
                    navigate(`/app/admin/project-members/${record.id}/edit?projectId=${record.projectId}`, {
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
    [canManageProjectMembers, i18n.language, t, deleteForm, location.pathname, location.search, navigate],
  );

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)", color: "#fff" }}>
            <ShareAltOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("project_members.page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("project_members.page_description")}
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
            placeholder={t("project_members.select_project_placeholder")}
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
            <Alert type="info" showIcon message={t("project_members.select_project_message")} />
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
                  placeholder={t("project_members.search_placeholder")}
                  value={searchInput}
                  onChange={(event) => setSearchInput(event.target.value)}
                  onSearch={(value) => setSearchInput(value)}
                  style={{ width: isMobile ? "100%" : undefined, maxWidth: isMobile ? undefined : 360 }}
                />
                <Flex gap={8} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"}>
                  {selectedProjectId ? (
                    <Button onClick={() => navigate("/app/projects/roles")}>
                      {t("project_members.go_to_roles")}
                    </Button>
                  ) : null}
                  {canManageProjectMembers ? (
                    <Button
                      type="primary"
                      icon={<PlusOutlined />}
                      size="large"
                      onClick={() =>
                        navigate(`/app/admin/project-members/new?projectId=${selectedProjectId}`, {
                          state: { from: `${location.pathname}${location.search}` },
                        })
                      }
                      block={isMobile}
                    >
                      {t("project_members.create_action")}
                    </Button>
                  ) : null}
                </Flex>
              </Flex>

              {canReadProjects && projectAssignmentsQuery.isLoading && (Array.isArray(assignmentsData?.items) ? assignmentsData.items.length : 0) === 0 ? (
                <Skeleton active paragraph={{ rows: 6 }} />
              ) : (
                <Table
                  rowKey="id"
                  columns={columns}
                  dataSource={canReadProjects && Array.isArray(assignmentsData?.items) ? assignmentsData.items : []}
                  loading={canReadProjects ? projectAssignmentsQuery.isLoading : false}
                  scroll={{ x: "max-content" }}
                  pagination={{
                    current: assignmentsData?.page ?? paging.page,
                    pageSize: assignmentsData?.pageSize ?? paging.pageSize,
                    total: assignmentsData?.total ?? 0,
                    showSizeChanger: true,
                    pageSizeOptions: [10, 25, 50, 100],
                  }}
                  onChange={(nextPagination, _, sorter) => {
                    const resolvedSorter = sorter as SorterResult<ProjectAssignment>;
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
        title={deleteTarget ? t("project_members.delete_modal_title_with_name", { name: deleteTarget.userDisplayName ?? deleteTarget.userEmail ?? deleteTarget.userId }) : t("project_members.delete_modal_title")}
        open={deleteTarget !== null && canManageProjectMembers}
        onCancel={() => {
          setDeleteTarget(null);
          deleteForm.resetFields();
        }}
        onOk={() => {
          deleteForm.validateFields().then((values) => {
            if (!deleteTarget) {
              return;
            }
            deleteProjectAssignmentMutation.mutate(
              { id: deleteTarget.id, input: { reason: values.reason } },
              {
                onSuccess: () => {
                  setDeleteTarget(null);
                  deleteForm.resetFields();
                  notification.success({ message: t("project_members.messages.deleted") });
                },
                onError: (error) => handleError(t("project_members.messages.delete_failed"), error),
              },
            );
          }).catch(() => undefined);
        }}
        okButtonProps={{ danger: true }}
        confirmLoading={deleteProjectAssignmentMutation.isPending}
      >
        <Form form={deleteForm} layout="vertical">
          <Typography.Paragraph type="secondary">{t("project_members.delete_description")}</Typography.Paragraph>
          <Form.Item name="reason" label={t("project_members.fields.change_reason")} rules={[{ required: true, message: t("project_members.validation.change_reason_required") }]}> 
            <Input.TextArea rows={4} placeholder={t("project_members.placeholders.change_reason")} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
