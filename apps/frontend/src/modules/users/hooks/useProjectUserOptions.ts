import { useEffect, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { listUsers } from "../api/usersApi";
import type { User } from "../types/users";

type UserOption = { label: string; value: string };

export function useProjectUserOptions(enabled: boolean, toLabel: (user: User) => string) {
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [buffer, setBuffer] = useState<UserOption[]>([]);
  const [total, setTotal] = useState(0);
  const [visibleCount, setVisibleCount] = useState(5);
  const pageSize = 5;
  const serverPageSize = 10;

  const usersQuery = useQuery({
    queryKey: ["project-user-options", { page }],
    enabled,
    queryFn: ({ signal }) => listUsers({ page, pageSize: serverPageSize, sortBy: "createdAt", sortOrder: "desc" }, signal),
    staleTime: 60_000,
  });

  useEffect(() => {
    if (!usersQuery.data) {
      return;
    }
    const nextOptions = usersQuery.data.items.map((item) => ({ label: toLabel(item), value: item.id }));
    setTotal(usersQuery.data.total ?? Math.max(total, (page - 1) * serverPageSize + nextOptions.length));
    setBuffer((current) => {
      if (page === 1) {
        return nextOptions;
      }
      const existing = new Set(current.map((item) => item.value));
      const merged = [...current];
      nextOptions.forEach((item) => {
        if (!existing.has(item.value)) {
          merged.push(item);
        }
      });
      return merged;
    });
  }, [page, toLabel, usersQuery.data]);

  const filteredBuffer = useMemo(() => {
    const term = search.trim().toLowerCase();
    if (!term) {
      return buffer;
    }
    return buffer.filter((item) => item.label.toLowerCase().includes(term));
  }, [buffer, search]);

  const hasMore = filteredBuffer.length < visibleCount
    ? total > buffer.length
    : filteredBuffer.length > visibleCount || total > buffer.length;

  const handleSearch = (value: string) => {
    setSearch(value);
    setVisibleCount(pageSize);
    if (!value.trim()) {
      return;
    }
    // keep current buffer to allow client-side filtering
  };

  const loadMore = () => {
    if (usersQuery.isFetching) {
      return;
    }
    const nextVisible = visibleCount + pageSize;
    setVisibleCount(nextVisible);
    if (buffer.length < total && nextVisible > buffer.length) {
      setPage((current) => current + 1);
    }
  };

  const result = useMemo(
    () => ({
      options: filteredBuffer.slice(0, visibleCount),
      search,
      hasMore,
      loading: usersQuery.isFetching,
      onSearch: handleSearch,
      onLoadMore: loadMore,
    }),
    [filteredBuffer, hasMore, search, usersQuery.isFetching, visibleCount],
  );

  return result;
}
