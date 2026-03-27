import type { PaginationInput } from "../../../shared/types/pagination";

export interface MetricDefinitionListInput extends PaginationInput {
  search?: string;
  metricType?: string;
  status?: string;
  ownerUserId?: string;
}

export interface MetricDefinitionItem {
  id: string;
  code: string;
  name: string;
  metricType: string;
  ownerUserId: string;
  targetValue: number;
  thresholdValue: number;
  status: string;
  updatedAt: string;
}

export interface MetricCollectionScheduleListInput extends PaginationInput {
  metricDefinitionId?: string;
  status?: string;
  collectorType?: string;
}

export interface MetricCollectionScheduleItem {
  id: string;
  metricDefinitionId: string;
  metricCode: string;
  metricName: string;
  collectionFrequency: string;
  collectorType: string;
  nextRunAt: string;
  status: string;
  updatedAt: string;
}

export interface MetricResultListInput extends PaginationInput {
  metricDefinitionId?: string;
  projectId?: string;
  status?: string;
  gateType?: string;
  from?: string;
  to?: string;
}

export interface MetricResultItem {
  id: string;
  metricDefinitionId: string;
  metricCode: string;
  metricName: string;
  metricType: string;
  measuredAt: string;
  measuredValue: number;
  targetValue: number;
  thresholdValue: number;
  status: string;
  sourceRef: string;
  qualityGateResultId?: string | null;
}

export interface MetricTrendPoint {
  metricDefinitionId: string;
  metricCode: string;
  measuredAt: string;
  measuredValue: number;
  status: string;
}

export interface MetricCurrentVsTargetItem {
  metricDefinitionId: string;
  metricCode: string;
  metricName: string;
  currentValue: number;
  targetValue: number;
  thresholdValue: number;
  status: string;
}

export interface MetricsDashboardSummary {
  breachCount: number;
  openActions: number;
  trend: MetricTrendPoint[];
  currentVsTarget: MetricCurrentVsTargetItem[];
}

export interface MetricResultsResponse {
  items: MetricResultItem[];
  total: number;
  page: number;
  pageSize: number;
  summary: MetricsDashboardSummary;
}

export interface QualityGateListInput extends PaginationInput {
  projectId?: string;
  gateType?: string;
  result?: string;
}

export interface QualityGateResultItem {
  id: string;
  projectId: string;
  projectName: string;
  gateType: string;
  evaluatedAt: string;
  result: string;
  reason?: string | null;
  overrideReason?: string | null;
  evaluatedByUserId?: string | null;
  overriddenByUserId?: string | null;
  metrics: MetricResultItem[];
}

export interface MetricReviewListInput extends PaginationInput {
  projectId?: string;
  status?: string;
  reviewedBy?: string;
  search?: string;
}

export interface MetricReviewItem {
  id: string;
  projectId: string;
  projectName: string;
  reviewPeriod: string;
  reviewedBy: string;
  status: string;
  summary?: string | null;
  openActionCount: number;
  updatedAt: string;
}

export interface TrendReportListInput extends PaginationInput {
  projectId?: string;
  metricDefinitionId?: string;
  status?: string;
  periodFrom?: string;
  periodTo?: string;
  search?: string;
}

export interface TrendReportItem {
  id: string;
  projectId: string;
  projectName: string;
  metricDefinitionId: string;
  metricCode: string;
  metricName: string;
  periodFrom: string;
  periodTo: string;
  status: string;
  reportRef?: string | null;
  trendDirection?: string | null;
  variance?: number | null;
  recommendedAction?: string | null;
  updatedAt: string;
}

export interface PerformanceBaselineListInput extends PaginationInput {
  scopeType?: string;
  metricName?: string;
  status?: string;
  search?: string;
}

export interface PerformanceBaselineItem {
  id: string;
  scopeType: string;
  scopeRef: string;
  metricName: string;
  targetValue: number;
  thresholdValue: number;
  status: string;
  updatedAt: string;
}

export interface CapacityReviewListInput extends PaginationInput {
  scopeRef?: string;
  status?: string;
  reviewedBy?: string;
  search?: string;
}

export interface CapacityReviewItem {
  id: string;
  scopeRef: string;
  reviewPeriod: string;
  reviewedBy: string;
  status: string;
  summary: string;
  actionCount: number;
  updatedAt: string;
}

export interface SlowOperationReviewListInput extends PaginationInput {
  operationType?: string;
  ownerUserId?: string;
  status?: string;
  search?: string;
}

export interface SlowOperationReviewItem {
  id: string;
  operationType: string;
  operationKey: string;
  observedLatencyMs: number;
  frequencyPerHour?: number | null;
  status: string;
  ownerUserId: string;
  optimizationSummary?: string | null;
  updatedAt: string;
}

