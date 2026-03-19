import { useQuery } from "@tanstack/react-query";
import { getProjectTypeRoleRequirement } from "../api/usersApi";

const projectTypeRoleRequirementDetailQueryKey = ["admin", "project-type-role-requirements", "detail"];

export function useProjectTypeRoleRequirementDetail(requirementId?: string) {
  return useQuery({
    queryKey: [...projectTypeRoleRequirementDetailQueryKey, requirementId],
    enabled: Boolean(requirementId),
    queryFn: ({ signal }) => getProjectTypeRoleRequirement(requirementId!, signal),
    staleTime: 15_000,
  });
}
