import { useMemo, useState } from "react";
import { Alert, Button, Card, Drawer, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { SearchOutlined } from "@ant-design/icons";
import { useSearchParams } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useAssessmentFinding, useAssessmentFindings, useAssessmentPackage, useCreateAssessmentFinding, useTransitionAssessmentFinding } from "../hooks/useAssessment";
import type { AssessmentFindingListItem } from "../types/assessment";

const { Title, Paragraph, Text } = Typography;

export function AssessmentFindingsPage() {
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.assessment.workspaceRead, permissions.assessment.workspaceManage, permissions.assessment.workspaceReview);
  const canReview = permissionState.hasPermission(permissions.assessment.workspaceReview);
  const [messageApi, contextHolder] = message.useMessage();
  const [searchParams] = useSearchParams();
  const packageIdFromQuery = searchParams.get("packageId") ?? undefined;
  const [filters, setFilters] = useState({ packageId: packageIdFromQuery, status: undefined as string | undefined, search: "", page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [selectedFindingId, setSelectedFindingId] = useState<string | null>(null);
  const [transitionTarget, setTransitionTarget] = useState<"accepted" | "closed" | null>(null);
  const [form] = Form.useForm();
  const [transitionForm] = Form.useForm();

  const findingsQuery = useAssessmentFindings(filters, canRead);
  const selectedFindingQuery = useAssessmentFinding(selectedFindingId, canRead && Boolean(selectedFindingId));
  const selectedPackageQuery = useAssessmentPackage(filters.packageId ?? null, canRead && Boolean(filters.packageId));
  const createFindingMutation = useCreateAssessmentFinding();
  const transitionFindingMutation = useTransitionAssessmentFinding();

  const columns = useMemo<ColumnsType<AssessmentFindingListItem>>(
    () => [
      { title: "Package", dataIndex: "packageCode" },
      { title: "Title", dataIndex: "title" },
      { title: "Severity", dataIndex: "severity", render: (value: string) => <Tag color={value === "high" ? "red" : value === "medium" ? "gold" : "blue"}>{value}</Tag> },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
      { title: "Evidence", render: (_, item) => `${item.evidenceEntityType}:${item.evidenceEntityId}` },
      { title: "Owner", dataIndex: "ownerUserId", render: (value) => value ?? "-" },
      { title: "Updated", dataIndex: "updatedAt", render: (value: string) => new Date(value).toLocaleString() },
      { title: "Actions", render: (_, item) => <Button size="small" onClick={() => setSelectedFindingId(item.id)}>Open</Button> },
    ],
    [],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Assessment findings are not available for this account." />;
  }

  const createFinding = async () => {
    const values = await form.validateFields();
    try {
      await createFindingMutation.mutateAsync(values);
      form.resetFields();
      setCreateOpen(false);
      void messageApi.success("Assessment finding created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create assessment finding");
      void messageApi.error(presentation.description);
    }
  };

  const submitTransition = async () => {
    if (!selectedFindingId || !transitionTarget) {
      return;
    }

    const values = await transitionForm.validateFields();
    try {
      await transitionFindingMutation.mutateAsync({ id: selectedFindingId, input: { targetStatus: transitionTarget, summary: values.summary } });
      transitionForm.resetFields();
      setTransitionTarget(null);
      void messageApi.success(`Finding moved to ${transitionTarget}.`);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to transition assessment finding");
      void messageApi.error(presentation.description);
    }
  };

  const evidenceOptions = (selectedPackageQuery.data?.evidenceReferences ?? []).map((reference) => ({
    value: `${reference.entityType}::${reference.entityId}`,
    label: `${reference.title} (${reference.entityType}:${reference.entityId})`,
    entityType: reference.entityType,
    entityId: reference.entityId,
    route: reference.route,
  }));

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #7c3aed, #0f766e)", color: "#fff" }}>
            <SearchOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Assessment Findings</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Track assessor findings against exact evidence references and move them through acceptance and closure with traceable summaries.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input value={filters.packageId} placeholder="Package Id" style={{ width: 260 }} onChange={(event) => setFilters((current) => ({ ...current, packageId: event.target.value || undefined, page: 1 }))} />
            <Select allowClear placeholder="Status" style={{ width: 180 }} options={["open", "accepted", "closed"].map((value) => ({ value, label: value }))} value={filters.status} onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))} />
            <Input.Search allowClear placeholder="Search findings" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
          </Flex>
          <Button type="primary" disabled={!canReview || !filters.packageId} onClick={() => setCreateOpen(true)}>New finding</Button>
        </Flex>

        <Table
          rowKey="id"
          loading={findingsQuery.isLoading}
          columns={columns}
          dataSource={findingsQuery.data?.items ?? []}
          pagination={{
            current: findingsQuery.data?.page ?? filters.page,
            pageSize: findingsQuery.data?.pageSize ?? filters.pageSize,
            total: findingsQuery.data?.total ?? 0,
            showSizeChanger: true,
            onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })),
          }}
        />
      </Card>

      <Modal title="Create finding" open={createOpen} onOk={() => void createFinding()} onCancel={() => { setCreateOpen(false); form.resetFields(); }} confirmLoading={createFindingMutation.isPending} destroyOnHidden width={720}>
        <Form form={form} layout="vertical" initialValues={{ packageId: filters.packageId }}>
          <Form.Item label="Package Id" name="packageId" rules={[{ required: true, message: "Package is required." }]}>
            <Input disabled />
          </Form.Item>
          <Form.Item label="Title" name="title" rules={[{ required: true, message: "Title is required." }]}>
            <Input />
          </Form.Item>
          <Form.Item label="Description" name="description" rules={[{ required: true, message: "Description is required." }]}>
            <Input.TextArea rows={4} />
          </Form.Item>
          <Flex gap={12}>
            <Form.Item label="Severity" name="severity" initialValue="medium" style={{ flex: 1 }}>
              <Select options={["low", "medium", "high"].map((value) => ({ value, label: value }))} />
            </Form.Item>
            <Form.Item label="Owner" name="ownerUserId" style={{ flex: 1 }}>
              <Input />
            </Form.Item>
          </Flex>
          <Form.Item label="Evidence Reference" name="evidenceRef" rules={[{ required: true, message: "Evidence reference is required." }]}>
            <Select
              showSearch
              optionFilterProp="label"
              options={evidenceOptions}
              onChange={(_, option) => {
                const typedOption = option as { entityType: string; entityId: string; route?: string };
                form.setFieldsValue({
                  evidenceEntityType: typedOption.entityType,
                  evidenceEntityId: typedOption.entityId,
                  evidenceRoute: typedOption.route,
                });
              }}
            />
          </Form.Item>
          <Form.Item name="evidenceEntityType" hidden><Input /></Form.Item>
          <Form.Item name="evidenceEntityId" hidden><Input /></Form.Item>
          <Form.Item name="evidenceRoute" hidden><Input /></Form.Item>
        </Form>
      </Modal>

      <Drawer title="Finding Detail" open={Boolean(selectedFindingId)} width={680} onClose={() => setSelectedFindingId(null)} destroyOnHidden>
        {selectedFindingQuery.data ? (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <DescriptionsTable label="Finding" value={selectedFindingQuery.data.title} />
            <DescriptionsTable label="Package" value={selectedFindingQuery.data.packageCode} />
            <DescriptionsTable label="Status" value={selectedFindingQuery.data.status} />
            <DescriptionsTable label="Evidence" value={`${selectedFindingQuery.data.evidenceEntityType}:${selectedFindingQuery.data.evidenceEntityId}`} />
            <DescriptionsTable label="Description" value={selectedFindingQuery.data.description} />
            <Flex gap={8}>
              <Button disabled={!canReview || selectedFindingQuery.data.status !== "open"} onClick={() => setTransitionTarget("accepted")}>Accept finding</Button>
              <Button disabled={!canReview || selectedFindingQuery.data.status !== "accepted"} onClick={() => setTransitionTarget("closed")}>Close finding</Button>
            </Flex>
          </Space>
        ) : null}
      </Drawer>

      <Modal title={transitionTarget === "accepted" ? "Accept finding" : "Close finding"} open={Boolean(transitionTarget)} onOk={() => void submitTransition()} onCancel={() => { setTransitionTarget(null); transitionForm.resetFields(); }} confirmLoading={transitionFindingMutation.isPending} destroyOnHidden>
        <Form form={transitionForm} layout="vertical">
          <Form.Item label="Summary" name="summary" rules={[{ required: true, message: "Summary is required." }]}>
            <Input.TextArea rows={4} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}

function DescriptionsTable({ label, value }: { label: string; value: string }) {
  return (
    <Card size="small">
      <Space direction="vertical" size={2}>
        <Text type="secondary">{label}</Text>
        <Text>{value}</Text>
      </Space>
    </Card>
  );
}
