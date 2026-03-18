import { useMemo, useState } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { listUsers } from "../api/usersApi";
import type { User } from "../types/users";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";

type UserOption = { label: string; value: string };

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
      const loaded = allPages.reduce((sum, page) => sum + page.items.length, 0);
      if (lastPage.total && loaded < lastPage.total) {
        return allPages.length + 1;
      }
      return lastPage.items.length === pageSize ? allPages.length + 1 : undefined;
    },
    staleTime: 60_000,
  });

  const options = useMemo(() => {
    const items = usersQuery.data?.pages.flatMap((page) => page.items) ?? [];
    return items.map((item) => ({ label: toLabel(item), value: item.id }));
  }, [toLabel, usersQuery.data]);

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
      options,
      search,
      hasMore,
      loading: usersQuery.isFetching,
      onSearch: handleSearch,
      onLoadMore: loadMore,
    }),
    [hasMore, options, search, usersQuery.isFetching],
  );

  return result;
}
