export interface WorkflowDefinitionSummary {
  id: string;
  code: string;
  name: string;
  status: "draft" | "active" | "archived";
}

export interface CreateWorkflowDefinitionInput {
  name: string;
}

export interface UpdateWorkflowDefinitionInput {
  workflowDefinitionId: string;
  name: string;
}

export interface WorkflowDefinitionActionInput {
  workflowDefinitionId: string;
}
