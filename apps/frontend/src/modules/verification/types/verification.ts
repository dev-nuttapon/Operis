import type { PaginatedResult, PaginationInput } from "../../../shared/types/pagination";

export type TestPlanStatus = "draft" | "review" | "approved" | "baseline";
export type TestCaseStatus = "draft" | "ready" | "active" | "retired";
export type TestExecutionResult = "passed" | "failed" | "retest";
export type UatStatus = "draft" | "submitted" | "approved" | "rejected";

export interface VerificationHistoryItem {
  id: string;
  eventType: string;
  summary?: string | null;
  reason?: string | null;
  actorUserId?: string | null;
  occurredAt: string;
}

export interface TestPlanListInput extends PaginationInput {
  search?: string;
  projectId?: string;
  status?: string;
  ownerUserId?: string;
  coverageStatus?: string;
}

export interface TestPlanListItem {
  id: string;
  projectId: string;
  projectName: string;
  code: string;
  title: string;
  ownerUserId: string;
  status: TestPlanStatus;
  coverageStatus: "missing" | "partial" | "complete";
  linkedRequirementCount: number;
  coveredRequirementCount: number;
  updatedAt: string;
}

export interface TestCaseListItem {
  id: string;
  testPlanId: string;
  testPlanCode: string;
  projectId: string;
  projectName: string;
  code: string;
  title: string;
  status: TestCaseStatus;
  requirementId?: string | null;
  requirementCode?: string | null;
  latestResult?: TestExecutionResult | null;
  latestExecutedAt?: string | null;
  updatedAt: string;
}

export interface TestPlanDetail {
  id: string;
  projectId: string;
  projectName: string;
  code: string;
  title: string;
  scopeSummary: string;
  ownerUserId: string;
  status: TestPlanStatus;
  entryCriteria?: string | null;
  exitCriteria?: string | null;
  linkedRequirementIds: string[];
  coverageStatus: "missing" | "partial" | "complete";
  testCases: TestCaseListItem[];
  history: VerificationHistoryItem[];
  createdAt: string;
  updatedAt: string;
}

export interface TestCaseListInput extends PaginationInput {
  search?: string;
  testPlanId?: string;
  requirementId?: string;
  status?: string;
  latestResult?: string;
}

export interface TestExecutionListInput extends PaginationInput {
  testCaseId?: string;
  result?: string;
  executedBy?: string;
  from?: string;
  to?: string;
}

export interface TestExecutionItem {
  id: string;
  testCaseId: string;
  testCaseCode: string;
  executedBy: string;
  executedAt: string;
  result: TestExecutionResult;
  evidenceRef?: string | null;
  isSensitiveEvidence: boolean;
  evidenceClassification?: string | null;
  notes?: string | null;
}

export interface TestCaseDetail {
  id: string;
  testPlanId: string;
  testPlanCode: string;
  projectId: string;
  projectName: string;
  code: string;
  title: string;
  preconditions?: string | null;
  steps: string[];
  expectedResult: string;
  requirementId?: string | null;
  requirementCode?: string | null;
  status: TestCaseStatus;
  latestResult?: TestExecutionResult | null;
  executions: TestExecutionItem[];
  history: VerificationHistoryItem[];
  createdAt: string;
  updatedAt: string;
}

export interface TestPlanFormInput {
  projectId: string;
  code: string;
  title: string;
  scopeSummary: string;
  ownerUserId: string;
  entryCriteria?: string | null;
  exitCriteria?: string | null;
  linkedRequirementIds?: string[];
}

export interface TestPlanUpdateInput extends Omit<TestPlanFormInput, "projectId" | "code"> {
  status?: TestPlanStatus | null;
}

export interface TestCaseFormInput {
  testPlanId: string;
  code: string;
  title: string;
  preconditions?: string | null;
  steps?: string[];
  expectedResult: string;
  requirementId?: string | null;
  status?: TestCaseStatus | null;
}

export interface TestCaseUpdateInput extends Omit<TestCaseFormInput, "testPlanId" | "code"> {}

export interface TestExecutionFormInput {
  testCaseId: string;
  result: TestExecutionResult;
  evidenceRef?: string | null;
  notes?: string | null;
  isSensitiveEvidence: boolean;
  evidenceClassification?: string | null;
}

export interface ExecutionExportInput {
  testCaseId?: string;
  result?: string;
  executedBy?: string;
  from?: string;
  to?: string;
}

export interface ExecutionExportResult {
  status: "queued" | "completed";
  count: number;
  items: TestExecutionItem[];
  message?: string | null;
}

export interface UatSignoffListInput extends PaginationInput {
  projectId?: string;
  status?: string;
  submittedBy?: string;
}

export interface UatSignoffListItem {
  id: string;
  projectId: string;
  projectName: string;
  releaseId?: string | null;
  status: UatStatus;
  submittedBy?: string | null;
  approvedBy?: string | null;
  evidenceCount: number;
  updatedAt: string;
}

export interface UatSignoffDetail {
  id: string;
  projectId: string;
  projectName: string;
  releaseId?: string | null;
  scopeSummary: string;
  submittedBy?: string | null;
  submittedAt?: string | null;
  approvedBy?: string | null;
  approvedAt?: string | null;
  status: UatStatus;
  decisionReason?: string | null;
  evidenceRefs: string[];
  history: VerificationHistoryItem[];
  createdAt: string;
  updatedAt: string;
}

export interface UatSignoffFormInput {
  projectId: string;
  releaseId?: string | null;
  scopeSummary: string;
  evidenceRefs?: string[];
  decisionReason?: string | null;
}

export interface UatSignoffUpdateInput extends Omit<UatSignoffFormInput, "projectId"> {}

export interface VerificationDecisionInput {
  reason?: string | null;
}

export type TestPlanListResult = PaginatedResult<TestPlanListItem>;
export type TestCaseListResult = PaginatedResult<TestCaseListItem>;
export type TestExecutionListResult = PaginatedResult<TestExecutionItem>;
export type UatSignoffListResult = PaginatedResult<UatSignoffListItem>;
