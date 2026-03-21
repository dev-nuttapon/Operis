import { useQuery } from "@tanstack/react-query";
import { getNotification } from "../api/notificationsApi";
import type { NotificationDetail } from "../types/notifications";

export function useNotificationDetail(notificationId: string | null, enabled = true) {
  return useQuery<NotificationDetail | null>({
    queryKey: ["notifications", "detail", notificationId],
    queryFn: ({ signal }) => (notificationId ? getNotification(notificationId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(notificationId),
    staleTime: 30_000,
  });
}
