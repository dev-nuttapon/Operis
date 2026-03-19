import { useMemo, useState } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { listDepartments } from "../api/usersApi";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";

export function useDepartmentFilterOptions({
  enabled,
  divisionId,
  pageSize = 5,
}: {
  enabled: boolean;
  divisionId?: string;
  pageSize?: number;
}) {
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search, 300);

  const departmentsQuery = useInfiniteQuery({
    queryKey: ["department-filter-options", { divisionId, search: debouncedSearch, pageSize }],
    enabled,
    queryFn: ({ signal, pageParam }) =>
      listDepartments(
        {
          page: pageParam as number,
          pageSize,
          search: debouncedSearch,
          sortBy: "displayOrder",
          sortOrder: "asc",
          divisionId,
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

  const items = useMemo(() => departmentsQuery.data?.pages.flatMap((page) => page.items) ?? [], [departmentsQuery.data]);
  const options = useMemo(() => items.map((item) => ({ label: item.name, value: item.id })), [items]);

  const loadMore = () => {
    if (!departmentsQuery.isFetching && departmentsQuery.hasNextPage) {
      void departmentsQuery.fetchNextPage();
    }
  };

  return {
    options,
    loading: departmentsQuery.isFetching,
    hasMore: Boolean(departmentsQuery.hasNextPage),
    search,
    onSearch: setSearch,
    onLoadMore: loadMore,
  };
}
