export interface WorkflowDefinitionSummary {
  id: string;
  code: string;
  name: string;
  status: "draft" | "active" | "archived";
  documentTemplateId?: string | null;
}

export type WorkflowStatusFilter = "all" | WorkflowDefinitionSummary["status"];

export type WorkflowStepType = "submit" | "peer_review" | "review" | "approve";

export interface WorkflowStepRoute {
  action: WorkflowStepType;
  nextDisplayOrder?: number | null;
}

export interface WorkflowStep {
  id?: string;
  name: string;
  stepType: WorkflowStepType;
  displayOrder: number;
  isRequired: boolean;
  documentId?: string | null;
  minApprovals?: number;
  roleIds: string[];
  routes?: WorkflowStepRoute[];
}

export interface WorkflowDefinitionDetail extends WorkflowDefinitionSummary {
  hasInstances: boolean;
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
  documentTemplateId?: string | null;
  steps: WorkflowStep[];
}

export interface UpdateWorkflowDefinitionInput {
  workflowDefinitionId: string;
  name: string;
  documentTemplateId?: string | null;
  steps: WorkflowStep[];
}

export interface WorkflowDefinitionActionInput {
  workflowDefinitionId: string;
}

export interface WorkflowInstance {
  id: string;
  projectId: string;
  documentId: string;
  workflowDefinitionId: string;
  status: string;
  currentStepOrder: number;
  startedAt: string;
  completedAt: string | null;
  createdAt: string;
}

export interface WorkflowInstanceStep {
  id: string;
  workflowStepId: string;
  stepType: WorkflowStepType;
  displayOrder: number;
  isRequired: boolean;
  status: string;
  startedAt: string | null;
  completedAt: string | null;
  roleIds: string[];
}

export interface WorkflowInstanceAction {
  id: string;
  workflowInstanceStepId: string;
  action: string;
  actorUserId: string | null;
  actorEmail: string | null;
  actorDisplayName: string | null;
  comment: string | null;
  createdAt: string;
}

export interface WorkflowInstanceDetail {
  instance: WorkflowInstance;
  steps: WorkflowInstanceStep[];
  actions: WorkflowInstanceAction[];
}

export interface CreateWorkflowInstanceInput {
  projectId: string;
  documentId: string;
  workflowDefinitionId?: string;
}

export interface WorkflowStepActionInput {
  workflowInstanceId: string;
  workflowInstanceStepId: string;
  action: WorkflowStepType;
  comment?: string;
}

export interface WorkflowTaskListInput {
  page?: number;
  pageSize?: number;
  projectId?: string;
}

export interface WorkflowTaskItem {
  workflowInstanceId: string;
  workflowInstanceStepId: string;
  projectId: string;
  projectName: string;
  documentId: string;
  documentName: string;
  stepName: string;
  stepType: WorkflowStepType;
  roleName: string;
  status: string;
  dueAt?: string | null;
  canAct: boolean;
}

export interface WorkflowTaskListResponse {
  items: WorkflowTaskItem[];
  total: number;
  page: number;
  pageSize: number;
}
