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
