import { apiRequest } from "../../../shared/lib/apiClient";
import type { PaginatedResult } from "../../../shared/types/pagination";
import type { BusinessAuditEventItem, ListAuditLogsInput } from "../types/audits";

export function listAuditLogs(input: ListAuditLogsInput, signal?: AbortSignal) {
  const params = new URLSearchParams();

  if (input.module) params.set("module", input.module);
  if (input.eventType) params.set("eventType", input.eventType);
  if (input.entityType) params.set("entityType", input.entityType);
  if (input.entityId) params.set("entityId", input.entityId);
  if (input.actor) params.set("actor", input.actor);
  if (input.from) params.set("from", input.from);
  if (input.to) params.set("to", input.to);
  if (input.page) params.set("page", String(input.page));
  if (input.pageSize) params.set("pageSize", String(input.pageSize));

  const query = params.toString();
  return apiRequest<PaginatedResult<BusinessAuditEventItem>>(`/api/v1/audit-events${query ? `?${query}` : ""}`, { signal });
}
