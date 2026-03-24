import { apiRequest } from "../../../shared/lib/apiClient";
import type {
  CreateWorkflowDefinitionInput,
  UpdateWorkflowDefinitionInput,
  WorkflowDefinitionListInput,
  WorkflowDefinitionListResponse,
  WorkflowDefinitionDetail,
  WorkflowDefinitionSummary,
  WorkflowInstanceDetail,
  CreateWorkflowInstanceInput,
  WorkflowStepActionInput,
  WorkflowTaskListInput,
  WorkflowTaskListResponse,
} from "../types/workflows";

function toListQuery(input?: WorkflowDefinitionListInput) {
  if (!input) {
    return "";
  }

  const params = new URLSearchParams();
  if (input.status && input.status !== "all") {
    params.set("status", input.status);
  }
  if (input.page) {
    params.set("page", String(input.page));
  }
  if (input.pageSize) {
    params.set("pageSize", String(input.pageSize));
  }

  const query = params.toString();
  return query ? `?${query}` : "";
}

export async function listWorkflowDefinitions(
  input?: WorkflowDefinitionListInput,
  signal?: AbortSignal,
): Promise<WorkflowDefinitionListResponse> {
  return apiRequest<WorkflowDefinitionListResponse>(`/api/v1/steps/definitions${toListQuery(input)}`, { signal });
}

export async function getWorkflowDefinition(
  workflowDefinitionId: string,
  signal?: AbortSignal,
): Promise<WorkflowDefinitionDetail> {
  return apiRequest<WorkflowDefinitionDetail>(`/api/v1/steps/definitions/${workflowDefinitionId}`, { signal });
}

export async function createWorkflowDefinition(input: CreateWorkflowDefinitionInput): Promise<WorkflowDefinitionDetail> {
  return apiRequest<WorkflowDefinitionDetail>("/api/v1/steps/definitions", {
    method: "POST",
    body: input,
  });
}

export async function updateWorkflowDefinition(input: UpdateWorkflowDefinitionInput): Promise<WorkflowDefinitionDetail> {
  return apiRequest<WorkflowDefinitionDetail>(`/api/v1/steps/definitions/${input.workflowDefinitionId}`, {
    method: "PUT",
    body: { name: input.name, documentTemplateId: input.documentTemplateId ?? null, steps: input.steps },
  });
}

export async function activateWorkflowDefinition(workflowDefinitionId: string): Promise<WorkflowDefinitionSummary> {
  return apiRequest<WorkflowDefinitionSummary>(`/api/v1/steps/definitions/${workflowDefinitionId}/activate`, {
    method: "POST",
  });
}

export async function archiveWorkflowDefinition(workflowDefinitionId: string): Promise<WorkflowDefinitionSummary> {
  return apiRequest<WorkflowDefinitionSummary>(`/api/v1/steps/definitions/${workflowDefinitionId}/archive`, {
    method: "POST",
  });
}

export async function createWorkflowInstance(input: CreateWorkflowInstanceInput): Promise<WorkflowInstanceDetail> {
  return apiRequest<WorkflowInstanceDetail>("/api/v1/steps/instances", {
    method: "POST",
    body: input,
  });
}

export async function getWorkflowInstance(workflowInstanceId: string, signal?: AbortSignal): Promise<WorkflowInstanceDetail> {
  return apiRequest<WorkflowInstanceDetail>(`/api/v1/steps/instances/${workflowInstanceId}`, { signal });
}

export async function getWorkflowInstanceByDocument(documentId: string, signal?: AbortSignal): Promise<WorkflowInstanceDetail> {
  return apiRequest<WorkflowInstanceDetail>(`/api/v1/steps/instances/by-document/${documentId}`, { signal });
}

export async function applyWorkflowStepAction(input: WorkflowStepActionInput): Promise<WorkflowInstanceDetail> {
  return apiRequest<WorkflowInstanceDetail>(
    `/api/v1/steps/instances/${input.workflowInstanceId}/steps/${input.workflowInstanceStepId}/actions`,
    {
      method: "POST",
      body: { action: input.action, comment: input.comment },
    },
  );
}

export async function listWorkflowTasks(input: WorkflowTaskListInput): Promise<WorkflowTaskListResponse> {
  const page = input.page ?? 1;
  const pageSize = input.pageSize ?? 10;
  const params = new URLSearchParams();
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));
  if (input.projectId) {
    params.set("projectId", input.projectId);
  }
  const query = params.toString();
  return apiRequest<WorkflowTaskListResponse>(`/api/v1/steps/tasks?${query}`);
}
