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

export interface WorkflowDefinitionListInput {
  status?: WorkflowStatusFilter;
  page?: number;
  pageSize?: number;
}

export interface WorkflowDefinitionListResponse {
  items: WorkflowDefinitionSummary[];
  total: number;
  page: number;
  pageSize: number;
  statusSummary: WorkflowDefinitionStatusSummary;
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
