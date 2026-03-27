import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  addAccessRecertificationDecision,
  addCapaAction,
  completeAccessRecertification,
  closeCapa,
  createCapaEffectivenessReview,
  createAccessRecertification,
  approveAccessReview,
  createBackupEvidence,
  createAutomationJob,
  createCapaRecord,
  createClassificationPolicy,
  createAccessReview,
  createConfigurationAudit,
  createDrDrill,
  createEscalationEvent,
  createExternalDependency,
  createLegalHold,
  createPrivilegedAccessEvent,
  createRestoreVerification,
  createSecretRotation,
  createSecurityIncident,
  createSecurityReview,
  createSupplier,
  createSupplierAgreement,
  createVulnerability,
  getAccessRecertification,
  getAutomationJob,
  getCapaRecord,
  getSecurityIncident,
  getSupplier,
  listAccessRecertifications,
  listAccessReviews,
  listAutomationJobRuns,
  listAutomationJobs,
  listBackupEvidence,
  listCapaEffectivenessReviews,
  listCapaRecords,
  listClassificationPolicies,
  listConfigurationAudits,
  listDrDrills,
  listEscalationEvents,
  listExternalDependencies,
  listLegalHolds,
  listPrivilegedAccessEvents,
  listRestoreVerifications,
  listSecretRotations,
  listSecurityIncidents,
  listSecurityReviews,
  listSupplierAgreements,
  listSuppliers,
  listVulnerabilities,
  releaseLegalHold,
  reopenCapa,
  transitionAutomationJob,
  verifyCapa,
  updateClassificationPolicy,
  updateAccessRecertification,
  updateAccessReview,
  updateAutomationJob,
  updateCapaRecord,
  updateDrDrill,
  updateExternalDependency,
  updatePrivilegedAccessEvent,
  updateSecretRotation,
  updateSecurityIncident,
  updateSupplier,
  updateSupplierAgreement,
  updateSecurityReview,
  updateVulnerability,
  executeAutomationJob,
} from "../api/operationsApi";
import type {
  AddAccessRecertificationDecisionInput,
  ApproveAccessReviewInput,
  CloseCapaInput,
  CreateAutomationJobInput,
  CreateBackupEvidenceInput,
  CreateCapaActionInput,
  CreateCapaEffectivenessReviewInput,
  CreateCapaRecordInput,
  CreateAccessRecertificationInput,
  CreateAccessReviewInput,
  CreateClassificationPolicyInput,
  CreateConfigurationAuditInput,
  CreateDrDrillInput,
  CreateEscalationEventInput,
  CreateExternalDependencyInput,
  CreateLegalHoldInput,
  CreatePrivilegedAccessEventInput,
  CreateRestoreVerificationInput,
  CreateSecretRotationInput,
  CreateSecurityIncidentInput,
  CreateSecurityReviewInput,
  CreateSupplierAgreementInput,
  CreateSupplierInput,
  CreateVulnerabilityInput,
  OperationsListInput,
  ReleaseLegalHoldInput,
  ReopenCapaInput,
  TransitionAutomationJobInput,
  UpdateDrDrillInput,
  UpdateClassificationPolicyInput,
  UpdateAccessRecertificationInput,
  UpdateAccessReviewInput,
  UpdateAutomationJobInput,
  UpdateCapaRecordInput,
  UpdateExternalDependencyInput,
  UpdatePrivilegedAccessEventInput,
  UpdateSecretRotationInput,
  UpdateSecurityIncidentInput,
  UpdateSupplierAgreementInput,
  UpdateSupplierInput,
  UpdateSecurityReviewInput,
  UpdateVulnerabilityInput,
  VerifyCapaInput,
  ExecuteAutomationJobInput,
} from "../types/operations";

