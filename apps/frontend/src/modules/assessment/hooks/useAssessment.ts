import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createAssessmentFinding,
  createAssessmentNote,
  createAssessmentPackage,
  createControlCatalogItem,
  createControlMapping,
  getAssessmentFinding,
  getAssessmentPackage,
  getControlCatalogItem,
  getControlMapping,
  listAssessmentFindings,
  listAssessmentPackages,
  listControlCatalog,
  listControlCoverage,
  listControlMappings,
  transitionControlMapping,
  transitionAssessmentFinding,
  transitionAssessmentPackage,
  updateControlCatalogItem,
} from "../api/assessmentApi";
import type {
  AssessmentFindingListInput,
  AssessmentPackageListInput,
  ControlCatalogListInput,
  ControlCoverageListInput,
  ControlMappingListInput,
  CreateControlCatalogItemInput,
  CreateControlMappingInput,
  CreateAssessmentFindingInput,
  CreateAssessmentNoteInput,
  CreateAssessmentPackageInput,
  TransitionControlMappingInput,
  TransitionAssessmentFindingInput,
  TransitionAssessmentPackageInput,
  UpdateControlCatalogItemInput,
} from "../types/assessment";

export function useAssessmentPackages(input: AssessmentPackageListInput, enabled = true) {
  return useQuery({
    queryKey: ["assessment", "packages", input],
    queryFn: ({ signal }) => listAssessmentPackages(input, signal),
    enabled,
  });
}

export function useAssessmentPackage(packageId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["assessment", "packages", packageId],
    queryFn: ({ signal }) => (packageId ? getAssessmentPackage(packageId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(packageId),
  });
}

export function useAssessmentFindings(input: AssessmentFindingListInput, enabled = true) {
  return useQuery({
    queryKey: ["assessment", "findings", input],
    queryFn: ({ signal }) => listAssessmentFindings(input, signal),
    enabled,
  });
}

export function useAssessmentFinding(findingId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["assessment", "finding", findingId],
    queryFn: ({ signal }) => (findingId ? getAssessmentFinding(findingId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(findingId),
  });
}

export function useControlCatalog(input: ControlCatalogListInput, enabled = true) {
  return useQuery({
    queryKey: ["assessment", "controls", "catalog", input],
    queryFn: ({ signal }) => listControlCatalog(input, signal),
    enabled,
  });
}

export function useControlCatalogItem(controlId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["assessment", "controls", "catalog", controlId],
    queryFn: ({ signal }) => (controlId ? getControlCatalogItem(controlId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(controlId),
  });
}

export function useControlMappings(input: ControlMappingListInput, enabled = true) {
  return useQuery({
    queryKey: ["assessment", "controls", "mappings", input],
    queryFn: ({ signal }) => listControlMappings(input, signal),
    enabled,
  });
}

export function useControlMapping(mappingId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["assessment", "controls", "mapping", mappingId],
    queryFn: ({ signal }) => (mappingId ? getControlMapping(mappingId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(mappingId),
  });
}

export function useControlCoverage(input: ControlCoverageListInput, enabled = true) {
  return useQuery({
    queryKey: ["assessment", "controls", "coverage", input],
    queryFn: ({ signal }) => listControlCoverage(input, signal),
    enabled,
  });
}

function useInvalidateAssessment() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: ["assessment"] });
  };
}

export function useCreateAssessmentPackage() {
  const invalidate = useInvalidateAssessment();
  return useMutation({
    mutationFn: (input: CreateAssessmentPackageInput) => createAssessmentPackage(input),
    onSuccess: invalidate,
  });
}

export function useTransitionAssessmentPackage() {
  const invalidate = useInvalidateAssessment();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: TransitionAssessmentPackageInput }) => transitionAssessmentPackage(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateAssessmentNote() {
  const invalidate = useInvalidateAssessment();
  return useMutation({
    mutationFn: ({ packageId, input }: { packageId: string; input: CreateAssessmentNoteInput }) => createAssessmentNote(packageId, input),
    onSuccess: invalidate,
  });
}

export function useCreateAssessmentFinding() {
  const invalidate = useInvalidateAssessment();
  return useMutation({
    mutationFn: (input: CreateAssessmentFindingInput) => createAssessmentFinding(input),
    onSuccess: invalidate,
  });
}

export function useTransitionAssessmentFinding() {
  const invalidate = useInvalidateAssessment();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: TransitionAssessmentFindingInput }) => transitionAssessmentFinding(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateControlCatalogItem() {
  const invalidate = useInvalidateAssessment();
  return useMutation({
    mutationFn: (input: CreateControlCatalogItemInput) => createControlCatalogItem(input),
    onSuccess: invalidate,
  });
}

export function useUpdateControlCatalogItem() {
  const invalidate = useInvalidateAssessment();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateControlCatalogItemInput }) => updateControlCatalogItem(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateControlMapping() {
  const invalidate = useInvalidateAssessment();
  return useMutation({
    mutationFn: (input: CreateControlMappingInput) => createControlMapping(input),
    onSuccess: invalidate,
  });
}

export function useTransitionControlMapping() {
  const invalidate = useInvalidateAssessment();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: TransitionControlMappingInput }) => transitionControlMapping(id, input),
    onSuccess: invalidate,
  });
}
