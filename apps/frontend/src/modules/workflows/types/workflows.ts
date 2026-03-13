export interface WorkflowDefinitionSummary {
  id: string;
  code: string;
  name: string;
  status: "draft" | "active" | "archived";
}

export type WorkflowStatusFilter = "all" | WorkflowDefinitionSummary["status"];

export interface WorkflowDefinitionStatusSummary {
  all: number;
  draft: number;
  active: number;
  archived: number;
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
