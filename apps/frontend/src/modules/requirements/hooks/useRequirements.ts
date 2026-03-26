import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  approveRequirement,
  baselineRequirement,
  createRequirement,
  createRequirementBaseline,
  createTraceabilityLink,
  deleteTraceabilityLink,
  getRequirement,
  listRequirementBaselines,
  listRequirements,
  listTraceabilityMatrix,
  submitRequirement,
  supersedeRequirement,
  updateRequirement,
} from "../api/requirementsApi";
import type {
  RequirementBaselineInput,
  RequirementFormInput,
  RequirementListInput,
  RequirementUpdateInput,
  TraceabilityLinkInput,
  TraceabilityListInput,
} from "../types/requirements";

export function useRequirements(input?: RequirementListInput, enabled = true) {
  return useQuery({
    queryKey: ["requirements", "list", input],
    queryFn: ({ signal }) => listRequirements(input, signal),
    enabled,
  });
}

export function useRequirement(requirementId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["requirements", "detail", requirementId],
    queryFn: ({ signal }) => (requirementId ? getRequirement(requirementId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(requirementId),
  });
}

export function useRequirementBaselines(projectId?: string, status?: string, page = 1, pageSize = 10, enabled = true) {
  return useQuery({
    queryKey: ["requirements", "baselines", { projectId, status, page, pageSize }],
    queryFn: ({ signal }) => listRequirementBaselines(projectId, status, page, pageSize, signal),
    enabled,
  });
}

export function useTraceabilityMatrix(input?: TraceabilityListInput, enabled = true) {
  return useQuery({
    queryKey: ["requirements", "traceability", input],
    queryFn: ({ signal }) => listTraceabilityMatrix(input, signal),
    enabled,
  });
}

function useInvalidateRequirements() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: ["requirements"] });
  };
}

export function useCreateRequirement() {
  const invalidate = useInvalidateRequirements();
  return useMutation({
    mutationFn: (input: RequirementFormInput) => createRequirement(input),
    onSuccess: invalidate,
  });
}

export function useUpdateRequirement() {
  const invalidate = useInvalidateRequirements();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: RequirementUpdateInput }) => updateRequirement(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateRequirementBaseline() {
  const invalidate = useInvalidateRequirements();
  return useMutation({
    mutationFn: (input: RequirementBaselineInput) => createRequirementBaseline(input),
    onSuccess: invalidate,
  });
}

export function useCreateTraceabilityLink() {
  const invalidate = useInvalidateRequirements();
  return useMutation({
    mutationFn: (input: TraceabilityLinkInput) => createTraceabilityLink(input),
    onSuccess: invalidate,
  });
}

export function useDeleteTraceabilityLink() {
  const invalidate = useInvalidateRequirements();
  return useMutation({
    mutationFn: (linkId: string) => deleteTraceabilityLink(linkId),
    onSuccess: invalidate,
  });
}

export function useRequirementActions() {
  const invalidate = useInvalidateRequirements();
  return {
    submit: useMutation({
      mutationFn: (requirementId: string) => submitRequirement(requirementId),
      onSuccess: invalidate,
    }),
    approve: useMutation({
      mutationFn: ({ requirementId, reason }: { requirementId: string; reason: string }) => approveRequirement(requirementId, reason),
      onSuccess: invalidate,
    }),
    baseline: useMutation({
      mutationFn: (requirementId: string) => baselineRequirement(requirementId),
      onSuccess: invalidate,
    }),
    supersede: useMutation({
      mutationFn: ({ requirementId, reason }: { requirementId: string; reason: string }) => supersedeRequirement(requirementId, reason),
      onSuccess: invalidate,
    }),
  };
}
