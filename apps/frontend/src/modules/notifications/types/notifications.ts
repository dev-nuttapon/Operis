export interface NotificationListItem {
  id: string;
  title: string;
  description: string;
  source: string;
  status: "unread" | "read";
  createdAt: string;
  readAt?: string | null;
}

export type NotificationDetail = NotificationListItem;

export interface NotificationListInput {
  page?: number;
  pageSize?: number;
  unreadOnly?: boolean;
}

export interface NotificationListResponse {
  items: NotificationListItem[];
  total: number;
  page: number;
  pageSize: number;
}

export interface NotificationQueueItem {
  id: string;
  channel: string;
  targetRef: string;
  payloadRef: string;
  queuedAt: string;
  status: string;
  retryCount: number;
  lastError?: string | null;
  lastRetriedAt?: string | null;
}

export interface NotificationQueueInput {
  channel?: string;
  status?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface NotificationQueueResponse {
  items: NotificationQueueItem[];
  total: number;
  page: number;
  pageSize: number;
}

export interface CreateNotificationQueueInput {
  channel: string;
  targetRef: string;
  payloadRef: string;
  status: string;
  lastError?: string | null;
}
