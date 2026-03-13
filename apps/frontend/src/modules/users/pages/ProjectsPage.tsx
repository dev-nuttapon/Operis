import { useMemo, useState } from "react";
import { App, Button, Card, DatePicker, Form, Input, Modal, Select, Space, Table, Tag, Typography } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { DeleteOutlined, EditOutlined, FolderOpenOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useTranslation } from "react-i18next";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { formatDate, toApiSortOrder } from "../utils/adminUsersPresentation";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import type { CreateProjectInput, Project, UpdateProjectInput, User } from "../types/users";

type ProjectFormValues = {
  code: string;
  name: string;
  projectType: string;
  ownerUserId?: string;
  sponsorUserId?: string;
  methodology?: string;
  phase?: string;
  status: string;
  statusReason?: string;
  plannedPeriod?: [dayjs.Dayjs | null, dayjs.Dayjs | null];
  actualPeriod?: [dayjs.Dayjs | null, dayjs.Dayjs | null];
};

function toUserLabel(user: User) {
  return user.keycloak?.email ?? user.keycloak?.username ?? user.id;
}

function normalizeProjectPayload(values: ProjectFormValues): Omit<CreateProjectInput, "id"> {
  return {
    code: values.code,
    name: values.name,
    projectType: values.projectType,
    ownerUserId: values.ownerUserId,
    sponsorUserId: values.sponsorUserId,
    methodology: values.methodology,
    phase: values.phase,
    status: values.status,
    statusReason: values.statusReason,
    plannedStartAt: values.plannedPeriod?.[0]?.startOf("day").toISOString(),
    plannedEndAt: values.plannedPeriod?.[1]?.endOf("day").toISOString(),
    startAt: values.actualPeriod?.[0]?.startOf("day").toISOString(),
    endAt: values.actualPeriod?.[1]?.endOf("day").toISOString(),
  };
}

function toInitialValues(project: Project): ProjectFormValues {
  return {
    code: project.code,
    name: project.name,
    projectType: project.projectType,
    ownerUserId: project.ownerUserId ?? undefined,
    sponsorUserId: project.sponsorUserId ?? undefined,
    methodology: project.methodology ?? undefined,
    phase: project.phase ?? undefined,
    status: project.status,
    statusReason: project.statusReason ?? undefined,
    plannedPeriod:
      project.plannedStartAt || project.plannedEndAt
        ? [project.plannedStartAt ? dayjs(project.plannedStartAt) : null, project.plannedEndAt ? dayjs(project.plannedEndAt) : null]
        : undefined,
    actualPeriod:
      project.startAt || project.endAt
        ? [project.startAt ? dayjs(project.startAt) : null, project.endAt ? dayjs(project.endAt) : null]
        : undefined,
  };
}

function ProjectForm({
  form,
  t,
  userOptions,
}: {
  form: FormInstance<ProjectFormValues>;
  t: ReturnType<typeof useTranslation>["t"];
  userOptions: { label: string; value: string }[];
}) {
  const projectStatusOptions = [
    { value: "planned", label: t("projects.options.status.planned") },
    { value: "active", label: t("projects.options.status.active") },
    { value: "onhold", label: t("projects.options.status.on_hold") },
    { value: "completed", label: t("projects.options.status.completed") },
    { value: "cancelled", label: t("projects.options.status.cancelled") },
  ];
  const projectTypeOptions = [
    { value: "Internal", label: t("projects.options.project_type.internal") },
    { value: "Customer", label: t("projects.options.project_type.customer") },
    { value: "Compliance", label: t("projects.options.project_type.compliance") },
    { value: "Improvement", label: t("projects.options.project_type.improvement") },
  ];
  const methodologyOptions = [
    { value: "Agile", label: t("projects.options.methodology.agile") },
    { value: "Waterfall", label: t("projects.options.methodology.waterfall") },
    { value: "Hybrid", label: t("projects.options.methodology.hybrid") },
  ];
  const phaseOptions = [
    { value: "Initiation", label: t("projects.options.phase.initiation") },
    { value: "Planning", label: t("projects.options.phase.planning") },
    { value: "Execution", label: t("projects.options.phase.execution") },
    { value: "Monitoring", label: t("projects.options.phase.monitoring") },
    { value: "Closure", label: t("projects.options.phase.closure") },
  ];

  return (
    <Form form={form} layout="vertical">
      <Form.Item name="code" label={t("projects.fields.code")} rules={[{ required: true }]}> 
        <Input placeholder={t("projects.placeholders.code")} />
      </Form.Item>
      <Form.Item name="name" label={t("projects.fields.name")} rules={[{ required: true }]}> 
        <Input placeholder={t("projects.placeholders.name")} />
      </Form.Item>
      <Form.Item name="projectType" label={t("projects.fields.project_type")} initialValue="Internal" rules={[{ required: true }]}> 
        <Select options={projectTypeOptions} />
      </Form.Item>
      <Form.Item name="ownerUserId" label={t("projects.fields.owner")}> 
        <Select allowClear showSearch optionFilterProp="label" options={userOptions} placeholder={t("projects.placeholders.owner")} />
      </Form.Item>
      <Form.Item name="sponsorUserId" label={t("projects.fields.sponsor")}> 
        <Select allowClear showSearch optionFilterProp="label" options={userOptions} placeholder={t("projects.placeholders.sponsor")} />
      </Form.Item>
      <Form.Item name="methodology" label={t("projects.fields.methodology")}> 
        <Select allowClear options={methodologyOptions} placeholder={t("projects.placeholders.methodology")} />
      </Form.Item>
      <Form.Item name="phase" label={t("projects.fields.phase")}> 
        <Select allowClear options={phaseOptions} placeholder={t("projects.placeholders.phase")} />
      </Form.Item>
      <Form.Item name="status" label={t("projects.fields.status")} initialValue="planned" rules={[{ required: true }]}> 
        <Select options={projectStatusOptions} />
      </Form.Item>
      <Form.Item name="statusReason" label={t("projects.fields.status_reason")}> 
        <Input.TextArea rows={3} placeholder={t("projects.placeholders.status_reason")} />
      </Form.Item>
      <Form.Item name="plannedPeriod" label={t("projects.fields.planned_period")}> 
        <DatePicker.RangePicker style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item name="actualPeriod" label={t("projects.fields.actual_period")}> 
        <DatePicker.RangePicker style={{ width: "100%" }} />
      </Form.Item>
    </Form>
  );
}

