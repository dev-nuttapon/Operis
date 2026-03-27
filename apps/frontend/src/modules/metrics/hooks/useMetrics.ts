import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createMetricCollectionSchedule,
  createMetricDefinition,
  createMetricReview,
  createTrendReport,
  evaluateQualityGate,
  getTrendReport,
  listMetricCollectionSchedules,
  listMetricDefinitions,
  listMetricReviews,
  listMetricResults,
  listQualityGates,
  listTrendReports,
  overrideQualityGate,
  updateMetricReview,
  updateMetricDefinition,
  updateTrendReport,
} from "../api/metricsApi";
import type {
  CreateMetricCollectionScheduleInput,
  CreateMetricDefinitionInput,
  CreateMetricReviewInput,
  CreateTrendReportInput,
  EvaluateQualityGateInput,
  MetricCollectionScheduleListInput,
  MetricDefinitionListInput,
  MetricReviewListInput,
  MetricResultListInput,
  OverrideQualityGateInput,
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
