import type { PaginatedResult, PaginationInput } from "../../../shared/types/pagination";

export interface ReleaseListInput extends PaginationInput {
  search?: string;
  status?: string;
  projectId?: string;
  releaseId?: string;
}

export interface ReleaseListItem {
  id: string;
  projectId: string;
  projectName: string;
  releaseCode: string;
  title: string;
  plannedAt?: string | null;
  releasedAt?: string | null;
  status: string;
  latestQualityGateResult?: string | null;
  checklistCompleted: number;
  checklistTotal: number;
  updatedAt: string;
}

export interface DeploymentChecklistItem {
  id: string;
  releaseId: string;
  releaseCode: string;
  checklistItem: string;
  ownerUserId: string;
  status: string;
  completedAt?: string | null;
  evidenceRef?: string | null;
  updatedAt: string;
}

export interface ReleaseNoteItem {
  id: string;
  releaseId: string;
  releaseCode: string;
  summary: string;
  includedChanges: string;
  knownIssues?: string | null;
  status: string;
  publishedAt?: string | null;
  updatedAt: string;
}

export interface ReleaseDetail extends ReleaseListItem {
  qualityGateResult?: string | null;
  qualityGateOverrideReason?: string | null;
  approvedByUserId?: string | null;
  approvedAt?: string | null;
  checklistItems: DeploymentChecklistItem[];
  notes: ReleaseNoteItem[];
  createdAt: string;
}

export interface ReleaseFormInput {
  projectId: string;
  releaseCode: string;
  title: string;
  plannedAt?: string | null;
}

export interface ReleaseUpdateInput {
  title: string;
  plannedAt?: string | null;
}

export interface ReleaseCommandResponse {
  id: string;
  releaseCode: string;
  status: string;
}

export interface ApproveReleaseInput {
  reason?: string | null;
}

export interface ExecuteReleaseInput {
  overrideReason?: string | null;
}

export interface DeploymentChecklistFormInput {
  releaseId: string;
  checklistItem: string;
  ownerUserId: string;
  status: string;
  evidenceRef?: string | null;
}

export interface DeploymentChecklistUpdateInput {
  checklistItem: string;
  ownerUserId: string;
  status: string;
  completedAt?: string | null;
  evidenceRef?: string | null;
}

export interface ReleaseNoteFormInput {
  releaseId: string;
  summary: string;
  includedChanges: string;
  knownIssues?: string | null;
}

export type ReleaseListResult = PaginatedResult<ReleaseListItem>;
export type DeploymentChecklistListResult = PaginatedResult<DeploymentChecklistItem>;
export type ReleaseNotesListResult = PaginatedResult<ReleaseNoteItem>;
