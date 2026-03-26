import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  approveBaselineRegistry,
  approveChangeRequest,
  approveConfigurationItem,
  closeChangeRequest,
  createBaselineRegistry,
  createChangeRequest,
  createConfigurationItem,
  getBaselineRegistry,
  getChangeRequest,
  getConfigurationItem,
  implementChangeRequest,
  listBaselineRegistry,
  listChangeRequests,
  listConfigurationItems,
  rejectChangeRequest,
  submitChangeRequest,
  supersedeBaselineRegistry,
  updateChangeRequest,
  updateConfigurationItem,
} from "../api/changeControlApi";
import type {
  BaselineOverrideInput,
  BaselineRegistryFormInput,
  ChangeControlListInput,
  ChangeRequestFormInput,
  ChangeRequestUpdateInput,
  ConfigurationItemFormInput,
  ConfigurationItemUpdateInput,
} from "../types/changeControl";

export function useChangeRequests(input?: ChangeControlListInput, enabled = true) {
  return useQuery({
    queryKey: ["change-control", "change-requests", input],
    queryFn: ({ signal }) => listChangeRequests(input, signal),
    enabled,
  });
}

export function useChangeRequest(changeRequestId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["change-control", "change-request", changeRequestId],
    queryFn: ({ signal }) => (changeRequestId ? getChangeRequest(changeRequestId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(changeRequestId),
  });
}

export function useConfigurationItems(input?: ChangeControlListInput, enabled = true) {
  return useQuery({
    queryKey: ["change-control", "configuration-items", input],
    queryFn: ({ signal }) => listConfigurationItems(input, signal),
    enabled,
  });
}

export function useConfigurationItem(configurationItemId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["change-control", "configuration-item", configurationItemId],
    queryFn: ({ signal }) => (configurationItemId ? getConfigurationItem(configurationItemId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(configurationItemId),
  });
}

export function useBaselineRegistry(input?: ChangeControlListInput, enabled = true) {
  return useQuery({
    queryKey: ["change-control", "baseline-registry", input],
    queryFn: ({ signal }) => listBaselineRegistry(input, signal),
    enabled,
  });
}

export function useBaselineRegistryItem(baselineRegistryId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["change-control", "baseline-registry-item", baselineRegistryId],
    queryFn: ({ signal }) => (baselineRegistryId ? getBaselineRegistry(baselineRegistryId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(baselineRegistryId),
  });
}

function useInvalidateChangeControl() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: ["change-control"] });
  };
}

export function useCreateChangeRequest() {
  const invalidate = useInvalidateChangeControl();
  return useMutation({
    mutationFn: (input: ChangeRequestFormInput) => createChangeRequest(input),
    onSuccess: invalidate,
  });
}

export function useUpdateChangeRequest() {
  const invalidate = useInvalidateChangeControl();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: ChangeRequestUpdateInput }) => updateChangeRequest(id, input),
    onSuccess: invalidate,
  });
}

export function useChangeRequestActions() {
  const invalidate = useInvalidateChangeControl();
  return {
    submit: useMutation({ mutationFn: (id: string) => submitChangeRequest(id), onSuccess: invalidate }),
    approve: useMutation({ mutationFn: ({ id, reason }: { id: string; reason: string }) => approveChangeRequest(id, reason), onSuccess: invalidate }),
    reject: useMutation({ mutationFn: ({ id, reason }: { id: string; reason: string }) => rejectChangeRequest(id, reason), onSuccess: invalidate }),
    implement: useMutation({ mutationFn: ({ id, summary }: { id: string; summary: string }) => implementChangeRequest(id, summary), onSuccess: invalidate }),
    close: useMutation({ mutationFn: ({ id, summary }: { id: string; summary: string }) => closeChangeRequest(id, summary), onSuccess: invalidate }),
  };
}

export function useCreateConfigurationItem() {
  const invalidate = useInvalidateChangeControl();
  return useMutation({
    mutationFn: (input: ConfigurationItemFormInput) => createConfigurationItem(input),
    onSuccess: invalidate,
  });
}

export function useUpdateConfigurationItem() {
  const invalidate = useInvalidateChangeControl();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: ConfigurationItemUpdateInput }) => updateConfigurationItem(id, input),
    onSuccess: invalidate,
  });
}

export function useApproveConfigurationItem() {
  const invalidate = useInvalidateChangeControl();
  return useMutation({
    mutationFn: (id: string) => approveConfigurationItem(id),
    onSuccess: invalidate,
  });
}

export function useCreateBaselineRegistry() {
  const invalidate = useInvalidateChangeControl();
  return useMutation({
    mutationFn: (input: BaselineRegistryFormInput) => createBaselineRegistry(input),
    onSuccess: invalidate,
  });
}

export function useBaselineRegistryActions() {
  const invalidate = useInvalidateChangeControl();
  return {
    approve: useMutation({ mutationFn: (id: string) => approveBaselineRegistry(id), onSuccess: invalidate }),
    supersede: useMutation({ mutationFn: ({ id, input }: { id: string; input: BaselineOverrideInput }) => supersedeBaselineRegistry(id, input), onSuccess: invalidate }),
  };
}
