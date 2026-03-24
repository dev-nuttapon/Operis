import { useQuery } from "@tanstack/react-query";
import { listProjects } from "../api/usersApi";
import type { ListProjectsInput } from "../types/users";

export function useProjectList(input: ListProjectsInput, enabled = true) {
  return useQuery({
    queryKey: ["projects", "list", input],
    queryFn: ({ signal }) => listProjects(input, signal),
    enabled,
    staleTime: 15_000,
  });
}
