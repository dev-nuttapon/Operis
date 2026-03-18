export interface BusinessAuditEventItem {
  id: string;
  occurredAt: string;
  module: string;
  eventType: string;
  entityType: string;
  entityId: string | null;
  summary: string | null;
  reason: string | null;
  actorUserId: string | null;
  actorEmail: string | null;
  actorDisplayName: string | null;
  metadataJson: string | null;
}

export interface ListAuditLogsInput {
  module?: string;
  eventType?: string;
  entityType?: string;
  entityId?: string;
  actor?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}
