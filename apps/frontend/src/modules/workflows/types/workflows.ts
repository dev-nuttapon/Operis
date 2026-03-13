export interface WorkflowDefinitionSummary {
  id: string;
  code: string;
  name: string;
  status: "draft" | "active" | "archived";
}

export interface CreateWorkflowDefinitionInput {
  name: string;
}
