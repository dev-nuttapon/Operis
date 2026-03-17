import { apiDownload, apiRequest } from "../../../shared/lib/apiClient";

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
  publishedVersionCode: string | null;
  publishedRevision: number | null;
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
    body: payload,
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

export interface DocumentVersionListItem {
  id: string;
  documentId: string;
  revision: number;
  versionCode: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedByUserId: string | null;
  uploadedAt: string;
  isPublished: boolean;
}

export interface DocumentHistoryItem {
  id: string;
  documentId: string;
  eventType: string;
  summary: string | null;
  reason: string | null;
  actorUserId: string | null;
  actorEmail: string | null;
  actorDisplayName: string | null;
  status: string | null;
  statusCode: number | null;
  source: string | null;
  beforeJson: string | null;
  afterJson: string | null;
  metadataJson: string | null;
  occurredAt: string;
}

export function listDocumentVersions(documentId: string, signal?: AbortSignal) {
  return apiRequest<DocumentVersionListItem[]>(`/api/v1/documents/${documentId}/versions`, { signal });
}

export function listDocumentHistory(documentId: string, signal?: AbortSignal) {
  return apiRequest<DocumentHistoryItem[]>(`/api/v1/documents/${documentId}/history`, { signal });
}

export function deleteDocumentVersion(documentId: string, versionId: string, signal?: AbortSignal) {
  return apiRequest<void>(`/api/v1/documents/${documentId}/versions/${versionId}`, {
    method: "DELETE",
    signal,
  });
}

export function publishDocumentVersion(documentId: string, versionId: string, signal?: AbortSignal) {
  return apiRequest<void>(`/api/v1/documents/${documentId}/versions/${versionId}/publish`, {
    method: "POST",
    signal,
  });
}

export function unpublishDocumentVersion(documentId: string, signal?: AbortSignal) {
  return apiRequest<void>(`/api/v1/documents/${documentId}/versions/unpublish`, {
    method: "POST",
    signal,
  });
}

export function updateDocument(documentId: string, documentName: string, signal?: AbortSignal) {
  return apiRequest<DocumentListItem>(`/api/v1/documents/${documentId}`, {
    method: "PUT",
    body: { documentName },
    signal,
  });
}

export function deleteDocument(documentId: string, reason: string, signal?: AbortSignal) {
  return apiRequest<void>(`/api/v1/documents/${documentId}`, {
    method: "DELETE",
    body: { reason },
    signal,
  });
}

export function downloadDocument(documentId: string, signal?: AbortSignal) {
  return apiDownload(`/api/v1/documents/${documentId}/download`, { signal });
}
