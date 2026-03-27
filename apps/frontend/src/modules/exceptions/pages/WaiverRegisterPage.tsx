import { useEffect, useMemo, useState } from "react";
import { Alert, Button, Card, Checkbox, DatePicker, Divider, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { DeleteOutlined, ExceptionOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useProjectOptions } from "../../users";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateWaiver, useUpdateWaiver, useWaiver, useWaivers } from "../hooks/useExceptions";
import type { CompensatingControlInput, CreateWaiverInput, UpdateWaiverInput, WaiverListItem } from "../types/exceptions";

const { Title, Paragraph } = Typography;
const processAreaOptions = ["project_governance", "requirements_traceability", "verification", "change_control", "operations_review"].map((value) => ({ label: value, value }));
const statusOptions = ["draft", "submitted", "approved", "rejected", "expired", "closed"].map((value) => ({ label: value, value }));

export function WaiverRegisterPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.exceptions.read, permissions.exceptions.manage, permissions.exceptions.approve);
  const canManage = permissionState.hasPermission(permissions.exceptions.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ projectId: undefined as string | undefined, processArea: undefined as string | undefined, status: undefined as string | undefined, onlyExpired: false, search: "", page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<WaiverListItem | null>(null);
  const [form] = Form.useForm();
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 50 });
  const query = useWaivers(filters, canRead);
  const editingDetail = useWaiver(editing?.id, Boolean(editing) && canRead);
  const createMutation = useCreateWaiver();
  const updateMutation = useUpdateWaiver();

  const columns = useMemo<ColumnsType<WaiverListItem>>(
    () => [
      { title: "Waiver Code", dataIndex: "waiverCode" },
      { title: "Project", render: (_, item) => item.projectName ?? "-" },
      { title: "Process Area", dataIndex: "processArea" },
      { title: "Expiry", dataIndex: "expiresAt" },
      { title: "Controls", dataIndex: "compensatingControlCount" },
      { title: "Status", dataIndex: "status", render: (value: string, item) => <Tag color={item.isExpired ? "red" : "blue"}>{value}</Tag> },
      {
        title: "Actions",
        render: (_, item) => (
          <Space size={8}>
            <Button size="small" href={`/app/exceptions/waivers/${item.id}`}>Open</Button>
            <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button>
          </Space>
        ),
      },
    ],
    [canManage],
  );

  useEffect(() => {
    if (!createOpen) {
      return;
    }

    form.setFieldsValue({
      compensatingControls: [
        {
          controlCode: "",
          description: "",
          ownerUserId: "",
          status: "planned",
        },
      ],
    });
  }, [createOpen, form]);

  useEffect(() => {
    if (!editing || !editingDetail.data) {
      return;
    }

    form.setFieldsValue({
      waiverCode: editingDetail.data.waiverCode,
      projectId: editingDetail.data.projectId ?? undefined,
      processArea: editingDetail.data.processArea,
      scopeSummary: editingDetail.data.scopeSummary,
      requestedByUserId: editingDetail.data.requestedByUserId,
      justification: editingDetail.data.justification,
      effectiveFrom: dayjs(editingDetail.data.effectiveFrom),
      expiresAt: dayjs(editingDetail.data.expiresAt),
      compensatingControls:
        editingDetail.data.compensatingControls.length > 0
          ? editingDetail.data.compensatingControls.map((control) => ({
              controlCode: control.controlCode,
              description: control.description,
              ownerUserId: control.ownerUserId,
              status: control.status,
            }))
          : [
              {
                controlCode: "",
                description: "",
                ownerUserId: "",
                status: "planned",
              },
            ],
    });
  }, [editing, editingDetail.data, form]);

  if (!canRead) {
    return <Alert type="warning" showIcon message="Process waivers are not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(mapFormValues(values));
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Waiver created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create waiver");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: mapFormValues(values) as UpdateWaiverInput });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Waiver updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update waiver");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #b45309, #7c2d12)", color: "#fff" }}>
            <ExceptionOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Process Waivers</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Govern temporary deviations, their compensating controls, and review/expiry status in one register.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select allowClear showSearch placeholder="Project" style={{ width: 240 }} options={projectOptions.options} value={filters.projectId} onSearch={projectOptions.onSearch} onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
            <Select allowClear placeholder="Process Area" style={{ width: 220 }} options={processAreaOptions} value={filters.processArea} onChange={(value) => setFilters((current) => ({ ...current, processArea: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={statusOptions} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input.Search allowClear placeholder="Search code or scope" style={{ width: 220 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Checkbox checked={filters.onlyExpired} onChange={(event) => setFilters((current) => ({ ...current, onlyExpired: event.target.checked, page: 1 }))}>Only expired</Checkbox>
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New waiver</Button>
        </Flex>
        <Table rowKey="id" loading={query.isLoading} columns={columns} dataSource={query.data?.items ?? []} pagination={{ current: query.data?.page ?? filters.page, pageSize: query.data?.pageSize ?? filters.pageSize, total: query.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create waiver" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden width={720}>
        <WaiverForm form={form} projectOptions={projectOptions.options} />
      </Modal>

      <Modal title="Edit waiver" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending || editingDetail.isLoading} destroyOnHidden width={720}>
        <WaiverForm form={form} projectOptions={projectOptions.options} />
      </Modal>
    </Space>
  );
}

function WaiverForm({ form, projectOptions }: { form: any; projectOptions: Array<{ label: string; value: string }> }) {
  return (
    <Form form={form} layout="vertical">
      <Form.Item label="Waiver Code" name="waiverCode" rules={[{ required: true, message: "Waiver code is required." }]}>
        <Input />
      </Form.Item>
      <Form.Item label="Project" name="projectId">
        <Select allowClear showSearch options={projectOptions} />
      </Form.Item>
      <Form.Item label="Process Area" name="processArea" rules={[{ required: true, message: "Process area is required." }]}>
        <Select options={processAreaOptions} />
      </Form.Item>
      <Form.Item label="Scope Summary" name="scopeSummary" rules={[{ required: true, message: "Scope summary is required." }]}>
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item label="Requested By" name="requestedByUserId" rules={[{ required: true, message: "Requester is required." }]}>
        <Input />
      </Form.Item>
      <Form.Item label="Justification" name="justification" rules={[{ required: true, message: "Justification is required." }]}>
        <Input.TextArea rows={4} />
      </Form.Item>
      <Flex gap={12}>
        <Form.Item label="Effective From" name="effectiveFrom" rules={[{ required: true, message: "Effective date is required." }]} style={{ flex: 1 }}>
          <DatePicker style={{ width: "100%" }} />
        </Form.Item>
        <Form.Item label="Expires At" name="expiresAt" rules={[{ required: true, message: "Expiry date is required." }]} style={{ flex: 1 }}>
          <DatePicker style={{ width: "100%" }} />
        </Form.Item>
      </Flex>
      <Divider plain>Compensating Controls</Divider>
      <Form.List name="compensatingControls">
        {(fields, { add, remove }) => (
          <Space direction="vertical" size={12} style={{ width: "100%" }}>
            {fields.map((field) => (
              <Card
                key={field.key}
                size="small"
                title={`Control ${field.name + 1}`}
                extra={fields.length > 1 ? <Button type="text" danger icon={<DeleteOutlined />} onClick={() => remove(field.name)} /> : null}
              >
                <Form.Item label="Control Code" name={[field.name, "controlCode"]} rules={[{ required: true, message: "Control code is required." }]}>
                  <Input />
                </Form.Item>
                <Form.Item label="Description" name={[field.name, "description"]} rules={[{ required: true, message: "Description is required." }]}>
                  <Input.TextArea rows={3} />
                </Form.Item>
                <Flex gap={12}>
                  <Form.Item label="Owner" name={[field.name, "ownerUserId"]} rules={[{ required: true, message: "Control owner is required." }]} style={{ flex: 1 }}>
                    <Input />
                  </Form.Item>
                  <Form.Item label="Status" name={[field.name, "status"]} style={{ flex: 1 }} initialValue="planned">
                    <Select
                      options={[
                        { label: "planned", value: "planned" },
                        { label: "active", value: "active" },
                        { label: "verified", value: "verified" },
                        { label: "retired", value: "retired" },
                      ]}
                    />
                  </Form.Item>
                </Flex>
              </Card>
            ))}
            <Button icon={<PlusOutlined />} onClick={() => add({ controlCode: "", description: "", ownerUserId: "", status: "planned" })}>
              Add compensating control
            </Button>
          </Space>
        )}
      </Form.List>
    </Form>
  );
}

function mapFormValues(values: any): CreateWaiverInput {
  return {
    waiverCode: values.waiverCode,
    projectId: values.projectId ?? null,
    processArea: values.processArea,
    scopeSummary: values.scopeSummary,
    requestedByUserId: values.requestedByUserId,
    justification: values.justification,
    effectiveFrom: values.effectiveFrom?.format("YYYY-MM-DD") ?? null,
    expiresAt: values.expiresAt?.format("YYYY-MM-DD") ?? null,
    compensatingControls: normalizeCompensatingControls(values.compensatingControls),
  };
}

function normalizeCompensatingControls(values: unknown): CompensatingControlInput[] {
  if (!Array.isArray(values)) {
    return [];
  }

  const controls: CompensatingControlInput[] = [];
  values.forEach((value) => {
      if (!value || typeof value !== "object") {
        return;
      }

      const control = value as Record<string, unknown>;
      const normalized: CompensatingControlInput = {
        controlCode: String(control.controlCode ?? "").trim(),
        description: String(control.description ?? "").trim(),
        ownerUserId: String(control.ownerUserId ?? "").trim(),
        status: String(control.status ?? "planned").trim() || "planned",
      };

      if (normalized.controlCode && normalized.description && normalized.ownerUserId) {
        controls.push(normalized);
      }
    });

  return controls;
}
