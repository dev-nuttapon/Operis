import { useMemo, useState } from "react";
import { useWorkflowDefinitions } from "./useWorkflowDefinitions";
import type { WorkflowDefinitionStatusSummary, WorkflowStatusFilter } from "../types/workflows";

export function useWorkflowDefinitionsScreen() {
  const [paging, setPaging] = useState({ page: 1, pageSize: 10 });
  const [statusFilter, setStatusFilter] = useState<WorkflowStatusFilter>("all");

  const definitionsQuery = useWorkflowDefinitions({
    page: paging.page,
    pageSize: paging.pageSize,
    status: statusFilter,
  });

  const definitions = definitionsQuery.data?.items ?? [];
  const statusSummary = useMemo<WorkflowDefinitionStatusSummary>(
    () =>
      definitionsQuery.data?.statusSummary ?? {
        all: 0,
        draft: 0,
        active: 0,
        archived: 0,
      },
    [definitionsQuery.data?.statusSummary],
  );

  return {
    definitionsQuery,
    statusFilter,
    setStatusFilter: (filter: WorkflowStatusFilter) => {
      setStatusFilter(filter);
      setPaging((current) => ({ ...current, page: 1 }));
    },
    statusSummary,
    filteredDefinitions: definitions,
    paging,
    setPaging,
  };
}
