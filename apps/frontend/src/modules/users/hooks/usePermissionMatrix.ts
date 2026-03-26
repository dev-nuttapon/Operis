import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { applyPermissionMatrix, getPermissionMatrix } from "../api/usersApi";
import type { ApplyPermissionMatrixInput } from "../types/users";

const permissionMatrixQueryKey = ["admin", "permission-matrix"];

export function usePermissionMatrix() {
  return useQuery({
    queryKey: permissionMatrixQueryKey,
    queryFn: ({ signal }) => getPermissionMatrix(signal),
    staleTime: 15_000,
  });
}

export function useApplyPermissionMatrix() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: ApplyPermissionMatrixInput) => applyPermissionMatrix(input),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: permissionMatrixQueryKey });
    },
  });
}
