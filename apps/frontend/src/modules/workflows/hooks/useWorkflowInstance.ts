import { useQuery } from "@tanstack/react-query";
import { getWorkflowInstance, getWorkflowInstanceByDocument } from "../api/workflowsApi";

export function useWorkflowInstance(workflowInstanceId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["workflows", "instance", workflowInstanceId],
    enabled: enabled && Boolean(workflowInstanceId),
    queryFn: ({ signal }) =>
      workflowInstanceId ? getWorkflowInstance(workflowInstanceId, signal) : Promise.resolve(null),
    staleTime: 15_000,
  });
}

export function useWorkflowInstanceByDocument(documentId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["workflows", "instance-by-document", documentId],
    enabled: enabled && Boolean(documentId),
    queryFn: ({ signal }) =>
      documentId ? getWorkflowInstanceByDocument(documentId, signal) : Promise.resolve(null),
    staleTime: 15_000,
  });
}
