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
