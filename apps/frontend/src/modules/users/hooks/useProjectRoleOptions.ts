import { useMemo, useState } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { listProjectRoles } from "../api/usersApi";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";

export function useProjectRoleOptions({
  enabled,
  projectId,
  pageSize = 10,
  allowWithoutProjectId = false,
}: {
  enabled: boolean;
  projectId?: string;
  pageSize?: number;
  allowWithoutProjectId?: boolean;
}) {
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search, 300);

  const rolesQuery = useInfiniteQuery({
    queryKey: ["project-role-options", { projectId, search: debouncedSearch, pageSize }],
    enabled: enabled && (allowWithoutProjectId || Boolean(projectId)),
    queryFn: ({ signal, pageParam }) =>
      listProjectRoles(
        {
          page: pageParam as number,
          pageSize,
          search: debouncedSearch,
          sortBy: "displayOrder",
          sortOrder: "asc",
          divisionId: projectId,
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

  const items = useMemo(() => rolesQuery.data?.pages.flatMap((page) => page.items) ?? [], [rolesQuery.data]);
  const options = useMemo(() => items.map((item) => ({ label: item.name, value: item.id })), [items]);
  const hasMore = Boolean(rolesQuery.hasNextPage);

  const handleSearch = (value: string) => {
    setSearch(value);
  };

  const loadMore = () => {
    if (!rolesQuery.isFetching && rolesQuery.hasNextPage) {
      void rolesQuery.fetchNextPage();
    }
  };

  return {
    options,
    items,
    hasMore,
    loading: rolesQuery.isFetching,
    onSearch: handleSearch,
    onLoadMore: loadMore,
    search,
  };
}
