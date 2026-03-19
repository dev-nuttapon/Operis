import { useQuery } from "@tanstack/react-query";
import { listProjectHistory } from "../api/usersApi";
import type { ProjectHistoryListInput } from "../types/users";

export function useProjectHistory(projectId: string | null, input?: ProjectHistoryListInput, enabled = true) {
  return useQuery({
    queryKey: ["projects", "history", projectId, input],
    queryFn: ({ signal }) =>
      projectId ? listProjectHistory(projectId, input, signal) : Promise.resolve({ items: [], total: 0, page: 1, pageSize: 10 }),
    enabled: enabled && Boolean(projectId),
  });
}
