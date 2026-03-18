import { useInfiniteQuery } from "@tanstack/react-query";
import { useMemo, useState } from "react";
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
  const [departmentSearch, setDepartmentSearch] = useState("");
  const [jobTitleSearch, setJobTitleSearch] = useState("");

  const departmentsQuery = useInfiniteQuery({
    queryKey: ["users", publicAccess ? "public" : "admin", "departments", divisionId, departmentSearch],
    enabled: Boolean(divisionId),
    queryFn: ({ signal, pageParam }) => {
      const input = {
        page: pageParam as number,
        pageSize: 10,
        divisionId,
        search: departmentSearch,
        sortBy: "displayOrder",
        sortOrder: "asc",
      };

      return publicAccess
        ? listPublicDepartmentsByDivision(divisionId!, input, signal)
        : listDepartments(input, signal);
    },
    initialPageParam: 1,
    getNextPageParam: (lastPage, allPages) => {
      const loaded = allPages.reduce((sum, page) => sum + page.items.length, 0);
      if (lastPage.total && loaded < lastPage.total) {
        return allPages.length + 1;
      }
      return lastPage.items.length === 10 ? allPages.length + 1 : undefined;
    },
    staleTime: 5 * 60_000,
  });

  const jobTitlesQuery = useInfiniteQuery({
    queryKey: ["users", publicAccess ? "public" : "admin", "job-titles", departmentId, jobTitleSearch],
    enabled: Boolean(departmentId),
    queryFn: ({ signal, pageParam }) => {
      const input = {
        page: pageParam as number,
        pageSize: 10,
        departmentId,
        search: jobTitleSearch,
        sortBy: "displayOrder",
        sortOrder: "asc",
      };

      return publicAccess
        ? listPublicJobTitlesByDepartment(departmentId!, input, signal)
        : listJobTitles(input, signal);
    },
    initialPageParam: 1,
    getNextPageParam: (lastPage, allPages) => {
      const loaded = allPages.reduce((sum, page) => sum + page.items.length, 0);
      if (lastPage.total && loaded < lastPage.total) {
        return allPages.length + 1;
      }
      return lastPage.items.length === 10 ? allPages.length + 1 : undefined;
    },
    staleTime: 5 * 60_000,
  });

  const departmentItems = useMemo(
    () => departmentsQuery.data?.pages.flatMap((page) => page.items) ?? [],
    [departmentsQuery.data],
  );
  const jobTitleItems = useMemo(
    () => jobTitlesQuery.data?.pages.flatMap((page) => page.items) ?? [],
    [jobTitlesQuery.data],
  );

  return {
    departmentsQuery,
    jobTitlesQuery,
    departmentItems,
    jobTitleItems,
    departmentHasMore: Boolean(departmentsQuery.hasNextPage),
    jobTitleHasMore: Boolean(jobTitlesQuery.hasNextPage),
    onDepartmentSearch: setDepartmentSearch,
    onJobTitleSearch: setJobTitleSearch,
    onDepartmentLoadMore: () => {
      if (!departmentsQuery.isFetching && departmentsQuery.hasNextPage) {
        void departmentsQuery.fetchNextPage();
      }
    },
    onJobTitleLoadMore: () => {
      if (!jobTitlesQuery.isFetching && jobTitlesQuery.hasNextPage) {
        void jobTitlesQuery.fetchNextPage();
      }
    },
  };
}
