import { useEffect, useMemo, useState } from "react";
import { Button, Card, Input, Space, Table, Typography, Alert, Skeleton, Dropdown, Flex, Grid } from "antd";
import type { ColumnsType } from "antd/es/table";
import { PlusOutlined, FileTextOutlined, MoreOutlined, EditOutlined, HistoryOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { useDocumentTemplates } from "../hooks/useDocumentTemplates";
import type { DocumentTemplateListItem } from "../types/documentTemplates";

export function DocumentTemplatesPage() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canReadDocuments = permissionState.hasPermission(permissions.documents.read);
  const [paging, setPaging] = useState({ page: 1, pageSize: 10, search: "" });
  const [searchInput, setSearchInput] = useState("");

  const templatesQuery = useDocumentTemplates({ page: paging.page, pageSize: paging.pageSize, search: paging.search }, canReadDocuments);

  useEffect(() => {
    setPaging((current) => ({ ...current, page: 1, search: searchInput }));
  }, [searchInput]);

  const columns = useMemo<ColumnsType<DocumentTemplateListItem>>(
    () => [
      { title: t("documents.templates.columns.name"), dataIndex: "name" },
      {
        title: t("documents.templates.columns.document_count"),
        dataIndex: "documentCount",
        align: "center",
        render: (value: number) => value ?? 0,
      },
      {
        title: t("documents.templates.columns.created_at"),
        dataIndex: "createdAt",
        render: (value: string) =>
          new Date(value).toLocaleDateString(i18n.language.startsWith("th") ? "th-TH" : "en-US"),
      },
      {
        title: t("documents.templates.columns.actions"),
        key: "actions",
        align: "center",
        render: (_, record) => (
          <Dropdown
            menu={{
              items: [
                {
                  key: "edit",
                  icon: <EditOutlined />,
                  label: t("documents.templates.actions.edit"),
                  onClick: () => navigate(`/app/document-templates/${record.id}/edit`),
                },
                {
                  key: "history",
                  icon: <HistoryOutlined />,
                  label: t("documents.templates.actions.history"),
                  onClick: () =>
                    navigate(`/app/document-templates/${record.id}/history`, {
                      state: { templateName: record.name, from: "/app/document-templates" },
                    }),
                },
              ],
            }}
            trigger={["click"]}
          >
            <Button size="small" icon={<MoreOutlined />} />
          </Dropdown>
        ),
      },
    ],
    [i18n.language, navigate, t],
  );

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
              background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)",
              color: "#fff",
            }}
          >
            <FileTextOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("documents.templates.title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("documents.templates.description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canReadDocuments ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
        ) : (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Flex
              gap={12}
              wrap={!isMobile}
              vertical={isMobile}
              align={isMobile ? "stretch" : "center"}
              justify="space-between"
              style={{ width: "100%" }}
            >
              <Input.Search
                allowClear
                placeholder={t("documents.templates.search_placeholder")}
                value={searchInput}
                onChange={(event) => setSearchInput(event.target.value)}
                onSearch={(value) => setSearchInput(value)}
                style={{ width: isMobile ? "100%" : undefined, maxWidth: isMobile ? undefined : 360 }}
              />
              <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate("/app/document-templates/new")} block={isMobile}>
                {t("documents.templates.actions.create_new")}
              </Button>
            </Flex>

            {templatesQuery.isLoading && (templatesQuery.data?.items?.length ?? 0) === 0 ? (
              <Skeleton active paragraph={{ rows: 5 }} />
            ) : (
              <Table
                rowKey="id"
                columns={columns}
                dataSource={templatesQuery.data?.items ?? []}
                loading={templatesQuery.isLoading}
                pagination={{
                  current: templatesQuery.data?.page ?? paging.page,
                  pageSize: templatesQuery.data?.pageSize ?? paging.pageSize,
                  total: templatesQuery.data?.total ?? 0,
                  showSizeChanger: true,
                  pageSizeOptions: [10, 25, 50, 100],
                  onChange: (page, pageSize) => setPaging((current) => ({ ...current, page, pageSize })),
                }}
                locale={{ emptyText: t("documents.templates.empty") }}
              />
            )}
          </Space>
        )}
      </Card>
    </Space>
  );
}
