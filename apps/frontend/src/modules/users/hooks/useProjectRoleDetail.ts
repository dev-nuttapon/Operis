import { useQuery } from "@tanstack/react-query";
import { getProjectRole } from "../api/usersApi";

export function useProjectRoleDetail(projectRoleId: string | undefined) {
  const query = useQuery({
    queryKey: ["admin", "project-role-detail", projectRoleId],
    enabled: Boolean(projectRoleId),
    queryFn: ({ signal }) => getProjectRole(projectRoleId!, signal),
    staleTime: 15_000,
  });

  return {
    data: query.data,
    loading: query.isFetching,
    error: query.error,
  };
}

