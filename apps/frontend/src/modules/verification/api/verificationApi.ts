import { apiRequest } from "../../../shared/lib/apiClient";
import type {
  ExecutionExportInput,
  ExecutionExportResult,
  TestCaseDetail,
  TestCaseFormInput,
  TestCaseListInput,
  TestCaseListResult,
  TestCaseUpdateInput,
  TestExecutionFormInput,
  TestExecutionListInput,
  TestExecutionListResult,
  TestPlanDetail,
  TestPlanFormInput,
  TestPlanListInput,
  TestPlanListResult,
  TestPlanUpdateInput,
  UatSignoffDetail,
  UatSignoffFormInput,
  UatSignoffListInput,
  UatSignoffListResult,
  UatSignoffUpdateInput,
  VerificationDecisionInput,
} from "../types/verification";

function toQuery<T extends object>(input?: T) {
  const params = new URLSearchParams();
  if (!input) {
    return "";
  }

  for (const [key, value] of Object.entries(input as Record<string, unknown>)) {
    if (value === undefined || value === null || value === "") {
      continue;
    }

    params.set(key, String(value));
  }

  const query = params.toString();
  return query ? `?${query}` : "";
}

export const listTestPlans = (input?: TestPlanListInput, signal?: AbortSignal) =>
  apiRequest<TestPlanListResult>(`/api/v1/test-plans${toQuery(input)}`, { signal });
export const getTestPlan = (id: string, signal?: AbortSignal) =>
  apiRequest<TestPlanDetail>(`/api/v1/test-plans/${id}`, { signal });
export const createTestPlan = (input: TestPlanFormInput) =>
  apiRequest<TestPlanDetail>("/api/v1/test-plans", { method: "POST", body: input });
export const updateTestPlan = (id: string, input: TestPlanUpdateInput) =>
  apiRequest<TestPlanDetail>(`/api/v1/test-plans/${id}`, { method: "PUT", body: input });
export const submitTestPlan = (id: string) =>
  apiRequest<TestPlanDetail>(`/api/v1/test-plans/${id}/submit`, { method: "PUT" });
export const approveTestPlan = (id: string, input: VerificationDecisionInput) =>
  apiRequest<TestPlanDetail>(`/api/v1/test-plans/${id}/approve`, { method: "PUT", body: input });
export const baselineTestPlan = (id: string, input: VerificationDecisionInput) =>
  apiRequest<TestPlanDetail>(`/api/v1/test-plans/${id}/baseline`, { method: "PUT", body: input });

export const listTestCases = (input?: TestCaseListInput, signal?: AbortSignal) =>
  apiRequest<TestCaseListResult>(`/api/v1/test-cases${toQuery(input)}`, { signal });
export const getTestCase = (id: string, signal?: AbortSignal) =>
  apiRequest<TestCaseDetail>(`/api/v1/test-cases/${id}`, { signal });
export const createTestCase = (input: TestCaseFormInput) =>
  apiRequest<TestCaseDetail>("/api/v1/test-cases", { method: "POST", body: input });
export const updateTestCase = (id: string, input: TestCaseUpdateInput) =>
  apiRequest<TestCaseDetail>(`/api/v1/test-cases/${id}`, { method: "PUT", body: input });

export const listTestExecutions = (input?: TestExecutionListInput, signal?: AbortSignal) =>
  apiRequest<TestExecutionListResult>(`/api/v1/test-executions${toQuery(input)}`, { signal });
export const createTestExecution = (input: TestExecutionFormInput) =>
  apiRequest("/api/v1/test-executions", { method: "POST", body: input });
export const exportTestExecutions = (input: ExecutionExportInput) =>
  apiRequest<ExecutionExportResult>("/api/v1/test-executions/export", { method: "POST", body: input });

export const listUatSignoffs = (input?: UatSignoffListInput, signal?: AbortSignal) =>
  apiRequest<UatSignoffListResult>(`/api/v1/uat-signoffs${toQuery(input)}`, { signal });
export const getUatSignoff = (id: string, signal?: AbortSignal) =>
  apiRequest<UatSignoffDetail>(`/api/v1/uat-signoffs/${id}`, { signal });
export const createUatSignoff = (input: UatSignoffFormInput) =>
  apiRequest<UatSignoffDetail>("/api/v1/uat-signoffs", { method: "POST", body: input });
export const updateUatSignoff = (id: string, input: UatSignoffUpdateInput) =>
  apiRequest<UatSignoffDetail>(`/api/v1/uat-signoffs/${id}`, { method: "PUT", body: input });
export const submitUatSignoff = (id: string) =>
  apiRequest<UatSignoffDetail>(`/api/v1/uat-signoffs/${id}/submit`, { method: "PUT" });
export const approveUatSignoff = (id: string, input: VerificationDecisionInput) =>
  apiRequest<UatSignoffDetail>(`/api/v1/uat-signoffs/${id}/approve`, { method: "PUT", body: input });
export const rejectUatSignoff = (id: string, input: VerificationDecisionInput) =>
  apiRequest<UatSignoffDetail>(`/api/v1/uat-signoffs/${id}/reject`, { method: "PUT", body: input });
