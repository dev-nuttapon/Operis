import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  assessRisk,
  closeIssue,
  closeRisk,
  createIssue,
  createIssueAction,
  createRisk,
  getIssue,
  getRisk,
  listIssues,
  listRisks,
  mitigateRisk,
  resolveIssue,
  updateIssue,
  updateIssueAction,
  updateRisk,
} from "../api/risksApi";
import type {
  IssueActionInput,
  IssueActionUpdateInput,
  IssueFormInput,
  IssueListInput,
  IssueResolutionInput,
  IssueUpdateInput,
  RiskFormInput,
  RiskListInput,
  RiskTransitionInput,
  RiskUpdateInput,
} from "../types/risks";

export function useRisks(input?: RiskListInput, enabled = true) {
  return useQuery({
    queryKey: ["risks", "list", input],
    queryFn: ({ signal }) => listRisks(input, signal),
    enabled,
  });
}

export function useRisk(riskId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["risks", "detail", riskId],
    queryFn: ({ signal }) => (riskId ? getRisk(riskId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(riskId),
  });
}

export function useIssues(input?: IssueListInput, enabled = true) {
  return useQuery({
    queryKey: ["issues", "list", input],
    queryFn: ({ signal }) => listIssues(input, signal),
    enabled,
  });
}

export function useIssue(issueId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["issues", "detail", issueId],
    queryFn: ({ signal }) => (issueId ? getIssue(issueId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(issueId),
  });
}

function useInvalidateRisks() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: ["risks"] });
    await queryClient.invalidateQueries({ queryKey: ["issues"] });
  };
}

export function useCreateRisk() {
  const invalidate = useInvalidateRisks();
  return useMutation({
    mutationFn: (input: RiskFormInput) => createRisk(input),
    onSuccess: invalidate,
  });
}

export function useUpdateRisk() {
  const invalidate = useInvalidateRisks();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: RiskUpdateInput }) => updateRisk(id, input),
    onSuccess: invalidate,
  });
}

export function useRiskActions() {
  const invalidate = useInvalidateRisks();
  return {
    assess: useMutation({ mutationFn: ({ id, input }: { id: string; input: RiskTransitionInput }) => assessRisk(id, input), onSuccess: invalidate }),
    mitigate: useMutation({ mutationFn: ({ id, input }: { id: string; input: RiskTransitionInput }) => mitigateRisk(id, input), onSuccess: invalidate }),
    close: useMutation({ mutationFn: ({ id, input }: { id: string; input: RiskTransitionInput }) => closeRisk(id, input), onSuccess: invalidate }),
  };
}

export function useCreateIssue() {
  const invalidate = useInvalidateRisks();
  return useMutation({
    mutationFn: (input: IssueFormInput) => createIssue(input),
    onSuccess: invalidate,
  });
}

export function useUpdateIssue() {
  const invalidate = useInvalidateRisks();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: IssueUpdateInput }) => updateIssue(id, input),
    onSuccess: invalidate,
  });
}

export function useIssueActions() {
  const invalidate = useInvalidateRisks();
  return {
    createAction: useMutation({ mutationFn: ({ issueId, input }: { issueId: string; input: IssueActionInput }) => createIssueAction(issueId, input), onSuccess: invalidate }),
    updateAction: useMutation({ mutationFn: ({ issueId, actionId, input }: { issueId: string; actionId: string; input: IssueActionUpdateInput }) => updateIssueAction(issueId, actionId, input), onSuccess: invalidate }),
    resolve: useMutation({ mutationFn: ({ id, input }: { id: string; input: IssueResolutionInput }) => resolveIssue(id, input), onSuccess: invalidate }),
    close: useMutation({ mutationFn: ({ id, input }: { id: string; input: IssueResolutionInput }) => closeIssue(id, input), onSuccess: invalidate }),
  };
}
