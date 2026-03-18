import { useMemo, useState } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { listJobTitles, listPublicJobTitlesByDepartment } from "../api/usersApi";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";

export function useJobTitleOptions({
  enabled,
  departmentId,
  pageSize = 10,
  publicAccess = false,
}: {
  enabled: boolean;
  departmentId?: string;
  pageSize?: number;
  publicAccess?: boolean;
}) {
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search, 300);

  const jobTitlesQuery = useInfiniteQuery({
    queryKey: ["job-title-options", { departmentId, publicAccess, search: debouncedSearch, pageSize }],
    enabled: enabled && Boolean(departmentId),
    queryFn: ({ signal, pageParam }) => {
      const input = {
        page: pageParam as number,
        pageSize,
        search: debouncedSearch,
        sortBy: "displayOrder",
        sortOrder: "asc",
        departmentId,
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
      return lastPage.items.length === pageSize ? allPages.length + 1 : undefined;
    },
    staleTime: 60_000,
  });

  const items = useMemo(() => jobTitlesQuery.data?.pages.flatMap((page) => page.items) ?? [], [jobTitlesQuery.data]);
  const options = useMemo(() => items.map((item) => ({ label: item.name, value: item.id })), [items]);
  const hasMore = Boolean(jobTitlesQuery.hasNextPage);

  const handleSearch = (value: string) => {
    setSearch(value);
  };

  const loadMore = () => {
    if (!jobTitlesQuery.isFetching && jobTitlesQuery.hasNextPage) {
      void jobTitlesQuery.fetchNextPage();
    }
  };

  return {
    options,
    items,
    hasMore,
    loading: jobTitlesQuery.isFetching,
    onSearch: handleSearch,
    onLoadMore: loadMore,
    search,
  };
}
