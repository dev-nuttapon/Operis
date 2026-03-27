import { useMemo, useState } from "react";
import { Alert, Button, Card, DatePicker, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { FormInstance } from "antd";
import type { ColumnsType } from "antd/es/table";
import { FileProtectOutlined, PlusOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCreateSupplierAgreement, useSupplierAgreements, useSuppliers, useUpdateSupplierAgreement } from "../hooks/useOperations";
import type { CreateSupplierAgreementInput, SupplierAgreement, UpdateSupplierAgreementInput } from "../types/operations";

const { Title, Paragraph } = Typography;

type AgreementFormValues = Omit<CreateSupplierAgreementInput, "effectiveFrom" | "effectiveTo"> & {
  effectiveFrom?: dayjs.Dayjs;
  effectiveTo?: dayjs.Dayjs;
};

export function SupplierAgreementsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.operations.read);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ search: "", supplierId: undefined as string | undefined, agreementType: undefined as string | undefined, status: undefined as string | undefined, page: 1, pageSize: 25 });
  const [editing, setEditing] = useState<SupplierAgreement | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [form] = Form.useForm<AgreementFormValues>();
  const agreementsQuery = useSupplierAgreements({ ...filters, sortBy: "effectiveTo", sortOrder: "asc" }, canRead);
  const suppliersQuery = useSuppliers({ page: 1, pageSize: 100, sortBy: "name", sortOrder: "asc" }, canRead);
  const supplierOptions = (suppliersQuery.data?.items ?? []).map((supplier) => ({ label: supplier.name, value: supplier.id }));
  const createMutation = useCreateSupplierAgreement();
  const updateMutation = useUpdateSupplierAgreement();

  const columns = useMemo<ColumnsType<SupplierAgreement>>(
    () => [
      { title: "Supplier", dataIndex: "supplierName" },
      { title: "Agreement Type", dataIndex: "agreementType" },
      { title: "Effective From", dataIndex: "effectiveFrom" },
      { title: "Effective To", dataIndex: "effectiveTo", render: (value?: string | null) => value ?? "-" },
      { title: "Evidence", dataIndex: "evidenceRef", render: (value: string) => <Typography.Text copyable>{value}</Typography.Text> },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Actions", key: "actions", render: (_, item) => <Button size="small" disabled={!canManage} onClick={() => setEditing(item)}>Edit</Button> },
    ],
    [canManage],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Supplier agreement data is not available for this account." />;
  }

  const mapValues = (values: AgreementFormValues): CreateSupplierAgreementInput => ({
    supplierId: values.supplierId,
    agreementType: values.agreementType,
    effectiveFrom: values.effectiveFrom?.format("YYYY-MM-DD") ?? "",
    effectiveTo: values.effectiveTo ? values.effectiveTo.format("YYYY-MM-DD") : null,
    slaTerms: values.slaTerms ?? null,
    evidenceRef: values.evidenceRef,
    status: values.status,
  });

  const submitCreate = async () => {
    const values = await form.validateFields();
    try {
      await createMutation.mutateAsync(mapValues(values));
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Supplier agreement created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create supplier agreement");
      void messageApi.error(presentation.description);
    }
  };

  const submitUpdate = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    try {
      await updateMutation.mutateAsync({ id: editing.id, input: mapValues(values) as UpdateSupplierAgreementInput });
      form.resetFields();
      setEditing(null);
      void messageApi.success("Supplier agreement updated.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to update supplier agreement");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1d4ed8, #0f172a)", color: "#fff" }}>
            <FileProtectOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>SLA/Contract Evidence</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Track governing agreements, effective dates, SLA terms, and controlled evidence references per supplier.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search supplier, type, or evidence" style={{ width: 260 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))} />
            <Select allowClear showSearch placeholder="Supplier" style={{ width: 240 }} options={supplierOptions} value={filters.supplierId} onChange={(value) => setFilters((current) => ({ ...current, supplierId: value, page: 1 }))} />
            <Select allowClear placeholder="Agreement Type" style={{ width: 180 }} options={["sla", "msa", "contract", "nda"].map((value) => ({ label: value, value }))} value={filters.agreementType} onChange={(value) => setFilters((current) => ({ ...current, agreementType: value, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["Draft", "Approved", "Active", "Archived"].map((value) => ({ label: value, value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New agreement</Button>
        </Flex>

        <Table rowKey="id" loading={agreementsQuery.isLoading} columns={columns} dataSource={agreementsQuery.data?.items ?? []} pagination={{ current: agreementsQuery.data?.page ?? filters.page, pageSize: agreementsQuery.data?.pageSize ?? filters.pageSize, total: agreementsQuery.data?.total ?? 0, showSizeChanger: true, defaultPageSize: 25, onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })) }} />
      </Card>

      <Modal title="Create supplier agreement" open={createOpen} onOk={() => void submitCreate()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createMutation.isPending} destroyOnHidden>
        <SupplierAgreementForm form={form} supplierOptions={supplierOptions} />
      </Modal>

      <Modal title="Edit supplier agreement" open={Boolean(editing)} onOk={() => void submitUpdate()} onCancel={() => { setEditing(null); form.resetFields(); }} confirmLoading={updateMutation.isPending} destroyOnHidden afterOpenChange={(open) => { if (open && editing) { form.setFieldsValue({ ...editing, effectiveFrom: editing.effectiveFrom ? dayjs(editing.effectiveFrom) : undefined, effectiveTo: editing.effectiveTo ? dayjs(editing.effectiveTo) : undefined }); } }}>
        <SupplierAgreementForm form={form} supplierOptions={supplierOptions} />
      </Modal>
    </Space>
  );
}

function SupplierAgreementForm({ form, supplierOptions }: { form: FormInstance<AgreementFormValues>; supplierOptions: Array<{ label: string; value: string }> }) {
  return (
    <Form form={form} layout="vertical" initialValues={{ agreementType: "sla", status: "Draft" }}>
      <Form.Item label="Supplier" name="supplierId" rules={[{ required: true, message: "Supplier is required." }]}>
        <Select showSearch options={supplierOptions} />
      </Form.Item>
      <Form.Item label="Agreement Type" name="agreementType" rules={[{ required: true, message: "Agreement type is required." }]}>
        <Select options={["sla", "msa", "contract", "nda"].map((value) => ({ label: value, value }))} />
      </Form.Item>
      <Form.Item label="Effective From" name="effectiveFrom" rules={[{ required: true, message: "Effective start date is required." }]}>
        <DatePicker style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="Effective To" name="effectiveTo">
        <DatePicker style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item label="SLA Terms" name="slaTerms">
        <Input.TextArea rows={4} placeholder="Response time, support window, recovery target..." />
      </Form.Item>
      <Form.Item label="Evidence Ref" name="evidenceRef" rules={[{ required: true, message: "Evidence reference is required." }]}>
        <Input placeholder="minio://supplier-evidence/acme/msa-2026.pdf" />
      </Form.Item>
      <Form.Item label="Status" name="status" rules={[{ required: true, message: "Status is required." }]}>
        <Select options={["Draft", "Approved", "Active", "Archived"].map((value) => ({ label: value, value }))} />
      </Form.Item>
    </Form>
  );
}
