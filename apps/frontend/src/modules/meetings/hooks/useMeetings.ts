import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  applyDecision,
  approveDecision,
  approveMeeting,
  createDecision,
  createMeeting,
  getDecision,
  getMeeting,
  getMeetingMinutes,
  listDecisions,
  listMeetings,
  updateDecision,
  updateMeeting,
  updateMeetingMinutes,
} from "../api/meetingsApi";
import type {
  DecisionFormInput,
  DecisionListInput,
  DecisionTransitionInput,
  DecisionUpdateInput,
  MeetingFormInput,
  MeetingListInput,
  MeetingMinutesInput,
  MeetingUpdateInput,
} from "../types/meetings";

export function useMeetings(input?: MeetingListInput, enabled = true) {
  return useQuery({
    queryKey: ["meetings", "list", input],
    queryFn: ({ signal }) => listMeetings(input, signal),
    enabled,
  });
}

export function useMeeting(meetingId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["meetings", "detail", meetingId],
    queryFn: ({ signal }) => (meetingId ? getMeeting(meetingId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(meetingId),
  });
}

export function useMeetingMinutes(meetingId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["meetings", "minutes", meetingId],
    queryFn: ({ signal }) => (meetingId ? getMeetingMinutes(meetingId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(meetingId),
  });
}

export function useDecisions(input?: DecisionListInput, enabled = true) {
  return useQuery({
    queryKey: ["decisions", "list", input],
    queryFn: ({ signal }) => listDecisions(input, signal),
    enabled,
  });
}

export function useDecision(decisionId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["decisions", "detail", decisionId],
    queryFn: ({ signal }) => (decisionId ? getDecision(decisionId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(decisionId),
  });
}

function useInvalidateMeetings() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: ["meetings"] });
    await queryClient.invalidateQueries({ queryKey: ["decisions"] });
  };
}

export function useCreateMeeting() {
  const invalidate = useInvalidateMeetings();
  return useMutation({
    mutationFn: (input: MeetingFormInput) => createMeeting(input),
    onSuccess: invalidate,
  });
}

export function useUpdateMeeting() {
  const invalidate = useInvalidateMeetings();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: MeetingUpdateInput }) => updateMeeting(id, input),
    onSuccess: invalidate,
  });
}

export function useMeetingActions() {
  const invalidate = useInvalidateMeetings();
  return {
    approve: useMutation({
      mutationFn: ({ id, input }: { id: string; input: DecisionTransitionInput }) => approveMeeting(id, input),
      onSuccess: invalidate,
    }),
    updateMinutes: useMutation({
      mutationFn: ({ id, input }: { id: string; input: MeetingMinutesInput }) => updateMeetingMinutes(id, input),
      onSuccess: invalidate,
    }),
  };
}

export function useCreateDecision() {
  const invalidate = useInvalidateMeetings();
  return useMutation({
    mutationFn: (input: DecisionFormInput) => createDecision(input),
    onSuccess: invalidate,
  });
}

export function useUpdateDecision() {
  const invalidate = useInvalidateMeetings();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: DecisionUpdateInput }) => updateDecision(id, input),
    onSuccess: invalidate,
  });
}

export function useDecisionActions() {
  const invalidate = useInvalidateMeetings();
  return {
    approve: useMutation({
      mutationFn: ({ id, input }: { id: string; input: DecisionTransitionInput }) => approveDecision(id, input),
      onSuccess: invalidate,
    }),
    apply: useMutation({
      mutationFn: ({ id, input }: { id: string; input: DecisionTransitionInput }) => applyDecision(id, input),
      onSuccess: invalidate,
    }),
  };
}
