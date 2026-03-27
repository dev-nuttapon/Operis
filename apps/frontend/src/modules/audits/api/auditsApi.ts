import { apiRequest } from "../../../shared/lib/apiClient";
import type { PaginatedResult } from "../../../shared/types/pagination";
import type {
  AuditEventItem,
  AuditPlanDetail,
  AuditPlanListInput,
  AuditPlanListItem,
  CloseAuditFindingInput,
  CreateAuditFindingInput,
  CreateAuditPlanInput,
  CreateEvidenceRuleInput,
  CreateEvidenceExportInput,
  EvaluateEvidenceRulesInput,
  EvidenceRuleDetail,
  EvidenceRuleListInput,
  EvidenceRuleListItem,
  EvidenceRuleResultDetail,
  EvidenceRuleResultListInput,
  EvidenceRuleResultListItem,
  EvidenceExportDetail,
  EvidenceExportItem,
  EvidenceExportListInput,
  ListAuditEventsInput,
  UpdateEvidenceRuleInput,
  UpdateAuditFindingInput,
  UpdateAuditPlanInput,
} from "../types/audits";

export function listAuditEvents(input: ListAuditEventsInput, signal?: AbortSignal) {
  const params = new URLSearchParams();

  if (input.projectId) params.set("projectId", input.projectId);
  if (input.entityType) params.set("entityType", input.entityType);
  if (input.action) params.set("action", input.action);
  if (input.actorUserId) params.set("actorUserId", input.actorUserId);
  if (input.outcome) params.set("outcome", input.outcome);
  if (input.from) params.set("from", input.from);
  if (input.to) params.set("to", input.to);
  if (input.page) params.set("page", String(input.page));
  if (input.pageSize) params.set("pageSize", String(input.pageSize));

  const query = params.toString();
  return apiRequest<PaginatedResult<AuditEventItem>>(`/api/v1/audit-events${query ? `?${query}` : ""}`, { signal });
}

export function listAuditPlans(input: AuditPlanListInput, signal?: AbortSignal) {
  const params = new URLSearchParams();

  if (input.projectId) params.set("projectId", input.projectId);
  if (input.status) params.set("status", input.status);
  if (input.ownerUserId) params.set("ownerUserId", input.ownerUserId);
  if (input.page) params.set("page", String(input.page));
  if (input.pageSize) params.set("pageSize", String(input.pageSize));

  const query = params.toString();
  return apiRequest<PaginatedResult<AuditPlanListItem>>(`/api/v1/audit-plans${query ? `?${query}` : ""}`, { signal });
}

export function getAuditPlan(auditPlanId: string, signal?: AbortSignal) {
  return apiRequest<AuditPlanDetail>(`/api/v1/audit-plans/${auditPlanId}`, { signal });
}

export function createAuditPlan(input: CreateAuditPlanInput) {
  return apiRequest<AuditPlanDetail>("/api/v1/audit-plans", { method: "POST", body: input });
}

export function updateAuditPlan(auditPlanId: string, input: UpdateAuditPlanInput) {
  return apiRequest<AuditPlanDetail>(`/api/v1/audit-plans/${auditPlanId}`, { method: "PUT", body: input });
}

export function createAuditFinding(input: CreateAuditFindingInput) {
  return apiRequest("/api/v1/audit-findings", { method: "POST", body: input });
}

export function updateAuditFinding(auditFindingId: string, input: UpdateAuditFindingInput) {
  return apiRequest(`/api/v1/audit-findings/${auditFindingId}`, { method: "PUT", body: input });
}

export function closeAuditFinding(auditFindingId: string, input: CloseAuditFindingInput) {
  return apiRequest(`/api/v1/audit-findings/${auditFindingId}/close`, { method: "PUT", body: input });
}

export function listEvidenceExports(input: EvidenceExportListInput, signal?: AbortSignal) {
  const params = new URLSearchParams();

  if (input.scopeType) params.set("scopeType", input.scopeType);
  if (input.status) params.set("status", input.status);
  if (input.requestedBy) params.set("requestedBy", input.requestedBy);
  if (input.page) params.set("page", String(input.page));
  if (input.pageSize) params.set("pageSize", String(input.pageSize));

  const query = params.toString();
  return apiRequest<PaginatedResult<EvidenceExportItem>>(`/api/v1/evidence-exports${query ? `?${query}` : ""}`, { signal });
}

export function getEvidenceExport(exportId: string, signal?: AbortSignal) {
  return apiRequest<EvidenceExportDetail>(`/api/v1/evidence-exports/${exportId}`, { signal });
}

export function createEvidenceExport(input: CreateEvidenceExportInput) {
  return apiRequest<EvidenceExportDetail>("/api/v1/evidence-exports", { method: "POST", body: input });
}

export function listEvidenceRules(input: EvidenceRuleListInput, signal?: AbortSignal) {
  const params = new URLSearchParams();

  if (input.search) params.set("search", input.search);
  if (input.status) params.set("status", input.status);
  if (input.processArea) params.set("processArea", input.processArea);
  if (input.artifactType) params.set("artifactType", input.artifactType);
  if (input.projectId) params.set("projectId", input.projectId);
  if (input.page) params.set("page", String(input.page));
  if (input.pageSize) params.set("pageSize", String(input.pageSize));

  const query = params.toString();
  return apiRequest<PaginatedResult<EvidenceRuleListItem>>(`/api/v1/audits/evidence-rules${query ? `?${query}` : ""}`, { signal });
}

export function createEvidenceRule(input: CreateEvidenceRuleInput) {
  return apiRequest<EvidenceRuleDetail>("/api/v1/audits/evidence-rules", { method: "POST", body: input });
}

export function updateEvidenceRule(ruleId: string, input: UpdateEvidenceRuleInput) {
  return apiRequest<EvidenceRuleDetail>(`/api/v1/audits/evidence-rules/${ruleId}`, { method: "PUT", body: input });
}

export function evaluateEvidenceRules(input: EvaluateEvidenceRulesInput) {
  return apiRequest<EvidenceRuleResultDetail>("/api/v1/audits/evidence-rules/evaluate", { method: "POST", body: input });
}

export function listEvidenceRuleResults(input: EvidenceRuleResultListInput, signal?: AbortSignal) {
  const params = new URLSearchParams();

  if (input.scopeType) params.set("scopeType", input.scopeType);
  if (input.status) params.set("status", input.status);
  if (input.processArea) params.set("processArea", input.processArea);
  if (input.projectId) params.set("projectId", input.projectId);
  if (input.page) params.set("page", String(input.page));
  if (input.pageSize) params.set("pageSize", String(input.pageSize));

  const query = params.toString();
  return apiRequest<PaginatedResult<EvidenceRuleResultListItem>>(`/api/v1/audits/evidence-results${query ? `?${query}` : ""}`, { signal });
}

export function getEvidenceRuleResult(resultId: string, signal?: AbortSignal) {
  return apiRequest<EvidenceRuleResultDetail>(`/api/v1/audits/evidence-results/${resultId}`, { signal });
}
