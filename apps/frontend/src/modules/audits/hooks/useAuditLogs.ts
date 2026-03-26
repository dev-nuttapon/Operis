import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  closeAuditFinding,
  createAuditFinding,
  createAuditPlan,
  createEvidenceExport,
  getAuditPlan,
  getEvidenceExport,
  listAuditEvents,
  listAuditPlans,
  listEvidenceExports,
  updateAuditFinding,
  updateAuditPlan,
} from "../api/auditsApi";
import type {
  AuditPlanListInput,
  CloseAuditFindingInput,
  CreateAuditFindingInput,
  CreateAuditPlanInput,
  CreateEvidenceExportInput,
  EvidenceExportListInput,
  ListAuditEventsInput,
  UpdateAuditFindingInput,
  UpdateAuditPlanInput,
} from "../types/audits";

export function useAuditEvents(filters: ListAuditEventsInput, enabled = true) {
  return useQuery({
    queryKey: ["audits", "events", filters],
    queryFn: ({ signal }) => listAuditEvents(filters, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useAuditPlans(filters: AuditPlanListInput, enabled = true) {
  return useQuery({
    queryKey: ["audits", "plans", filters],
    queryFn: ({ signal }) => listAuditPlans(filters, signal),
    enabled,
  });
}

export function useAuditPlan(auditPlanId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["audits", "plan", auditPlanId],
    queryFn: ({ signal }) => (auditPlanId ? getAuditPlan(auditPlanId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(auditPlanId),
  });
}

export function useEvidenceExports(filters: EvidenceExportListInput, enabled = true) {
  return useQuery({
    queryKey: ["audits", "exports", filters],
    queryFn: ({ signal }) => listEvidenceExports(filters, signal),
    enabled,
  });
}

export function useEvidenceExport(exportId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["audits", "export", exportId],
    queryFn: ({ signal }) => (exportId ? getEvidenceExport(exportId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(exportId),
  });
}

function useInvalidateAudits() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: ["audits"] });
  };
}

export function useCreateAuditPlan() {
  const invalidate = useInvalidateAudits();
  return useMutation({
    mutationFn: (input: CreateAuditPlanInput) => createAuditPlan(input),
    onSuccess: invalidate,
  });
}

export function useUpdateAuditPlan() {
  const invalidate = useInvalidateAudits();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateAuditPlanInput }) => updateAuditPlan(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateAuditFinding() {
  const invalidate = useInvalidateAudits();
  return useMutation({
    mutationFn: (input: CreateAuditFindingInput) => createAuditFinding(input),
    onSuccess: invalidate,
  });
}

export function useUpdateAuditFinding() {
  const invalidate = useInvalidateAudits();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateAuditFindingInput }) => updateAuditFinding(id, input),
    onSuccess: invalidate,
  });
}

export function useCloseAuditFinding() {
  const invalidate = useInvalidateAudits();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: CloseAuditFindingInput }) => closeAuditFinding(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateEvidenceExport() {
  const invalidate = useInvalidateAudits();
  return useMutation({
    mutationFn: (input: CreateEvidenceExportInput) => createEvidenceExport(input),
    onSuccess: invalidate,
  });
}
