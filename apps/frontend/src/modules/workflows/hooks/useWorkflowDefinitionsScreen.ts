import { useMemo, useState } from "react";
import { useWorkflowDefinitions } from "./useWorkflowDefinitions";
import type { WorkflowDefinitionStatusSummary, WorkflowDefinitionSummary, WorkflowStatusFilter } from "../types/workflows";

function buildStatusSummary(definitions: WorkflowDefinitionSummary[]): WorkflowDefinitionStatusSummary {
  return definitions.reduce<WorkflowDefinitionStatusSummary>(
    (summary, definition) => {
      summary.all += 1;
      summary[definition.status] += 1;
      return summary;
    },
    {
      all: 0,
      draft: 0,
      active: 0,
      archived: 0,
    },
  );
}

export function useWorkflowDefinitionsScreen() {
  const definitionsQuery = useWorkflowDefinitions();
  const [statusFilter, setStatusFilter] = useState<WorkflowStatusFilter>("all");

  const definitions = definitionsQuery.data ?? [];

  const statusSummary = useMemo(() => buildStatusSummary(definitions), [definitions]);
  const filteredDefinitions = useMemo(() => {
    if (statusFilter === "all") {
      return definitions;
    }

    return definitions.filter((definition) => definition.status === statusFilter);
  }, [definitions, statusFilter]);

  return {
    definitionsQuery,
    statusFilter,
    setStatusFilter,
    statusSummary,
    filteredDefinitions,
  };
}
