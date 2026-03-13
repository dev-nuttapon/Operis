import { apiRequest } from "../../../shared/lib/apiClient";
import type { CreateWorkflowDefinitionInput, UpdateWorkflowDefinitionInput, WorkflowDefinitionSummary } from "../types/workflows";

export async function listWorkflowDefinitions(signal?: AbortSignal): Promise<WorkflowDefinitionSummary[]> {
  return apiRequest<WorkflowDefinitionSummary[]>("/api/v1/workflows/definitions", { signal });
}

export async function createWorkflowDefinition(input: CreateWorkflowDefinitionInput): Promise<WorkflowDefinitionSummary> {
  return apiRequest<WorkflowDefinitionSummary>("/api/v1/workflows/definitions", {
    method: "POST",
    body: input,
  });
}

export async function updateWorkflowDefinition(input: UpdateWorkflowDefinitionInput): Promise<WorkflowDefinitionSummary> {
  return apiRequest<WorkflowDefinitionSummary>(`/api/v1/workflows/definitions/${input.workflowDefinitionId}`, {
    method: "PUT",
    body: { name: input.name },
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
