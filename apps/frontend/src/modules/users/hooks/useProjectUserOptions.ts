import { useMemo, useState } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { listUsers } from "../api/usersApi";
import type { User } from "../types/users";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";

export function useProjectUserOptions(enabled: boolean, toLabel: (user: User) => string) {
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search, 300);
  const pageSize = 5;

  const usersQuery = useInfiniteQuery({
    queryKey: ["project-user-options", { search: debouncedSearch }],
    enabled,
    queryFn: ({ signal, pageParam }) =>
      listUsers({ page: pageParam as number, pageSize, search: debouncedSearch, sortBy: "createdAt", sortOrder: "desc" }, signal),
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

  const items = useMemo(() => usersQuery.data?.pages.flatMap((page) => page.items) ?? [], [usersQuery.data]);

  const options = useMemo(() => items.map((item) => ({ label: toLabel(item), value: item.id })), [items, toLabel]);

  const hasMore = Boolean(usersQuery.hasNextPage);

  const handleSearch = (value: string) => {
    setSearch(value);
  };

  const loadMore = () => {
    if (!usersQuery.isFetching && usersQuery.hasNextPage) {
      void usersQuery.fetchNextPage();
    }
  };

  const result = useMemo(
    () => ({
      items,
      options,
      search,
      hasMore,
      loading: usersQuery.isFetching,
      onSearch: handleSearch,
      onLoadMore: loadMore,
    }),
    [hasMore, items, options, search, usersQuery.isFetching],
  );

  return result;
}
