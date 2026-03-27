import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createAdoptionRule,
  createMetricCollectionSchedule,
  createMetricDefinition,
  createMetricReview,
  createPerformanceBaseline,
  createCapacityReview,
  createSlowOperationReview,
  createTrendReport,
  evaluateAdoptionRules,
  evaluateQualityGate,
  evaluatePerformanceGate,
  getTrendReport,
  listAdoptionRules,
  listAdoptionScorecards,
  listCapacityReviews,
  listMetricCollectionSchedules,
  listMetricDefinitions,
  listMetricReviews,
  listMetricResults,
  listPerformanceBaselines,
  listPerformanceGates,
  listQualityGates,
  listSlowOperationReviews,
  listTrendReports,
  overridePerformanceGate,
  overrideQualityGate,
  updateAdoptionRule,
  updatePerformanceBaseline,
  updateCapacityReview,
  updateSlowOperationReview,
  updateMetricReview,
  updateMetricDefinition,
  updateTrendReport,
} from "../api/metricsApi";
import type {
  AdoptionRuleListInput,
  AdoptionScorecardListInput,
  CreateAdoptionRuleInput,
  CapacityReviewListInput,
  CreateMetricCollectionScheduleInput,
  CreateMetricDefinitionInput,
  CreateMetricReviewInput,
  CreatePerformanceBaselineInput,
  CreateCapacityReviewInput,
  CreateSlowOperationReviewInput,
  CreateTrendReportInput,
  EvaluateQualityGateInput,
  EvaluatePerformanceGateInput,
  EvaluateAdoptionRulesInput,
  PerformanceBaselineListInput,
  PerformanceGateListInput,
  MetricCollectionScheduleListInput,
  MetricDefinitionListInput,
  MetricReviewListInput,
  MetricResultListInput,
  OverrideQualityGateInput,
  OverridePerformanceGateInput,
  SlowOperationReviewListInput,
  UpdatePerformanceBaselineInput,
  UpdateAdoptionRuleInput,
  UpdateCapacityReviewInput,
  UpdateSlowOperationReviewInput,
  QualityGateListInput,
  TrendReportListInput,
  UpdateMetricReviewInput,
  UpdateMetricDefinitionInput,
  UpdateTrendReportInput,
} from "../types/metrics";

export function useMetricDefinitions(input: MetricDefinitionListInput, enabled = true) {
  return useQuery({
    queryKey: ["metrics", "definitions", input],
    queryFn: ({ signal }) => listMetricDefinitions(input, signal),
    enabled,
  });
}

export function useMetricCollectionSchedules(input: MetricCollectionScheduleListInput, enabled = true) {
  return useQuery({
    queryKey: ["metrics", "schedules", input],
    queryFn: ({ signal }) => listMetricCollectionSchedules(input, signal),
    enabled,
  });
}

export function useMetricResults(input: MetricResultListInput, enabled = true) {
  return useQuery({
    queryKey: ["metrics", "results", input],
    queryFn: ({ signal }) => listMetricResults(input, signal),
    enabled,
  });
}

export function useQualityGates(input: QualityGateListInput, enabled = true) {
  return useQuery({
    queryKey: ["metrics", "quality-gates", input],
    queryFn: ({ signal }) => listQualityGates(input, signal),
    enabled,
  });
}

export function useMetricReviews(input: MetricReviewListInput, enabled = true) {
  return useQuery({
    queryKey: ["metrics", "reviews", input],
    queryFn: ({ signal }) => listMetricReviews(input, signal),
    enabled,
  });
}

export function useTrendReports(input: TrendReportListInput, enabled = true) {
  return useQuery({
    queryKey: ["metrics", "trend-reports", input],
    queryFn: ({ signal }) => listTrendReports(input, signal),
    enabled,
  });
}

export function useTrendReport(id: string | undefined, enabled = true) {
  return useQuery({
    queryKey: ["metrics", "trend-reports", id],
    queryFn: ({ signal }) => getTrendReport(id!, signal),
    enabled: enabled && Boolean(id),
  });
}

export function usePerformanceBaselines(input: PerformanceBaselineListInput, enabled = true) {
  return useQuery({
    queryKey: ["metrics", "performance-baselines", input],
    queryFn: ({ signal }) => listPerformanceBaselines(input, signal),
    enabled,
  });
}

