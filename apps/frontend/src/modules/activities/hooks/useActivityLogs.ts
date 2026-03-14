import { useQuery } from "@tanstack/react-query";
import { listActivityLogs } from "../api/activitiesApi";
import type { ListActivityLogsInput } from "../types/activities";

export function useActivityLogs(filters: ListActivityLogsInput) {
  return useQuery({
    queryKey: ["admin", "activity-logs", filters],
    queryFn: ({ signal }) => listActivityLogs(filters, signal),
    staleTime: 15_000,
  });
}