export function useAccessReviews(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "access-reviews", input],
    queryFn: ({ signal }) => listAccessReviews(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useAutomationJobs(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "automation-jobs", input],
    queryFn: ({ signal }) => listAutomationJobs(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useAutomationJob(id: string | undefined, enabled = true) {
  return useQuery({
    queryKey: ["operations", "automation-jobs", id],
    queryFn: ({ signal }) => getAutomationJob(id!, signal),
    staleTime: 15_000,
    enabled: enabled && Boolean(id),
  });
}

export function useAutomationJobRuns(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "automation-job-runs", input],
    queryFn: ({ signal }) => listAutomationJobRuns(input, signal),
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

export function useBackupEvidence(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "backup-evidence", input],
    queryFn: ({ signal }) => listBackupEvidence(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useRestoreVerifications(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "restore-verifications", input],
    queryFn: ({ signal }) => listRestoreVerifications(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useDrDrills(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "dr-drills", input],
    queryFn: ({ signal }) => listDrDrills(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useLegalHolds(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "legal-holds", input],
    queryFn: ({ signal }) => listLegalHolds(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useCapaRecords(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "capa", input],
    queryFn: ({ signal }) => listCapaRecords(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useCapaRecord(id: string | undefined, enabled = true) {
  return useQuery({
    queryKey: ["operations", "capa", id],
    queryFn: ({ signal }) => getCapaRecord(id!, signal),
    staleTime: 15_000,
    enabled: enabled && Boolean(id),
  });
}

export function useCapaEffectivenessReviews(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "capa-effectiveness", input],
    queryFn: ({ signal }) => listCapaEffectivenessReviews(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useEscalationEvents(input: OperationsListInput, enabled = true) {
  return useQuery({
    queryKey: ["operations", "escalations", input],
    queryFn: ({ signal }) => listEscalationEvents(input, signal),
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

export function useCreateAutomationJob() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: CreateAutomationJobInput) => createAutomationJob(input),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["operations", "automation-jobs"] });
    },
  });
}

export function useUpdateAutomationJob() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateAutomationJobInput }) => updateAutomationJob(id, input),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["operations", "automation-jobs"] });
    },
  });
}

export function useTransitionAutomationJob() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: TransitionAutomationJobInput }) => transitionAutomationJob(id, input),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["operations", "automation-jobs"] });
      await queryClient.invalidateQueries({ queryKey: ["operations", "automation-job-runs"] });
    },
  });
}

export function useExecuteAutomationJob() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: ExecuteAutomationJobInput }) => executeAutomationJob(id, input),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["operations", "automation-jobs"] });
      await queryClient.invalidateQueries({ queryKey: ["operations", "automation-job-runs"] });
    },
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

export function useCreateBackupEvidence() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateBackupEvidenceInput) => createBackupEvidence(input),
    onSuccess: invalidate,
  });
}

export function useCreateRestoreVerification() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateRestoreVerificationInput) => createRestoreVerification(input),
    onSuccess: invalidate,
  });
}

export function useCreateDrDrill() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateDrDrillInput) => createDrDrill(input),
    onSuccess: invalidate,
  });
}

export function useUpdateDrDrill() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateDrDrillInput }) => updateDrDrill(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateLegalHold() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateLegalHoldInput) => createLegalHold(input),
    onSuccess: invalidate,
  });
}

export function useReleaseLegalHold() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: ReleaseLegalHoldInput }) => releaseLegalHold(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateCapaRecord() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateCapaRecordInput) => createCapaRecord(input),
    onSuccess: invalidate,
  });
}

export function useUpdateCapaRecord() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateCapaRecordInput }) => updateCapaRecord(id, input),
    onSuccess: invalidate,
  });
}

export function useAddCapaAction() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: CreateCapaActionInput }) => addCapaAction(id, input),
    onSuccess: invalidate,
  });
}

export function useVerifyCapa() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: VerifyCapaInput }) => verifyCapa(id, input),
    onSuccess: invalidate,
  });
}

export function useCloseCapa() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: CloseCapaInput }) => closeCapa(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateCapaEffectivenessReview() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateCapaEffectivenessReviewInput) => createCapaEffectivenessReview(input),
    onSuccess: invalidate,
  });
}

export function useReopenCapa() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: ReopenCapaInput }) => reopenCapa(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateEscalationEvent() {
  const invalidate = useInvalidateOperations();
  return useMutation({
    mutationFn: (input: CreateEscalationEventInput) => createEscalationEvent(input),
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
