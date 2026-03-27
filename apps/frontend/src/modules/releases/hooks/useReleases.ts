import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  approveRelease,
  createDeploymentChecklist,
  createRelease,
  createReleaseNote,
  executeRelease,
  getRelease,
  listDeploymentChecklists,
  listReleaseNotes,
  listReleases,
  publishReleaseNote,
  updateDeploymentChecklist,
  updateRelease,
} from "../api/releasesApi";
import type {
  ApproveReleaseInput,
  DeploymentChecklistFormInput,
  DeploymentChecklistUpdateInput,
  ExecuteReleaseInput,
  ReleaseFormInput,
  ReleaseListInput,
  ReleaseNoteFormInput,
  ReleaseUpdateInput,
} from "../types/releases";

export function useReleases(input?: ReleaseListInput, enabled = true) {
  return useQuery({
    queryKey: ["releases", "list", input],
    queryFn: ({ signal }) => listReleases(input, signal),
    enabled,
  });
}

export function useRelease(releaseId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["releases", "detail", releaseId],
    queryFn: ({ signal }) => (releaseId ? getRelease(releaseId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(releaseId),
  });
}

export function useDeploymentChecklists(input?: ReleaseListInput, enabled = true) {
  return useQuery({
    queryKey: ["releases", "checklists", input],
    queryFn: ({ signal }) => listDeploymentChecklists(input, signal),
    enabled,
  });
}

export function useReleaseNotes(input?: ReleaseListInput, enabled = true) {
  return useQuery({
    queryKey: ["releases", "notes", input],
    queryFn: ({ signal }) => listReleaseNotes(input, signal),
    enabled,
  });
}

function useInvalidateReleases() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: ["releases"] });
  };
}

export function useCreateRelease() {
  const invalidate = useInvalidateReleases();
  return useMutation({
    mutationFn: (input: ReleaseFormInput) => createRelease(input),
    onSuccess: invalidate,
  });
}

export function useUpdateRelease() {
  const invalidate = useInvalidateReleases();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: ReleaseUpdateInput }) => updateRelease(id, input),
    onSuccess: invalidate,
  });
}

export function useReleaseActions() {
  const invalidate = useInvalidateReleases();
  return {
    approve: useMutation({
      mutationFn: ({ id, input }: { id: string; input: ApproveReleaseInput }) => approveRelease(id, input),
      onSuccess: invalidate,
    }),
    execute: useMutation({
      mutationFn: ({ id, input }: { id: string; input: ExecuteReleaseInput }) => executeRelease(id, input),
      onSuccess: invalidate,
    }),
  };
}

export function useCreateDeploymentChecklist() {
  const invalidate = useInvalidateReleases();
  return useMutation({
    mutationFn: (input: DeploymentChecklistFormInput) => createDeploymentChecklist(input),
    onSuccess: invalidate,
  });
}

export function useUpdateDeploymentChecklist() {
  const invalidate = useInvalidateReleases();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: DeploymentChecklistUpdateInput }) => updateDeploymentChecklist(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateReleaseNote() {
  const invalidate = useInvalidateReleases();
  return useMutation({
    mutationFn: (input: ReleaseNoteFormInput) => createReleaseNote(input),
    onSuccess: invalidate,
  });
}

export function usePublishReleaseNote() {
  const invalidate = useInvalidateReleases();
  return useMutation({
    mutationFn: (id: string) => publishReleaseNote(id),
    onSuccess: invalidate,
  });
}
