import { apiRequest } from "../../../shared/lib/apiClient";

export interface DocumentListItem {
  id: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedByUserId: string | null;
  uploadedAt: string;
}

export function listDocuments(signal?: AbortSignal) {
  return apiRequest<DocumentListItem[]>("/api/v1/documents", { signal });
}

export function uploadDocument(file: File, signal?: AbortSignal) {
  const formData = new FormData();
  formData.append("file", file);

  return apiRequest<DocumentListItem>("/api/v1/documents", {
    method: "POST",
    body: formData,
    signal,
  });
}
