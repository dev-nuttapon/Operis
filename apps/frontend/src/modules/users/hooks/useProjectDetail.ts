import { useQuery } from "@tanstack/react-query";
import { getProject } from "../api/usersApi";

export function useProjectDetail(projectId: string | undefined) {
  return useQuery({
    queryKey: ["projects", "detail", projectId],
    enabled: Boolean(projectId),
    queryFn: ({ signal }) => getProject(projectId!, signal),
    staleTime: 15_000,
  });
}
