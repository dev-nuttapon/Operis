import { useMemo, useState } from "react";
import { Alert, Button, Card, Flex, Input, Select, Space, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { FileTextOutlined, PlusOutlined, SettingOutlined, UploadOutlined, EyeOutlined, DownloadOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { useDocuments, useDocumentTypes } from "../hooks/useDocuments";
import { downloadDocument, type DocumentListItem } from "../api/documentsApi";
import { useProjectOptions } from "../../users";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { saveBlobAsFile } from "../utils/download";

const { Title, Paragraph, Text } = Typography;

const statusColors: Record<string, string> = {
  draft: "default",
  review: "gold",
  approved: "green",
  rejected: "red",
  baseline: "blue",
  archived: "purple",
};

export function DocumentDashboardPage() {
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.documents.read);
  const canUpload = permissionState.hasPermission(permissions.documents.upload);
  const canManageVersions = permissionState.hasPermission(permissions.documents.manageVersions);
  const canManageTypes = permissionState.hasPermission(permissions.documents.deactivate);
  const [filters, setFilters] = useState({
    search: "",
    documentTypeId: undefined as string | undefined,
    projectId: undefined as string | undefined,
    status: undefined as string | undefined,
    classification: undefined as string | undefined,
    page: 1,
    pageSize: 10,
  });

  const documentsQuery = useDocuments(filters, canRead);
  const documentTypesQuery = useDocumentTypes({ page: 1, pageSize: 100 }, canRead);
  const projectOptions = useProjectOptions({ enabled: canRead, assignedOnly: false, pageSize: 20 });
  const documents = documentsQuery.data?.items ?? [];

  const columns = useMemo<ColumnsType<DocumentListItem>>(
    () => [
      {
        title: "Title",
        dataIndex: "title",
        key: "title",
        render: (_, item) => (
          <Space direction="vertical" size={0}>
            <Text strong>{item.title}</Text>
            <Text type="secondary">{item.documentTypeName ?? "Unclassified type"}</Text>
          </Space>
        ),
      },
      { title: "Project", dataIndex: "projectName", key: "projectName", render: (value) => value ?? "-" },
      { title: "Phase", dataIndex: "phaseCode", key: "phaseCode", render: (value) => value ?? "-" },
      { title: "Owner", dataIndex: "ownerUserId", key: "ownerUserId", render: (value) => value ?? "-" },
      {
        title: "Status",
        dataIndex: "status",
        key: "status",
        render: (value: string) => <Tag color={statusColors[value] ?? "default"}>{value}</Tag>,
      },
      { title: "Version", dataIndex: "currentVersionNumber", key: "currentVersionNumber", render: (value) => (value ? `v${value}` : "-") },
      { title: "Classification", dataIndex: "classification", key: "classification" },
      {
        title: "Updated",
        dataIndex: "updatedAt",
        key: "updatedAt",
        render: (value: string) => new Date(value).toLocaleString(),
      },
      {
        title: "Actions",
        key: "actions",
        render: (_, item) => (
          <Flex gap={8} wrap>
            <Button size="small" icon={<EyeOutlined />} onClick={() => navigate(`/app/documents/${item.id}`)}>
              View
            </Button>
            <Button
              size="small"
              icon={<DownloadOutlined />}
              disabled={!item.currentFileName}
              onClick={() => {
                void downloadDocument(item.id)
                  .then(({ blob, fileName }) => saveBlobAsFile(blob, fileName ?? item.currentFileName ?? "document"))
                  .catch(() => null);
              }}
            >
              Download
            </Button>
            <Button
              size="small"
              icon={<UploadOutlined />}
              disabled={!canManageVersions}
              onClick={() => navigate(`/app/documents/${item.id}/versions/new`, { state: { documentName: item.title, from: `/app/documents/${item.id}` } })}
            >
              Add version
            </Button>
          </Flex>
        ),
      },
    ],
    [canManageVersions, navigate],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Document access is not available for this account." />;
  }

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div
            style={{
              width: 48,
              height: 48,
              borderRadius: 14,
              display: "grid",
              placeItems: "center",
              background: "linear-gradient(135deg, #0f766e, #0f172a)",
              color: "#fff",
            }}
          >
            <FileTextOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>
              Document Register
            </Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Governed project documents, version state, and approval readiness in one register.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex justify="space-between" gap={12} wrap="wrap" style={{ marginBottom: 16 }}>
          <Flex gap={12} wrap="wrap">
            <Input.Search
              placeholder="Search title, type, or project"
              allowClear
              style={{ width: 260 }}
              value={filters.search}
              onChange={(event) => setFilters((current) => ({ ...current, search: event.target.value, page: 1 }))}
              onSearch={(value) => setFilters((current) => ({ ...current, search: value, page: 1 }))}
            />
            <Select
              allowClear
              placeholder="Type"
              style={{ width: 200 }}
              options={(documentTypesQuery.data?.items ?? []).map((item) => ({ label: `${item.code} · ${item.name}`, value: item.id }))}
              value={filters.documentTypeId}
              onChange={(value) => setFilters((current) => ({ ...current, documentTypeId: value, page: 1 }))}
            />
            <Select
              allowClear
              showSearch
              placeholder="Project"
              style={{ width: 220 }}
              options={projectOptions.options}
              value={filters.projectId}
              onSearch={projectOptions.onSearch}
              onPopupScroll={(event) => {
                const target = event.target as HTMLDivElement;
                if (target.scrollTop + target.clientHeight >= target.scrollHeight - 24) {
                  projectOptions.onLoadMore();
                }
              }}
              onChange={(value) => setFilters((current) => ({ ...current, projectId: value, page: 1 }))}
            />
            <Select
              allowClear
              placeholder="Status"
              style={{ width: 160 }}
              options={["draft", "review", "approved", "rejected", "baseline", "archived"].map((item) => ({ label: item, value: item }))}
              value={filters.status}
              onChange={(value) => setFilters((current) => ({ ...current, status: value, page: 1 }))}
            />
            <Select
              allowClear
              placeholder="Classification"
              style={{ width: 180 }}
              options={["public", "internal", "confidential", "restricted"].map((item) => ({ label: item, value: item }))}
              value={filters.classification}
              onChange={(value) => setFilters((current) => ({ ...current, classification: value, page: 1 }))}
            />
          </Flex>

          <Flex gap={8} wrap="wrap">
            <Button type="primary" icon={<PlusOutlined />} disabled={!canUpload} onClick={() => navigate("/app/documents/new")}>
              New document
            </Button>
            <Button icon={<SettingOutlined />} disabled={!canManageTypes} onClick={() => navigate("/app/documents/types")}>
              Document types
            </Button>
          </Flex>
        </Flex>

        <Table<DocumentListItem>
          rowKey="id"
          loading={documentsQuery.isLoading}
          columns={columns}
          dataSource={documents}
          pagination={{
            current: documentsQuery.data?.page ?? filters.page,
            pageSize: documentsQuery.data?.pageSize ?? filters.pageSize,
            total: documentsQuery.data?.total ?? 0,
            showSizeChanger: true,
            onChange: (page, pageSize) => setFilters((current) => ({ ...current, page, pageSize })),
          }}
        />
      </Card>
    </Space>
  );
}
