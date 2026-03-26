import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getSystemSettings, updateSystemSettings } from "../api/usersApi";
import type { UpdateSystemSettingsInput } from "../types/users";

const systemSettingsQueryKey = ["admin", "system-settings"];

export function useSystemSettings() {
  return useQuery({
    queryKey: systemSettingsQueryKey,
    queryFn: ({ signal }) => getSystemSettings(signal),
    staleTime: 15_000,
  });
}

export function useUpdateSystemSettings() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: UpdateSystemSettingsInput) => updateSystemSettings(input),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: systemSettingsQueryKey });
    },
  });
}
