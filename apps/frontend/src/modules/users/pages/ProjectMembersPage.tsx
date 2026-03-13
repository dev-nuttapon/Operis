import { useMemo, useState } from "react";
import { Alert, App, Button, Card, Checkbox, DatePicker, Form, Input, Modal, Select, Space, Table, Tag, Typography } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { DeleteOutlined, EditOutlined, PlusOutlined, ShareAltOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useTranslation } from "react-i18next";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { formatDate, toApiSortOrder } from "../utils/adminUsersPresentation";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import type { ProjectAssignment, UpdateProjectAssignmentInput, User } from "../types/users";

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
}: {
  form: FormInstance<ProjectMemberFormValues>;
  t: ReturnType<typeof useTranslation>["t"];
  userOptions: { label: string; value: string }[];
  projectRoleOptions: { label: string; value: string }[];
  reportingOptions: { label: string; value: string }[];
  includeReason: boolean;
}) {
  return (
    <Form form={form} layout="vertical" initialValues={{ isPrimary: false }}>
      <Form.Item name="userId" label={t("project_members.fields.user")} rules={[{ required: true }]}> 
        <Select showSearch options={userOptions} placeholder={t("project_members.placeholders.user")} />
      </Form.Item>
      <Form.Item name="projectRoleId" label={t("project_members.fields.project_role")} rules={[{ required: true }]}> 
        <Select options={projectRoleOptions} placeholder={t("project_members.placeholders.project_role")} />
      </Form.Item>
      <Form.Item name="reportsToUserId" label={t("project_members.fields.reports_to")}> 
        <Select allowClear showSearch options={reportingOptions} placeholder={t("project_members.placeholders.reports_to")} />
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
  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<ProjectAssignment | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<ProjectAssignment | null>(null);
  const [createForm] = Form.useForm<ProjectMemberFormValues>();
  const [editForm] = Form.useForm<ProjectMemberFormValues>();
  const [deleteForm] = Form.useForm<{ reason: string }>();

  const {
    projectsQuery,
    projectRolesQuery,
    projectAssignmentsQuery,
    projectMemberUsersQuery,
    createProjectAssignmentMutation,
    updateProjectAssignmentMutation,
    deleteProjectAssignmentMutation,
  } = useProjectAdmin({
    projects: { page: 1, pageSize: 100, sortBy: "name", sortOrder: "asc" },
    projectRoles: { projectId: selectedProjectId, page: 1, pageSize: 100, sortBy: "displayOrder", sortOrder: "asc" },
    projectAssignments: selectedProjectId ? { projectId: selectedProjectId, ...paging } : null,
  });

  const handleError = (fallbackTitle: string, error: unknown) => {
    const presentation = getApiErrorPresentation(error, fallbackTitle);
    notification.error({ message: presentation.title, description: presentation.description });
  };

  const projectOptions = (projectsQuery.data?.items ?? []).map((item) => ({ label: `${item.code} - ${item.name}`, value: item.id }));
  const projectRoleOptions = (projectRolesQuery.data?.items ?? []).map((item) => ({ label: item.name, value: item.id }));
  const userMap = new Map((projectMemberUsersQuery.data?.items ?? []).map((item) => [item.id, toUserLabel(item)]));
  const userOptions = (projectMemberUsersQuery.data?.items ?? []).map((item) => ({ label: userMap.get(item.id) ?? item.id, value: item.id }));
  const reportingOptions = (projectAssignmentsQuery.data?.items ?? [])
    .filter((item) => item.id !== editTarget?.id)
    .map((item) => ({
      label: userMap.get(item.userId) ?? item.userDisplayName ?? item.userEmail ?? item.userId,
      value: item.userId,
    }));

  const columns = useMemo<ColumnsType<ProjectAssignment>>(
    () => [
      {
        title: t("project_members.columns.member"),
        dataIndex: "userDisplayName",
        render: (_, record) => userMap.get(record.userId) ?? record.userDisplayName ?? record.userEmail ?? record.userId,
      },
      {
        title: t("project_members.columns.project_role"),
        dataIndex: "projectRoleName",
      },
      {
        title: t("project_members.columns.reports_to"),
        dataIndex: "reportsToDisplayName",
        render: (_, record) => (record.reportsToUserId ? userMap.get(record.reportsToUserId) ?? record.reportsToDisplayName ?? record.reportsToUserId : "-"),
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
    [canManageProjectMembers, editForm, i18n.language, t, userMap, deleteForm],
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
            placeholder={t("project_members.select_project_placeholder")}
            options={projectOptions}
            value={selectedProjectId}
            onChange={(value) => {
              setSelectedProjectId(value);
              setPaging((current) => ({ ...current, page: 1 }));
            }}
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
                  onSearch={(value) => setPaging((current) => ({ ...current, page: 1, search: value }))}
                />
                {canManageProjectMembers ? (
                  <Button type="primary" icon={<PlusOutlined />} size="large" onClick={() => setCreateOpen(true)}>
                    {t("project_members.create_action")}
                  </Button>
                ) : null}
              </Space>

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
        <ProjectMemberForm form={createForm} t={t} userOptions={userOptions} projectRoleOptions={projectRoleOptions} reportingOptions={reportingOptions} includeReason={false} />
      </Modal>

      <Modal
        title={editTarget ? t("project_members.edit_modal_title_with_name", { name: userMap.get(editTarget.userId) ?? editTarget.userDisplayName ?? editTarget.userEmail ?? editTarget.userId }) : t("project_members.edit_modal_title")}
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
        <ProjectMemberForm form={editForm} t={t} userOptions={userOptions} projectRoleOptions={projectRoleOptions} reportingOptions={reportingOptions} includeReason />
      </Modal>

      <Modal
        title={deleteTarget ? t("project_members.delete_modal_title_with_name", { name: userMap.get(deleteTarget.userId) ?? deleteTarget.userDisplayName ?? deleteTarget.userEmail ?? deleteTarget.userId }) : t("project_members.delete_modal_title")}
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
