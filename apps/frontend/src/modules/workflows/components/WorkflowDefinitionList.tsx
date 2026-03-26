import { Tag, Skeleton, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { CheckCircleOutlined, EditOutlined, StopOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import type { WorkflowDefinitionSummary } from "../types/workflows";
import { ActionMenu } from "../../../shared/components/ActionMenu";

interface WorkflowDefinitionListProps {
  canManage: boolean;
  definitions: WorkflowDefinitionSummary[];
  isLoading: boolean;
  isMutating: boolean;
  pagination: {
    page: number;
    pageSize: number;
    total: number;
  };
  onPageChange: (page: number, pageSize: number) => void;
  onEdit: (workflowDefinitionId: string) => void;
  onActivate: (workflowDefinitionId: string) => void;
  onArchive: (workflowDefinitionId: string) => void;
}

export function WorkflowDefinitionList({
  canManage,
  definitions,
  isLoading,
  isMutating,
  pagination,
  onPageChange,
  onEdit,
  onActivate,
  onArchive,
}: WorkflowDefinitionListProps) {
  const { t } = useTranslation();
  const statusLabels: Record<string, string> = {
    draft: t("workflow_definitions.filters.draft"),
    active: t("workflow_definitions.filters.active"),
    archived: t("workflow_definitions.filters.archived"),
  };

  if (isLoading && definitions.length === 0) {
    return <Skeleton active paragraph={{ rows: 6 }} />;
  }

  const columns: ColumnsType<WorkflowDefinitionSummary> = [
    {
      title: t("workflow_definitions.columns.name"),
      dataIndex: "name",
      ellipsis: true,
    },
    {
      title: t("workflow_definitions.columns.code"),
      dataIndex: "code",
      width: 180,
      ellipsis: true,
    },
    {
      title: t("workflow_definitions.columns.status"),
      dataIndex: "status",
      width: 140,
      render: (value: string) => <Tag>{statusLabels[value] ?? value}</Tag>,
    },
    {
      title: t("workflow_definitions.columns.template"),
      dataIndex: "documentTemplateId",
      width: 180,
      render: (value?: string | null) =>
        value ? t("workflow_definitions.columns.template_enabled") : t("workflow_definitions.columns.template_none"),
    },
    {
      title: t("workflow_definitions.columns.actions"),
      key: "actions",
      width: 240,
      render: (_value, item) => {
        if (!canManage) return null;
        const items = [
          {
            key: "edit",
            label: t("common.actions.edit"),
            icon: <EditOutlined />,
            disabled: isMutating,
            onClick: () => onEdit(item.id),
          },
          {
            key: "activate",
            label: t("workflow_definitions.actions.activate"),
            icon: <CheckCircleOutlined />,
            disabled: isMutating || item.status === "active",
            onClick: () => onActivate(item.id),
          },
          {
            key: "archive",
            label: t("workflow_definitions.actions.archive"),
            icon: <StopOutlined />,
            danger: true,
            disabled: isMutating || item.status === "archived",
            onClick: () => onArchive(item.id),
          },
        ];
        return <ActionMenu items={items} />;
      },
    },
  ];

  return (
    <Table
      rowKey="id"
      dataSource={definitions}
      loading={isLoading}
      locale={{ emptyText: t("workflow_definitions.empty") }}
      pagination={{
        current: pagination.page,
        pageSize: pagination.pageSize,
        total: pagination.total,
        showSizeChanger: true,
        pageSizeOptions: [10, 25, 50, 100],
        onChange: onPageChange,
      }}
      columns={columns}
      scroll={{ x: "max-content" }}
    />
  );
}
