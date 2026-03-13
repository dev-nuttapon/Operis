import { Button, List, Space, Tag } from "antd";
import { lazy, Suspense } from "react";
import type { WorkflowDefinitionSummary } from "../types/workflows";

const WorkflowDefinitionEditForm = lazy(async () => {
  const module = await import("./WorkflowDefinitionEditForm");
  return { default: module.WorkflowDefinitionEditForm };
});

interface WorkflowDefinitionListProps {
  definitions: WorkflowDefinitionSummary[];
  isLoading: boolean;
  isMutating: boolean;
  editingWorkflowDefinitionId: string | null;
  onStartEdit: (workflowDefinitionId: string) => void;
  onCancelEdit: () => void;
  onUpdate: (workflowDefinitionId: string, name: string) => void;
  onActivate: (workflowDefinitionId: string) => void;
  onArchive: (workflowDefinitionId: string) => void;
}

export function WorkflowDefinitionList({
  definitions,
  isLoading,
  isMutating,
  editingWorkflowDefinitionId,
  onStartEdit,
  onCancelEdit,
  onUpdate,
  onActivate,
  onArchive,
}: WorkflowDefinitionListProps) {
  return (
    <List
      dataSource={definitions}
      loading={isLoading}
      locale={{ emptyText: "No workflow definitions yet." }}
      renderItem={(item) => {
        const actions = [
          <Button
            key="edit"
            type="link"
            disabled={isMutating}
            onClick={() => onStartEdit(item.id)}
          >
            Edit
          </Button>,
          item.status !== "active" ? (
            <Button
              key="activate"
              type="link"
              disabled={isMutating}
              onClick={() => onActivate(item.id)}
            >
              Activate
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
              Archive
            </Button>
          ) : null,
        ].filter(Boolean);

        return (
          <List.Item actions={actions}>
            {editingWorkflowDefinitionId === item.id ? (
              <Suspense fallback={null}>
                <WorkflowDefinitionEditForm
                  initialName={item.name}
                  isSubmitting={isMutating}
                  onCancel={onCancelEdit}
                  onSubmit={(name) => onUpdate(item.id, name)}
                />
              </Suspense>
            ) : (
              <Space>
                <span>{item.name}</span>
                <Tag>{item.status}</Tag>
                <Tag>{item.code}</Tag>
              </Space>
            )}
          </List.Item>
        );
      }}
    />
  );
}