export interface PerformanceGateListInput extends PaginationInput {
  scopeRef?: string;
  result?: string;
  search?: string;
}

export interface PerformanceGateItem {
  id: string;
  scopeRef: string;
  evaluatedAt: string;
  result: string;
  reason?: string | null;
  overrideReason?: string | null;
  evidenceRef?: string | null;
  evaluatedByUserId?: string | null;
  overriddenByUserId?: string | null;
}

export interface AdoptionRuleListInput extends PaginationInput {
  processArea?: string;
  scopeType?: string;
  status?: string;
  search?: string;
}

export interface AdoptionRuleItem {
  id: string;
  ruleCode: string;
  processArea: string;
  scopeType: string;
  thresholdPercentage: number;
  status: string;
  updatedAt: string;
}

export interface AdoptionAnomalyItem {
  id: string;
  projectId: string;
  projectName: string;
  adoptionRuleId: string;
  ruleCode: string;
  processArea: string;
  severity: string;
  summary: string;
  status: string;
  detectedAt: string;
}

export interface AdoptionScorecardListInput extends PaginationInput {
  projectId?: string;
  processArea?: string;
  scopeType?: string;
  scoreState?: string;
  search?: string;
}

export interface AdoptionScorecardItem {
  id: string;
  projectId: string;
  projectName: string;
  adoptionRuleId: string;
  ruleCode: string;
  processArea: string;
  scopeType: string;
  thresholdPercentage: number;
  scorePercentage: number;
  scoreState: string;
  evidenceCount: number;
  expectedCount: number;
  calculatedAt: string;
  anomalies: AdoptionAnomalyItem[];
}

export interface CreateMetricDefinitionInput {
  code: string;
  name: string;
  metricType: string;
  ownerUserId: string;
  targetValue?: number | null;
  thresholdValue?: number | null;
}

export interface UpdateMetricDefinitionInput {
  name: string;
  metricType: string;
  ownerUserId: string;
  targetValue?: number | null;
  thresholdValue?: number | null;
  status: string;
}

export interface CreateMetricCollectionScheduleInput {
  metricDefinitionId: string;
  collectionFrequency: string;
  collectorType: string;
  status?: string;
}

export interface EvaluateQualityGateMetricInput {
  metricDefinitionId: string;
  measuredValue: number;
  measuredAt?: string | null;
  sourceRef: string;
}

export interface EvaluateQualityGateInput {
  projectId: string;
  gateType: string;
  reason?: string | null;
  metricInputs: EvaluateQualityGateMetricInput[];
}

export interface OverrideQualityGateInput {
  reason: string;
}

export interface CreateMetricReviewInput {
  projectId: string;
  reviewPeriod: string;
  reviewedBy: string;
  summary?: string | null;
  openActionCount?: number;
}

export interface UpdateMetricReviewInput extends CreateMetricReviewInput {
  status: string;
}

export interface CreateTrendReportInput {
  projectId: string;
  metricDefinitionId?: string | null;
  periodFrom?: string | null;
  periodTo?: string | null;
  status: string;
  reportRef?: string | null;
  trendDirection?: string | null;
  variance?: number | null;
  recommendedAction?: string | null;
}

export interface UpdateTrendReportInput extends CreateTrendReportInput {}

export interface CreatePerformanceBaselineInput {
  scopeType: string;
  scopeRef: string;
  metricName: string;
  targetValue?: number | null;
  thresholdValue?: number | null;
}

export interface UpdatePerformanceBaselineInput extends CreatePerformanceBaselineInput {
  status: string;
}

export interface CreateCapacityReviewInput {
  scopeRef: string;
  reviewPeriod: string;
  reviewedBy: string;
  summary: string;
  actionCount?: number;
}

export interface UpdateCapacityReviewInput extends CreateCapacityReviewInput {
  status: string;
}

export interface CreateSlowOperationReviewInput {
  operationType: string;
  operationKey: string;
  observedLatencyMs: number;
  frequencyPerHour?: number | null;
  ownerUserId: string;
  optimizationSummary?: string | null;
  status?: string;
}

export interface UpdateSlowOperationReviewInput extends CreateSlowOperationReviewInput {
  status: string;
}

export interface EvaluatePerformanceGateInput {
  scopeRef: string;
  result: string;
  reason?: string | null;
  evidenceRef?: string | null;
}

export interface OverridePerformanceGateInput {
  reason: string;
}

export interface CreateAdoptionRuleInput {
  ruleCode: string;
  processArea: string;
  scopeType: string;
  thresholdPercentage?: number | null;
  status?: string;
}

export interface UpdateAdoptionRuleInput {
  processArea: string;
  scopeType: string;
  thresholdPercentage?: number | null;
  status: string;
}

export interface EvaluateAdoptionRulesInput {
  projectId?: string | null;
  scopeType?: string | null;
  processArea?: string | null;
}
