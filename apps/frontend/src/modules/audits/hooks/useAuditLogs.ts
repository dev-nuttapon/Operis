import { useQuery } from "@tanstack/react-query";
import { listAuditLogs } from "../api/auditsApi";
import type { ListAuditLogsInput } from "../types/audits";

export function useAuditLogs(filters: ListAuditLogsInput) {
  return useQuery({
    queryKey: ["admin", "audit-logs", filters],
    queryFn: ({ signal }) => listAuditLogs(filters, signal),
    staleTime: 15_000,
  });
}