export function useCapacityReviews(input: CapacityReviewListInput, enabled = true) {
  return useQuery({
    queryKey: ["metrics", "capacity-reviews", input],
    queryFn: ({ signal }) => listCapacityReviews(input, signal),
    enabled,
  });
}

export function useSlowOperationReviews(input: SlowOperationReviewListInput, enabled = true) {
  return useQuery({
    queryKey: ["metrics", "slow-operations", input],
    queryFn: ({ signal }) => listSlowOperationReviews(input, signal),
    enabled,
  });
}

export function usePerformanceGates(input: PerformanceGateListInput, enabled = true) {
  return useQuery({
    queryKey: ["metrics", "performance-gates", input],
    queryFn: ({ signal }) => listPerformanceGates(input, signal),
    enabled,
  });
}

export function useAdoptionRules(input: AdoptionRuleListInput, enabled = true) {
  return useQuery({
    queryKey: ["metrics", "adoption-rules", input],
    queryFn: ({ signal }) => listAdoptionRules(input, signal),
    enabled,
  });
}

export function useAdoptionScorecards(input: AdoptionScorecardListInput, enabled = true) {
  return useQuery({
    queryKey: ["metrics", "adoption-scorecards", input],
    queryFn: ({ signal }) => listAdoptionScorecards(input, signal),
    enabled,
  });
}

function useInvalidateMetrics() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: ["metrics"] });
  };
}

export function useCreateMetricDefinition() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: (input: CreateMetricDefinitionInput) => createMetricDefinition(input),
    onSuccess: invalidate,
  });
}

export function useUpdateMetricDefinition() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateMetricDefinitionInput }) => updateMetricDefinition(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateMetricCollectionSchedule() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: (input: CreateMetricCollectionScheduleInput) => createMetricCollectionSchedule(input),
    onSuccess: invalidate,
  });
}

export function useEvaluateQualityGate() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: (input: EvaluateQualityGateInput) => evaluateQualityGate(input),
    onSuccess: invalidate,
  });
}

export function useOverrideQualityGate() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: OverrideQualityGateInput }) => overrideQualityGate(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateMetricReview() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: (input: CreateMetricReviewInput) => createMetricReview(input),
    onSuccess: invalidate,
  });
}

export function useUpdateMetricReview() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateMetricReviewInput }) => updateMetricReview(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateTrendReport() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: (input: CreateTrendReportInput) => createTrendReport(input),
    onSuccess: invalidate,
  });
}

export function useUpdateTrendReport() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateTrendReportInput }) => updateTrendReport(id, input),
    onSuccess: invalidate,
  });
}

export function useCreatePerformanceBaseline() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: (input: CreatePerformanceBaselineInput) => createPerformanceBaseline(input),
    onSuccess: invalidate,
  });
}

export function useUpdatePerformanceBaseline() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdatePerformanceBaselineInput }) => updatePerformanceBaseline(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateCapacityReview() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: (input: CreateCapacityReviewInput) => createCapacityReview(input),
    onSuccess: invalidate,
  });
}

export function useUpdateCapacityReview() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateCapacityReviewInput }) => updateCapacityReview(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateSlowOperationReview() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: (input: CreateSlowOperationReviewInput) => createSlowOperationReview(input),
    onSuccess: invalidate,
  });
}

export function useUpdateSlowOperationReview() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateSlowOperationReviewInput }) => updateSlowOperationReview(id, input),
    onSuccess: invalidate,
  });
}

export function useEvaluatePerformanceGate() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: (input: EvaluatePerformanceGateInput) => evaluatePerformanceGate(input),
    onSuccess: invalidate,
  });
}

export function useOverridePerformanceGate() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: OverridePerformanceGateInput }) => overridePerformanceGate(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateAdoptionRule() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: (input: CreateAdoptionRuleInput) => createAdoptionRule(input),
    onSuccess: invalidate,
  });
}

export function useUpdateAdoptionRule() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateAdoptionRuleInput }) => updateAdoptionRule(id, input),
    onSuccess: invalidate,
  });
}

export function useEvaluateAdoptionRules() {
  const invalidate = useInvalidateMetrics();
  return useMutation({
    mutationFn: (input: EvaluateAdoptionRulesInput) => evaluateAdoptionRules(input),
    onSuccess: invalidate,
  });
}
