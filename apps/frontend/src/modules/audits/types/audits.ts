export type AuditLogStatus = "success" | "failed" | "denied";

export interface AuditLogListItem {
  id: string;
  occurredAt: string;
  module: string;
  action: string;
  entityType: string;
  entityId: string | null;
  actorUserId: string | null;
  actorEmail: string | null;
  actorDisplayName: string | null;
  status: AuditLogStatus | string;
  httpMethod: string | null;
  requestPath: string | null;
}

export interface AuditLogItem {
  id: string;
  occurredAt: string;
  module: string;
  action: string;
  entityType: string;
  entityId: string | null;
  actorType: string;
  actorUserId: string | null;
  actorEmail: string | null;
  actorDisplayName: string | null;
  departmentId: string | null;
  tenantId: string | null;
  requestId: string | null;
  traceId: string | null;
  correlationId: string | null;
  httpMethod: string | null;
  requestPath: string | null;
  ipAddress: string | null;
  userAgent: string | null;
  status: AuditLogStatus | string;
  statusCode: number | null;
  errorCode: string | null;
  errorMessage: string | null;
  reason: string | null;
  source: string;
  beforeJson: string | null;
  afterJson: string | null;
  changesJson: string | null;
  metadataJson: string | null;
  isSensitive: boolean;
  retentionClass: string | null;
}

export interface ListAuditLogsInput {
  module?: string;
  action?: string;
  entityType?: string;
  entityId?: string;
  actor?: string;
  status?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}
