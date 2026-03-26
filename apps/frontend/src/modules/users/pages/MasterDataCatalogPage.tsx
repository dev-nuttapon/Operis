import { useEffect, useMemo, useState } from "react";
import { Alert, App, Button, Card, Form, Grid, Input, Modal, Select, Skeleton, Space, Table, Tag, Typography, Flex } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { DatabaseOutlined, DeleteOutlined, EditOutlined, PlusOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";
import { ActionMenu } from "../../../shared/components/ActionMenu";
import { useMasterDataCatalog } from "../hooks/useMasterDataCatalog";
import type { MasterDataCatalogItem } from "../types/users";
import { formatDate, toApiSortOrder } from "../utils/adminUsersPresentation";

type EditFormValues = {
  domain: string;
  code: string;
  name: string;
  status: string;
  displayOrder: number;
  reason: string;
};

export function MasterDataCatalogPage() {
  const { t, i18n } = useTranslation();
  const { notification } = App.useApp();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canRead = permissionState.hasPermission(permissions.masterData.read);
  const canManage = permissionState.hasAnyPermission(permissions.masterData.managePermanentOrg, permissions.masterData.manageProjectStructures);
  const [paging, setPaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    domain: "",
    status: "Active",
    sortBy: "displayOrder",
    sortOrder: "asc" as "asc" | "desc",
  });
  const [searchInput, setSearchInput] = useState("");
  const [editorOpen, setEditorOpen] = useState(false);
  const [editing, setEditing] = useState<MasterDataCatalogItem | null>(null);
  const [archiveTarget, setArchiveTarget] = useState<MasterDataCatalogItem | null>(null);
  const [form] = Form.useForm<EditFormValues>();
  const [archiveForm] = Form.useForm<{ reason: string }>();
  const debouncedSearch = useDebouncedValue(searchInput, 300);

  useEffect(() => {
    setPaging((current) => ({ ...current, page: 1, search: debouncedSearch }));
  }, [debouncedSearch]);

  const { listQuery, createMutation, updateMutation, archiveMutation } = useMasterDataCatalog({
    list: paging.domain || paging.status || debouncedSearch
      ? { ...paging, search: debouncedSearch, domain: paging.domain || undefined, status: paging.status || undefined }
      : { ...paging, search: debouncedSearch },
  });

  const data = listQuery.data as { items?: MasterDataCatalogItem[]; page?: number; pageSize?: number; total?: number } | undefined;

  const openCreate = () => {
    setEditing(null);
    setEditorOpen(true);
    form.setFieldsValue({ domain: "", code: "", name: "", status: "Active", displayOrder: 0, reason: "" });
  };

  const handleError = (fallbackTitle: string, error: unknown) => {
    const presentation = getApiErrorPresentation(error, fallbackTitle);
    notification.error({ message: presentation.title, description: presentation.description });
  };

  const columns = useMemo<ColumnsType<MasterDataCatalogItem>>(
    () => [
      { title: t("master_data_catalog.columns.domain"), dataIndex: "domain", sorter: true },
      { title: t("master_data_catalog.columns.code"), dataIndex: "code" },
      { title: t("master_data_catalog.columns.name"), dataIndex: "name", sorter: true },
      {
        title: t("master_data_catalog.columns.status"),
        dataIndex: "status",
        render: (value: string) => <Tag color={value === "Active" ? "green" : "default"}>{value}</Tag>,
      },
      {
        title: t("master_data_catalog.columns.last_changed"),
        dataIndex: "lastChangedAt",
        render: (value: string | null) => formatDate(value, i18n.language),
      },
      {
        title: t("admin_users.columns.actions"),
        key: "actions",
        render: (_, record) =>
          canManage ? (
            <ActionMenu
              items={[
                {
                  key: "edit",
                  icon: <EditOutlined />,
                      label: t("common.actions.edit"),
                      onClick: () => {
                        setEditing(record);
                        setEditorOpen(true);
                        form.setFieldsValue({
                      domain: record.domain,
                      code: record.code,
                      name: record.name,
                      status: record.status,
                      displayOrder: record.displayOrder,
                      reason: "",
                    });
                  },
                },
                ...(record.status === "Active"
                  ? [{
                      key: "archive",
                      icon: <DeleteOutlined />,
                      label: t("common.actions.archive"),
                      danger: true,
                      onClick: () => {
                        setArchiveTarget(record);
                        archiveForm.resetFields();
                      },
                    }]
                  : []),
              ]}
            />
          ) : null,
      },
    ],
    [archiveForm, canManage, form, i18n.language, t],
  );

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #b45309, #92400e)", color: "#fff" }}>
            <DatabaseOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>{t("master_data_catalog.page_title")}</Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>{t("master_data_catalog.page_description")}</Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canRead ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
        ) : (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Flex gap={12} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"} justify="space-between">
              <Input.Search
                allowClear
                placeholder={t("master_data_catalog.search_placeholder")}
                value={searchInput}
                onChange={(event) => setSearchInput(event.target.value)}
                onSearch={(value) => setSearchInput(value)}
                style={{ width: isMobile ? "100%" : 320 }}
              />
              <Flex gap={8} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"}>
                <Input
                  placeholder={t("master_data_catalog.domain_placeholder")}
                  value={paging.domain}
                  onChange={(event) => setPaging((current) => ({ ...current, page: 1, domain: event.target.value }))}
                  style={{ width: isMobile ? "100%" : 180 }}
                />
                <Select
                  value={paging.status}
                  options={[
                    { label: t("master_data_catalog.status.active"), value: "Active" },
                    { label: t("master_data_catalog.status.archived"), value: "Archived" },
                  ]}
                  onChange={(value) => setPaging((current) => ({ ...current, status: value, page: 1 }))}
                  style={{ width: isMobile ? "100%" : 160 }}
                />
                {canManage ? (
                  <Button type="primary" icon={<PlusOutlined />} onClick={openCreate} block={isMobile}>
                    {t("master_data_catalog.create_action")}
                  </Button>
                ) : null}
              </Flex>
            </Flex>

            {listQuery.isLoading && (Array.isArray(data?.items) ? data.items.length : 0) === 0 ? (
              <Skeleton active paragraph={{ rows: 6 }} />
            ) : (
              <Table
                rowKey="id"
                columns={columns}
                dataSource={Array.isArray(data?.items) ? data.items : []}
                loading={listQuery.isLoading}
                scroll={{ x: "max-content" }}
                pagination={{
                  current: data?.page ?? paging.page,
                  pageSize: data?.pageSize ?? paging.pageSize,
                  total: data?.total ?? 0,
                  showSizeChanger: true,
                  pageSizeOptions: [10, 25, 50, 100],
                }}
                onChange={(nextPagination, _, sorter) => {
                  const resolvedSorter = sorter as SorterResult<MasterDataCatalogItem>;
                  setPaging((current) => ({
                    ...current,
                    page: nextPagination.current ?? current.page,
                    pageSize: nextPagination.pageSize ?? current.pageSize,
                    sortBy: typeof resolvedSorter.field === "string" ? resolvedSorter.field : current.sortBy,
                    sortOrder: toApiSortOrder(resolvedSorter.order) ?? current.sortOrder,
                  }));
                }}
              />
            )}
          </Space>
        )}
      </Card>

      <Modal
        title={editing ? t("master_data_catalog.edit_modal_title") : t("master_data_catalog.create_modal_title")}
        open={editorOpen}
        onCancel={() => {
          setEditorOpen(false);
          setEditing(null);
          form.resetFields();
        }}
        onOk={() => {
          form.validateFields().then((values) => {
            const onSuccess = () => {
              setEditorOpen(false);
              setEditing(null);
              form.resetFields();
              notification.success({ message: editing ? t("master_data_catalog.messages.updated", { name: values.name }) : t("master_data_catalog.messages.created", { name: values.name }) });
            };
            const onError = (error: unknown) => handleError(editing ? t("master_data_catalog.messages.update_failed") : t("master_data_catalog.messages.create_failed"), error);

            if (editing) {
              updateMutation.mutate({ id: editing.id, ...values }, { onSuccess, onError });
            } else {
              createMutation.mutate(values, { onSuccess, onError });
            }
          }).catch(() => undefined);
        }}
        confirmLoading={createMutation.isPending || updateMutation.isPending}
      >
        <Form form={form} layout="vertical" initialValues={{ status: "Active", displayOrder: 0 }}>
          <Form.Item name="domain" label={t("master_data_catalog.fields.domain")} rules={[{ required: true }]}>
            <Input placeholder={t("master_data_catalog.placeholders.domain")} />
          </Form.Item>
          <Form.Item name="code" label={t("master_data_catalog.fields.code")} rules={[{ required: true }]}>
            <Input placeholder={t("master_data_catalog.placeholders.code")} />
          </Form.Item>
          <Form.Item name="name" label={t("master_data_catalog.fields.name")} rules={[{ required: true }]}>
            <Input placeholder={t("master_data_catalog.placeholders.name")} />
          </Form.Item>
          <Form.Item name="status" label={t("master_data_catalog.fields.status")} rules={[{ required: true }]}>
            <Select options={[{ label: t("master_data_catalog.status.active"), value: "Active" }, { label: t("master_data_catalog.status.archived"), value: "Archived" }]} />
          </Form.Item>
          <Form.Item name="displayOrder" label={t("master_data_catalog.fields.display_order")} rules={[{ required: true }]}>
            <Input type="number" />
          </Form.Item>
          <Form.Item name="reason" label={t("master_data_catalog.fields.reason")} rules={[{ required: true }]}>
            <Input.TextArea rows={4} placeholder={t("master_data_catalog.placeholders.reason")} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={t("master_data_catalog.archive_modal_title")}
        open={archiveTarget !== null}
        onCancel={() => {
          setArchiveTarget(null);
          archiveForm.resetFields();
        }}
        onOk={() => {
          archiveForm.validateFields().then((values) => {
            if (!archiveTarget) {
              return;
            }

            archiveMutation.mutate(
              { id: archiveTarget.id, input: { reason: values.reason } },
              {
                onSuccess: () => {
                  setArchiveTarget(null);
                  archiveForm.resetFields();
                  notification.success({ message: t("master_data_catalog.messages.archived", { name: archiveTarget.name }) });
                },
                onError: (error) => handleError(t("master_data_catalog.messages.archive_failed"), error),
              },
            );
          }).catch(() => undefined);
        }}
        confirmLoading={archiveMutation.isPending}
      >
        <Form form={archiveForm} layout="vertical">
          <Form.Item name="reason" label={t("master_data_catalog.fields.reason")} rules={[{ required: true }]}>
            <Input.TextArea rows={4} placeholder={t("master_data_catalog.placeholders.reason")} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
