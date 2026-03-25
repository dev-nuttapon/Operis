import { useMutation } from "@tanstack/react-query";
import { refreshJobTitlesCache } from "../api/usersApi";

export function useRefreshJobTitlesCache() {
  return useMutation({
    mutationFn: () => refreshJobTitlesCache(),
  });
}
