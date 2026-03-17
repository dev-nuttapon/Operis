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

export function listDocuments(signal?: AbortSignal) {
  return apiRequest<DocumentListItem[]>("/api/v1/documents", { signal });
}

export function uploadDocument(file: File, documentName: string, signal?: AbortSignal) {
  const formData = new FormData();
  formData.append("file", file);
  formData.append("documentName", documentName);

  return apiRequest<DocumentListItem>("/api/v1/documents", {
    method: "POST",
    body: formData,
    signal,
  });
}
