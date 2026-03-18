import { useEffect, useMemo, useState } from "react";
import { Alert, App, Button, Card, Checkbox, DatePicker, Form, Input, Modal, Select, Space, Table, Tag, Typography, Skeleton } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { DeleteOutlined, EditOutlined, PlusOutlined, ShareAltOutlined } from "@ant-design/icons";
import { useNavigate, useSearchParams } from "react-router-dom";
import dayjs from "dayjs";
import { useTranslation } from "react-i18next";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { formatDate, toApiSortOrder } from "../utils/adminUsersPresentation";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import { useProjectUserOptions } from "../hooks/useProjectUserOptions";
import { useProjectOptions } from "../hooks/useProjectOptions";
import { useProjectRoleOptions } from "../hooks/useProjectRoleOptions";
import type { ProjectAssignment, UpdateProjectAssignmentInput, User } from "../types/users";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";

type ProjectMemberFormValues = {
  userId: string;
  projectRoleId: string;
  reportsToUserId?: string;
  isPrimary: boolean;
  period?: [dayjs.Dayjs | null, dayjs.Dayjs | null];
  reason?: string;
};

function toUserLabel(user: User) {
  return user.keycloak?.email ?? user.keycloak?.username ?? user.id;
}

function ProjectMemberForm({
  form,
  t,
  userOptions,
  projectRoleOptions,
  reportingOptions,
  includeReason,
  userOptionsLoading,
  onUserSearch,
  onUserLoadMore,
  userHasMore,
  roleOptionsLoading,
  onRoleSearch,
  onRoleLoadMore,
  roleHasMore,
}: {
  form: FormInstance<ProjectMemberFormValues>;
  t: ReturnType<typeof useTranslation>["t"];
  userOptions: { label: string; value: string }[];
  projectRoleOptions: { label: string; value: string }[];
  reportingOptions: { label: string; value: string }[];
  includeReason: boolean;
  userOptionsLoading?: boolean;
  onUserSearch?: (value: string) => void;
  onUserLoadMore?: () => void;
  userHasMore?: boolean;
  roleOptionsLoading?: boolean;
  onRoleSearch?: (value: string) => void;
  onRoleLoadMore?: () => void;
  roleHasMore?: boolean;
}) {
  return (
    <Form form={form} layout="vertical" initialValues={{ isPrimary: false }}>
      <Form.Item name="userId" label={t("project_members.fields.user")} rules={[{ required: true }]}> 
        <Select
          allowClear
          showSearch
          filterOption={false}
          options={userOptions}
          placeholder={t("project_members.placeholders.user")}
          loading={userOptionsLoading}
          onSearch={onUserSearch}
          dropdownRender={(menu) => (
            <>
              {menu}
              {userHasMore ? (
                <div style={{ padding: 8 }}>
                  <button
                    type="button"
                    onMouseDown={(event) => event.preventDefault()}
                    onClick={() => onUserLoadMore?.()}
                    style={{
                      width: "100%",
                      border: "none",
                      background: "transparent",
                      color: "#1677ff",
                      cursor: "pointer",
                      padding: 4,
                    }}
                  >
                    {t("projects.load_more_users")}
                  </button>
                </div>
              ) : null}
            </>
          )}
        />
      </Form.Item>
      <Form.Item name="projectRoleId" label={t("project_members.fields.project_role")} rules={[{ required: true }]}> 
        <Select
          showSearch
          filterOption={false}
          options={projectRoleOptions}
          placeholder={t("project_members.placeholders.project_role")}
          loading={roleOptionsLoading}
          onSearch={onRoleSearch}
          dropdownRender={(menu) => (
            <>
              {menu}
              {roleHasMore ? (
                <div style={{ padding: 8 }}>
                  <button
                    type="button"
                    onMouseDown={(event) => event.preventDefault()}
                    onClick={() => onRoleLoadMore?.()}
                    style={{
                      width: "100%",
                      border: "none",
                      background: "transparent",
                      color: "#1677ff",
                      cursor: "pointer",
                      padding: 4,
                    }}
                  >
                    {t("projects.load_more_roles")}
                  </button>
                </div>
              ) : null}
            </>
          )}
        />
      </Form.Item>
      <Form.Item name="reportsToUserId" label={t("project_members.fields.reports_to")}> 
        <Select
          allowClear
          showSearch
          filterOption={false}
          options={reportingOptions}
          placeholder={t("project_members.placeholders.reports_to")}
          loading={userOptionsLoading}
          onSearch={onUserSearch}
          dropdownRender={(menu) => (
            <>
              {menu}
              {userHasMore ? (
                <div style={{ padding: 8 }}>
                  <button
                    type="button"
                    onMouseDown={(event) => event.preventDefault()}
                    onClick={() => onUserLoadMore?.()}
                    style={{
                      width: "100%",
                      border: "none",
                      background: "transparent",
                      color: "#1677ff",
                      cursor: "pointer",
                      padding: 4,
                    }}
                  >
                    {t("projects.load_more_users")}
                  </button>
                </div>
              ) : null}
            </>
          )}
        />
      </Form.Item>
      <Form.Item name="period" label={t("project_members.fields.period")}> 
        <DatePicker.RangePicker style={{ width: "100%" }} />
      </Form.Item>
      {includeReason ? (
        <Form.Item name="reason" label={t("project_members.fields.change_reason")} rules={[{ required: true, message: t("project_members.validation.change_reason_required") }]}> 
          <Input.TextArea rows={4} placeholder={t("project_members.placeholders.change_reason")} />
        </Form.Item>
      ) : null}
      <Form.Item name="isPrimary" valuePropName="checked"> 
        <Checkbox>{t("project_members.fields.is_primary")}</Checkbox>
      </Form.Item>
    </Form>
  );
}

