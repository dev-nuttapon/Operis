import { useMemo, useState } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { listJobTitles } from "../api/usersApi";

export function useJobTitleOptions({
  enabled,
  departmentId,
  pageSize = 10,
}: {
  enabled: boolean;
  departmentId?: string;
  pageSize?: number;
}) {
  const [search, setSearch] = useState("");

  const jobTitlesQuery = useInfiniteQuery({
    queryKey: ["job-title-options", { departmentId, search, pageSize }],
    enabled: enabled && Boolean(departmentId),
    queryFn: ({ signal, pageParam }) =>
      listJobTitles(
        {
          page: pageParam as number,
          pageSize,
          search,
          sortBy: "displayOrder",
          sortOrder: "asc",
          departmentId,
        },
        signal,
      ),
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
