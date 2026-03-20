import { apiRequest } from "../../../shared/lib/apiClient";
import type {
  CreateWorkflowDefinitionInput,
  UpdateWorkflowDefinitionInput,
  WorkflowDefinitionListInput,
  WorkflowDefinitionListResponse,
  WorkflowDefinitionDetail,
  WorkflowDefinitionSummary,
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
  return apiRequest<WorkflowDefinitionListResponse>(`/api/v1/workflows/definitions${toListQuery(input)}`, { signal });
}

export async function getWorkflowDefinition(
  workflowDefinitionId: string,
  signal?: AbortSignal,
): Promise<WorkflowDefinitionDetail> {
  return apiRequest<WorkflowDefinitionDetail>(`/api/v1/workflows/definitions/${workflowDefinitionId}`, { signal });
}

export async function createWorkflowDefinition(input: CreateWorkflowDefinitionInput): Promise<WorkflowDefinitionDetail> {
  return apiRequest<WorkflowDefinitionDetail>("/api/v1/workflows/definitions", {
    method: "POST",
    body: input,
  });
}

export async function updateWorkflowDefinition(input: UpdateWorkflowDefinitionInput): Promise<WorkflowDefinitionDetail> {
  return apiRequest<WorkflowDefinitionDetail>(`/api/v1/workflows/definitions/${input.workflowDefinitionId}`, {
    method: "PUT",
    body: { name: input.name, steps: input.steps },
  });
}

export async function activateWorkflowDefinition(workflowDefinitionId: string): Promise<WorkflowDefinitionSummary> {
  return apiRequest<WorkflowDefinitionSummary>(`/api/v1/workflows/definitions/${workflowDefinitionId}/activate`, {
    method: "POST",
  });
}

export async function archiveWorkflowDefinition(workflowDefinitionId: string): Promise<WorkflowDefinitionSummary> {
  return apiRequest<WorkflowDefinitionSummary>(`/api/v1/workflows/definitions/${workflowDefinitionId}/archive`, {
    method: "POST",
  });
}
