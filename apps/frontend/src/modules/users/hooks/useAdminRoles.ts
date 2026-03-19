import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { listRoles } from "../api/usersApi";

export function useAdminRoles({ enabled }: { enabled: boolean }) {
  const rolesQuery = useQuery({
    queryKey: ["admin", "roles"],
    enabled,
    queryFn: ({ signal }) => listRoles(signal),
    staleTime: 5 * 60_000,
  });

  const roles = rolesQuery.data ?? [];
  const options = useMemo(
    () =>
      roles
        .slice()
        .sort((a, b) => a.displayOrder - b.displayOrder || a.name.localeCompare(b.name))
        .map((item) => ({
          value: item.id,
          label: item.name,
          description: item.description,
        })),
    [roles],
  );

  return {
    roles,
    options,
    loading: rolesQuery.isFetching,
  };
}

