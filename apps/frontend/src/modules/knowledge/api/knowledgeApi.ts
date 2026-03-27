import { apiRequest } from "../../../shared/lib/apiClient";
import type { PaginatedResult } from "../../../shared/types/pagination";
import type {
  CreateLessonLearnedInput,
  LessonLearnedItem,
  LessonLearnedListInput,
  PublishLessonLearnedInput,
  UpdateLessonLearnedInput,
} from "../types/knowledge";

function buildQuery(input: Record<string, string | number | undefined | null>) {
  const params = new URLSearchParams();
  for (const [key, value] of Object.entries(input)) {
    if (value !== undefined && value !== null && value !== "") {
      params.set(key, String(value));
    }
  }

  const query = params.toString();
  return query ? `?${query}` : "";
}

export function listLessonsLearned(input: LessonLearnedListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<LessonLearnedItem>>(`/api/v1/lessons-learned${buildQuery({ ...input })}`, { signal });
}

export function getLessonLearned(lessonId: string, signal?: AbortSignal) {
  return apiRequest<LessonLearnedItem>(`/api/v1/lessons-learned/${lessonId}`, { signal });
}

export function createLessonLearned(input: CreateLessonLearnedInput) {
  return apiRequest<LessonLearnedItem>("/api/v1/lessons-learned", { method: "POST", body: input });
}

export function updateLessonLearned(lessonId: string, input: UpdateLessonLearnedInput) {
  return apiRequest<LessonLearnedItem>(`/api/v1/lessons-learned/${lessonId}`, { method: "PUT", body: input });
}

export function publishLessonLearned(lessonId: string, input: PublishLessonLearnedInput) {
  return apiRequest<LessonLearnedItem>(`/api/v1/lessons-learned/${lessonId}/publish`, { method: "PUT", body: input });
}
