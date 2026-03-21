import { useMutation, useQueryClient } from "@tanstack/react-query";
import { markAllNotificationsRead, markNotificationRead, seedNotifications } from "../api/notificationsApi";

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

  return {
    markReadMutation,
    markAllReadMutation,
    seedMutation,
  };
}
