import { useMemo, useState } from "react";
import { Alert, App, Button, Card, DatePicker, Descriptions, Drawer, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { PlusOutlined, SafetyCertificateOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useAddCapaAction, useCapaRecord, useCapaRecords, useCloseCapa, useCreateCapaRecord, useUpdateCapaRecord, useVerifyCapa } from "../hooks/useOperations";
import type { CapaRecord, CreateCapaActionInput, CreateCapaRecordInput, UpdateCapaRecordInput } from "../types/operations";

type CapaFormValues = CreateCapaRecordInput;
type CapaActionFormValues = Omit<CreateCapaActionInput, "dueDate"> & { dueDate: dayjs.Dayjs };

export function CapaRegisterPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.operations.read, permissions.operations.manage, permissions.operations.approve);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const canApprove = permissionState.hasPermission(permissions.operations.approve);
  const { notification } = App.useApp();
  const [filters, setFilters] = useState({ search: "", sourceType: undefined as string | undefined, ownerUserId: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [actionOpen, setActionOpen] = useState(false);
  const [capaForm] = Form.useForm<CapaFormValues>();
  const [actionForm] = Form.useForm<CapaActionFormValues>();
  const listQuery = useCapaRecords({ ...filters, sortBy: "createdAt", sortOrder: "desc" }, canRead);
  const detailQuery = useCapaRecord(selectedId ?? undefined, Boolean(selectedId));
  const createMutation = useCreateCapaRecord();
  const updateMutation = useUpdateCapaRecord();
  const actionMutation = useAddCapaAction();
  const verifyMutation = useVerifyCapa();
  const closeMutation = useCloseCapa();

  const columns = useMemo<ColumnsType<CapaRecord>>(
    () => [
      { title: "Source", key: "source", render: (_, item) => `${item.sourceType}: ${item.sourceRef}` },
      { title: "Title", dataIndex: "title" },
      { title: "Owner", dataIndex: "ownerUserId" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "closed" ? "green" : value === "verified" ? "blue" : "gold"}>{value}</Tag> },
      { title: "Actions", key: "actions", render: (_, item) => (
        <Flex gap={8} wrap>
          <Button size="small" onClick={() => setSelectedId(item.id)}>Detail</Button>
          <Button size="small" disabled={!canManage} onClick={() => { capaForm.setFieldsValue(item); setSelectedId(item.id); setEditOpen(true); }}>Edit</Button>
          <Button size="small" disabled={!canManage} onClick={() => { setSelectedId(item.id); actionForm.resetFields(); setActionOpen(true); }}>Add Action</Button>
          <Button size="small" disabled={!canApprove || item.status === "verified" || item.status === "closed"} onClick={() => void handleVerify(item.id, item.rootCauseSummary)}>Verify</Button>
          <Button size="small" type="primary" disabled={!canApprove || item.status !== "verified"} onClick={() => void handleClose(item.id)}>Close</Button>
        </Flex>
      ) },
    ],
    [canApprove, canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="CAPA records are not available for this account." />;
  }

  const handleVerify = async (id: string, rootCauseSummary?: string | null) => {
    try {
      await verifyMutation.mutateAsync({ id, input: { rootCauseSummary } });
      notification.success({ message: "CAPA verified." });
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to verify CAPA");
      notification.error({ message: presentation.title, description: presentation.description });
    }
  };

  const handleClose = async (id: string) => {
    try {
      await closeMutation.mutateAsync({ id, input: { closureSummary: "Closed from CAPA register." } });
      notification.success({ message: "CAPA closed." });
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to close CAPA");
      notification.error({ message: presentation.title, description: presentation.description });
    }
  };

  const submitCreate = async () => {
    const values = await capaForm.validateFields();
    try {
      await createMutation.mutateAsync(values);
      setCreateOpen(false);
      capaForm.resetFields();
      notification.success({ message: "CAPA created." });
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create CAPA");
      notification.error({ message: presentation.title, description: presentation.description });
    }
  };

  const submitUpdate = async () => {
    const values = await capaForm.validateFields();
    if (!selectedId) return;
    try {
      await updateMutation.mutateAsync({ id: selectedId, input: values as UpdateCapaRecordInput });
      setEditOpen(false);
      notification.success({ message: "CAPA updated." });
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update CAPA");
      notification.error({ message: presentation.title, description: presentation.description });
    }
  };

  const submitAction = async () => {
    const values = await actionForm.validateFields();
    if (!selectedId) return;
    try {
      await actionMutation.mutateAsync({ id: selectedId, input: { ...values, dueDate: values.dueDate.format("YYYY-MM-DD") } });
      setActionOpen(false);
      actionForm.resetFields();
      notification.success({ message: "CAPA action added." });
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to add CAPA action");
      notification.error({ message: presentation.title, description: presentation.description });
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #b45309, #1d4ed8)", color: "#fff" }}>
            <SafetyCertificateOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>CAPA Register</Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Govern corrective and preventive actions with root cause, action tracking, verification, and controlled closure.
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search title, source, or owner" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Select allowClear placeholder="Source type" style={{ width: 180 }} options={["audit_finding", "incident", "defect", "qa_review"].map((value) => ({ label: value, value }))} value={filters.sourceType} onChange={(value) => setFilters((current) => ({ ...current, sourceType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["open", "root_cause_analysis", "action_planned", "action_in_progress", "verified", "closed"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New CAPA</Button>
        </Flex>

        <Table rowKey="id" loading={listQuery.isLoading} columns={columns} dataSource={listQuery.data?.items ?? []} pagination={{ current: listQuery.data?.page ?? filters.page, pageSize: listQuery.data?.pageSize ?? filters.pageSize, total: listQuery.data?.total ?? 0, showSizeChanger: true, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Drawer open={Boolean(selectedId)} onClose={() => setSelectedId(null)} title="CAPA Detail" width={720}>
        {detailQuery.data ? (
          <Space direction="vertical" style={{ width: "100%" }} size={16}>
            <Descriptions column={2} items={[
              { label: "Source", children: `${detailQuery.data.sourceType}: ${detailQuery.data.sourceRef}` },
              { label: "Owner", children: detailQuery.data.ownerUserId },
              { label: "Status", children: detailQuery.data.status },
              { label: "Verified By", children: detailQuery.data.verifiedBy ?? "-" },
            ]} />
            <Typography.Paragraph>{detailQuery.data.rootCauseSummary || "-"}</Typography.Paragraph>
            <Table rowKey="id" pagination={false} dataSource={detailQuery.data.actions} columns={[
              { title: "Action", dataIndex: "actionDescription" },
              { title: "Assigned To", dataIndex: "assignedTo" },
              { title: "Due Date", dataIndex: "dueDate" },
              { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
            ]} />
          </Space>
        ) : null}
      </Drawer>

      <Modal title="Create CAPA" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); capaForm.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <CapaForm form={capaForm} />
      </Modal>
      <Modal title="Edit CAPA" open={editOpen} onOk={() => void submitUpdate()} onCancel={() => setEditOpen(false)} confirmLoading={updateMutation.isPending} destroyOnHidden>
        <CapaForm form={capaForm} />
      </Modal>
      <Modal title="Add CAPA Action" open={actionOpen} onOk={() => void submitAction()} onCancel={() => { setActionOpen(false); actionForm.resetFields(); }} confirmLoading={actionMutation.isPending} destroyOnHidden>
        <Form form={actionForm} layout="vertical" initialValues={{ status: "open" }}>
          <Form.Item label="Action Description" name="actionDescription" rules={[{ required: true, message: "Action description is required." }]}><Input.TextArea rows={4} /></Form.Item>
          <Form.Item label="Assigned To" name="assignedTo" rules={[{ required: true, message: "Assignee is required." }]}><Input placeholder="owner@example.com" /></Form.Item>
          <Form.Item label="Due Date" name="dueDate" rules={[{ required: true, message: "Due date is required." }]}><DatePicker style={{ width: "100%" }} /></Form.Item>
          <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}><Select options={["open", "in_progress", "done"].map((value) => ({ label: value, value }))} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}

function CapaForm({ form }: { form: ReturnType<typeof Form.useForm<CapaFormValues>>[0] }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ status: "open" }}>
      <Form.Item label="Source Type" name="sourceType" rules={[{ required: true, message: "Source type is required." }]}><Select options={["audit_finding", "incident", "defect", "qa_review"].map((value) => ({ label: value, value }))} /></Form.Item>
      <Form.Item label="Source Reference" name="sourceRef" rules={[{ required: true, message: "Source reference is required." }]}><Input placeholder="AF-001" /></Form.Item>
      <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}><Input /></Form.Item>
      <Form.Item label="Owner User" name="ownerUserId" rules={[{ required: true, message: "Owner is required." }]}><Input placeholder="owner@example.com" /></Form.Item>
      <Form.Item label="Root Cause Summary" name="rootCauseSummary"><Input.TextArea rows={4} /></Form.Item>
      <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}><Select options={["open", "root_cause_analysis", "action_planned", "action_in_progress", "verified", "closed"].map((value) => ({ label: value, value }))} /></Form.Item>
    </Form>
  );
}
