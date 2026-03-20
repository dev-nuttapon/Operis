import { Button, List, Space, Tag, Skeleton } from "antd";
import { useTranslation } from "react-i18next";
import type { WorkflowDefinitionSummary } from "../types/workflows";

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

  return (
    <List
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
      renderItem={(item) => {
        const actions = canManage
          ? [
              <Button
                key="edit"
                type="link"
                disabled={isMutating}
                onClick={() => onEdit(item.id)}
              >
                {t("common.actions.edit")}
              </Button>,
              item.status !== "active" ? (
                <Button
                  key="activate"
                  type="link"
                  disabled={isMutating}
                  onClick={() => onActivate(item.id)}
                >
                  {t("workflow_definitions.actions.activate")}
                </Button>
              ) : null,
              item.status !== "archived" ? (
                <Button
                  key="archive"
                  type="link"
                  danger
                  disabled={isMutating}
                  onClick={() => onArchive(item.id)}
                >
                  {t("workflow_definitions.actions.archive")}
                </Button>
              ) : null,
            ].filter(Boolean)
          : [];

        return (
          <List.Item actions={actions}>
            <Space>
              <span>{item.name}</span>
              <Tag>{statusLabels[item.status] ?? item.status}</Tag>
              <Tag>{item.code}</Tag>
            </Space>
          </List.Item>
        );
      }}
    />
  );
}
