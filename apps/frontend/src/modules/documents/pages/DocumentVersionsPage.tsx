import { Alert, App, Button, Card, Descriptions, Form, Input, Modal, Space, Table, Tag, Typography } from "antd";
import { ArrowLeftOutlined, CheckOutlined, ClockCircleOutlined, DeleteOutlined, DownloadOutlined, FolderOpenOutlined, SendOutlined, StopOutlined, UploadOutlined } from "@ant-design/icons";
import { useNavigate, useParams } from "react-router-dom";
import type { ColumnsType } from "antd/es/table";
import {
  downloadDocument,
  type DocumentApprovalItem,
  type DocumentLinkItem,
  type DocumentVersionListItem,
} from "../api/documentsApi";
import {
  useApproveDocument,
  useArchiveDocument,
  useBaselineDocument,
  useCreateDocumentLink,
  useDeleteDocument,
  useDocument,
  useRejectDocument,
  useSubmitDocument,
} from "../hooks/useDocuments";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { saveBlobAsFile } from "../utils/download";

const { Title, Paragraph, Text } = Typography;

export function DocumentVersionsPage() {
  const { notification } = App.useApp();
  const navigate = useNavigate();
  const { documentId } = useParams<{ documentId: string }>();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.documents.read);
  const canManageVersions = permissionState.hasPermission(permissions.documents.manageVersions);
  const canApprove = permissionState.hasPermission(permissions.documents.publish);
  const canArchive = permissionState.hasPermission(permissions.documents.deactivate);
  const documentQuery = useDocument(documentId ?? null, canRead);
  const submitMutation = useSubmitDocument();
  const approveMutation = useApproveDocument();
  const rejectMutation = useRejectDocument();
  const baselineMutation = useBaselineDocument();
  const archiveMutation = useArchiveDocument();
  const deleteMutation = useDeleteDocument();
  const linkMutation = useCreateDocumentLink();
  const [reviewForm] = Form.useForm<{ stepName: string; reviewerUserId: string; decisionReason: string }>();
  const [linkForm] = Form.useForm<{ targetEntityType: string; targetEntityId: string; linkType: string }>();

  const detail = documentQuery.data;
  const versionColumns: ColumnsType<DocumentVersionListItem> = [
    { title: "Version", dataIndex: "versionNumber", key: "versionNumber", render: (value) => `v${value}` },
    { title: "File", dataIndex: "fileName", key: "fileName" },
    { title: "Uploaded by", dataIndex: "uploadedBy", key: "uploadedBy" },
    { title: "Uploaded at", dataIndex: "uploadedAt", key: "uploadedAt", render: (value) => new Date(value).toLocaleString() },
    { title: "Status", dataIndex: "status", key: "status", render: (value) => <Tag>{value}</Tag> },
  ];
  const approvalColumns: ColumnsType<DocumentApprovalItem> = [
    { title: "Step", dataIndex: "stepName", key: "stepName" },
    { title: "Reviewer", dataIndex: "reviewerUserId", key: "reviewerUserId" },
    { title: "Decision", dataIndex: "decision", key: "decision", render: (value) => <Tag color={value === "approved" ? "green" : value === "rejected" ? "red" : "gold"}>{value}</Tag> },
    { title: "Reason", dataIndex: "decisionReason", key: "decisionReason", render: (value) => value ?? "-" },
    { title: "Decided at", dataIndex: "decidedAt", key: "decidedAt", render: (value) => (value ? new Date(value).toLocaleString() : "-") },
  ];
  const linkColumns: ColumnsType<DocumentLinkItem> = [
    { title: "Target type", dataIndex: "targetEntityType", key: "targetEntityType" },
    { title: "Target id", dataIndex: "targetEntityId", key: "targetEntityId" },
    { title: "Link type", dataIndex: "linkType", key: "linkType" },
  ];

  if (!canRead) {
    return <Alert type="warning" showIcon message="Document access is not available for this account." />;
  }

  if (!detail) {
    return <Card loading={documentQuery.isLoading} />;
  }

  const reviewPayload = async () => reviewForm.validateFields();

  const runWithNotice = async (work: () => Promise<unknown>, message: string) => {
    await work();
    notification.success({ message });
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Button icon={<ArrowLeftOutlined />} onClick={() => navigate("/app/documents")} style={{ width: "fit-content" }}>
        Back to register
      </Button>

      <Card variant="borderless">
        <Space align="start" size={16}>
          <div
            style={{
              width: 48,
              height: 48,
              borderRadius: 14,
              display: "grid",
              placeItems: "center",
              background: "linear-gradient(135deg, #1d4ed8, #0f172a)",
              color: "#fff",
            }}
          >
            <FolderOpenOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>{detail.title}</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Phase 2 document detail with version history, approval chain, and governed links.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card
        variant="borderless"
        extra={
          <Space wrap>
            <Button icon={<DownloadOutlined />} onClick={() => void downloadDocument(detail.id).then(({ blob, fileName }) => saveBlobAsFile(blob, fileName ?? detail.title)).catch(() => null)}>
              Download
            </Button>
            <Button icon={<UploadOutlined />} disabled={!canManageVersions} onClick={() => navigate(`/app/documents/${detail.id}/versions/new`, { state: { documentName: detail.title, from: `/app/documents/${detail.id}` } })}>
              Add version
            </Button>
            <Button icon={<DeleteOutlined />} disabled={!permissionState.hasPermission(permissions.documents.deleteDraft)} onClick={() => {
              Modal.confirm({
                title: "Delete draft document?",
                content: "Only draft documents can be deleted.",
                okButtonProps: { danger: true },
                onOk: async () => {
                  await runWithNotice(() => deleteMutation.mutateAsync({ documentId: detail.id, reason: "Removed from draft register" }), "Document deleted");
                  navigate("/app/documents");
                },
              });
            }}>
              Delete draft
            </Button>
          </Space>
        }
      >
        <Descriptions column={2} size="small">
          <Descriptions.Item label="Type">{detail.documentTypeName ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Project">{detail.projectName ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Phase">{detail.phaseCode ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Owner">{detail.ownerUserId ?? "-"}</Descriptions.Item>
          <Descriptions.Item label="Status"><Tag>{detail.status}</Tag></Descriptions.Item>
          <Descriptions.Item label="Classification">{detail.classification}</Descriptions.Item>
          <Descriptions.Item label="Retention">{detail.retentionClass}</Descriptions.Item>
          <Descriptions.Item label="Tags">{detail.tags.length > 0 ? detail.tags.join(", ") : "-"}</Descriptions.Item>
        </Descriptions>
      </Card>

      <Card
        variant="borderless"
        title="Workflow actions"
        extra={<Text type="secondary">Current state: {detail.status}</Text>}
      >
        <Form form={reviewForm} layout="vertical">
          <Form.Item name="stepName" label="Step name" initialValue="document_review">
            <Input />
          </Form.Item>
          <Form.Item name="reviewerUserId" label="Reviewer user id" initialValue={detail.ownerUserId ?? ""}>
            <Input />
          </Form.Item>
          <Form.Item name="decisionReason" label="Reason / note">
            <Input.TextArea rows={3} />
          </Form.Item>
        </Form>

        <Space wrap>
          <Button
            icon={<SendOutlined />}
            disabled={!canManageVersions || detail.versions.length === 0}
            loading={submitMutation.isPending}
            onClick={async () => {
              const payload = await reviewPayload();
              await runWithNotice(() => submitMutation.mutateAsync({ documentId: detail.id, payload }), "Document submitted for review");
            }}
          >
            Submit
          </Button>
          <Button
            icon={<CheckOutlined />}
            disabled={!canApprove}
            loading={approveMutation.isPending}
            onClick={async () => {
              const payload = await reviewPayload();
              await runWithNotice(() => approveMutation.mutateAsync({ documentId: detail.id, payload }), "Document approved");
            }}
          >
            Approve
          </Button>
          <Button
            icon={<StopOutlined />}
            danger
            disabled={!canApprove}
            loading={rejectMutation.isPending}
            onClick={async () => {
              const payload = await reviewPayload();
              await runWithNotice(() => rejectMutation.mutateAsync({ documentId: detail.id, payload }), "Document rejected");
            }}
          >
            Reject
          </Button>
          <Button
            icon={<ClockCircleOutlined />}
            disabled={!canApprove}
            loading={baselineMutation.isPending}
            onClick={async () => runWithNotice(() => baselineMutation.mutateAsync({ documentId: detail.id }), "Document baselined")}
          >
            Baseline
          </Button>
          <Button
            disabled={!canArchive}
            loading={archiveMutation.isPending}
            onClick={() => {
              Modal.confirm({
                title: "Archive document?",
                content: "Only baselined documents should be archived.",
                onOk: async () => {
                  const values = await reviewForm.validateFields();
                  await runWithNotice(() => archiveMutation.mutateAsync({ documentId: detail.id, reason: values.decisionReason || "Archived from document governance" }), "Document archived");
                },
              });
            }}
          >
            Archive
          </Button>
        </Space>
      </Card>

      <Card variant="borderless" title="Version history">
        <Table<DocumentVersionListItem> rowKey="id" columns={versionColumns} dataSource={detail.versions} pagination={false} />
      </Card>

      <Card variant="borderless" title="Approval chain">
        <Table<DocumentApprovalItem> rowKey="id" columns={approvalColumns} dataSource={detail.approvals} pagination={false} />
      </Card>

      <Card variant="borderless" title="Governed links">
        <Form
          form={linkForm}
          layout="inline"
          onFinish={async (values) => {
            await runWithNotice(() => linkMutation.mutateAsync({ documentId: detail.id, payload: values }), "Link created");
            linkForm.resetFields();
          }}
          style={{ marginBottom: 16 }}
        >
          <Form.Item name="targetEntityType" rules={[{ required: true, message: "Required" }]}>
            <Input placeholder="target type" />
          </Form.Item>
          <Form.Item name="targetEntityId" rules={[{ required: true, message: "Required" }]}>
            <Input placeholder="target id" />
          </Form.Item>
          <Form.Item name="linkType" rules={[{ required: true, message: "Required" }]}>
            <Input placeholder="link type" />
          </Form.Item>
          <Form.Item>
            <Button htmlType="submit" loading={linkMutation.isPending}>Add link</Button>
          </Form.Item>
        </Form>
        <Table<DocumentLinkItem> rowKey="id" columns={linkColumns} dataSource={detail.links} pagination={false} />
      </Card>
    </Space>
  );
}
