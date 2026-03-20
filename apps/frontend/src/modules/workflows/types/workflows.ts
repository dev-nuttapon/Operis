export interface WorkflowDefinitionSummary {
  id: string;
  code: string;
  name: string;
  status: "draft" | "active" | "archived";
}

export type WorkflowStatusFilter = "all" | WorkflowDefinitionSummary["status"];

export type WorkflowStepType = "submit" | "peer_review" | "review" | "approve";

export interface WorkflowStep {
  id?: string;
  name: string;
  stepType: WorkflowStepType;
  displayOrder: number;
  isRequired: boolean;
  roleIds: string[];
}

export interface WorkflowDefinitionDetail extends WorkflowDefinitionSummary {
  steps: WorkflowStep[];
}

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
  steps: WorkflowStep[];
}

export interface UpdateWorkflowDefinitionInput {
  workflowDefinitionId: string;
  name: string;
  steps: WorkflowStep[];
}

export interface WorkflowDefinitionActionInput {
  workflowDefinitionId: string;
}
