import { useMemo, useState } from "react";
import { App, Button, Card, DatePicker, Form, Input, Modal, Select, Space, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { DeleteOutlined, EditOutlined, FolderOpenOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useTranslation } from "react-i18next";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { formatDate, toApiSortOrder } from "../utils/adminUsersPresentation";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import type { Project } from "../types/users";

const projectStatusOptions = [
  { value: "Planned", label: "Planned" },
  { value: "Active", label: "Active" },
  { value: "OnHold", label: "On Hold" },
  { value: "Completed", label: "Completed" },
  { value: "Cancelled", label: "Cancelled" },
];

export function ProjectsPage() {
  const { t, i18n } = useTranslation();
  const { notification } = App.useApp();
  const [paging, setPaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    sortBy: "createdAt",
    sortOrder: "desc" as "asc" | "desc",
  });
  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<Project | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Project | null>(null);
  const [createForm] = Form.useForm();
  const [editForm] = Form.useForm();
  const [deleteForm] = Form.useForm();

  const {
    projectsQuery,
    createProjectMutation,
    updateProjectMutation,
    deleteProjectMutation,
  } = useProjectAdmin({
    projects: paging,
    projectRoles: { page: 1, pageSize: 10 },
    projectAssignments: null,
  });

  const handleError = (fallbackTitle: string, error: unknown) => {
    const presentation = getApiErrorPresentation(error, fallbackTitle);
    notification.error({ message: presentation.title, description: presentation.description });
  };

  const columns = useMemo<ColumnsType<Project>>(
    () => [
      { title: t("projects.columns.code"), dataIndex: "code", sorter: true },
      { title: t("projects.columns.name"), dataIndex: "name", sorter: true },
      {
        title: t("projects.columns.status"),
        dataIndex: "status",
        sorter: true,
        render: (value: string) => (
          <Tag color={value === "Active" ? "green" : value === "Completed" ? "blue" : value === "Cancelled" ? "red" : "gold"}>
            {value}
          </Tag>
        ),
      },
      {
        title: t("projects.columns.start_at"),
        dataIndex: "startAt",
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
        render: (_, record) => (
          <Space>
            <Button
              icon={<EditOutlined />}
              onClick={() => {
                setEditTarget(record);
                editForm.setFieldsValue({
                  code: record.code,
                  name: record.name,
                  status: record.status,
                  period:
                    record.startAt || record.endAt
                      ? [record.startAt ? dayjs(record.startAt) : null, record.endAt ? dayjs(record.endAt) : null]
                      : undefined,
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
    [deleteForm, editForm, i18n.language, t],
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
              {t("projects.page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("projects.page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Space wrap style={{ width: "100%", marginBottom: 16, justifyContent: "space-between" }} size={[12, 12]}>
          <Input.Search
            allowClear
            placeholder={t("projects.search_placeholder")}
            style={{ width: 360, maxWidth: "100%" }}
            onSearch={(value) => setPaging((current) => ({ ...current, page: 1, search: value }))}
          />
          <Button type="primary" icon={<PlusOutlined />} size="large" onClick={() => setCreateOpen(true)}>
            {t("projects.create_action")}
          </Button>
        </Space>

        <Table
          rowKey="id"
          columns={columns}
          dataSource={projectsQuery.data?.items ?? []}
          loading={projectsQuery.isLoading}
          pagination={{
            current: projectsQuery.data?.page ?? paging.page,
            pageSize: projectsQuery.data?.pageSize ?? paging.pageSize,
            total: projectsQuery.data?.total ?? 0,
            showSizeChanger: true,
            pageSizeOptions: [10, 25, 50, 100],
          }}
          onChange={(nextPagination, _, sorter) => {
            const resolvedSorter = sorter as SorterResult<Project>;
            setPaging((current) => ({
              ...current,
              page: nextPagination.current ?? current.page,
              pageSize: nextPagination.pageSize ?? current.pageSize,
              sortBy: typeof resolvedSorter.field === "string" ? resolvedSorter.field : current.sortBy,
              sortOrder: toApiSortOrder(resolvedSorter.order) ?? current.sortOrder,
            }));
          }}
        />
      </Card>

      <Modal
        title={t("projects.create_modal_title")}
        open={createOpen}
        onCancel={() => {
          setCreateOpen(false);
          createForm.resetFields();
        }}
        onOk={() => {
          createForm.validateFields().then((values) => {
            createProjectMutation.mutate(
              {
                code: values.code,
                name: values.name,
                status: values.status,
                startAt: values.period?.[0]?.startOf("day").toISOString(),
                endAt: values.period?.[1]?.endOf("day").toISOString(),
              },
              {
                onSuccess: () => {
                  setCreateOpen(false);
                  createForm.resetFields();
                  notification.success({ message: t("projects.messages.created", { name: values.name }) });
                },
                onError: (error) => handleError(t("projects.messages.create_failed"), error),
              },
            );
          }).catch(() => undefined);
        }}
        confirmLoading={createProjectMutation.isPending}
      >
        <Form form={createForm} layout="vertical">
          <Form.Item name="code" label={t("projects.fields.code")} rules={[{ required: true }]}>
            <Input placeholder={t("projects.placeholders.code")} />
          </Form.Item>
          <Form.Item name="name" label={t("projects.fields.name")} rules={[{ required: true }]}>
            <Input placeholder={t("projects.placeholders.name")} />
          </Form.Item>
          <Form.Item name="status" label={t("projects.fields.status")} initialValue="Planned" rules={[{ required: true }]}>
            <Select options={projectStatusOptions} />
          </Form.Item>
          <Form.Item name="period" label={t("projects.fields.period")}>
            <DatePicker.RangePicker style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={editTarget ? t("projects.edit_modal_title_with_name", { name: editTarget.name }) : t("projects.edit_modal_title")}
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
            updateProjectMutation.mutate(
              {
                id: editTarget.id,
                code: values.code,
                name: values.name,
                status: values.status,
                startAt: values.period?.[0]?.startOf("day").toISOString(),
                endAt: values.period?.[1]?.endOf("day").toISOString(),
              },
              {
                onSuccess: () => {
                  setEditTarget(null);
                  editForm.resetFields();
                  notification.success({ message: t("projects.messages.updated", { name: values.name }) });
                },
                onError: (error) => handleError(t("projects.messages.update_failed"), error),
              },
            );
          }).catch(() => undefined);
        }}
        confirmLoading={updateProjectMutation.isPending}
      >
        <Form form={editForm} layout="vertical">
          <Form.Item name="code" label={t("projects.fields.code")} rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="name" label={t("projects.fields.name")} rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="status" label={t("projects.fields.status")} rules={[{ required: true }]}>
            <Select options={projectStatusOptions} />
          </Form.Item>
          <Form.Item name="period" label={t("projects.fields.period")}>
            <DatePicker.RangePicker style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={deleteTarget ? t("projects.delete_modal_title_with_name", { name: deleteTarget.name }) : t("projects.delete_modal_title")}
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
