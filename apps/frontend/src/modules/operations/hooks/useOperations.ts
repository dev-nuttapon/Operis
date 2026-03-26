import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  approveAccessReview,
  createAccessReview,
  createConfigurationAudit,
  createExternalDependency,
  createSecurityReview,
  listAccessReviews,
  listConfigurationAudits,
  listExternalDependencies,
  listSecurityReviews,
  updateAccessReview,
  updateExternalDependency,
  updateSecurityReview,
} from "../api/operationsApi";
import type {
  ApproveAccessReviewInput,
  CreateAccessReviewInput,
  CreateConfigurationAuditInput,
  CreateExternalDependencyInput,
  CreateSecurityReviewInput,
  OperationsListInput,
  UpdateAccessReviewInput,
  UpdateExternalDependencyInput,
  UpdateSecurityReviewInput,
} from "../types/operations";

export function useAccessReviews(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "access-reviews", input],
    queryFn: ({ signal }) => listAccessReviews(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useSecurityReviews(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "security-reviews", input],
    queryFn: ({ signal }) => listSecurityReviews(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useExternalDependencies(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "external-dependencies", input],
    queryFn: ({ signal }) => listExternalDependencies(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useConfigurationAudits(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "configuration-audits", input],
    queryFn: ({ signal }) => listConfigurationAudits(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

function useInvalidateOperations() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: ["operations"] });
  };
}

export function useCreateAccessReview() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateAccessReviewInput) => createAccessReview(input),
    onSuccess: invalidate,
  });
}

export function useUpdateAccessReview() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateAccessReviewInput }) => updateAccessReview(id, input),
    onSuccess: invalidate,
  });
}

export function useApproveAccessReview() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: ApproveAccessReviewInput }) => approveAccessReview(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateSecurityReview() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateSecurityReviewInput) => createSecurityReview(input),
    onSuccess: invalidate,
  });
}

export function useUpdateSecurityReview() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateSecurityReviewInput }) => updateSecurityReview(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateExternalDependency() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateExternalDependencyInput) => createExternalDependency(input),
    onSuccess: invalidate,
  });
}

export function useUpdateExternalDependency() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateExternalDependencyInput }) => updateExternalDependency(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateConfigurationAudit() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateConfigurationAuditInput) => createConfigurationAudit(input),
    onSuccess: invalidate,
  });
}
