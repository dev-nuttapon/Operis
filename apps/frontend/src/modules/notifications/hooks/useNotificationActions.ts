import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createNotificationQueueItem, markAllNotificationsRead, markNotificationRead, retryNotificationQueueItem, seedNotifications } from "../api/notificationsApi";
import type { CreateNotificationQueueInput } from "../types/notifications";

export function useNotificationActions() {
  const queryClient = useQueryClient();

  const markReadMutation = useMutation({
    mutationFn: (notificationId: string) => markNotificationRead(notificationId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["notifications"] });
    },
  });

  const markAllReadMutation = useMutation({
    mutationFn: () => markAllNotificationsRead(),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["notifications"] });
    },
  });

  const seedMutation = useMutation({
    mutationFn: (count: number) => seedNotifications(count),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["notifications"] });
    },
  });

  const enqueueMutation = useMutation({
    mutationFn: (input: CreateNotificationQueueInput) => createNotificationQueueItem(input),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["notifications", "queue"] });
    },
  });

  const retryMutation = useMutation({
    mutationFn: (id: string) => retryNotificationQueueItem(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["notifications", "queue"] });
    },
  });

  return {
    markReadMutation,
    markAllReadMutation,
    seedMutation,
    enqueueMutation,
    retryMutation,
  };
}
