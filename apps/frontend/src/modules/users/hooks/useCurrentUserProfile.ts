import { useQuery } from "@tanstack/react-query";
import { getCurrentUser } from "../api/usersApi";

export function useCurrentUserProfile() {
  return useQuery({
    queryKey: ["current-user-profile"],
    queryFn: ({ signal }) => getCurrentUser(signal, false),
    staleTime: 60_000,
  });
}
