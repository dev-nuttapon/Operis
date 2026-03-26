import type { PaginatedResult, PaginationInput } from "../../../shared/types/pagination";

export type MeetingStatus = "draft" | "approved" | "archived";
export type MeetingMinutesStatus = "draft" | "reviewed" | "approved" | "archived";
export type DecisionStatus = "proposed" | "approved" | "applied" | "archived";

export interface MeetingListInput extends PaginationInput {
  search?: string;
  projectId?: string;
  meetingType?: string;
  meetingDateFrom?: string;
  meetingDateTo?: string;
  status?: string;
}

export interface DecisionListInput extends PaginationInput {
  search?: string;
  projectId?: string;
  decisionType?: string;
  status?: string;
  meetingId?: string;
}

export interface MeetingListItem {
  id: string;
  projectId: string;
  projectName: string;
  meetingType: string;
  title: string;
  meetingAt: string;
  facilitatorUserId: string;
  status: MeetingStatus;
  isRestricted: boolean;
  updatedAt: string;
}

export interface MeetingAttendeeItem {
  id: string;
  meetingRecordId: string;
  userId: string;
  attendanceStatus: string;
}

export interface MeetingMinutes {
  id: string;
  meetingRecordId: string;
  summary?: string | null;
  decisionsSummary?: string | null;
  actionsSummary?: string | null;
  status: MeetingMinutesStatus;
  updatedAt: string;
}

export interface MeetingHistoryItem {
  id: string;
  eventType: string;
  summary?: string | null;
  reason?: string | null;
  actorUserId?: string | null;
  occurredAt: string;
}

export interface DecisionListItem {
  id: string;
  projectId: string;
  projectName: string;
  meetingId?: string | null;
  meetingTitle?: string | null;
  code: string;
  title: string;
  decisionType: string;
  approvedBy?: string | null;
  status: DecisionStatus;
  isRestricted: boolean;
  updatedAt: string;
}

export interface MeetingDetail {
  id: string;
  projectId: string;
  projectName: string;
  meetingType: string;
  title: string;
  meetingAt: string;
  facilitatorUserId: string;
  status: MeetingStatus;
  agenda?: string | null;
  discussionSummary?: string | null;
  isRestricted: boolean;
  classification?: string | null;
  minutes: MeetingMinutes;
  attendees: MeetingAttendeeItem[];
  decisions: DecisionListItem[];
  history: MeetingHistoryItem[];
  createdAt: string;
  updatedAt: string;
}

export interface DecisionDetail {
  id: string;
  projectId: string;
  projectName: string;
  meetingId?: string | null;
  meetingTitle?: string | null;
  code: string;
  title: string;
  decisionType: string;
  rationale: string;
  alternativesConsidered?: string | null;
  impactedArtifacts: string[];
  approvedBy?: string | null;
  approvedAt?: string | null;
  status: DecisionStatus;
  isRestricted: boolean;
  classification?: string | null;
  history: MeetingHistoryItem[];
  createdAt: string;
  updatedAt: string;
}

export interface MeetingFormInput {
  projectId: string;
  meetingType: string;
  title: string;
  meetingAt: string;
  facilitatorUserId: string;
  attendeeUserIds?: string[];
  agenda?: string | null;
  discussionSummary?: string | null;
  isRestricted: boolean;
  classification?: string | null;
}

export interface MeetingUpdateInput extends Omit<MeetingFormInput, "projectId"> {}

export interface MeetingMinutesInput {
  summary?: string | null;
  decisionsSummary?: string | null;
  actionsSummary?: string | null;
  status?: string | null;
  attendeeUserIds?: string[];
}

export interface DecisionFormInput {
  projectId: string;
  meetingId?: string | null;
  code: string;
  title: string;
  decisionType: string;
  rationale: string;
  alternativesConsidered?: string | null;
  impactedArtifacts?: string[];
  isRestricted: boolean;
  classification?: string | null;
}

export interface DecisionUpdateInput extends Omit<DecisionFormInput, "projectId" | "code" | "meetingId"> {}
export interface DecisionTransitionInput {
  reason?: string | null;
}

export type MeetingListResult = PaginatedResult<MeetingListItem>;
export type DecisionListResult = PaginatedResult<DecisionListItem>;
