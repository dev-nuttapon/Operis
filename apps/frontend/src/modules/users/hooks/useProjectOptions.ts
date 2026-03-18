import { useMemo, useState } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { listProjects } from "../api/usersApi";
import type { Project } from "../types/users";

type ProjectOption = { label: string; value: string };

export function useProjectOptions({
  enabled,
  assignedOnly,
  pageSize = 10,
}: {
  enabled: boolean;
  assignedOnly?: boolean;
  pageSize?: number;
}) {
  const [search, setSearch] = useState("");

  const projectsQuery = useInfiniteQuery({
    queryKey: ["project-options", { search, assignedOnly, pageSize }],
    enabled,
    queryFn: ({ signal, pageParam }) =>
      listProjects(
        {
          page: pageParam as number,
          pageSize,
          search,
          sortBy: "name",
          sortOrder: "asc",
          assignedOnly,
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

  const items = useMemo(() => projectsQuery.data?.pages.flatMap((page) => page.items) ?? [], [projectsQuery.data]);
  const itemsById = useMemo(() => {
    const map = new Map<string, Project>();
    for (const item of items) {
      map.set(item.id, item);
    }
    return map;
  }, [items]);
  const options: ProjectOption[] = useMemo(
    () => items.map((item) => ({ label: `${item.code} - ${item.name}`, value: item.id })),
    [items],
  );

  const hasMore = Boolean(projectsQuery.hasNextPage);

  const handleSearch = (value: string) => {
    setSearch(value);
  };

  const loadMore = () => {
    if (!projectsQuery.isFetching && projectsQuery.hasNextPage) {
      void projectsQuery.fetchNextPage();
    }
  };

  return {
    options,
    items,
    itemsById,
    hasMore,
    loading: projectsQuery.isFetching,
    onSearch: handleSearch,
    onLoadMore: loadMore,
    search,
  };
}
