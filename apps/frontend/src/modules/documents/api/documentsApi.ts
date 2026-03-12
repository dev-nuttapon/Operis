import { apiRequest } from "../../../shared/lib/apiClient";

export interface DocumentListItem {
  id: string;
  fileName: string;
  uploadedAt: string;
}

export function listDocuments(signal?: AbortSignal) {
  return apiRequest<DocumentListItem[]>("/api/v1/documents", { signal });
}
