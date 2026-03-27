import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createWaiver, getWaiver, listWaivers, transitionWaiver, updateWaiver } from "../api/exceptionsApi";
import type { CreateWaiverInput, TransitionWaiverInput, UpdateWaiverInput, WaiverListInput } from "../types/exceptions";

export function useWaivers(input: WaiverListInput, enabled = true) {
  return useQuery({
    queryKey: ["exceptions", "waivers", input],
    queryFn: ({ signal }) => listWaivers(input, signal),
    enabled,
  });
}

export function useWaiver(waiverId: string | undefined, enabled = true) {
  return useQuery({
    queryKey: ["exceptions", "waivers", waiverId],
    queryFn: ({ signal }) => getWaiver(waiverId!, signal),
    enabled: enabled && Boolean(waiverId),
  });
}

function useInvalidateExceptions() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: ["exceptions"] });
  };
}

export function useCreateWaiver() {
  const invalidate = useInvalidateExceptions();
  return useMutation({
    mutationFn: (input: CreateWaiverInput) => createWaiver(input),
    onSuccess: invalidate,
  });
}

export function useUpdateWaiver() {
  const invalidate = useInvalidateExceptions();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateWaiverInput }) => updateWaiver(id, input),
    onSuccess: invalidate,
  });
}

export function useTransitionWaiver() {
  const invalidate = useInvalidateExceptions();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: TransitionWaiverInput }) => transitionWaiver(id, input),
    onSuccess: invalidate,
  });
}
