import { useMemo } from "react";
import { useWorkflowDefinitions } from "./useWorkflowDefinitions";
import type { WorkflowStatusFilter } from "../types/workflows";

export function useWorkflowDefinitionOptions({
  enabled,
  status = "active",
  pageSize = 100,
}: {
  enabled: boolean;
  status?: WorkflowStatusFilter;
  pageSize?: number;
}) {
  const definitionsQuery = useWorkflowDefinitions({
    page: 1,
    pageSize,
    status,
  }, enabled);

  const options = useMemo(
    () => (definitionsQuery.data?.items ?? []).map((item) => ({ label: item.name, value: item.id })),
    [definitionsQuery.data],
  );

  return {
    options,
    items: definitionsQuery.data?.items ?? [],
    loading: definitionsQuery.isLoading,
  };
}
