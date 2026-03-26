import type { PaginatedResult, PaginationInput } from "../../../shared/types/pagination";

export type RiskStatus = "draft" | "assessed" | "mitigated" | "closed";
export type IssueStatus = "open" | "in_progress" | "resolved" | "closed";
export type IssueSeverity = "low" | "medium" | "high" | "critical";
export type IssueActionStatus = "open" | "in_progress" | "completed" | "verified";

export interface RiskListInput extends PaginationInput {
  search?: string;
  projectId?: string;
  status?: string;
  ownerUserId?: string;
  riskLevel?: number;
  nextReviewBefore?: string;
}

export interface RiskListItem {
  id: string;
  projectId: string;
  projectName: string;
  code: string;
  title: string;
  probability: number;
  impact: number;
  ownerUserId: string;
  status: RiskStatus;
  nextReviewAt?: string | null;
  updatedAt: string;
}

export interface RiskReviewItem {
  id: string;
  riskId: string;
  reviewedBy: string;
  reviewedAt: string;
  decision: string;
  notes?: string | null;
}

export interface RiskHistoryItem {
  id: string;
  eventType: string;
  summary?: string | null;
  reason?: string | null;
  actorUserId?: string | null;
  occurredAt: string;
}

export interface RiskDetail {
  id: string;
  projectId: string;
  projectName: string;
  code: string;
  title: string;
  description: string;
  probability: number;
  impact: number;
  ownerUserId: string;
  mitigationPlan?: string | null;
  cause?: string | null;
  effect?: string | null;
  contingencyPlan?: string | null;
  status: RiskStatus;
  nextReviewAt?: string | null;
  reviews: RiskReviewItem[];
  history: RiskHistoryItem[];
  createdAt: string;
  updatedAt: string;
}

export interface RiskFormInput {
  projectId: string;
  code: string;
  title: string;
  description: string;
  probability: number;
  impact: number;
  ownerUserId: string;
  mitigationPlan?: string | null;
  cause?: string | null;
  effect?: string | null;
  contingencyPlan?: string | null;
  nextReviewAt?: string | null;
}

export interface RiskUpdateInput extends Omit<RiskFormInput, "projectId" | "code"> {}

export interface RiskTransitionInput {
  notes?: string | null;
  mitigationPlan?: string | null;
  nextReviewAt?: string | null;
}

export interface IssueListInput extends PaginationInput {
  search?: string;
  projectId?: string;
  status?: string;
  ownerUserId?: string;
  severity?: string;
  dueBefore?: string;
  dueAfter?: string;
}

export interface IssueListItem {
  id: string;
  projectId: string;
  projectName: string;
  code: string;
  title: string;
  severity: IssueSeverity;
  ownerUserId: string;
  dueDate?: string | null;
  status: IssueStatus;
  openActionCount: number;
  isSensitive: boolean;
  updatedAt: string;
}

export interface IssueAction {
  id: string;
  issueId: string;
  actionDescription: string;
  assignedTo: string;
  dueDate?: string | null;
  status: IssueActionStatus;
  verificationNote?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface IssueDetail {
  id: string;
  projectId: string;
  projectName: string;
  code: string;
  title: string;
  description?: string | null;
  ownerUserId: string;
  dueDate?: string | null;
  status: IssueStatus;
  severity: IssueSeverity;
  rootIssue?: string | null;
  dependencies?: string | null;
  resolutionSummary?: string | null;
  isSensitive: boolean;
  sensitiveContext?: string | null;
  actions: IssueAction[];
  history: RiskHistoryItem[];
  createdAt: string;
  updatedAt: string;
}

export interface IssueFormInput {
  projectId: string;
  code: string;
  title: string;
  description: string;
  ownerUserId: string;
  dueDate?: string | null;
  severity: IssueSeverity;
  rootIssue?: string | null;
  dependencies?: string | null;
  isSensitive: boolean;
  sensitiveContext?: string | null;
}

export interface IssueUpdateInput extends Omit<IssueFormInput, "projectId" | "code"> {
  resolutionSummary?: string | null;
  status?: IssueStatus | null;
}

export interface IssueActionInput {
  actionDescription: string;
  assignedTo: string;
  dueDate?: string | null;
}

export interface IssueActionUpdateInput extends IssueActionInput {
  status: IssueActionStatus;
  verificationNote?: string | null;
}

export interface IssueResolutionInput {
  resolutionSummary?: string | null;
}

export type RiskListResult = PaginatedResult<RiskListItem>;
export type IssueListResult = PaginatedResult<IssueListItem>;
