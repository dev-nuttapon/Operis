import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createLessonLearned,
  getLessonLearned,
  listLessonsLearned,
  publishLessonLearned,
  updateLessonLearned,
} from "../api/knowledgeApi";
import type {
  CreateLessonLearnedInput,
  LessonLearnedListInput,
  PublishLessonLearnedInput,
  UpdateLessonLearnedInput,
} from "../types/knowledge";

export function useLessonsLearned(input: LessonLearnedListInput, enabled = true) {
  return useQuery({
    queryKey: ["knowledge", "lessons", input],
    queryFn: ({ signal }) => listLessonsLearned(input, signal),
    enabled,
  });
}

export function useLessonLearned(lessonId: string | undefined, enabled = true) {
  return useQuery({
    queryKey: ["knowledge", "lesson", lessonId],
    queryFn: ({ signal }) => getLessonLearned(lessonId!, signal),
    enabled: enabled && Boolean(lessonId),
  });
}

function useInvalidateKnowledge() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: ["knowledge"] });
  };
}

export function useCreateLessonLearned() {
  const invalidate = useInvalidateKnowledge();
  return useMutation({
    mutationFn: (input: CreateLessonLearnedInput) => createLessonLearned(input),
    onSuccess: invalidate,
  });
}

export function useUpdateLessonLearned() {
  const invalidate = useInvalidateKnowledge();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateLessonLearnedInput }) => updateLessonLearned(id, input),
    onSuccess: invalidate,
  });
}

export function usePublishLessonLearned() {
  const invalidate = useInvalidateKnowledge();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: PublishLessonLearnedInput }) => publishLessonLearned(id, input),
    onSuccess: invalidate,
  });
}
