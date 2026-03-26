import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  approveTestPlan,
  approveUatSignoff,
  baselineTestPlan,
  createTestCase,
  createTestExecution,
  createTestPlan,
  createUatSignoff,
  exportTestExecutions,
  getTestCase,
  getTestPlan,
  getUatSignoff,
  listTestCases,
  listTestExecutions,
  listTestPlans,
  listUatSignoffs,
  rejectUatSignoff,
  submitTestPlan,
  submitUatSignoff,
  updateTestCase,
  updateTestPlan,
  updateUatSignoff,
} from "../api/verificationApi";
import type {
  ExecutionExportInput,
  TestCaseFormInput,
  TestCaseListInput,
  TestCaseUpdateInput,
  TestExecutionFormInput,
  TestExecutionListInput,
  TestPlanFormInput,
  TestPlanListInput,
  TestPlanUpdateInput,
  UatSignoffFormInput,
  UatSignoffListInput,
  UatSignoffUpdateInput,
  VerificationDecisionInput,
} from "../types/verification";

export function useTestPlans(input?: TestPlanListInput, enabled = true) {
  return useQuery({
    queryKey: ["verification", "test-plans", input],
    queryFn: ({ signal }) => listTestPlans(input, signal),
    enabled,
  });
}

export function useTestPlan(testPlanId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["verification", "test-plan", testPlanId],
    queryFn: ({ signal }) => (testPlanId ? getTestPlan(testPlanId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(testPlanId),
  });
}

export function useTestCases(input?: TestCaseListInput, enabled = true) {
  return useQuery({
    queryKey: ["verification", "test-cases", input],
    queryFn: ({ signal }) => listTestCases(input, signal),
    enabled,
  });
}

export function useTestCase(testCaseId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["verification", "test-case", testCaseId],
    queryFn: ({ signal }) => (testCaseId ? getTestCase(testCaseId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(testCaseId),
  });
}

export function useTestExecutions(input?: TestExecutionListInput, enabled = true) {
  return useQuery({
    queryKey: ["verification", "test-executions", input],
    queryFn: ({ signal }) => listTestExecutions(input, signal),
    enabled,
  });
}

export function useUatSignoffs(input?: UatSignoffListInput, enabled = true) {
  return useQuery({
    queryKey: ["verification", "uat-signoffs", input],
    queryFn: ({ signal }) => listUatSignoffs(input, signal),
    enabled,
  });
}

export function useUatSignoff(uatSignoffId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["verification", "uat-signoff", uatSignoffId],
    queryFn: ({ signal }) => (uatSignoffId ? getUatSignoff(uatSignoffId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(uatSignoffId),
  });
}

function useInvalidateVerification() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: ["verification"] });
    await queryClient.invalidateQueries({ queryKey: ["requirements"] });
  };
}

export function useCreateTestPlan() {
  const invalidate = useInvalidateVerification();
  return useMutation({
    mutationFn: (input: TestPlanFormInput) => createTestPlan(input),
    onSuccess: invalidate,
  });
}

export function useUpdateTestPlan() {
  const invalidate = useInvalidateVerification();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: TestPlanUpdateInput }) => updateTestPlan(id, input),
    onSuccess: invalidate,
  });
}

export function useTestPlanActions() {
  const invalidate = useInvalidateVerification();
  return {
    submit: useMutation({ mutationFn: (id: string) => submitTestPlan(id), onSuccess: invalidate }),
    approve: useMutation({ mutationFn: ({ id, input }: { id: string; input: VerificationDecisionInput }) => approveTestPlan(id, input), onSuccess: invalidate }),
    baseline: useMutation({ mutationFn: ({ id, input }: { id: string; input: VerificationDecisionInput }) => baselineTestPlan(id, input), onSuccess: invalidate }),
  };
}

export function useCreateTestCase() {
  const invalidate = useInvalidateVerification();
  return useMutation({
    mutationFn: (input: TestCaseFormInput) => createTestCase(input),
    onSuccess: invalidate,
  });
}

export function useUpdateTestCase() {
  const invalidate = useInvalidateVerification();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: TestCaseUpdateInput }) => updateTestCase(id, input),
    onSuccess: invalidate,
  });
}

export function useCreateTestExecution() {
  const invalidate = useInvalidateVerification();
  return useMutation({
    mutationFn: (input: TestExecutionFormInput) => createTestExecution(input),
    onSuccess: invalidate,
  });
}

export function useExportTestExecutions() {
  return useMutation({
    mutationFn: (input: ExecutionExportInput) => exportTestExecutions(input),
  });
}

export function useCreateUatSignoff() {
  const invalidate = useInvalidateVerification();
  return useMutation({
    mutationFn: (input: UatSignoffFormInput) => createUatSignoff(input),
    onSuccess: invalidate,
  });
}

export function useUpdateUatSignoff() {
  const invalidate = useInvalidateVerification();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UatSignoffUpdateInput }) => updateUatSignoff(id, input),
    onSuccess: invalidate,
  });
}

export function useUatActions() {
  const invalidate = useInvalidateVerification();
  return {
    submit: useMutation({ mutationFn: (id: string) => submitUatSignoff(id), onSuccess: invalidate }),
    approve: useMutation({ mutationFn: ({ id, input }: { id: string; input: VerificationDecisionInput }) => approveUatSignoff(id, input), onSuccess: invalidate }),
    reject: useMutation({ mutationFn: ({ id, input }: { id: string; input: VerificationDecisionInput }) => rejectUatSignoff(id, input), onSuccess: invalidate }),
  };
}
