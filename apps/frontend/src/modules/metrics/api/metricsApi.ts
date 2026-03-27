import { apiRequest } from "../../../shared/lib/apiClient";
import type { PaginatedResult } from "../../../shared/types/pagination";
import type {
  AdoptionRuleItem,
  AdoptionRuleListInput,
  AdoptionScorecardItem,
  AdoptionScorecardListInput,
  CreateAdoptionRuleInput,
  CreateMetricCollectionScheduleInput,
  CreateMetricDefinitionInput,
  CreateMetricReviewInput,
  CreatePerformanceBaselineInput,
  CreateCapacityReviewInput,
  CreateSlowOperationReviewInput,
  CreateTrendReportInput,
  EvaluateQualityGateInput,
  EvaluatePerformanceGateInput,
  CapacityReviewItem,
  CapacityReviewListInput,
  MetricCollectionScheduleItem,
  MetricCollectionScheduleListInput,
  MetricDefinitionItem,
  MetricDefinitionListInput,
  MetricReviewItem,
  MetricReviewListInput,
  MetricResultsResponse,
  MetricResultListInput,
  EvaluateAdoptionRulesInput,
  OverrideQualityGateInput,
  OverridePerformanceGateInput,
  PerformanceBaselineItem,
  PerformanceBaselineListInput,
  PerformanceGateItem,
  PerformanceGateListInput,
  QualityGateListInput,
  QualityGateResultItem,
  SlowOperationReviewItem,
  SlowOperationReviewListInput,
  TrendReportItem,
  TrendReportListInput,
  UpdatePerformanceBaselineInput,
  UpdateAdoptionRuleInput,
  UpdateCapacityReviewInput,
  UpdateSlowOperationReviewInput,
  UpdateMetricReviewInput,
  UpdateMetricDefinitionInput,
  UpdateTrendReportInput,
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

export function listMetricReviews(input: MetricReviewListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<MetricReviewItem>>(`/api/v1/metric-reviews${buildQuery({ ...input })}`, { signal });
}

export function createMetricReview(input: CreateMetricReviewInput) {
  return apiRequest<MetricReviewItem>("/api/v1/metric-reviews", { method: "POST", body: input });
}

export function updateMetricReview(metricReviewId: string, input: UpdateMetricReviewInput) {
  return apiRequest<MetricReviewItem>(`/api/v1/metric-reviews/${metricReviewId}`, { method: "PUT", body: input });
}

export function listTrendReports(input: TrendReportListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<TrendReportItem>>(`/api/v1/trend-reports${buildQuery({ ...input })}`, { signal });
}

export function getTrendReport(trendReportId: string, signal?: AbortSignal) {
  return apiRequest<TrendReportItem>(`/api/v1/trend-reports/${trendReportId}`, { signal });
}

export function createTrendReport(input: CreateTrendReportInput) {
  return apiRequest<TrendReportItem>("/api/v1/trend-reports", { method: "POST", body: input });
}

export function updateTrendReport(trendReportId: string, input: UpdateTrendReportInput) {
  return apiRequest<TrendReportItem>(`/api/v1/trend-reports/${trendReportId}`, { method: "PUT", body: input });
}

export function listPerformanceBaselines(input: PerformanceBaselineListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<PerformanceBaselineItem>>(`/api/v1/performance-baselines${buildQuery({ ...input })}`, { signal });
}

export function createPerformanceBaseline(input: CreatePerformanceBaselineInput) {
  return apiRequest("/api/v1/performance-baselines", { method: "POST", body: input });
}

export function updatePerformanceBaseline(performanceBaselineId: string, input: UpdatePerformanceBaselineInput) {
  return apiRequest(`/api/v1/performance-baselines/${performanceBaselineId}`, { method: "PUT", body: input });
}

export function listCapacityReviews(input: CapacityReviewListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<CapacityReviewItem>>(`/api/v1/capacity-reviews${buildQuery({ ...input })}`, { signal });
}

export function createCapacityReview(input: CreateCapacityReviewInput) {
  return apiRequest<CapacityReviewItem>("/api/v1/capacity-reviews", { method: "POST", body: input });
}

export function updateCapacityReview(capacityReviewId: string, input: UpdateCapacityReviewInput) {
  return apiRequest<CapacityReviewItem>(`/api/v1/capacity-reviews/${capacityReviewId}`, { method: "PUT", body: input });
}

export function listSlowOperationReviews(input: SlowOperationReviewListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<SlowOperationReviewItem>>(`/api/v1/slow-operations${buildQuery({ ...input })}`, { signal });
}

export function createSlowOperationReview(input: CreateSlowOperationReviewInput) {
  return apiRequest<SlowOperationReviewItem>("/api/v1/slow-operations", { method: "POST", body: input });
}

export function updateSlowOperationReview(slowOperationReviewId: string, input: UpdateSlowOperationReviewInput) {
  return apiRequest<SlowOperationReviewItem>(`/api/v1/slow-operations/${slowOperationReviewId}`, { method: "PUT", body: input });
}

export function listPerformanceGates(input: PerformanceGateListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<PerformanceGateItem>>(`/api/v1/performance-gates${buildQuery({ ...input })}`, { signal });
}

export function evaluatePerformanceGate(input: EvaluatePerformanceGateInput) {
  return apiRequest<PerformanceGateItem>("/api/v1/performance-gates/evaluate", { method: "POST", body: input });
}

export function overridePerformanceGate(performanceGateId: string, input: OverridePerformanceGateInput) {
  return apiRequest<PerformanceGateItem>(`/api/v1/performance-gates/${performanceGateId}/override`, { method: "PUT", body: input });
}

export function listAdoptionRules(input: AdoptionRuleListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<AdoptionRuleItem>>(`/api/v1/metrics/adoption-rules${buildQuery({ ...input })}`, { signal });
}

export function createAdoptionRule(input: CreateAdoptionRuleInput) {
  return apiRequest<AdoptionRuleItem>("/api/v1/metrics/adoption-rules", { method: "POST", body: input });
}

export function updateAdoptionRule(adoptionRuleId: string, input: UpdateAdoptionRuleInput) {
  return apiRequest<AdoptionRuleItem>(`/api/v1/metrics/adoption-rules/${adoptionRuleId}`, { method: "PUT", body: input });
}

export function evaluateAdoptionRules(input: EvaluateAdoptionRulesInput) {
  return apiRequest<PaginatedResult<AdoptionScorecardItem>>("/api/v1/metrics/adoption-rules/evaluate", { method: "POST", body: input });
}

export function listAdoptionScorecards(input: AdoptionScorecardListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<AdoptionScorecardItem>>(`/api/v1/metrics/adoption-scorecards${buildQuery({ ...input })}`, { signal });
}
