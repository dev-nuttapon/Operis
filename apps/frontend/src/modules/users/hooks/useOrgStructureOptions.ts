import { useQuery } from "@tanstack/react-query";
import {
  listDepartments,
  listJobTitles,
  listPublicDepartmentsByDivision,
  listPublicJobTitlesByDepartment,
} from "../api/usersApi";

interface UseOrgStructureOptionsInput {
  divisionId?: string;
  departmentId?: string;
  publicAccess?: boolean;
}

export function useOrgStructureOptions(input: UseOrgStructureOptionsInput) {
  const { divisionId, departmentId, publicAccess = false } = input;

  const departmentsQuery = useQuery({
    queryKey: ["users", publicAccess ? "public" : "admin", "departments", divisionId],
    queryFn: ({ signal }) =>
      publicAccess
        ? listPublicDepartmentsByDivision(divisionId!, signal)
        : listDepartments({ page: 1, pageSize: 100, divisionId }, signal),
    enabled: Boolean(divisionId),
    staleTime: 5 * 60_000,
  });

  const jobTitlesQuery = useQuery({
    queryKey: ["users", publicAccess ? "public" : "admin", "job-titles", departmentId],
    queryFn: ({ signal }) =>
      publicAccess
        ? listPublicJobTitlesByDepartment(departmentId!, signal)
        : listJobTitles({ page: 1, pageSize: 100, departmentId }, signal),
    enabled: Boolean(departmentId),
    staleTime: 5 * 60_000,
  });

  return {
    departmentsQuery,
    jobTitlesQuery,
  };
}
