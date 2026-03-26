import { useMemo, useState } from "react";
import { Alert, Button, Card, Descriptions, Form, Input, Modal, Space, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useAuth } from "../../auth";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import {
  useCreateProcessAsset,
  useCreateProcessAssetVersion,
  useProcessAsset,
  useProcessAssetActions,
  useProcessAssets,
  useUpdateProcessAsset,
} from "../hooks/useGovernance";
import type { ProcessAssetFormInput, ProcessAssetListItem, ProcessAssetVersionFormInput } from "../types/governance";

const { Title, Text } = Typography;

export function ProcessLibraryPage() {
  const { user } = useAuth();
  const permissionState = usePermissions();
  const canManage = permissionState.hasPermission(permissions.governance.processLibraryManage);
  const [search, setSearch] = useState("");
  const [assetModalOpen, setAssetModalOpen] = useState(false);
  const [versionModalOpen, setVersionModalOpen] = useState(false);
  const [selectedAssetId, setSelectedAssetId] = useState<string | null>(null);
  const [approvalReason, setApprovalReason] = useState("");
  const [assetForm] = Form.useForm<ProcessAssetFormInput>();
  const [versionForm] = Form.useForm<ProcessAssetVersionFormInput>();
  const assetsQuery = useProcessAssets({ page: 1, pageSize: 20, search });
  const selectedAssetQuery = useProcessAsset(selectedAssetId, Boolean(selectedAssetId));
  const createAsset = useCreateProcessAsset();
  const updateAsset = useUpdateProcessAsset();
  const createVersion = useCreateProcessAssetVersion();
  const actions = useProcessAssetActions();
  const error = assetsQuery.error ?? selectedAssetQuery.error ?? createAsset.error ?? updateAsset.error ?? createVersion.error;
  const detail = selectedAssetQuery.data;

  const columns = useMemo<ColumnsType<ProcessAssetListItem>>(
    () => [
      { title: "Code", dataIndex: "code", key: "code" },
      { title: "Name", dataIndex: "name", key: "name" },
      { title: "Category", dataIndex: "category", key: "category" },
      { title: "Owner", dataIndex: "ownerUserId", key: "ownerUserId" },
      { title: "Current Version", key: "currentVersion", render: (_, item) => item.currentVersion ? `v${item.currentVersion.versionNumber}` : "-" },
      { title: "Status", key: "status", render: (_, item) => <Tag>{item.status}</Tag> },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Space wrap>
            <Button size="small" onClick={() => setSelectedAssetId(item.id)}>View</Button>
            {canManage ? <Button size="small" onClick={() => openEdit(item.id)}>Edit</Button> : null}
            {canManage ? <Button size="small" onClick={() => openVersion(item.id)}>New Version</Button> : null}
          </Space>
        ),
      },
    ],
    [canManage]
  );

  const openEdit = (assetId: string) => {
    setSelectedAssetId(assetId);
    const asset = assetsQuery.data?.items.find((item) => item.id === assetId);
    if (asset) {
      assetForm.setFieldsValue({
        code: asset.code,
        name: asset.name,
        category: asset.category,
        ownerUserId: asset.ownerUserId,
        effectiveFrom: asset.effectiveFrom ?? undefined,
        effectiveTo: asset.effectiveTo ?? undefined,
      });
      setAssetModalOpen(true);
    }
  };

  const openVersion = (assetId: string) => {
    setSelectedAssetId(assetId);
    versionForm.resetFields();
    setVersionModalOpen(true);
  };

  const handleCreate = () => {
    assetForm.resetFields();
    assetForm.setFieldsValue({ ownerUserId: String(user?.email ?? user?.name ?? "") });
    setSelectedAssetId(null);
    setAssetModalOpen(true);
  };

  const submitAsset = async () => {
    const values = await assetForm.validateFields();
    if (selectedAssetId) {
      await updateAsset.mutateAsync({ id: selectedAssetId, input: values });
    } else {
      await createAsset.mutateAsync(values);
    }
    setAssetModalOpen(false);
  };

  const submitVersion = async () => {
    const values = await versionForm.validateFields();
    if (!selectedAssetId) return;
    await createVersion.mutateAsync({ id: selectedAssetId, input: values });
    setVersionModalOpen(false);
  };

  const latestVersion = detail?.versions[0];

  return (
    <Space direction="vertical" size={16} style={{ width: "100%" }}>
      <Card>
        <Space direction="vertical" size={4}>
          <Title level={3} style={{ margin: 0 }}>Process Library</Title>
          <Text type="secondary">Phase 1 process assets, governed versions, and activation workflow.</Text>
        </Space>
      </Card>

      {error ? <Alert type="error" showIcon message={getApiErrorPresentation(error).title} description={getApiErrorPresentation(error).description} /> : null}

      <Card
        extra={
          <Space>
            <Input.Search placeholder="Search code or name" allowClear onSearch={setSearch} style={{ width: 240 }} />
            {canManage ? <Button type="primary" onClick={handleCreate}>New Process Asset</Button> : null}
          </Space>
        }
      >
        <Table rowKey="id" columns={columns} dataSource={assetsQuery.data?.items ?? []} loading={assetsQuery.isLoading} pagination={false} />
      </Card>

      {detail ? (
        <Card title={`${detail.code} · ${detail.name}`}>
          <Descriptions size="small" column={2}>
            <Descriptions.Item label="Category">{detail.category}</Descriptions.Item>
            <Descriptions.Item label="Status"><Tag>{detail.status}</Tag></Descriptions.Item>
            <Descriptions.Item label="Owner">{detail.ownerUserId}</Descriptions.Item>
            <Descriptions.Item label="Current Version">{detail.currentVersionId ?? "-"}</Descriptions.Item>
          </Descriptions>
          <Table
            rowKey="id"
            pagination={false}
            dataSource={detail.versions}
            style={{ marginTop: 16 }}
            columns={[
              { title: "Version", key: "version", render: (_, item) => `v${item.versionNumber}` },
              { title: "Title", dataIndex: "title", key: "title" },
              { title: "Status", key: "status", render: (_, item) => <Tag>{item.status}</Tag> },
              { title: "Change Summary", dataIndex: "changeSummary", key: "changeSummary" },
              {
                title: "Workflow",
                key: "workflow",
                render: (_, item) => canManage ? (
                  <Space wrap>
                    {item.status === "draft" ? (
                      <Button size="small" onClick={() => void actions.submitReview.mutateAsync({ processAssetId: detail.id, versionId: item.id })}>Submit Review</Button>
                    ) : null}
                    {item.status === "reviewed" ? (
                      <Button size="small" onClick={() => void actions.approve.mutateAsync({ processAssetId: detail.id, versionId: item.id, changeSummary: approvalReason || "Approved for activation" })}>Approve</Button>
                    ) : null}
                    {item.status === "approved" ? (
                      <Button size="small" onClick={() => void actions.activate.mutateAsync({ processAssetId: detail.id, versionId: item.id })}>Activate</Button>
                    ) : null}
                    {item.status === "active" ? (
                      <Button size="small" danger onClick={() => void actions.deprecate.mutateAsync(detail.id)}>Deprecate Asset</Button>
                    ) : null}
                  </Space>
                ) : null,
              },
            ]}
          />
          {canManage && latestVersion?.status === "reviewed" ? (
            <Input
              value={approvalReason}
              onChange={(event) => setApprovalReason(event.target.value)}
              placeholder="Change summary required for approval"
              style={{ marginTop: 12 }}
            />
          ) : null}
        </Card>
      ) : null}

      <Modal title={selectedAssetId ? "Edit Process Asset" : "New Process Asset"} open={assetModalOpen} onCancel={() => setAssetModalOpen(false)} onOk={() => void submitAsset()} confirmLoading={createAsset.isPending || updateAsset.isPending}>
        <Form layout="vertical" form={assetForm}>
          <Form.Item name="code" label="Code" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="name" label="Name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="category" label="Category" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="ownerUserId" label="Owner User Id" rules={[{ required: true }]}><Input /></Form.Item>
          {!selectedAssetId ? <Form.Item name="initialVersionTitle" label="Initial Version Title" rules={[{ required: true }]}><Input /></Form.Item> : null}
          {!selectedAssetId ? <Form.Item name="initialVersionSummary" label="Initial Version Summary" rules={[{ required: true }]}><Input.TextArea rows={3} /></Form.Item> : null}
          {!selectedAssetId ? <Form.Item name="initialContentRef" label="Content Ref"><Input /></Form.Item> : null}
        </Form>
      </Modal>

      <Modal title="New Version" open={versionModalOpen} onCancel={() => setVersionModalOpen(false)} onOk={() => void submitVersion()} confirmLoading={createVersion.isPending}>
        <Form layout="vertical" form={versionForm}>
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="summary" label="Summary" rules={[{ required: true }]}><Input.TextArea rows={3} /></Form.Item>
          <Form.Item name="contentRef" label="Content Ref"><Input /></Form.Item>
          <Form.Item name="changeSummary" label="Change Summary"><Input.TextArea rows={2} /></Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
