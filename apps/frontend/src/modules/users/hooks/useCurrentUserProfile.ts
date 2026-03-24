import { useQuery } from "@tanstack/react-query";
import { getCurrentUser } from "../api/usersApi";

interface CurrentUserProfileOptions {
  includeIdentity?: boolean;
}

export function useCurrentUserProfile(options?: CurrentUserProfileOptions) {
  const includeIdentity = Boolean(options?.includeIdentity);

  return useQuery({
    queryKey: ["current-user-profile", includeIdentity],
    queryFn: ({ signal }) => getCurrentUser(signal, includeIdentity),
    staleTime: 60_000,
  });
}
