import { useMemo, useState } from "react";
import { Alert, App, Button, Card, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { PlusOutlined, SafetyCertificateOutlined, UndoOutlined } from "@ant-design/icons";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useCapaEffectivenessReviews, useCapaRecords, useCreateCapaEffectivenessReview, useReopenCapa } from "../hooks/useOperations";
import type { CapaEffectivenessReview, CreateCapaEffectivenessReviewInput, ReopenCapaInput } from "../types/operations";

type ReviewFormValues = CreateCapaEffectivenessReviewInput;
type ReopenFormValues = ReopenCapaInput;

const effectivenessOptions = [
  { label: "effective", value: "effective" },
  { label: "ineffective", value: "ineffective" },
];

const statusOptions = [
  { label: "accepted", value: "accepted" },
  { label: "ineffective", value: "ineffective" },
];

export function CapaEffectivenessPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.operations.read, permissions.operations.manage, permissions.operations.approve);
  const canManage = permissionState.hasPermission(permissions.operations.manage);
  const canApprove = permissionState.hasPermission(permissions.operations.approve);
  const { notification } = App.useApp();
  const [filters, setFilters] = useState({
    effectivenessResult: undefined as string | undefined,
    status: undefined as string | undefined,
    reviewedBy: undefined as string | undefined,
    search: "",
    page: 1,
    pageSize: 25,
  });
  const [createOpen, setCreateOpen] = useState(false);
  const [reopenTarget, setReopenTarget] = useState<CapaEffectivenessReview | null>(null);
  const [reviewForm] = Form.useForm<ReviewFormValues>();
  const [reopenForm] = Form.useForm<ReopenFormValues>();
  const listQuery = useCapaEffectivenessReviews({ ...filters, sortBy: "reviewedAt", sortOrder: "desc" }, canRead);
  const closedCapaQuery = useCapaRecords({ status: "closed", sortBy: "createdAt", sortOrder: "desc", page: 1, pageSize: 100 }, canManage);
  const createMutation = useCreateCapaEffectivenessReview();
  const reopenMutation = useReopenCapa();

  const capaOptions = useMemo(
    () => (closedCapaQuery.data?.items ?? []).map((item) => ({ label: `${item.title} (${item.sourceRef})`, value: item.id })),
    [closedCapaQuery.data?.items],
  );

  const columns = useMemo<ColumnsType<CapaEffectivenessReview>>(
    () => [
      { title: "CAPA", dataIndex: "capaTitle" },
      { title: "Owner", dataIndex: "capaOwnerUserId" },
      { title: "Result", dataIndex: "effectivenessResult", render: (value: string) => <Tag color={value === "effective" ? "green" : "red"}>{value}</Tag> },
      { title: "Review Status", dataIndex: "status", render: (value: string) => <Tag color={value === "accepted" ? "blue" : "volcano"}>{value}</Tag> },
      { title: "Reviewed By", dataIndex: "reviewedBy" },
      { title: "Reviewed At", dataIndex: "reviewedAt" },
      { title: "Reopen", render: (_, item) => item.reopenedAt ? `${item.reopenedAt} by ${item.reopenedBy ?? "-"}` : "-" },
      {
        title: "Actions",
        render: (_, item) => (
          <Button
            size="small"
            icon={<UndoOutlined />}
            disabled={!canApprove || item.status !== "ineffective" || item.capaStatus !== "closed"}
            onClick={() => setReopenTarget(item)}
          >
            Reopen CAPA
          </Button>
        ),
      },
    ],
    [canApprove],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="CAPA effectiveness reviews are not available for this account." />;
  }

  const submitCreate = async () => {
    const values = await reviewForm.validateFields();
    try {
      await createMutation.mutateAsync(values);
      reviewForm.resetFields();
      setCreateOpen(false);
      notification.success({ message: "CAPA effectiveness review created." });
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create CAPA effectiveness review");
      notification.error({ message: presentation.title, description: presentation.description });
    }
  };

  const submitReopen = async () => {
    const values = await reopenForm.validateFields();
    if (!reopenTarget) return;
    try {
      await reopenMutation.mutateAsync({ id: reopenTarget.capaRecordId, input: values });
      reopenForm.resetFields();
      setReopenTarget(null);
      notification.success({ message: "CAPA reopened." });
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to reopen CAPA");
      notification.error({ message: presentation.title, description: presentation.description });
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #166534, #1d4ed8)", color: "#fff" }}>
            <SafetyCertificateOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>CAPA Effectiveness</Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Review closed CAPA records, capture evidence, and reopen ineffective closures with traceable rationale.
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search allowClear placeholder="Search title, source, reviewer, or evidence" style={{ width: 280 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
            <Select allowClear placeholder="Effectiveness" style={{ width: 180 }} options={effectivenessOptions} value={filters.effectivenessResult} onChange={(value) => setFilters((current) => ({ ...current, effectivenessResult: value, page: 1 }))} />
            <Select allowClear placeholder="Review status" style={{ width: 180 }} options={statusOptions} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input allowClear placeholder="Reviewed by" style={{ width: 220 }} value={filters.reviewedBy} onChange={(event) => setFilters((current) => ({ ...current, reviewedBy: event.target.value || undefined, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New review</Button>
        </Flex>

        <Table
          rowKey="id"
          loading={listQuery.isLoading}
          columns={columns}
          dataSource={listQuery.data?.items ?? []}
          pagination={{
            current: listQuery.data?.page ?? filters.page,
            pageSize: listQuery.data?.pageSize ?? filters.pageSize,
            total: listQuery.data?.total ?? 0,
            showSizeChanger: true,
            onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })),
          }}
        />
      </Card>

      <Modal
        title="Create CAPA effectiveness review"
        open={createOpen}
        onOk={() => void submitCreate()}
        onCancel={() => {
          setCreateOpen(false);
          reviewForm.resetFields();
        }}
        confirmLoading={createMutation.isPending}
        destroyOnHidden
      >
        <Form form={reviewForm} layout="vertical">
          <Form.Item label="Closed CAPA" name="capaRecordId" rules={[{ required: true, message: "Closed CAPA is required." }]}>
            <Select showSearch options={capaOptions} loading={closedCapaQuery.isLoading} />
          </Form.Item>
          <Form.Item label="Effectiveness Result" name="effectivenessResult" rules={[{ required: true, message: "Effectiveness result is required." }]}>
            <Select options={effectivenessOptions} />
          </Form.Item>
          <Form.Item label="Evidence Reference" name="evidenceRef" rules={[{ required: true, message: "Evidence reference is required." }]}>
            <Input placeholder="minio://evidence/capa-effectiveness/..." />
          </Form.Item>
          <Form.Item label="Review Summary" name="reviewSummary">
            <Input.TextArea rows={4} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title="Reopen CAPA"
        open={Boolean(reopenTarget)}
        onOk={() => void submitReopen()}
        onCancel={() => {
          setReopenTarget(null);
          reopenForm.resetFields();
        }}
        confirmLoading={reopenMutation.isPending}
        destroyOnHidden
      >
        <Form form={reopenForm} layout="vertical">
          <Form.Item label="Reopen Reason" name="reopenReason" rules={[{ required: true, message: "Reopen reason is required." }]}>
            <Input.TextArea rows={4} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
