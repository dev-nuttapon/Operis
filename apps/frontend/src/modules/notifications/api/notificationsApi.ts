import { apiRequest } from "../../../shared/lib/apiClient";
import type {
  CreateNotificationQueueInput,
  NotificationDetail,
  NotificationListInput,
  NotificationListResponse,
  NotificationQueueInput,
  NotificationQueueResponse,
} from "../types/notifications";

export async function listNotifications(input: NotificationListInput, signal?: AbortSignal): Promise<NotificationListResponse> {
  const page = input.page ?? 1;
  const pageSize = input.pageSize ?? 10;
  const params = new URLSearchParams();
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));
  if (typeof input.unreadOnly === "boolean") {
    params.set("unreadOnly", String(input.unreadOnly));
  }
  const query = params.toString();
  return apiRequest<NotificationListResponse>(`/api/v1/notifications?${query}`, { signal });
}

export async function getNotification(notificationId: string, signal?: AbortSignal): Promise<NotificationDetail> {
  return apiRequest<NotificationDetail>(`/api/v1/notifications/${notificationId}`, { signal });
}

export async function markNotificationRead(notificationId: string): Promise<{ updated: number }> {
  return apiRequest<{ updated: number }>(`/api/v1/notifications/${notificationId}/read`, {
    method: "POST",
  });
}

export async function markAllNotificationsRead(): Promise<{ updated: number }> {
  return apiRequest<{ updated: number }>(`/api/v1/notifications/read-all`, {
    method: "POST",
  });
}

export async function seedNotifications(count: number): Promise<{ inserted: number }> {
  return apiRequest<{ inserted: number }>(`/api/v1/notifications/seed`, {
    method: "POST",
    body: { count },
  });
}

export async function listNotificationQueue(input: NotificationQueueInput, signal?: AbortSignal): Promise<NotificationQueueResponse> {
  const page = input.page ?? 1;
  const pageSize = input.pageSize ?? 25;
  const params = new URLSearchParams();
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));
  if (input.channel) params.set("channel", input.channel);
  if (input.status) params.set("status", input.status);
  if (input.search) params.set("search", input.search);
  const query = params.toString();
  return apiRequest<NotificationQueueResponse>(`/api/v1/notification-queue?${query}`, { signal });
}

export async function createNotificationQueueItem(input: CreateNotificationQueueInput) {
  return apiRequest(`/api/v1/notification-queue`, {
    method: "POST",
    body: input,
  });
}

export async function retryNotificationQueueItem(id: string) {
  return apiRequest(`/api/v1/notification-queue/${id}/retry`, {
    method: "PUT",
  });
}
