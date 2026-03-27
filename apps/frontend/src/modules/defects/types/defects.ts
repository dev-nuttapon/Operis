import type { PaginatedResult, PaginationInput } from "../../../shared/types/pagination";

export interface DefectListInput extends PaginationInput {
  projectId?: string;
  severity?: string;
  status?: string;
  ownerUserId?: string;
  search?: string;
}

export interface NonConformanceListInput extends PaginationInput {
  projectId?: string;
  status?: string;
  ownerUserId?: string;
  search?: string;
}

export interface DefectListItem {
  id: string;
  projectId: string;
  projectName: string;
  code: string;
  title: string;
  severity: string;
  ownerUserId: string;
  status: string;
  detectedInPhase?: string | null;
  updatedAt: string;
}

export interface DefectDetail extends DefectListItem {
  description: string;
  resolutionSummary?: string | null;
  correctiveActionRef?: string | null;
  affectedArtifactRefs: string[];
  createdAt: string;
}

export interface NonConformanceListItem {
  id: string;
  projectId: string;
  projectName: string;
  code: string;
  title: string;
  sourceType: string;
  ownerUserId: string;
  status: string;
  correctiveActionRef?: string | null;
  updatedAt: string;
}

export interface NonConformanceDetail extends NonConformanceListItem {
  description: string;
  rootCause?: string | null;
  resolutionSummary?: string | null;
  acceptedDisposition?: string | null;
  linkedFindingRefs: string[];
  createdAt: string;
}

export interface DefectFormInput {
  projectId: string;
  code: string;
  title: string;
  description: string;
  severity: string;
  ownerUserId: string;
  detectedInPhase?: string | null;
  correctiveActionRef?: string | null;
  affectedArtifactRefs?: string[];
}

export interface DefectUpdateInput extends Omit<DefectFormInput, "projectId" | "code"> {
  status: string;
}

export interface ResolveDefectInput {
  resolutionSummary: string;
  correctiveActionRef?: string | null;
}

export interface CloseDefectInput {
  resolutionSummary: string;
}

export interface NonConformanceFormInput {
  projectId: string;
  code: string;
  title: string;
  description: string;
  sourceType: string;
  ownerUserId: string;
  correctiveActionRef?: string | null;
  rootCause?: string | null;
  linkedFindingRefs?: string[];
}

export interface NonConformanceUpdateInput extends Omit<NonConformanceFormInput, "projectId" | "code"> {
  resolutionSummary?: string | null;
  acceptedDisposition?: string | null;
  status: string;
}

export interface CloseNonConformanceInput {
  correctiveActionRef?: string | null;
  acceptedDisposition?: string | null;
  resolutionSummary?: string | null;
}

export type DefectListResult = PaginatedResult<DefectListItem>;
export type NonConformanceListResult = PaginatedResult<NonConformanceListItem>;
