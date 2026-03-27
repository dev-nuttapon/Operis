import { useMemo, useState } from "react";
import { Alert, Button, Card, Drawer, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { ApartmentOutlined, LinkOutlined, PlusOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectOptions } from "../../users";
import {
  useControlCatalog,
  useControlCatalogItem,
  useControlMappings,
  useCreateControlCatalogItem,
  useCreateControlMapping,
  useTransitionControlMapping,
  useUpdateControlCatalogItem,
} from "../hooks/useAssessment";
import type {
  ControlCatalogItem,
  ControlMappingDetail,
  CreateControlCatalogItemInput,
  CreateControlMappingInput,
  UpdateControlCatalogItemInput,
} from "../types/assessment";

const { Title, Paragraph, Text } = Typography;

const controlSetOptions = ["cmmi", "iso9001", "security", "internal"].map((value) => ({ value, label: value }));
const statusOptions = ["draft", "active", "retired"].map((value) => ({ value, label: value }));
const processAreaOptions = [
  "project_governance",
  "requirements_traceability",
  "document_governance",
  "change_control",
  "verification",
  "audit_capa",
  "security_resilience",
].map((value) => ({ value, label: value }));
const targetModuleOptions = ["governance", "documents", "requirements", "change_control", "verification", "audits", "operations"].map((value) => ({ value, label: value }));
const evidenceStatusOptions = ["referenced", "verified", "gap"].map((value) => ({ value, label: value }));

export function ControlMappingPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.assessment.controlsRead, permissions.assessment.controlsManage);
  const canManage = permissionState.hasPermission(permissions.assessment.controlsManage);
  const [messageApi, contextHolder] = message.useMessage();
  const [controlFilters, setControlFilters] = useState({ projectId: undefined as string | undefined, controlSet: undefined as string | undefined, processArea: undefined as string | undefined, status: undefined as string | undefined, search: "", page: 1, pageSize: 25 });
  const [mappingFilters, setMappingFilters] = useState({ controlId: undefined as string | undefined, projectId: undefined as string | undefined, status: undefined as string | undefined, targetModule: undefined as string | undefined, search: "", page: 1, pageSize: 25 });
  const [createControlOpen, setCreateControlOpen] = useState(false);
  const [editingControl, setEditingControl] = useState<ControlCatalogItem | null>(null);
  const [createMappingOpen, setCreateMappingOpen] = useState(false);
  const [selectedControlId, setSelectedControlId] = useState<string | null>(null);
  const [selectedMappingId, setSelectedMappingId] = useState<string | null>(null);
  const [transitionTarget, setTransitionTarget] = useState<"active" | "retired" | null>(null);
  const [controlForm] = Form.useForm<CreateControlCatalogItemInput & { status?: string }>();
  const [mappingForm] = Form.useForm<CreateControlMappingInput>();
  const [transitionForm] = Form.useForm<{ reason: string }>();

  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 50 });
  const controlsQuery = useControlCatalog(controlFilters, canRead);
  const selectedControlQuery = useControlCatalogItem(selectedControlId, canRead && Boolean(selectedControlId));
  const mappingsQuery = useControlMappings(mappingFilters, canRead);
  const createControlMutation = useCreateControlCatalogItem();
  const updateControlMutation = useUpdateControlCatalogItem();
  const createMappingMutation = useCreateControlMapping();
  const transitionMappingMutation = useTransitionControlMapping();

  const controlColumns = useMemo<ColumnsType<ControlCatalogItem>>(
    () => [
      { title: "Control", render: (_, item) => <Space direction="vertical" size={0}><Text strong>{item.controlCode}</Text><Text type="secondary">{item.title}</Text></Space> },
      { title: "Set", dataIndex: "controlSet" },
      { title: "Process Area", dataIndex: "processArea", render: (value) => value ?? "cross-process" },
      { title: "Project", dataIndex: "projectName", render: (value) => value ?? "Global" },
      { title: "Mappings", dataIndex: "activeMappingCount" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "active" ? "green" : value === "retired" ? "default" : "gold"}>{value}</Tag> },
      {
        title: "Actions",
        render: (_, item) => (
          <Space size={8}>
            <Button size="small" onClick={() => { setSelectedControlId(item.id); setMappingFilters((current) => ({ ...current, controlId: item.id, page: 1 })); }}>View mappings</Button>
            <Button size="small" disabled={!canManage} onClick={() => setEditingControl(item)}>Edit</Button>
            <Button size="small" type="primary" disabled={!canManage} onClick={() => { setSelectedControlId(item.id); mappingForm.setFieldValue("controlId", item.id); setCreateMappingOpen(true); }}>Map evidence</Button>
          </Space>
        ),
      },
    ],
    [canManage, mappingForm],
  );

  const mappingColumns = useMemo<ColumnsType<ControlMappingDetail>>(
    () => [
      { title: "Control", render: (_, item) => <Space direction="vertical" size={0}><Text strong>{item.controlCode}</Text><Text type="secondary">{item.controlTitle}</Text></Space> },
      { title: "Target", render: (_, item) => `${item.targetModule}:${item.targetEntityType}:${item.targetEntityId}` },
      { title: "Evidence", dataIndex: "evidenceStatus", render: (value: string) => <Tag color={value === "verified" ? "green" : value === "gap" ? "red" : "blue"}>{value}</Tag> },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Project", dataIndex: "projectName", render: (value) => value ?? "Global" },
      {
        title: "Actions",
        render: (_, item) => (
          <Space size={8}>
            <Button size="small" onClick={() => setSelectedMappingId(item.id)}>Open</Button>
            <Button size="small" disabled={!canManage || item.status !== "draft"} onClick={() => { setSelectedMappingId(item.id); setTransitionTarget("active"); }}>Activate</Button>
            <Button size="small" disabled={!canManage || item.status !== "active"} onClick={() => { setSelectedMappingId(item.id); setTransitionTarget("retired"); }}>Retire</Button>
          </Space>
        ),
      },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Control mapping is not available for this account." />;
  }

  const submitCreateControl = async () => {
    const values = await controlForm.validateFields();
    try {
      await createControlMutation.mutateAsync(values);
      controlForm.resetFields();
      setCreateControlOpen(false);
      void messageApi.success("Control catalog item created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create control catalog item");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdateControl = async () => {
    if (!editingControl) {
      return;
    }

    const values = await controlForm.validateFields();
    try {
      await updateControlMutation.mutateAsync({ id: editingControl.id, input: values as UpdateControlCatalogItemInput });
      controlForm.resetFields();
      setEditingControl(null);
      void messageApi.success("Control catalog item updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update control catalog item");
      void messageApi.error(presentation.description);
    }
  };

  const submitCreateMapping = async () => {
    const values = await mappingForm.validateFields();
    try {
      const detail = await createMappingMutation.mutateAsync(values);
      setCreateMappingOpen(false);
      mappingForm.resetFields();
      setSelectedMappingId(detail.id);
      void messageApi.success("Control mapping created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create control mapping");
      void messageApi.error(presentation.description);
    }
  };

  const submitTransition = async () => {
    if (!selectedMappingId || !transitionTarget) {
      return;
    }

    const values = await transitionForm.validateFields();
    try {
      await transitionMappingMutation.mutateAsync({ id: selectedMappingId, input: { targetStatus: transitionTarget, reason: values.reason } });
      transitionForm.resetFields();
      setTransitionTarget(null);
      void messageApi.success(`Control mapping moved to ${transitionTarget}.`);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to transition control mapping");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1d4ed8, #0f766e)", color: "#fff" }}>
            <ApartmentOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Control Mapping</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Maintain the control catalog, attach governed evidence targets, and activate mappings only when the control-to-artifact link is ready for coverage review.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless" title="Control Catalog" extra={<Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateControlOpen(true)}>New control</Button>}>
        <Flex gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Select allowClear showSearch placeholder="Project" style={{ width: 220 }} options={projectOptions.options} onSearch={projectOptions.onSearch} value={controlFilters.projectId} onChange={(value) => setControlFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
          <Select allowClear placeholder="Control Set" style={{ width: 180 }} options={controlSetOptions} value={controlFilters.controlSet} onChange={(value) => setControlFilters((current) => ({ ...current, controlSet: value, page: 1 }))} />
          <Select allowClear placeholder="Process Area" style={{ width: 220 }} options={processAreaOptions} value={controlFilters.processArea} onChange={(value) => setControlFilters((current) => ({ ...current, processArea: value, page: 1 }))} />
          <Select allowClear placeholder="Status" style={{ width: 180 }} options={statusOptions} value={controlFilters.status} onChange={(value) => setControlFilters((current) => ({ ...current, status: value, page: 1 }))} />
          <Input.Search allowClear placeholder="Search control code or title" style={{ width: 240 }} value={controlFilters.search} onChange={(event) => setControlFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
        </Flex>
        <Table rowKey="id" loading={controlsQuery.isLoading} columns={controlColumns} dataSource={controlsQuery.data?.items ?? []} pagination={{ current: controlsQuery.data?.page ?? controlFilters.page, pageSize: controlsQuery.data?.pageSize ?? controlFilters.pageSize, total: controlsQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setControlFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Card variant="borderless" title="Control Mappings">
        <Flex gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Select allowClear showSearch placeholder="Project" style={{ width: 220 }} options={projectOptions.options} onSearch={projectOptions.onSearch} value={mappingFilters.projectId} onChange={(value) => setMappingFilters((current) => ({ ...current, projectId: value, page: 1 }))} />
          <Select allowClear placeholder="Status" style={{ width: 180 }} options={statusOptions} value={mappingFilters.status} onChange={(value) => setMappingFilters((current) => ({ ...current, status: value, page: 1 }))} />
          <Select allowClear placeholder="Target Module" style={{ width: 180 }} options={targetModuleOptions} value={mappingFilters.targetModule} onChange={(value) => setMappingFilters((current) => ({ ...current, targetModule: value, page: 1 }))} />
          <Input.Search allowClear placeholder="Search mapping target" style={{ width: 240 }} value={mappingFilters.search} onChange={(event) => setMappingFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
        </Flex>
        <Table rowKey="id" loading={mappingsQuery.isLoading} columns={mappingColumns} dataSource={mappingsQuery.data?.items ?? []} pagination={{ current: mappingsQuery.data?.page ?? mappingFilters.page, pageSize: mappingsQuery.data?.pageSize ?? mappingFilters.pageSize, total: mappingsQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setMappingFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create control catalog item" open={createControlOpen} onOk={() => void submitCreateControl()} onCancel={() => { setCreateControlOpen(false); controlForm.resetFields(); }} confirmLoading={createControlMutation.isPending} destroyOnHidden>
        <ControlCatalogForm form={controlForm} projectOptions={projectOptions.options} onProjectSearch={projectOptions.onSearch} />
      </Modal>

      <Modal
        title="Edit control catalog item"
        open={Boolean(editingControl)}
        onOk={() => void submitUpdateControl()}
        onCancel={() => { setEditingControl(null); controlForm.resetFields(); }}
        confirmLoading={updateControlMutation.isPending}
        destroyOnHidden
        afterOpenChange={(open) => {
          if (open && editingControl) {
            controlForm.setFieldsValue(editingControl);
          }
        }}
      >
        <ControlCatalogForm form={controlForm} projectOptions={projectOptions.options} onProjectSearch={projectOptions.onSearch} includeStatus />
      </Modal>

      <Modal
        title="Create control mapping"
        open={createMappingOpen}
        onOk={() => void submitCreateMapping()}
        onCancel={() => { setCreateMappingOpen(false); mappingForm.resetFields(); }}
        confirmLoading={createMappingMutation.isPending}
        destroyOnHidden
        afterOpenChange={(open) => {
          if (open && selectedControlId) {
            mappingForm.setFieldValue("controlId", selectedControlId);
          }
        }}
      >
        <Form form={mappingForm} layout="vertical">
          <Form.Item label="Control" name="controlId" rules={[{ required: true, message: "Control is required." }]}>
            <Select showSearch optionFilterProp="label" options={(controlsQuery.data?.items ?? []).map((item) => ({ value: item.id, label: `${item.controlCode} · ${item.title}` }))} />
          </Form.Item>
          <Form.Item label="Project" name="projectId">
            <Select allowClear showSearch optionFilterProp="label" options={projectOptions.options} onSearch={projectOptions.onSearch} />
          </Form.Item>
          <Form.Item label="Target Module" name="targetModule" rules={[{ required: true, message: "Target module is required." }]}>
            <Select options={targetModuleOptions} />
          </Form.Item>
          <Flex gap={12}>
            <Form.Item label="Target Entity Type" name="targetEntityType" rules={[{ required: true, message: "Target entity type is required." }]} style={{ flex: 1 }}>
              <Input />
            </Form.Item>
            <Form.Item label="Target Entity Id" name="targetEntityId" rules={[{ required: true, message: "Target entity id is required." }]} style={{ flex: 1 }}>
              <Input />
            </Form.Item>
          </Flex>
          <Form.Item label="Target Route" name="targetRoute" rules={[{ required: true, message: "Target route is required." }]}>
            <Input />
          </Form.Item>
          <Form.Item label="Evidence Status" name="evidenceStatus" initialValue="referenced">
            <Select options={evidenceStatusOptions} />
          </Form.Item>
          <Form.Item label="Notes" name="notes">
            <Input.TextArea rows={4} />
          </Form.Item>
        </Form>
      </Modal>

      <Drawer title="Control Detail" open={Boolean(selectedControlId)} width={720} onClose={() => setSelectedControlId(null)} destroyOnHidden>
        {selectedControlQuery.data ? (
          <Space direction="vertical" size={12} style={{ width: "100%" }}>
            <DetailCard label="Control Code" value={selectedControlQuery.data.controlCode} />
            <DetailCard label="Title" value={selectedControlQuery.data.title} />
            <DetailCard label="Control Set" value={selectedControlQuery.data.controlSet} />
            <DetailCard label="Process Area" value={selectedControlQuery.data.processArea ?? "cross-process"} />
            <DetailCard label="Status" value={selectedControlQuery.data.status} />
            <DetailCard label="Description" value={selectedControlQuery.data.description ?? "-"} />
          </Space>
        ) : null}
      </Drawer>

      <Drawer title="Mapping Detail" open={Boolean(selectedMappingId)} width={720} onClose={() => setSelectedMappingId(null)} destroyOnHidden>
        {mappingsQuery.data?.items.find((item) => item.id === selectedMappingId) ? (
          <Space direction="vertical" size={12} style={{ width: "100%" }}>
            {(() => {
              const mapping = mappingsQuery.data?.items.find((item) => item.id === selectedMappingId)!;
              return (
                <>
                  <DetailCard label="Control" value={`${mapping.controlCode} · ${mapping.controlTitle}`} />
                  <DetailCard label="Target" value={`${mapping.targetModule}:${mapping.targetEntityType}:${mapping.targetEntityId}`} />
                  <DetailCard label="Route" value={mapping.targetRoute} />
                  <DetailCard label="Evidence Status" value={mapping.evidenceStatus} />
                  <DetailCard label="Status" value={mapping.status} />
                  <DetailCard label="Notes" value={mapping.notes ?? "-"} />
                  <Button type="link" icon={<LinkOutlined />} onClick={() => window.location.assign(mapping.targetRoute)}>Open target route</Button>
                </>
              );
            })()}
          </Space>
        ) : null}
      </Drawer>

      <Modal title={transitionTarget === "active" ? "Activate control mapping" : "Retire control mapping"} open={Boolean(transitionTarget)} onOk={() => void submitTransition()} onCancel={() => { setTransitionTarget(null); transitionForm.resetFields(); }} confirmLoading={transitionMappingMutation.isPending} destroyOnHidden>
        <Form form={transitionForm} layout="vertical">
          <Form.Item label="Reason" name="reason" rules={[{ required: true, message: "Reason is required." }]}>
            <Input.TextArea rows={4} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}

function ControlCatalogForm({
  form,
  projectOptions,
  onProjectSearch,
  includeStatus = false,
}: {
  form: FormInstance<CreateControlCatalogItemInput & { status?: string }>;
  projectOptions: Array<{ label: string; value: string }>;
  onProjectSearch: (value: string) => void;
  includeStatus?: boolean;
}) {
  return (
    <Form form={form} layout="vertical">
      <Form.Item label="Control Code" name="controlCode" rules={[{ required: true, message: "Control code is required." }]}>
        <Input />
      </Form.Item>
      <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}>
        <Input />
      </Form.Item>
      <Form.Item label="Control Set" name="controlSet" rules={[{ required: true, message: "Control set is required." }]}>
        <Select options={controlSetOptions} />
      </Form.Item>
      <Form.Item label="Process Area" name="processArea">
        <Select allowClear options={processAreaOptions} />
      </Form.Item>
      <Form.Item label="Project" name="projectId">
        <Select allowClear showSearch optionFilterProp="label" options={projectOptions} onSearch={onProjectSearch} />
      </Form.Item>
      {includeStatus ? (
        <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
          <Select options={statusOptions} />
        </Form.Item>
      ) : null}
      <Form.Item label="Description" name="description">
        <Input.TextArea rows={4} />
      </Form.Item>
    </Form>
  );
}

function DetailCard({ label, value }: { label: string; value: string }) {
  return (
    <Card size="small">
      <Space direction="vertical" size={2}>
        <Text type="secondary">{label}</Text>
        <Text>{value}</Text>
      </Space>
    </Card>
  );
}
