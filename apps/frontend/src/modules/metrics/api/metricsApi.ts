import { apiRequest } from "../../../shared/lib/apiClient";
import type { PaginatedResult } from "../../../shared/types/pagination";
import type {
  CreateMetricCollectionScheduleInput,
  CreateMetricDefinitionInput,
  EvaluateQualityGateInput,
  MetricCollectionScheduleItem,
  MetricCollectionScheduleListInput,
  MetricDefinitionItem,
  MetricDefinitionListInput,
  MetricResultsResponse,
  MetricResultListInput,
  OverrideQualityGateInput,
  QualityGateListInput,
  QualityGateResultItem,
  UpdateMetricDefinitionInput,
} from "../types/metrics";

function buildQuery(input: Record<string, string | number | undefined | null>) {
  const params = new URLSearchParams();
  for (const [key, value] of Object.entries(input)) {
    if (value !== undefined && value !== "") {
      params.set(key, String(value));
    }
  }

  const query = params.toString();
  return query ? `?${query}` : "";
}

export function listMetricDefinitions(input: MetricDefinitionListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<MetricDefinitionItem>>(`/api/v1/metric-definitions${buildQuery({ ...input })}`, { signal });
}

export function createMetricDefinition(input: CreateMetricDefinitionInput) {
  return apiRequest("/api/v1/metric-definitions", { method: "POST", body: input });
}

export function updateMetricDefinition(metricDefinitionId: string, input: UpdateMetricDefinitionInput) {
  return apiRequest(`/api/v1/metric-definitions/${metricDefinitionId}`, { method: "PUT", body: input });
}

export function listMetricCollectionSchedules(input: MetricCollectionScheduleListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<MetricCollectionScheduleItem>>(`/api/v1/metric-collection-schedules${buildQuery({ ...input })}`, { signal });
}

export function createMetricCollectionSchedule(input: CreateMetricCollectionScheduleInput) {
  return apiRequest<MetricCollectionScheduleItem>("/api/v1/metric-collection-schedules", { method: "POST", body: input });
}

export function listMetricResults(input: MetricResultListInput, signal?: AbortSignal) {
  return apiRequest<MetricResultsResponse>(`/api/v1/metric-results${buildQuery({ ...input })}`, { signal });
}

export function listQualityGates(input: QualityGateListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<QualityGateResultItem>>(`/api/v1/quality-gates${buildQuery({ ...input })}`, { signal });
}

export function evaluateQualityGate(input: EvaluateQualityGateInput) {
  return apiRequest<QualityGateResultItem>("/api/v1/quality-gates/evaluate", { method: "POST", body: input });
}

export function overrideQualityGate(qualityGateId: string, input: OverrideQualityGateInput) {
  return apiRequest(`/api/v1/quality-gates/${qualityGateId}/override`, { method: "PUT", body: input });
}
