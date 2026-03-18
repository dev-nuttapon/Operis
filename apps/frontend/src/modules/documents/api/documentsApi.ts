import { apiDownload, apiRequest } from "../../../shared/lib/apiClient";
import type { PaginatedResult } from "../../../shared/types/pagination";

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

export type DocumentListInput = {
  page?: number;
  pageSize?: number;
  search?: string;
};

export function listDocuments(input?: DocumentListInput, signal?: AbortSignal) {
  const params = new URLSearchParams();
  if (input?.page) params.set("page", String(input.page));
  if (input?.pageSize) params.set("pageSize", String(input.pageSize));
  if (input?.search) params.set("search", input.search);
  const query = params.toString();
  return apiRequest<PaginatedResult<DocumentListItem>>(`/api/v1/documents${query ? `?${query}` : ""}`, { signal });
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
  beforeJson: string | null;
  afterJson: string | null;
  metadataJson: string | null;
  occurredAt: string;
}

export type DocumentVersionListInput = {
  page?: number;
  pageSize?: number;
  search?: string;
};

export function listDocumentVersions(documentId: string, input?: DocumentVersionListInput, signal?: AbortSignal) {
  const params = new URLSearchParams();
  if (input?.page) params.set("page", String(input.page));
  if (input?.pageSize) params.set("pageSize", String(input.pageSize));
  if (input?.search) params.set("search", input.search);
  const query = params.toString();
  return apiRequest<PaginatedResult<DocumentVersionListItem>>(
    `/api/v1/documents/${documentId}/versions${query ? `?${query}` : ""}`,
    { signal },
  );
}

export type DocumentHistoryListInput = {
  page?: number;
  pageSize?: number;
  search?: string;
};

export function listDocumentHistory(documentId: string, input?: DocumentHistoryListInput, signal?: AbortSignal) {
  const params = new URLSearchParams();
  if (input?.page) params.set("page", String(input.page));
  if (input?.pageSize) params.set("pageSize", String(input.pageSize));
  if (input?.search) params.set("search", input.search);
  const query = params.toString();
  return apiRequest<PaginatedResult<DocumentHistoryItem>>(
    `/api/v1/documents/${documentId}/history${query ? `?${query}` : ""}`,
    { signal },
  );
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
