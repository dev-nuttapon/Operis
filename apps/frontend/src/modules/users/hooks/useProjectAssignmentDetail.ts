import { useQuery } from "@tanstack/react-query";
import { getProjectAssignment } from "../api/usersApi";

const projectAssignmentDetailQueryKey = ["admin", "project-assignments", "detail"];

export function useProjectAssignmentDetail(assignmentId?: string) {
  return useQuery({
    queryKey: [...projectAssignmentDetailQueryKey, assignmentId],
    enabled: Boolean(assignmentId),
    queryFn: ({ signal }) => getProjectAssignment(assignmentId!, signal),
    staleTime: 15_000,
  });
}
