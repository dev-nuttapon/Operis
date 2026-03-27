import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createCompetencyReview,
  createRoleTrainingRequirement,
  createTrainingCourse,
  listCompetencyReviews,
  listProjectRoleOptions,
  listRoleTrainingRequirements,
  listTrainingCompletions,
  listTrainingCourses,
  recordTrainingCompletion,
  transitionTrainingCourse,
  updateCompetencyReview,
  updateRoleTrainingRequirement,
  updateTrainingCompletion,
  updateTrainingCourse,
} from "../api/learningApi";
import type {
  CompetencyReviewListInput,
  CreateCompetencyReviewInput,
  CreateRoleTrainingRequirementInput,
  CreateTrainingCourseInput,
  RecordTrainingCompletionInput,
  RoleTrainingMatrixInput,
  TrainingCompletionListInput,
  TrainingCourseListInput,
  TransitionTrainingCourseInput,
  UpdateCompetencyReviewInput,
  UpdateRoleTrainingRequirementInput,
  UpdateTrainingCompletionInput,
  UpdateTrainingCourseInput,
} from "../types/learning";

function useInvalidateLearning() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: ["learning"] });
  };
}

export function useTrainingCourses(input: TrainingCourseListInput, enabled = true) {
  return useQuery({
    queryKey: ["learning", "courses", input],
    queryFn: ({ signal }) => listTrainingCourses(input, signal),
    enabled,
  });
}

export function useRoleTrainingRequirements(input: RoleTrainingMatrixInput, enabled = true) {
  return useQuery({
    queryKey: ["learning", "role-matrix", input],
    queryFn: ({ signal }) => listRoleTrainingRequirements(input, signal),
    enabled,
  });
}

export function useTrainingCompletions(input: TrainingCompletionListInput, enabled = true) {
  return useQuery({
    queryKey: ["learning", "completions", input],
    queryFn: ({ signal }) => listTrainingCompletions(input, signal),
    enabled,
  });
}

export function useCompetencyReviews(input: CompetencyReviewListInput, enabled = true) {
  return useQuery({
    queryKey: ["learning", "competency-reviews", input],
    queryFn: ({ signal }) => listCompetencyReviews(input, signal),
    enabled,
  });
}

export function useProjectRoleOptions(projectId: string | undefined, enabled = true) {
  return useQuery({
    queryKey: ["learning", "project-role-options", projectId],
    queryFn: ({ signal }) => listProjectRoleOptions(projectId, signal),
    enabled,
  });
}

export function useCreateTrainingCourse() {
  const invalidate = useInvalidateLearning();
  return useMutation({
    mutationFn: (input: CreateTrainingCourseInput) => createTrainingCourse(input),
    onSuccess: invalidate,
  });
}

export function useUpdateTrainingCourse() {
  const invalidate = useInvalidateLearning();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateTrainingCourseInput }) => updateTrainingCourse(id, input),
    onSuccess: invalidate,
  });
}

export function useTransitionTrainingCourse() {
  const invalidate = useInvalidateLearning();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: TransitionTrainingCourseInput }) => transitionTrainingCourse(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateRoleTrainingRequirement() {
  const invalidate = useInvalidateLearning();
  return useMutation({
    mutationFn: (input: CreateRoleTrainingRequirementInput) => createRoleTrainingRequirement(input),
    onSuccess: invalidate,
  });
}

export function useUpdateRoleTrainingRequirement() {
  const invalidate = useInvalidateLearning();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateRoleTrainingRequirementInput }) => updateRoleTrainingRequirement(id, input),
    onSuccess: invalidate,
  });
}

export function useRecordTrainingCompletion() {
  const invalidate = useInvalidateLearning();
  return useMutation({
    mutationFn: (input: RecordTrainingCompletionInput) => recordTrainingCompletion(input),
    onSuccess: invalidate,
  });
}

export function useUpdateTrainingCompletion() {
  const invalidate = useInvalidateLearning();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateTrainingCompletionInput }) => updateTrainingCompletion(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateCompetencyReview() {
  const invalidate = useInvalidateLearning();
  return useMutation({
    mutationFn: (input: CreateCompetencyReviewInput) => createCompetencyReview(input),
    onSuccess: invalidate,
  });
}

export function useUpdateCompetencyReview() {
  const invalidate = useInvalidateLearning();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateCompetencyReviewInput }) => updateCompetencyReview(id, input),
    onSuccess: invalidate,
  });
}
