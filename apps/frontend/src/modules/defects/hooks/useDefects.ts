import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  closeDefect,
  closeNonConformance,
  createDefect,
  createNonConformance,
  getDefect,
  getNonConformance,
  listDefects,
  listNonConformances,
  resolveDefect,
  updateDefect,
  updateNonConformance,
} from "../api/defectsApi";
import type {
  CloseDefectInput,
  CloseNonConformanceInput,
  DefectFormInput,
  DefectListInput,
  DefectUpdateInput,
  NonConformanceFormInput,
  NonConformanceListInput,
  NonConformanceUpdateInput,
  ResolveDefectInput,
} from "../types/defects";

export function useDefects(input?: DefectListInput, enabled = true) {
  return useQuery({
    queryKey: ["defects", "list", input],
    queryFn: ({ signal }) => listDefects(input, signal),
    enabled,
  });
}

export function useDefect(defectId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["defects", "detail", defectId],
    queryFn: ({ signal }) => (defectId ? getDefect(defectId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(defectId),
  });
}

export function useNonConformances(input?: NonConformanceListInput, enabled = true) {
  return useQuery({
    queryKey: ["defects", "non-conformances", input],
    queryFn: ({ signal }) => listNonConformances(input, signal),
    enabled,
  });
}

export function useNonConformance(nonConformanceId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["defects", "non-conformance", nonConformanceId],
    queryFn: ({ signal }) => (nonConformanceId ? getNonConformance(nonConformanceId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(nonConformanceId),
  });
}

function useInvalidateDefects() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: ["defects"] });
  };
}

export function useCreateDefect() {
  const invalidate = useInvalidateDefects();
  return useMutation({ mutationFn: (input: DefectFormInput) => createDefect(input), onSuccess: invalidate });
}

export function useUpdateDefect() {
  const invalidate = useInvalidateDefects();
  return useMutation({ mutationFn: ({ id, input }: { id: string; input: DefectUpdateInput }) => updateDefect(id, input), onSuccess: invalidate });
}

export function useResolveDefect() {
  const invalidate = useInvalidateDefects();
  return useMutation({ mutationFn: ({ id, input }: { id: string; input: ResolveDefectInput }) => resolveDefect(id, input), onSuccess: invalidate });
}

export function useCloseDefect() {
  const invalidate = useInvalidateDefects();
  return useMutation({ mutationFn: ({ id, input }: { id: string; input: CloseDefectInput }) => closeDefect(id, input), onSuccess: invalidate });
}

export function useCreateNonConformance() {
  const invalidate = useInvalidateDefects();
  return useMutation({ mutationFn: (input: NonConformanceFormInput) => createNonConformance(input), onSuccess: invalidate });
}

export function useUpdateNonConformance() {
  const invalidate = useInvalidateDefects();
  return useMutation({ mutationFn: ({ id, input }: { id: string; input: NonConformanceUpdateInput }) => updateNonConformance(id, input), onSuccess: invalidate });
}

export function useCloseNonConformance() {
  const invalidate = useInvalidateDefects();
  return useMutation({ mutationFn: ({ id, input }: { id: string; input: CloseNonConformanceInput }) => closeNonConformance(id, input), onSuccess: invalidate });
}
