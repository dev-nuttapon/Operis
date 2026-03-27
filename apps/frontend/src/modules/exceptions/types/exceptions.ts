import type { PaginationInput } from "../../../shared/types/pagination";

export interface WaiverListInput extends PaginationInput {
  projectId?: string;
  processArea?: string;
  status?: string;
  onlyExpired?: boolean;
  search?: string;
}

export interface CompensatingControlItem {
  id: string;
  controlCode: string;
  description: string;
  ownerUserId: string;
  status: string;
  updatedAt: string;
}

export interface WaiverReviewItem {
  id: string;
  reviewType: string;
  outcomeStatus: string;
  reviewerUserId: string;
  notes?: string | null;
  reviewedAt: string;
  nextReviewAt?: string | null;
}

export interface WaiverListItem {
  id: string;
  waiverCode: string;
  projectId?: string | null;
  projectName?: string | null;
  processArea: string;
  scopeSummary: string;
  requestedByUserId: string;
  effectiveFrom: string;
  expiresAt: string;
  isExpired: boolean;
  status: string;
  compensatingControlCount: number;
  updatedAt: string;
}

export interface WaiverDetail extends WaiverListItem {
  justification: string;
  decisionReason?: string | null;
  decisionByUserId?: string | null;
  decisionAt?: string | null;
  closureReason?: string | null;
  compensatingControls: CompensatingControlItem[];
  reviews: WaiverReviewItem[];
  createdAt: string;
}

export interface CompensatingControlInput {
  controlCode: string;
  description: string;
  ownerUserId: string;
  status?: string;
}

export interface CreateWaiverInput {
  waiverCode: string;
  projectId?: string | null;
  processArea: string;
  scopeSummary: string;
  requestedByUserId: string;
  justification: string;
  effectiveFrom?: string | null;
  expiresAt?: string | null;
  compensatingControls?: CompensatingControlInput[];
}

export interface UpdateWaiverInput {
  projectId?: string | null;
  processArea: string;
  scopeSummary: string;
  requestedByUserId: string;
  justification: string;
  effectiveFrom?: string | null;
  expiresAt?: string | null;
  compensatingControls?: CompensatingControlInput[];
}

export interface TransitionWaiverInput {
  targetStatus: string;
  reason?: string | null;
  nextReviewAt?: string | null;
}