export function ProjectsPage() {
  const { t, i18n } = useTranslation();
  const { notification } = App.useApp();
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
  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<Project | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Project | null>(null);
  const [createForm] = Form.useForm<ProjectFormValues>();
  const [editForm] = Form.useForm<ProjectFormValues>();
  const [deleteForm] = Form.useForm();

  const { projectsQuery, projectMemberUsersQuery, createProjectMutation, updateProjectMutation, deleteProjectMutation } = useProjectAdmin({
    projects: paging,
    projectRoles: { page: 1, pageSize: 10 },
    projectAssignments: null,
  });

  const userOptions = useMemo(
    () => (projectMemberUsersQuery.data?.items ?? []).map((item) => ({ label: toUserLabel(item), value: item.id })),
    [projectMemberUsersQuery.data?.items],
  );

  const handleError = (fallbackTitle: string, error: unknown) => {
    const presentation = getApiErrorPresentation(error, fallbackTitle);
    notification.error({ message: presentation.title, description: presentation.description });
  };

  const columns = useMemo<ColumnsType<Project>>(
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
        render: (_, record) => (
          <Space>
            <Button
              icon={<EditOutlined />}
              onClick={() => {
                setEditTarget(record);
                editForm.setFieldsValue(toInitialValues(record));
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
    [deleteForm, editForm, i18n.language, projectStatusLabel, t],
  );

  const submitCreate = (values: ProjectFormValues) => {
    createProjectMutation.mutate(normalizeProjectPayload(values), {
      onSuccess: () => {
        setCreateOpen(false);
        createForm.resetFields();
        notification.success({ message: t("projects.messages.created", { name: values.name }) });
      },
      onError: (error) => handleError(t("projects.messages.create_failed"), error),
    });
  };

  const submitEdit = (values: ProjectFormValues) => {
    if (!editTarget) {
      return;
    }

    const payload: UpdateProjectInput = {
      id: editTarget.id,
      ...normalizeProjectPayload(values),
    };

    updateProjectMutation.mutate(payload, {
      onSuccess: () => {
        setEditTarget(null);
        editForm.resetFields();
        notification.success({ message: t("projects.messages.updated", { name: values.name }) });
      },
      onError: (error) => handleError(t("projects.messages.update_failed"), error),
    });
  };

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
          loading={projectsQuery.isLoading || projectMemberUsersQuery.isLoading}
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
          createForm.validateFields().then(submitCreate).catch(() => undefined);
        }}
        confirmLoading={createProjectMutation.isPending}
        width={720}
      >
        <ProjectForm form={createForm} t={t} userOptions={userOptions} />
      </Modal>

      <Modal
        title={editTarget ? t("projects.edit_modal_title_with_name", { name: editTarget.name }) : t("projects.edit_modal_title")}
        open={editTarget !== null}
        onCancel={() => {
          setEditTarget(null);
          editForm.resetFields();
        }}
        onOk={() => {
          editForm.validateFields().then(submitEdit).catch(() => undefined);
        }}
        confirmLoading={updateProjectMutation.isPending}
        width={720}
      >
        <ProjectForm form={editForm} t={t} userOptions={userOptions} />
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
