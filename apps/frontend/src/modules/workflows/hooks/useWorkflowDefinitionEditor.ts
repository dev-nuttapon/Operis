import { useState } from "react";

export function useWorkflowDefinitionEditor() {
  const [editingWorkflowDefinitionId, setEditingWorkflowDefinitionId] = useState<string | null>(null);

  return {
    editingWorkflowDefinitionId,
    startEditing: (workflowDefinitionId: string) => setEditingWorkflowDefinitionId(workflowDefinitionId),
    stopEditing: () => setEditingWorkflowDefinitionId(null),
  };
}
