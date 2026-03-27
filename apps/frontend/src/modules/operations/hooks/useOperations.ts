import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  addAccessRecertificationDecision,
  completeAccessRecertification,
  createAccessRecertification,
  approveAccessReview,
  createClassificationPolicy,
  createAccessReview,
  createConfigurationAudit,
  createExternalDependency,
  createPrivilegedAccessEvent,
  createSecretRotation,
  createSecurityIncident,
  createSecurityReview,
  createSupplier,
  createSupplierAgreement,
  createVulnerability,
  getAccessRecertification,
  getSecurityIncident,
  getSupplier,
  listAccessRecertifications,
  listAccessReviews,
  listClassificationPolicies,
  listConfigurationAudits,
  listExternalDependencies,
  listPrivilegedAccessEvents,
  listSecretRotations,
  listSecurityIncidents,
  listSecurityReviews,
  listSupplierAgreements,
  listSuppliers,
  listVulnerabilities,
  updateClassificationPolicy,
  updateAccessRecertification,
  updateAccessReview,
  updateExternalDependency,
  updatePrivilegedAccessEvent,
  updateSecretRotation,
  updateSecurityIncident,
  updateSupplier,
  updateSupplierAgreement,
  updateSecurityReview,
  updateVulnerability,
} from "../api/operationsApi";
import type {
  AddAccessRecertificationDecisionInput,
  ApproveAccessReviewInput,
  CreateAccessRecertificationInput,
  CreateAccessReviewInput,
  CreateClassificationPolicyInput,
  CreateConfigurationAuditInput,
  CreateExternalDependencyInput,
  CreatePrivilegedAccessEventInput,
  CreateSecretRotationInput,
  CreateSecurityIncidentInput,
  CreateSecurityReviewInput,
  CreateSupplierAgreementInput,
  CreateSupplierInput,
  CreateVulnerabilityInput,
  OperationsListInput,
  UpdateClassificationPolicyInput,
  UpdateAccessRecertificationInput,
  UpdateAccessReviewInput,
  UpdateExternalDependencyInput,
  UpdatePrivilegedAccessEventInput,
  UpdateSecretRotationInput,
  UpdateSecurityIncidentInput,
  UpdateSupplierAgreementInput,
  UpdateSupplierInput,
  UpdateSecurityReviewInput,
  UpdateVulnerabilityInput,
} from "../types/operations";

export function useAccessReviews(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "access-reviews", input],
    queryFn: ({ signal }) => listAccessReviews(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useAccessRecertifications(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "access-recertifications", input],
    queryFn: ({ signal }) => listAccessRecertifications(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useAccessRecertification(id: string | undefined, enabled = true) {
  return useQuery({
    queryKey: ["operations", "access-recertifications", id],
    queryFn: ({ signal }) => getAccessRecertification(id!, signal),
    staleTime: 15_000,
    enabled: enabled && Boolean(id),
  });
}

export function useSecurityIncidents(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "security-incidents", input],
    queryFn: ({ signal }) => listSecurityIncidents(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useSecurityIncident(id: string | undefined, enabled = true) {
  return useQuery({
    queryKey: ["operations", "security-incidents", id],
    queryFn: ({ signal }) => getSecurityIncident(id!, signal),
    staleTime: 15_000,
    enabled: enabled && Boolean(id),
  });
}

export function useVulnerabilities(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "vulnerabilities", input],
    queryFn: ({ signal }) => listVulnerabilities(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useSecretRotations(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "secret-rotations", input],
    queryFn: ({ signal }) => listSecretRotations(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function usePrivilegedAccessEvents(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "privileged-access-events", input],
    queryFn: ({ signal }) => listPrivilegedAccessEvents(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useClassificationPolicies(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "classification-policies", input],
    queryFn: ({ signal }) => listClassificationPolicies(input, signal),
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

export function useSuppliers(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "suppliers", input],
    queryFn: ({ signal }) => listSuppliers(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useSupplier(id: string | undefined, enabled = true) {
  return useQuery({
    queryKey: ["operations", "suppliers", id],
    queryFn: ({ signal }) => getSupplier(id!, signal),
    staleTime: 15_000,
    enabled: enabled && Boolean(id),
  });
}

export function useSupplierAgreements(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "supplier-agreements", input],
    queryFn: ({ signal }) => listSupplierAgreements(input, signal),
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

export function useCreateAccessRecertification() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateAccessRecertificationInput) => createAccessRecertification(input),
    onSuccess: invalidate,
  });
}

export function useUpdateAccessRecertification() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateAccessRecertificationInput }) => updateAccessRecertification(id, input),
    onSuccess: invalidate,
  });
}

export function useAddAccessRecertificationDecision() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: AddAccessRecertificationDecisionInput }) => addAccessRecertificationDecision(id, input),
    onSuccess: invalidate,
  });
}

export function useCompleteAccessRecertification() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (id: string) => completeAccessRecertification(id),
    onSuccess: invalidate,
  });
}

export function useCreateSecurityIncident() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateSecurityIncidentInput) => createSecurityIncident(input),
    onSuccess: invalidate,
  });
}

export function useUpdateSecurityIncident() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateSecurityIncidentInput }) => updateSecurityIncident(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateVulnerability() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateVulnerabilityInput) => createVulnerability(input),
    onSuccess: invalidate,
  });
}

export function useUpdateVulnerability() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateVulnerabilityInput }) => updateVulnerability(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateSecretRotation() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateSecretRotationInput) => createSecretRotation(input),
    onSuccess: invalidate,
  });
}

export function useUpdateSecretRotation() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateSecretRotationInput }) => updateSecretRotation(id, input),
    onSuccess: invalidate,
  });
}

export function useCreatePrivilegedAccessEvent() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreatePrivilegedAccessEventInput) => createPrivilegedAccessEvent(input),
    onSuccess: invalidate,
  });
}

export function useUpdatePrivilegedAccessEvent() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdatePrivilegedAccessEventInput }) => updatePrivilegedAccessEvent(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateClassificationPolicy() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateClassificationPolicyInput) => createClassificationPolicy(input),
    onSuccess: invalidate,
  });
}

export function useUpdateClassificationPolicy() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateClassificationPolicyInput }) => updateClassificationPolicy(id, input),
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

export function useCreateSupplier() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateSupplierInput) => createSupplier(input),
    onSuccess: invalidate,
  });
}

export function useUpdateSupplier() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateSupplierInput }) => updateSupplier(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateSupplierAgreement() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateSupplierAgreementInput) => createSupplierAgreement(input),
    onSuccess: invalidate,
  });
}

export function useUpdateSupplierAgreement() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateSupplierAgreementInput }) => updateSupplierAgreement(id, input),
    onSuccess: invalidate,
  });
}
