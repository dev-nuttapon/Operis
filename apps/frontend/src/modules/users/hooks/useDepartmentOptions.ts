import { useMemo, useState } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { listDepartments, listPublicDepartmentsByDivision } from "../api/usersApi";

export function useDepartmentOptions({
  enabled,
  divisionId,
  pageSize = 10,
  publicAccess = false,
}: {
  enabled: boolean;
  divisionId?: string;
  pageSize?: number;
  publicAccess?: boolean;
}) {
  const [search, setSearch] = useState("");

  const departmentsQuery = useInfiniteQuery({
    queryKey: ["department-options", { divisionId, publicAccess, search, pageSize }],
    enabled: enabled && Boolean(divisionId),
    queryFn: ({ signal, pageParam }) => {
      const input = {
        page: pageParam as number,
        pageSize,
        search,
        sortBy: "displayOrder",
        sortOrder: "asc",
        divisionId,
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
      return lastPage.items.length === pageSize ? allPages.length + 1 : undefined;
    },
    staleTime: 60_000,
  });

  const items = useMemo(() => departmentsQuery.data?.pages.flatMap((page) => page.items) ?? [], [departmentsQuery.data]);
  const options = useMemo(() => items.map((item) => ({ label: item.name, value: item.id })), [items]);
  const hasMore = Boolean(departmentsQuery.hasNextPage);

  const handleSearch = (value: string) => {
    setSearch(value);
  };

  const loadMore = () => {
    if (!departmentsQuery.isFetching && departmentsQuery.hasNextPage) {
      void departmentsQuery.fetchNextPage();
    }
  };

  return {
    options,
    items,
    hasMore,
    loading: departmentsQuery.isFetching,
    onSearch: handleSearch,
    onLoadMore: loadMore,
    search,
  };
}
