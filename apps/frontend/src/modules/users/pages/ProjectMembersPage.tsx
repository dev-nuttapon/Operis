import { useMemo, useState } from "react";
import { Alert, App, Button, Card, Checkbox, DatePicker, Form, Input, Modal, Select, Space, Table, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { DeleteOutlined, EditOutlined, PlusOutlined, ShareAltOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useTranslation } from "react-i18next";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { formatDate, toApiSortOrder } from "../utils/adminUsersPresentation";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import type { ProjectAssignment } from "../types/users";

export function ProjectMembersPage() {
  const { t, i18n } = useTranslation();
  const { notification } = App.useApp();
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
  const [createForm] = Form.useForm();
  const [editForm] = Form.useForm();

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
  const userMap = new Map(
    (projectMemberUsersQuery.data?.items ?? []).map((item) => [
      item.id,
      item.keycloak?.email ?? item.keycloak?.username ?? item.id,
    ]),
  );
  const userOptions = (projectMemberUsersQuery.data?.items ?? []).map((item) => ({
    label: userMap.get(item.id) ?? item.id,
    value: item.id,
  }));
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
            <Button
              icon={<EditOutlined />}
              onClick={() => {
                setEditTarget(record);
                editForm.setFieldsValue({
                  userId: record.userId,
                  projectRoleId: record.projectRoleId,
                  reportsToUserId: record.reportsToUserId ?? undefined,
                  isPrimary: record.isPrimary,
                  period: [record.startAt ? dayjs(record.startAt) : null, record.endAt ? dayjs(record.endAt) : null],
                });
              }}
            >
              {t("common.actions.edit")}
            </Button>
            <Button danger icon={<DeleteOutlined />} onClick={() => setDeleteTarget(record)}>
              {t("common.actions.delete")}
            </Button>
          </Space>
        ),
      },
    ],
    [editForm, i18n.language, t, userMap],
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
            placeholder={t("project_members.select_project_placeholder")}
            options={projectOptions}
            value={selectedProjectId}
            onChange={(value) => {
              setSelectedProjectId(value);
              setPaging((current) => ({ ...current, page: 1 }));
            }}
          />

          {!selectedProjectId ? (
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
                <Button type="primary" icon={<PlusOutlined />} size="large" onClick={() => setCreateOpen(true)}>
                  {t("project_members.create_action")}
                </Button>
              </Space>

              <Table
                rowKey="id"
                columns={columns}
                dataSource={projectAssignmentsQuery.data?.items ?? []}
                loading={projectAssignmentsQuery.isLoading}
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
        open={createOpen}
        onCancel={() => {
          setCreateOpen(false);
          createForm.resetFields();
        }}
        onOk={() => {
          createForm.validateFields().then((values) => {
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
          }).catch(() => undefined);
        }}
        confirmLoading={createProjectAssignmentMutation.isPending}
      >
        <Form form={createForm} layout="vertical" initialValues={{ isPrimary: false }}>
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
          <Form.Item name="isPrimary" valuePropName="checked">
            <Checkbox>{t("project_members.fields.is_primary")}</Checkbox>
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={editTarget ? t("project_members.edit_modal_title_with_name", { name: userMap.get(editTarget.userId) ?? editTarget.userDisplayName ?? editTarget.userEmail ?? editTarget.userId }) : t("project_members.edit_modal_title")}
        open={editTarget !== null}
        onCancel={() => {
          setEditTarget(null);
          editForm.resetFields();
        }}
        onOk={() => {
          editForm.validateFields().then((values) => {
            if (!editTarget || !selectedProjectId) {
              return;
            }
            updateProjectAssignmentMutation.mutate(
              {
                id: editTarget.id,
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
                  setEditTarget(null);
                  editForm.resetFields();
                  notification.success({ message: t("project_members.messages.updated") });
                },
                onError: (error) => handleError(t("project_members.messages.update_failed"), error),
              },
            );
          }).catch(() => undefined);
        }}
        confirmLoading={updateProjectAssignmentMutation.isPending}
      >
        <Form form={editForm} layout="vertical">
          <Form.Item name="userId" label={t("project_members.fields.user")} rules={[{ required: true }]}>
            <Select showSearch options={userOptions} />
          </Form.Item>
          <Form.Item name="projectRoleId" label={t("project_members.fields.project_role")} rules={[{ required: true }]}>
            <Select options={projectRoleOptions} />
          </Form.Item>
          <Form.Item name="reportsToUserId" label={t("project_members.fields.reports_to")}>
            <Select allowClear showSearch options={reportingOptions} />
          </Form.Item>
          <Form.Item name="period" label={t("project_members.fields.period")}>
            <DatePicker.RangePicker style={{ width: "100%" }} />
          </Form.Item>
          <Form.Item name="isPrimary" valuePropName="checked">
            <Checkbox>{t("project_members.fields.is_primary")}</Checkbox>
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={deleteTarget ? t("project_members.delete_modal_title_with_name", { name: userMap.get(deleteTarget.userId) ?? deleteTarget.userDisplayName ?? deleteTarget.userEmail ?? deleteTarget.userId }) : t("project_members.delete_modal_title")}
        open={deleteTarget !== null}
        onCancel={() => setDeleteTarget(null)}
        onOk={() => {
          if (!deleteTarget) {
            return;
          }
          deleteProjectAssignmentMutation.mutate(deleteTarget.id, {
            onSuccess: () => {
              setDeleteTarget(null);
              notification.success({ message: t("project_members.messages.deleted") });
            },
            onError: (error) => handleError(t("project_members.messages.delete_failed"), error),
          });
        }}
        okButtonProps={{ danger: true }}
        confirmLoading={deleteProjectAssignmentMutation.isPending}
      >
        <Typography.Paragraph type="secondary">
          {t("project_members.delete_description")}
        </Typography.Paragraph>
      </Modal>
    </Space>
  );
}
