import { useQuery } from "@tanstack/react-query";
import { listNotifications } from "../api/notificationsApi";
import type { NotificationListInput, NotificationListResponse } from "../types/notifications";

export function useNotifications(input: NotificationListInput, enabled = true) {
  return useQuery<NotificationListResponse>({
    queryKey: ["notifications", input],
    queryFn: ({ signal }) => listNotifications(input, signal),
    staleTime: 30_000,
    enabled,
  });
}
