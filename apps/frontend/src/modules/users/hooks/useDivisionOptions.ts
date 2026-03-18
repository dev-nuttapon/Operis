import { useMemo, useState } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { listDivisions } from "../api/usersApi";

export function useDivisionOptions({ enabled, pageSize = 10 }: { enabled: boolean; pageSize?: number }) {
  const [search, setSearch] = useState("");

  const divisionsQuery = useInfiniteQuery({
    queryKey: ["division-options", { search, pageSize }],
    enabled,
    queryFn: ({ signal, pageParam }) =>
      listDivisions(
        {
          page: pageParam as number,
          pageSize,
          search,
          sortBy: "displayOrder",
          sortOrder: "asc",
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

  const items = useMemo(() => divisionsQuery.data?.pages.flatMap((page) => page.items) ?? [], [divisionsQuery.data]);
  const itemsById = useMemo(() => new Map(items.map((item) => [item.id, item])), [items]);
  const options = useMemo(() => items.map((item) => ({ label: item.name, value: item.id })), [items]);
  const hasMore = Boolean(divisionsQuery.hasNextPage);

  const handleSearch = (value: string) => {
    setSearch(value);
  };

  const loadMore = () => {
    if (!divisionsQuery.isFetching && divisionsQuery.hasNextPage) {
      void divisionsQuery.fetchNextPage();
    }
  };

  return {
    options,
    items,
    itemsById,
    hasMore,
    loading: divisionsQuery.isFetching,
    onSearch: handleSearch,
    onLoadMore: loadMore,
    search,
  };
}
