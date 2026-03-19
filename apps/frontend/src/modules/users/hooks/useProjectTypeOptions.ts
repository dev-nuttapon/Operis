import { useMemo, useState } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { listProjectTypeTemplates } from "../api/usersApi";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";

export function useProjectTypeOptions({ enabled, pageSize = 10 }: { enabled: boolean; pageSize?: number }) {
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search, 300);

  const templatesQuery = useInfiniteQuery({
    queryKey: ["project-type-options", { search: debouncedSearch, pageSize }],
    enabled,
    queryFn: ({ signal, pageParam }) =>
      listProjectTypeTemplates(
        {
          page: pageParam as number,
          pageSize,
          search: debouncedSearch,
          sortBy: "projectType",
          sortOrder: "asc",
        },
        signal,
      ),
    initialPageParam: 1,
    getNextPageParam: (lastPage, allPages) => {
      const loaded = allPages.reduce((sum, page) => sum + (Array.isArray(page.items) ? page.items.length : 0), 0);
      const total = typeof lastPage.total === "number" ? lastPage.total : 0;
      const lastItemsCount = Array.isArray(lastPage.items) ? lastPage.items.length : 0;
      if (total > 0 && loaded < total) {
        return allPages.length + 1;
      }
      return lastItemsCount === pageSize ? allPages.length + 1 : undefined;
    },
    staleTime: 60_000,
  });

  const items = useMemo(() => templatesQuery.data?.pages.flatMap((page) => page.items) ?? [], [templatesQuery.data]);
  const options = useMemo(
    () => items.map((item) => ({ label: item.projectType, value: item.projectType })),
    [items],
  );
  const hasMore = Boolean(templatesQuery.hasNextPage);

  const handleSearch = (value: string) => {
    setSearch(value);
  };

  const loadMore = () => {
    if (!templatesQuery.isFetching && templatesQuery.hasNextPage) {
      void templatesQuery.fetchNextPage();
    }
  };

  return {
    options,
    items,
    hasMore,
    loading: templatesQuery.isFetching,
    onSearch: handleSearch,
    onLoadMore: loadMore,
    search,
  };
}
