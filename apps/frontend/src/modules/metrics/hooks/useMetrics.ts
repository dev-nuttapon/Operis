import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createMetricCollectionSchedule,
  createMetricDefinition,
  evaluateQualityGate,
  listMetricCollectionSchedules,
  listMetricDefinitions,
  listMetricResults,
  listQualityGates,
  overrideQualityGate,
  updateMetricDefinition,
} from "../api/metricsApi";
import type {
  CreateMetricCollectionScheduleInput,
  CreateMetricDefinitionInput,
  EvaluateQualityGateInput,
  MetricCollectionScheduleListInput,
  MetricDefinitionListInput,
  MetricResultListInput,
  OverrideQualityGateInput,
  QualityGateListInput,
  UpdateMetricDefinitionInput,
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
