import { useQuery } from "@tanstack/react-query";
import { getUser } from "../api/usersApi";

export function useAdminUserDetail(userId: string | undefined) {
  const query = useQuery({
    queryKey: ["admin", "users", userId],
    enabled: Boolean(userId),
    queryFn: ({ signal }) => getUser(userId!, signal),
    staleTime: 60_000,
  });

  return {
    data: query.data,
    loading: query.isFetching,
    error: query.error,
    refetch: query.refetch,
  };
}

