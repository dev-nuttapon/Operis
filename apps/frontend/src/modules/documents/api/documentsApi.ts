import { apiRequest } from "../../../shared/lib/apiClient";

export interface DocumentListItem {
  id: string;
  documentName: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedByUserId: string | null;
  uploadedAt: string;
  versionCode: string | null;
  revision: number | null;
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

export interface DocumentVersionCreateRequest {
  documentId: string;
  versionCode: string;
  file: File;
}

export function createDocumentVersion(payload: DocumentVersionCreateRequest, signal?: AbortSignal) {
  const formData = new FormData();
  formData.append("file", payload.file);
  formData.append("versionCode", payload.versionCode);

  return apiRequest(`/api/v1/documents/${payload.documentId}/versions`, {
    method: "POST",
    body: formData,
    signal,
  });
}
