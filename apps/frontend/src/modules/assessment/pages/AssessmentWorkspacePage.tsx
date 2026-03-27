import { useMemo, useState } from "react";
import { Alert, Button, Card, Descriptions, Drawer, Flex, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { AuditOutlined, PlusOutlined, SendOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectList } from "../../users/public";
import {
  useAssessmentPackage,
  useAssessmentPackages,
  useCreateAssessmentNote,
  useCreateAssessmentPackage,
  useTransitionAssessmentPackage,
} from "../hooks/useAssessment";
import type { AssessmentPackageListItem, CreateAssessmentPackageInput } from "../types/assessment";

const { Title, Paragraph, Text } = Typography;

const processAreaOptions = [
  "process-assets-planning",
  "requirements-traceability",
  "document-governance",
  "change-configuration",
  "verification-release",
  "audit-capa",
  "security-resilience",
].map((value) => ({ value, label: value }));

export function AssessmentWorkspacePage() {
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.assessment.workspaceRead, permissions.assessment.workspaceManage, permissions.assessment.workspaceReview);
  const canManage = permissionState.hasPermission(permissions.assessment.workspaceManage);
  const canReview = permissionState.hasPermission(permissions.assessment.workspaceReview);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState({ projectId: undefined as string | undefined, processArea: undefined as string | undefined, status: undefined as string | undefined, search: "", page: 1, pageSize: 25 });
  const [createOpen, setCreateOpen] = useState(false);
  const [selectedPackageId, setSelectedPackageId] = useState<string | null>(null);
  const [noteOpen, setNoteOpen] = useState(false);
  const [packageForm] = Form.useForm();
  const [noteForm] = Form.useForm();

  const packagesQuery = useAssessmentPackages(filters, canRead);
  const packageDetailQuery = useAssessmentPackage(selectedPackageId, canRead && Boolean(selectedPackageId));
  const createPackageMutation = useCreateAssessmentPackage();
  const transitionPackageMutation = useTransitionAssessmentPackage();
  const createNoteMutation = useCreateAssessmentNote();
  const projectQuery = useProjectList({ page: 1, pageSize: 100 }, canRead);

  const columns = useMemo<ColumnsType<AssessmentPackageListItem>>(
    () => [
      { title: "Package", render: (_, item) => <Space direction="vertical" size={0}><Text strong>{item.packageCode}</Text><Text type="secondary">{item.projectCode ?? "Portfolio"}</Text></Space> },
      { title: "Process Area", dataIndex: "processArea", render: (value) => value ?? "cross-process" },
      { title: "Scope", dataIndex: "scopeSummary" },
      { title: "Evidence", dataIndex: "evidenceCount" },
      { title: "Open Findings", dataIndex: "openFindingCount" },
      { title: "Status", dataIndex: "status", render: (value: string) => <Tag color={value === "shared" ? "blue" : value === "prepared" ? "gold" : value === "archived" ? "default" : "green"}>{value}</Tag> },
      {
        title: "Actions",
        render: (_, item) => (
          <Space size={8}>
            <Button size="small" onClick={() => setSelectedPackageId(item.id)}>Open</Button>
            <Button size="small" disabled={!canReview} onClick={() => navigate(`/app/assessment/findings?packageId=${item.id}`)}>Findings</Button>
          </Space>
        ),
      },
    ],
    [canReview, navigate],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Assessment workspace is not available for this account." />;
  }

  const createPackage = async () => {
    const values = await packageForm.validateFields();
    try {
      await createPackageMutation.mutateAsync(values as CreateAssessmentPackageInput);
      packageForm.resetFields();
      setCreateOpen(false);
      void messageApi.success("Assessment package created.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to create assessment package");
      void messageApi.error(presentation.description);
    }
  };

  const transitionPackage = async (targetStatus: string) => {
    if (!selectedPackageId) {
      return;
    }

    try {
      await transitionPackageMutation.mutateAsync({ id: selectedPackageId, input: { targetStatus } });
      void messageApi.success(`Package moved to ${targetStatus}.`);
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to transition package");
      void messageApi.error(presentation.description);
    }
  };

  const addNote = async () => {
    if (!selectedPackageId) {
      return;
    }

    const values = await noteForm.validateFields();
    try {
      await createNoteMutation.mutateAsync({ packageId: selectedPackageId, input: values });
      noteForm.resetFields();
      setNoteOpen(false);
      void messageApi.success("Assessor note added.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to add assessor note");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #1d4ed8, #0f766e)", color: "#fff" }}>
            <AuditOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Assessor Workspace</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Build evidence packages by project or process area, keep assessor notes together, and prepare shared evidence bundles for formal reviews.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Select
              allowClear
              showSearch
              placeholder="Project"
              style={{ width: 220 }}
              options={(projectQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))}
              value={filters.projectId}
              onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))}
              optionFilterProp="label"
            />
            <Select allowClear placeholder="Process Area" style={{ width: 220 }} options={processAreaOptions} value={filters.processArea} onChange={(value) => setFilters((current) => ({ ...current, processArea: value, page: 1 }))} />
            <Select
              allowClear
              placeholder="Status"
              style={{ width: 180 }}
              options={["draft", "prepared", "shared", "archived"].map((value) => ({ value, label: value }))}
              value={filters.status}
              onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))}
            />
            <Input.Search allowClear placeholder="Search package or scope" style={{ width: 240 }} value={filters.search} onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))} />
          </Flex>
          <Button type="primary" icon={<PlusOutlined />} disabled={!canManage} onClick={() => setCreateOpen(true)}>New package</Button>
        </Flex>

        <Table
          rowKey="id"
          loading={packagesQuery.isLoading}
          columns={columns}
          dataSource={packagesQuery.data?.items ?? []}
          pagination={{
            current: packagesQuery.data?.page ?? filters.page,
            pageSize: packagesQuery.data?.pageSize ?? filters.pageSize,
            total: packagesQuery.data?.total ?? 0,
            showSizeChanger: true,
            onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })),
          }}
        />
      </Card>

      <Modal title="Create assessment package" open={createOpen} onOk={() => void createPackage()} onCancel={() => { setCreateOpen(false); packageForm.resetFields(); }} confirmLoading={createPackageMutation.isPending} destroyOnHidden>
        <Form form={packageForm} layout="vertical">
          <Form.Item label="Project" name="projectId">
            <Select allowClear showSearch options={(projectQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))} optionFilterProp="label" />
          </Form.Item>
          <Form.Item label="Process Area" name="processArea">
            <Select allowClear options={processAreaOptions} />
          </Form.Item>
          <Form.Item label="Scope Summary" name="scopeSummary" rules={[{ required: true, message: "Scope summary is required." }]}>
            <Input.TextArea rows={4} />
          </Form.Item>
        </Form>
      </Modal>

      <Drawer title="Package Detail" open={Boolean(selectedPackageId)} width={880} onClose={() => setSelectedPackageId(null)} destroyOnHidden>
        {packageDetailQuery.data ? (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Descriptions bordered size="small" column={1}>
              <Descriptions.Item label="Package">{packageDetailQuery.data.packageCode}</Descriptions.Item>
              <Descriptions.Item label="Project">{packageDetailQuery.data.projectName ?? "Portfolio"}</Descriptions.Item>
              <Descriptions.Item label="Process Area">{packageDetailQuery.data.processArea ?? "cross-process"}</Descriptions.Item>
              <Descriptions.Item label="Status"><Tag>{packageDetailQuery.data.status}</Tag></Descriptions.Item>
              <Descriptions.Item label="Scope">{packageDetailQuery.data.scopeSummary}</Descriptions.Item>
            </Descriptions>

            <Flex gap={8} wrap="wrap">
              <Button disabled={!canManage || packageDetailQuery.data.status !== "draft"} onClick={() => void transitionPackage("prepared")}>Prepare package</Button>
              <Button type="primary" icon={<SendOutlined />} disabled={!canReview || packageDetailQuery.data.status !== "prepared"} onClick={() => void transitionPackage("shared")}>Share package</Button>
              <Button disabled={!canManage || (packageDetailQuery.data.status !== "prepared" && packageDetailQuery.data.status !== "shared")} onClick={() => void transitionPackage("archived")}>Archive package</Button>
              <Button disabled={!canReview} onClick={() => setNoteOpen(true)}>Add note</Button>
            </Flex>

            <Card size="small" title={`Evidence References (${packageDetailQuery.data.evidenceReferences.length})`}>
              <Table
                rowKey={(row) => `${row.entityType}-${row.entityId}`}
                pagination={false}
                size="small"
                columns={[
                  { title: "Title", dataIndex: "title" },
                  { title: "Module", dataIndex: "sourceModule" },
                  { title: "Process Area", dataIndex: "processArea" },
                  { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
                  { title: "Metadata", dataIndex: "metadataSummary", render: (value?: string | null) => value ?? "-" },
                  {
                    title: "Open",
                    render: (_, row) => (
                      <Button type="link" onClick={() => navigate(row.route)}>Go to source</Button>
                    ),
                  },
                ]}
                dataSource={packageDetailQuery.data.evidenceReferences}
              />
            </Card>

            <Card size="small" title={`Assessor Notes (${packageDetailQuery.data.notes.length})`}>
              <Table
                rowKey="id"
                pagination={false}
                size="small"
                columns={[
                  { title: "Type", dataIndex: "noteType" },
                  { title: "Note", dataIndex: "note" },
                  { title: "Created By", dataIndex: "createdByUserId" },
                  { title: "Created At", dataIndex: "createdAt", render: (value: string) => new Date(value).toLocaleString() },
                ]}
                dataSource={packageDetailQuery.data.notes}
              />
            </Card>

            <Card size="small" title={`Findings (${packageDetailQuery.data.findings.length})`}>
              <Table
                rowKey="id"
                pagination={false}
                size="small"
                columns={[
                  { title: "Title", dataIndex: "title" },
                  { title: "Severity", dataIndex: "severity" },
                  { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
                  { title: "Evidence", render: (_, row) => `${row.evidenceEntityType}:${row.evidenceEntityId}` },
                ]}
                dataSource={packageDetailQuery.data.findings}
              />
            </Card>
          </Space>
        ) : null}
      </Drawer>

      <Modal title="Add assessor note" open={noteOpen} onOk={() => void addNote()} onCancel={() => { setNoteOpen(false); noteForm.resetFields(); }} confirmLoading={createNoteMutation.isPending} destroyOnHidden>
        <Form form={noteForm} layout="vertical">
          <Form.Item label="Note Type" name="noteType" initialValue="assessor_note">
            <Select options={[{ value: "assessor_note", label: "assessor_note" }, { value: "follow_up", label: "follow_up" }, { value: "clarification", label: "clarification" }]} />
          </Form.Item>
          <Form.Item label="Note" name="note" rules={[{ required: true, message: "Note is required." }]}>
            <Input.TextArea rows={4} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
