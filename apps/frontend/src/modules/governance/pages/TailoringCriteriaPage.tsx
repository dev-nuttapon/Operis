import { useState } from "react";
import { Alert, Button, Card, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useCreateTailoringCriteria, useTailoringCriteria, useUpdateTailoringCriteria } from "../hooks/useGovernance";
import type { TailoringCriteria, TailoringCriteriaFormInput } from "../types/governance";

const { Title, Paragraph } = Typography;

export function TailoringCriteriaPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.governance.tailoringRead, permissions.governance.tailoringManage, permissions.governance.tailoringApprove);
  const canManage = permissionState.hasPermission(permissions.governance.tailoringManage);
  const [messageApi, contextHolder] = message.useMessage();
  const [selected, setSelected] = useState<TailoringCriteria | null>(null);
  const [open, setOpen] = useState(false);
  const [form] = Form.useForm<TailoringCriteriaFormInput>();
  const criteriaQuery = useTailoringCriteria({ page: 1, pageSize: 100 }, canRead);
  const createMutation = useCreateTailoringCriteria();
  const updateMutation = useUpdateTailoringCriteria();

  if (!canRead) {
    return <Alert type="warning" showIcon message="Tailoring criteria are not available for this account." />;
  }

  const submit = async () => {
    const values = await form.validateFields();
    try {
      if (selected) {
        await updateMutation.mutateAsync({ id: selected.id, input: values });
        void messageApi.success("Tailoring criteria updated.");
      } else {
        await createMutation.mutateAsync(values);
        void messageApi.success("Tailoring criteria created.");
      }
      setOpen(false);
      setSelected(null);
      form.resetFields();
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to save tailoring criteria");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={16} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Title level={3} style={{ margin: 0 }}>Tailoring Criteria</Title>
        <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
          Maintain the standard process references and criteria used to classify tailoring deviations.
        </Paragraph>
      </Card>
      <Card variant="borderless" extra={canManage ? <Button type="primary" onClick={() => { setSelected(null); form.resetFields(); form.setFieldsValue({ status: "draft" }); setOpen(true); }}>New criteria</Button> : null}>
        <Table
          rowKey="id"
          loading={criteriaQuery.isLoading}
          dataSource={criteriaQuery.data?.items ?? []}
          pagination={false}
          columns={[
            { title: "Criterion", render: (_, item: TailoringCriteria) => <Space direction="vertical" size={0}><strong>{item.criterionCode}</strong><span>{item.title}</span></Space> },
            { title: "Standard", dataIndex: "standardReference" },
            { title: "Linked Deviations", dataIndex: "linkedDeviationCount" },
            { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
            {
              title: "Actions",
              render: (_, item: TailoringCriteria) => (
                <Button size="small" disabled={!canManage} onClick={() => {
                  setSelected(item);
                  form.setFieldsValue({
                    criterionCode: item.criterionCode,
                    standardReference: item.standardReference,
                    title: item.title,
                    description: item.description ?? undefined,
                    status: item.status,
                  });
                  setOpen(true);
                }}>
                  Edit
                </Button>
              ),
            },
          ]}
        />
      </Card>
      <Modal title={selected ? "Edit Tailoring Criteria" : "New Tailoring Criteria"} open={open} onCancel={() => setOpen(false)} onOk={() => void submit()} confirmLoading={createMutation.isPending || updateMutation.isPending} destroyOnHidden>
        <Form form={form} layout="vertical">
          <Form.Item name="criterionCode" label="Criterion Code" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="standardReference" label="Standard Reference" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea rows={3} /></Form.Item>
          <Form.Item name="status" label="Status" rules={[{ required: true }]}><Select options={["draft", "active", "retired"].map((value) => ({ value, label: value }))} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
