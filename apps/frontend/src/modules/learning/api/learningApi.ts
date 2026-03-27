import { apiRequest } from "../../../shared/lib/apiClient";
import type { PaginatedResult } from "../../../shared/types/pagination";
import type {
  CompetencyReviewItem,
  CompetencyReviewListInput,
  CreateCompetencyReviewInput,
  CreateRoleTrainingRequirementInput,
  CreateTrainingCourseInput,
  ProjectRoleOption,
  RecordTrainingCompletionInput,
  RoleTrainingMatrixInput,
  RoleTrainingRequirementItem,
  TrainingCompletionItem,
  TrainingCompletionListInput,
  TrainingCourseItem,
  TrainingCourseListInput,
  TransitionTrainingCourseInput,
  UpdateCompetencyReviewInput,
  UpdateRoleTrainingRequirementInput,
  UpdateTrainingCompletionInput,
  UpdateTrainingCourseInput,
} from "../types/learning";

function toQuery(input: Record<string, string | number | boolean | undefined | null>) {
  const params = new URLSearchParams();
  Object.entries(input).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      params.set(key, String(value));
    }
  });
  const query = params.toString();
  return query ? `?${query}` : "";
}

export function listTrainingCourses(input: TrainingCourseListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<TrainingCourseItem>>(`/api/v1/learning/courses${toQuery(input as Record<string, string | number | boolean | undefined | null>)}`, { signal });
}

export function createTrainingCourse(input: CreateTrainingCourseInput) {
  return apiRequest<TrainingCourseItem>("/api/v1/learning/courses", { method: "POST", body: input });
}

export function updateTrainingCourse(courseId: string, input: UpdateTrainingCourseInput) {
  return apiRequest<TrainingCourseItem>(`/api/v1/learning/courses/${courseId}`, { method: "PUT", body: input });
}

export function transitionTrainingCourse(courseId: string, input: TransitionTrainingCourseInput) {
  return apiRequest<TrainingCourseItem>(`/api/v1/learning/courses/${courseId}/transition`, { method: "POST", body: input });
}

export function listRoleTrainingRequirements(input: RoleTrainingMatrixInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<RoleTrainingRequirementItem>>(`/api/v1/learning/role-matrix${toQuery(input as Record<string, string | number | boolean | undefined | null>)}`, { signal });
}

export function createRoleTrainingRequirement(input: CreateRoleTrainingRequirementInput) {
  return apiRequest<RoleTrainingRequirementItem>("/api/v1/learning/role-matrix", { method: "POST", body: input });
}

export function updateRoleTrainingRequirement(requirementId: string, input: UpdateRoleTrainingRequirementInput) {
  return apiRequest<RoleTrainingRequirementItem>(`/api/v1/learning/role-matrix/${requirementId}`, { method: "PUT", body: input });
}

export function listTrainingCompletions(input: TrainingCompletionListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<TrainingCompletionItem>>(`/api/v1/learning/completions${toQuery(input as Record<string, string | number | boolean | undefined | null>)}`, { signal });
}

export function recordTrainingCompletion(input: RecordTrainingCompletionInput) {
  return apiRequest<TrainingCompletionItem>("/api/v1/learning/completions", { method: "POST", body: input });
}

export function updateTrainingCompletion(completionId: string, input: UpdateTrainingCompletionInput) {
  return apiRequest<TrainingCompletionItem>(`/api/v1/learning/completions/${completionId}`, { method: "PUT", body: input });
}

export function listCompetencyReviews(input: CompetencyReviewListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<CompetencyReviewItem>>(`/api/v1/learning/competency-reviews${toQuery(input as Record<string, string | number | boolean | undefined | null>)}`, { signal });
}

export function createCompetencyReview(input: CreateCompetencyReviewInput) {
  return apiRequest<CompetencyReviewItem>("/api/v1/learning/competency-reviews", { method: "POST", body: input });
}

export function updateCompetencyReview(reviewId: string, input: UpdateCompetencyReviewInput) {
  return apiRequest<CompetencyReviewItem>(`/api/v1/learning/competency-reviews/${reviewId}`, { method: "PUT", body: input });
}

export function listProjectRoleOptions(projectId?: string, signal?: AbortSignal) {
  return apiRequest<ProjectRoleOption[]>(`/api/v1/learning/project-roles${toQuery({ projectId })}`, { signal });
}
