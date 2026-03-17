import { apiRequest } from "../../../shared/lib/apiClient";

export interface DocumentListItem {
  id: string;
  documentName: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedByUserId: string | null;
  uploadedAt: string;
}

export interface DocumentCreateRequest {
  documentName: string;
}

export function listDocuments(signal?: AbortSignal) {
  return apiRequest<DocumentListItem[]>("/api/v1/documents", { signal });
}

export function createDocument(payload: DocumentCreateRequest, signal?: AbortSignal) {
  return apiRequest<DocumentListItem>("/api/v1/documents", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
    signal,
  });
}