export function ProjectMembersPage() {
  const { t, i18n } = useTranslation();
  const { notification } = App.useApp();
  const navigate = useNavigate();
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
  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<ProjectAssignment | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<ProjectAssignment | null>(null);
  const [createForm] = Form.useForm<ProjectMemberFormValues>();
  const [editForm] = Form.useForm<ProjectMemberFormValues>();
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
    createProjectAssignmentMutation,
    updateProjectAssignmentMutation,
    deleteProjectAssignmentMutation,
  } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 1 },
    projectRoles: { page: 1, pageSize: 1 },
    projectAssignments: selectedProjectId ? { projectId: selectedProjectId, ...paging, search: debouncedSearch } : null,
  });
  const projectOptionsState = useProjectOptions({ enabled: canReadProjects });
  const projectRoleOptionsState = useProjectRoleOptions({ enabled: canManageProjectMembers, projectId: selectedProjectId });
  const userOptionsState = useProjectUserOptions(canManageProjectMembers, toUserLabel);

  const handleError = (fallbackTitle: string, error: unknown) => {
    const presentation = getApiErrorPresentation(error, fallbackTitle);
    notification.error({ message: presentation.title, description: presentation.description });
  };

  const projectOptions = projectOptionsState.options;
  const projectRoleOptions = projectRoleOptionsState.options;
  const projectRoleOptionList = useMemo(() => {
    const base = [...projectRoleOptions];
    const seen = new Set(base.map((item) => item.value));
    if (editTarget && !seen.has(editTarget.projectRoleId)) {
      base.push({
        value: editTarget.projectRoleId,
        label: editTarget.projectRoleName ?? editTarget.projectRoleId,
      });
    }
    return base;
  }, [editTarget, projectRoleOptions]);
  const userOptions = useMemo(() => {
    const base = [...userOptionsState.options];
    const seen = new Set(base.map((item) => item.value));
    const ensureOption = (value?: string, label?: string) => {
      if (!value || seen.has(value)) return;
      base.push({ value, label: label ?? value });
      seen.add(value);
    };
    if (editTarget) {
      ensureOption(editTarget.userId, editTarget.userDisplayName ?? editTarget.userEmail ?? editTarget.userId);
      ensureOption(editTarget.reportsToUserId, editTarget.reportsToDisplayName ?? editTarget.reportsToUserId ?? undefined);
    }
    return base;
  }, [editTarget, userOptionsState.options]);
  const reportingOptions = userOptions;

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
        render: (_, record) => (
          <Space>
            {canManageProjectMembers ? (
              <>
                <Button
                  icon={<EditOutlined />}
                  onClick={() => {
                    setEditTarget(record);
                    editForm.setFieldsValue({
                      userId: record.userId,
                      projectRoleId: record.projectRoleId,
                      reportsToUserId: record.reportsToUserId ?? undefined,
                      isPrimary: record.isPrimary,
                      reason: undefined,
                      period: [record.startAt ? dayjs(record.startAt) : null, record.endAt ? dayjs(record.endAt) : null],
                    });
                  }}
                >
                  {t("common.actions.edit")}
                </Button>
                <Button danger icon={<DeleteOutlined />} onClick={() => {
                  setDeleteTarget(record);
                  deleteForm.resetFields();
                }}>
                  {t("common.actions.delete")}
                </Button>
              </>
            ) : null}
          </Space>
        ),
      },
    ],
    [canManageProjectMembers, editForm, i18n.language, t, deleteForm],
  );

  const createAssignment = (values: ProjectMemberFormValues) => {
    if (!selectedProjectId) {
      return;
    }

    createProjectAssignmentMutation.mutate(
      {
        userId: values.userId,
        projectId: selectedProjectId,
        projectRoleId: values.projectRoleId,
        reportsToUserId: values.reportsToUserId,
        isPrimary: Boolean(values.isPrimary),
        startAt: values.period?.[0]?.startOf("day").toISOString(),
        endAt: values.period?.[1]?.endOf("day").toISOString(),
      },
      {
        onSuccess: () => {
          setCreateOpen(false);
          createForm.resetFields();
          notification.success({ message: t("project_members.messages.created") });
        },
        onError: (error) => handleError(t("project_members.messages.create_failed"), error),
      },
    );
  };

  const updateAssignment = (values: ProjectMemberFormValues) => {
    if (!editTarget || !selectedProjectId) {
      return;
    }

    const payload: UpdateProjectAssignmentInput = {
      id: editTarget.id,
      userId: values.userId,
      projectId: selectedProjectId,
      projectRoleId: values.projectRoleId,
      reportsToUserId: values.reportsToUserId,
      isPrimary: Boolean(values.isPrimary),
      startAt: values.period?.[0]?.startOf("day").toISOString(),
      endAt: values.period?.[1]?.endOf("day").toISOString(),
      reason: values.reason ?? "",
    };

    updateProjectAssignmentMutation.mutate(payload, {
      onSuccess: () => {
        setEditTarget(null);
        editForm.resetFields();
        notification.success({ message: t("project_members.messages.updated") });
      },
      onError: (error) => handleError(t("project_members.messages.update_failed"), error),
    });
  };

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
              <Space wrap style={{ width: "100%", marginBottom: 4, justifyContent: "space-between" }} size={[12, 12]}>
                <Input.Search
                  allowClear
                  placeholder={t("project_members.search_placeholder")}
                  style={{ width: 360, maxWidth: "100%" }}
                  value={searchInput}
                  onChange={(event) => setSearchInput(event.target.value)}
                  onSearch={(value) => setSearchInput(value)}
                />
                <Space>
                  {selectedProjectId ? (
                    <Button onClick={() => navigate(`/app/admin/project-roles?projectId=${selectedProjectId}`)}>
                      {t("project_members.go_to_roles")}
                    </Button>
                  ) : null}
                  {canManageProjectMembers ? (
                    <Button type="primary" icon={<PlusOutlined />} size="large" onClick={() => setCreateOpen(true)}>
                      {t("project_members.create_action")}
                    </Button>
                  ) : null}
                </Space>
              </Space>

              {canReadProjects && projectAssignmentsQuery.isLoading && (projectAssignmentsQuery.data?.items?.length ?? 0) === 0 ? (
                <Skeleton active paragraph={{ rows: 6 }} />
              ) : (
                <Table
                  rowKey="id"
                  columns={columns}
                  dataSource={canReadProjects ? (projectAssignmentsQuery.data?.items ?? []) : []}
                  loading={canReadProjects ? projectAssignmentsQuery.isLoading : false}
                  pagination={{
                    current: projectAssignmentsQuery.data?.page ?? paging.page,
                    pageSize: projectAssignmentsQuery.data?.pageSize ?? paging.pageSize,
                    total: projectAssignmentsQuery.data?.total ?? 0,
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
        title={t("project_members.create_modal_title")}
        open={createOpen && canManageProjectMembers}
        onCancel={() => {
          setCreateOpen(false);
          createForm.resetFields();
        }}
        onOk={() => {
          createForm.validateFields().then(createAssignment).catch(() => undefined);
        }}
        confirmLoading={createProjectAssignmentMutation.isPending}
        width={720}
      >
        <ProjectMemberForm
          form={createForm}
          t={t}
          userOptions={userOptions}
          projectRoleOptions={projectRoleOptionList}
          reportingOptions={reportingOptions}
          includeReason={false}
          userOptionsLoading={userOptionsState.loading}
          onUserSearch={userOptionsState.onSearch}
          onUserLoadMore={userOptionsState.onLoadMore}
          userHasMore={userOptionsState.hasMore}
          roleOptionsLoading={projectRoleOptionsState.loading}
          onRoleSearch={projectRoleOptionsState.onSearch}
          onRoleLoadMore={projectRoleOptionsState.onLoadMore}
          roleHasMore={projectRoleOptionsState.hasMore}
        />
      </Modal>

      <Modal
        title={editTarget ? t("project_members.edit_modal_title_with_name", { name: editTarget.userDisplayName ?? editTarget.userEmail ?? editTarget.userId }) : t("project_members.edit_modal_title")}
        open={editTarget !== null && canManageProjectMembers}
        onCancel={() => {
          setEditTarget(null);
          editForm.resetFields();
        }}
        onOk={() => {
          editForm.validateFields().then(updateAssignment).catch(() => undefined);
        }}
        confirmLoading={updateProjectAssignmentMutation.isPending}
        width={720}
      >
        <ProjectMemberForm
          form={editForm}
          t={t}
          userOptions={userOptions}
          projectRoleOptions={projectRoleOptionList}
          reportingOptions={reportingOptions}
          includeReason
          userOptionsLoading={userOptionsState.loading}
          onUserSearch={userOptionsState.onSearch}
          onUserLoadMore={userOptionsState.onLoadMore}
          userHasMore={userOptionsState.hasMore}
          roleOptionsLoading={projectRoleOptionsState.loading}
          onRoleSearch={projectRoleOptionsState.onSearch}
          onRoleLoadMore={projectRoleOptionsState.onLoadMore}
          roleHasMore={projectRoleOptionsState.hasMore}
        />
      </Modal>

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
