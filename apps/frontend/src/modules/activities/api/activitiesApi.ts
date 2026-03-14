import { apiRequest } from "../../../shared/lib/apiClient";
import type { PaginatedResult } from "../../../shared/types/pagination";
import type { ActivityLogItem, ListActivityLogsInput } from "../types/activities";

export function listActivityLogs(input: ListActivityLogsInput, signal?: AbortSignal) {
  const params = new URLSearchParams();

  if (input.module) params.set("module", input.module);
  if (input.action) params.set("action", input.action);
  if (input.entityType) params.set("entityType", input.entityType);
  if (input.entityId) params.set("entityId", input.entityId);
  if (input.actor) params.set("actor", input.actor);
  if (input.status) params.set("status", input.status);
  if (input.sortBy) params.set("sortBy", input.sortBy);
  if (input.sortOrder) params.set("sortOrder", input.sortOrder);
  if (input.from) params.set("from", input.from);
  if (input.to) params.set("to", input.to);
  if (input.page) params.set("page", String(input.page));
  if (input.pageSize) params.set("pageSize", String(input.pageSize));

  const query = params.toString();
  return apiRequest<PaginatedResult<ActivityLogItem>>(`/api/v1/activity-logs${query ? `?${query}` : ""}`, { signal });
}
