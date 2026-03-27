import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  closeAuditFinding,
  createAuditFinding,
  createAuditPlan,
  createEvidenceRule,
  createEvidenceExport,
  evaluateEvidenceRules,
  getAuditPlan,
  getEvidenceRuleResult,
  getEvidenceExport,
  listAuditEvents,
  listAuditPlans,
  listEvidenceRuleResults,
  listEvidenceRules,
  listEvidenceExports,
  updateEvidenceRule,
  updateAuditFinding,
  updateAuditPlan,
} from "../api/auditsApi";
import type {
  AuditPlanListInput,
  CloseAuditFindingInput,
  CreateAuditFindingInput,
  CreateAuditPlanInput,
  CreateEvidenceRuleInput,
  CreateEvidenceExportInput,
  EvaluateEvidenceRulesInput,
  EvidenceRuleListInput,
  EvidenceRuleResultListInput,
  EvidenceExportListInput,
  ListAuditEventsInput,
  UpdateEvidenceRuleInput,
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

export function useEvidenceRules(filters: EvidenceRuleListInput, enabled = true) {
  return useQuery({
    queryKey: ["audits", "evidence-rules", filters],
    queryFn: ({ signal }) => listEvidenceRules(filters, signal),
    enabled,
  });
}

export function useEvidenceRuleResults(filters: EvidenceRuleResultListInput, enabled = true) {
  return useQuery({
    queryKey: ["audits", "evidence-results", filters],
    queryFn: ({ signal }) => listEvidenceRuleResults(filters, signal),
    enabled,
  });
}

export function useEvidenceRuleResult(resultId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["audits", "evidence-result", resultId],
    queryFn: ({ signal }) => (resultId ? getEvidenceRuleResult(resultId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(resultId),
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

export function useCreateEvidenceRule() {
  const invalidate = useInvalidateAudits();
  return useMutation({
    mutationFn: (input: CreateEvidenceRuleInput) => createEvidenceRule(input),
    onSuccess: invalidate,
  });
}

export function useUpdateEvidenceRule() {
  const invalidate = useInvalidateAudits();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateEvidenceRuleInput }) => updateEvidenceRule(id, input),
    onSuccess: invalidate,
  });
}

export function useEvaluateEvidenceRules() {
  const invalidate = useInvalidateAudits();
  return useMutation({
    mutationFn: (input: EvaluateEvidenceRulesInput) => evaluateEvidenceRules(input),
    onSuccess: invalidate,
  });
}
