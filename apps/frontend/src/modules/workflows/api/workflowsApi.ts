import { apiRequest } from "../../../shared/lib/apiClient";
import type { CreateWorkflowDefinitionInput, WorkflowDefinitionSummary } from "../types/workflows";

export async function listWorkflowDefinitions(signal?: AbortSignal): Promise<WorkflowDefinitionSummary[]> {
  return apiRequest<WorkflowDefinitionSummary[]>("/api/v1/workflows/definitions", { signal });
}

export async function createWorkflowDefinition(input: CreateWorkflowDefinitionInput): Promise<WorkflowDefinitionSummary> {
  return apiRequest<WorkflowDefinitionSummary>("/api/v1/workflows/definitions", {
    method: "POST",
    body: input,
  });
}
