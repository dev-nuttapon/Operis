import { useQuery } from "@tanstack/react-query";
import { listNotificationQueue, listNotifications } from "../api/notificationsApi";
import type { NotificationListInput, NotificationListResponse, NotificationQueueInput, NotificationQueueResponse } from "../types/notifications";

export function useNotifications(input: NotificationListInput, enabled = true) {
  return useQuery<NotificationListResponse>({
    queryKey: ["notifications", input],
    queryFn: ({ signal }) => listNotifications(input, signal),
    staleTime: 30_000,
    enabled,
  });
}

export function useNotificationQueue(input: NotificationQueueInput, enabled = true) {
  return useQuery<NotificationQueueResponse>({
    queryKey: ["notifications", "queue", input],
    queryFn: ({ signal }) => listNotificationQueue(input, signal),
    staleTime: 30_000,
    enabled,
  });
}
