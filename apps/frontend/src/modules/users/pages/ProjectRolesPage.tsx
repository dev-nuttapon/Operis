import { useMemo, useState } from "react";
import { Alert, App, Button, Card, Form, Input, InputNumber, Modal, Select, Space, Table, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { DeleteOutlined, EditOutlined, PlusOutlined, SolutionOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { toApiSortOrder } from "../utils/adminUsersPresentation";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import type { ProjectRole } from "../types/users";

export function ProjectRolesPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const [selectedProjectId, setSelectedProjectId] = useState<string>();
  const [paging, setPaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    sortBy: "displayOrder",
    sortOrder: "asc" as "asc" | "desc",
  });
  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<ProjectRole | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<ProjectRole | null>(null);
  const [createForm] = Form.useForm();
  const [editForm] = Form.useForm();
  const [deleteForm] = Form.useForm();

  const {
    projectsQuery,
    projectRolesQuery,
    createProjectRoleMutation,
    updateProjectRoleMutation,
    deleteProjectRoleMutation,
  } = useProjectAdmin({
    projects: { page: 1, pageSize: 100, sortBy: "name", sortOrder: "asc" },
    projectRoles: { ...paging, projectId: selectedProjectId },
    projectAssignments: null,
  });

  const handleError = (fallbackTitle: string, error: unknown) => {
    const presentation = getApiErrorPresentation(error, fallbackTitle);
    notification.error({ message: presentation.title, description: presentation.description });
  };

  const projectOptions = (projectsQuery.data?.items ?? []).map((item) => ({ label: `${item.code} - ${item.name}`, value: item.id }));
  const selectedProjectName = projectsQuery.data?.items.find((item) => item.id === selectedProjectId)?.name ?? null;

  const columns = useMemo<ColumnsType<ProjectRole>>(
    () => [
      {
        title: t("project_roles.columns.name"),
        dataIndex: "name",
        sorter: true,
      },
      {
        title: t("project_roles.columns.display_order"),
        dataIndex: "displayOrder",
        sorter: true,
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
                  projectId: record.projectId ?? selectedProjectId,
                  name: record.name,
                  displayOrder: record.displayOrder,
                });
              }}
            >
              {t("common.actions.edit")}
            </Button>
            <Button
              danger
              icon={<DeleteOutlined />}
              onClick={() => {
                setDeleteTarget(record);
                deleteForm.resetFields();
              }}
            >
              {t("common.actions.delete")}
            </Button>
          </Space>
        ),
      },
    ],
    [deleteForm, editForm, selectedProjectId, t],
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
            placeholder={t("project_roles.select_project_placeholder")}
            options={projectOptions}
            value={selectedProjectId}
            onChange={(value) => {
              setSelectedProjectId(value);
              setPaging((current) => ({ ...current, page: 1 }));
            }}
          />

          {!selectedProjectId ? (
            <Alert type="info" showIcon message={t("project_roles.select_project_message")} />
          ) : (
            <>
              <Space wrap style={{ width: "100%", marginBottom: 4, justifyContent: "space-between" }} size={[12, 12]}>
                <Input.Search
                  allowClear
                  placeholder={t("project_roles.search_placeholder")}
                  style={{ width: 360, maxWidth: "100%" }}
                  onSearch={(value) => setPaging((current) => ({ ...current, page: 1, search: value }))}
                />
                <Button type="primary" icon={<PlusOutlined />} size="large" onClick={() => {
                  createForm.setFieldValue("projectId", selectedProjectId);
                  setCreateOpen(true);
                }}>
                  {t("project_roles.create_action")}
                </Button>
              </Space>

              <Table
                rowKey="id"
                columns={columns}
                dataSource={projectRolesQuery.data?.items ?? []}
                loading={projectRolesQuery.isLoading}
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
            </>
          )}
        </Space>
      </Card>

      <Modal
        title={t("project_roles.create_modal_title")}
        open={createOpen}
        onCancel={() => {
          setCreateOpen(false);
          createForm.resetFields();
        }}
        onOk={() => {
          createForm.validateFields().then((values) => {
            createProjectRoleMutation.mutate(
              {
                projectId: values.projectId,
                name: values.name,
                displayOrder: values.displayOrder,
              },
              {
                onSuccess: () => {
                  setCreateOpen(false);
                  createForm.resetFields();
                  notification.success({ message: t("project_roles.messages.created", { name: values.name }) });
                },
                onError: (error) => handleError(t("project_roles.messages.create_failed"), error),
              },
            );
          }).catch(() => undefined);
        }}
        confirmLoading={createProjectRoleMutation.isPending}
      >
        <Form form={createForm} layout="vertical" initialValues={{ projectId: selectedProjectId }}>
          <Form.Item name="projectId" label={t("project_roles.fields.project")} rules={[{ required: true }]}>
            <Select options={projectOptions} />
          </Form.Item>
          <Form.Item name="name" label={t("project_roles.fields.name")} rules={[{ required: true }]}>
            <Input placeholder={t("project_roles.placeholders.name")} />
          </Form.Item>
          <Form.Item name="displayOrder" label={t("project_roles.fields.display_order")} rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={editTarget ? t("project_roles.edit_modal_title_with_name", { name: editTarget.name }) : t("project_roles.edit_modal_title")}
        open={editTarget !== null}
        onCancel={() => {
          setEditTarget(null);
          editForm.resetFields();
        }}
        onOk={() => {
          editForm.validateFields().then((values) => {
            if (!editTarget) {
              return;
            }
            updateProjectRoleMutation.mutate(
              {
                id: editTarget.id,
                projectId: values.projectId,
                name: values.name,
                displayOrder: values.displayOrder,
              },
              {
                onSuccess: () => {
                  setEditTarget(null);
                  editForm.resetFields();
                  notification.success({ message: t("project_roles.messages.updated", { name: values.name }) });
                },
                onError: (error) => handleError(t("project_roles.messages.update_failed"), error),
              },
            );
          }).catch(() => undefined);
        }}
        confirmLoading={updateProjectRoleMutation.isPending}
      >
        <Form form={editForm} layout="vertical">
          <Form.Item name="projectId" label={t("project_roles.fields.project")} rules={[{ required: true }]}>
            <Select options={projectOptions} />
          </Form.Item>
          <Form.Item name="name" label={t("project_roles.fields.name")} rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="displayOrder" label={t("project_roles.fields.display_order")} rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={deleteTarget ? t("project_roles.delete_modal_title_with_name", { name: deleteTarget.name }) : t("project_roles.delete_modal_title")}
        open={deleteTarget !== null}
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
          <Typography.Paragraph type="secondary">
            {t("project_roles.delete_description", { project: selectedProjectName ?? "-" })}
          </Typography.Paragraph>
          <Form.Item name="reason" label={t("admin_users.fields.reason")} rules={[{ required: true, message: t("admin_users.validation.reason_required") }]}>
            <Input.TextArea rows={4} placeholder={t("project_roles.placeholders.delete_reason")} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
