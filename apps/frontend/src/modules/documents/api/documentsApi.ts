import { apiDownload, apiRequest } from "../../../shared/lib/apiClient";
import type { PaginatedResult } from "../../../shared/types/pagination";

export interface DocumentTypeListItem {
  id: string;
  code: string;
  name: string;
  moduleOwner: string;
  classificationDefault: string;
  retentionClassDefault: string;
  status: string;
  approvalRequired: boolean;
  updatedAt: string;
}

export interface DocumentTypeResponse extends DocumentTypeListItem {
  createdAt: string;
}

export interface DocumentListItem {
  id: string;
  title: string;
  documentName: string;
  documentTypeId: string | null;
  documentTypeCode: string | null;
  documentTypeName: string | null;
  projectId: string | null;
  projectName: string | null;
  phaseCode: string | null;
  ownerUserId: string | null;
  status: string;
  classification: string;
  retentionClass: string;
  currentVersionNumber: number | null;
  currentVersionStatus: string | null;
  currentFileName: string | null;
  currentMimeType: string | null;
  currentFileSize: number | null;
  updatedAt: string;
  fileName: string | null;
  contentType: string | null;
  sizeBytes: number | null;
  uploadedByUserId: string | null;
  uploadedAt: string;
  versionCode: string | null;
  revision: number | null;
  publishedVersionCode: string | null;
  publishedRevision: number | null;
}

export interface DocumentVersionListItem {
  id: string;
  documentId: string;
  versionNumber: number;
  fileName: string;
  mimeType: string;
  fileSize: number;
  uploadedBy: string;
  uploadedAt: string;
  status: string;
  revision: number;
  versionCode: string;
  contentType: string;
  sizeBytes: number;
  uploadedByUserId: string;
  isPublished: boolean;
}

export interface DocumentApprovalItem {
  id: string;
  documentVersionId: string;
  stepName: string;
  reviewerUserId: string;
  decision: string;
  decisionReason: string | null;
  decidedAt: string | null;
}

export interface DocumentLinkItem {
  id: string;
  sourceDocumentId: string;
  targetEntityType: string;
  targetEntityId: string;
  linkType: string;
}

export interface DocumentDetailResponse {
  id: string;
  title: string;
  documentTypeId: string | null;
  documentTypeCode: string | null;
  documentTypeName: string | null;
  projectId: string | null;
  projectName: string | null;
  phaseCode: string | null;
  ownerUserId: string | null;
  status: string;
  classification: string;
  retentionClass: string;
  tags: string[];
  currentVersionId: string | null;
  versions: DocumentVersionListItem[];
  approvals: DocumentApprovalItem[];
  links: DocumentLinkItem[];
  createdAt: string;
  updatedAt: string;
}

export interface DocumentCreateRequest {
  documentTypeId: string;
  projectId: string;
  phaseCode: string;
  ownerUserId: string;
  classification: string;
  retentionClass: string;
  title: string;
  tags?: string[];
}

export interface DocumentUpdateRequest extends DocumentCreateRequest {}

export interface DocumentTypeCreateRequest {
  code: string;
  name: string;
  moduleOwner: string;
  classificationDefault: string;
  retentionClassDefault: string;
  approvalRequired: boolean;
}

export interface DocumentTypeUpdateRequest extends DocumentTypeCreateRequest {
  status: string;
}

export interface DocumentApprovalDecisionRequest {
  stepName: string;
  reviewerUserId: string;
  decisionReason: string;
}

export interface DocumentLinkRequest {
  targetEntityType: string;
  targetEntityId: string;
  linkType: string;
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

export interface DocumentLookupRequest {
  documentIds: string[];
}

export type DocumentListInput = {
  page?: number;
  pageSize?: number;
  search?: string;
  documentTypeId?: string;
  projectId?: string;
  phaseCode?: string;
  status?: string;
  ownerUserId?: string;
  classification?: string;
  updatedAfter?: string;
};

export type DocumentVersionListInput = {
  page?: number;
  pageSize?: number;
  search?: string;
};

export type DocumentTypeListInput = {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: string;
};

export type DocumentHistoryListInput = {
  page?: number;
  pageSize?: number;
  search?: string;
};

function toQuery(input?: Record<string, string | number | undefined>) {
  const params = new URLSearchParams();
  if (!input) {
    return "";
  }

  for (const [key, value] of Object.entries(input)) {
    if (value !== undefined && value !== "") {
      params.set(key, String(value));
    }
  }

  const query = params.toString();
  return query ? `?${query}` : "";
}

function mapDocumentListItem(item: Omit<DocumentListItem, "documentName" | "fileName" | "contentType" | "sizeBytes" | "uploadedByUserId" | "uploadedAt" | "versionCode" | "revision" | "publishedVersionCode" | "publishedRevision">): DocumentListItem {
  return {
    ...item,
    documentName: item.title,
    fileName: item.currentFileName,
    contentType: item.currentMimeType,
    sizeBytes: item.currentFileSize,
    uploadedByUserId: item.ownerUserId,
    uploadedAt: item.updatedAt,
    versionCode: item.currentVersionNumber ? `v${item.currentVersionNumber}` : null,
    revision: item.currentVersionNumber,
    publishedVersionCode: item.currentVersionStatus === "approved" || item.status === "baseline" || item.status === "archived"
      ? (item.currentVersionNumber ? `v${item.currentVersionNumber}` : null)
      : null,
    publishedRevision: item.currentVersionStatus === "approved" || item.status === "baseline" || item.status === "archived"
      ? item.currentVersionNumber
      : null,
  };
}

function mapDocumentVersion(item: Omit<DocumentVersionListItem, "revision" | "versionCode" | "contentType" | "sizeBytes" | "uploadedByUserId" | "isPublished">): DocumentVersionListItem {
  return {
    ...item,
    revision: item.versionNumber,
    versionCode: `v${item.versionNumber}`,
    contentType: item.mimeType,
    sizeBytes: item.fileSize,
    uploadedByUserId: item.uploadedBy,
    isPublished: item.status === "approved",
  };
}

export function listDocuments(input?: DocumentListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<Omit<DocumentListItem, "documentName" | "fileName" | "contentType" | "sizeBytes" | "uploadedByUserId" | "uploadedAt" | "versionCode" | "revision" | "publishedVersionCode" | "publishedRevision">>>(
    `/api/v1/documents${toQuery(input)}`,
    { signal },
  ).then((result) => ({ ...result, items: result.items.map(mapDocumentListItem) }));
}

export function lookupDocumentsByIds(documentIds: string[], signal?: AbortSignal) {
  return apiRequest<Omit<DocumentListItem, "documentName" | "fileName" | "contentType" | "sizeBytes" | "uploadedByUserId" | "uploadedAt" | "versionCode" | "revision" | "publishedVersionCode" | "publishedRevision">[]>("/api/v1/documents/lookup", {
    method: "POST",
    body: { documentIds } satisfies DocumentLookupRequest,
    signal,
  }).then((items) => items.map(mapDocumentListItem));
}

export function getDocument(documentId: string, signal?: AbortSignal) {
  return apiRequest<DocumentDetailResponse>(`/api/v1/documents/${documentId}`, { signal });
}

export function createDocument(payload: DocumentCreateRequest, signal?: AbortSignal) {
  return apiRequest<Omit<DocumentListItem, "documentName" | "fileName" | "contentType" | "sizeBytes" | "uploadedByUserId" | "uploadedAt" | "versionCode" | "revision" | "publishedVersionCode" | "publishedRevision">>("/api/v1/documents", {
    method: "POST",
    body: payload,
    signal,
  }).then(mapDocumentListItem);
}

export function updateDocument(documentId: string, payload: DocumentUpdateRequest, signal?: AbortSignal) {
  return apiRequest<Omit<DocumentListItem, "documentName" | "fileName" | "contentType" | "sizeBytes" | "uploadedByUserId" | "uploadedAt" | "versionCode" | "revision" | "publishedVersionCode" | "publishedRevision">>(`/api/v1/documents/${documentId}`, {
    method: "PUT",
    body: payload,
    signal,
  }).then(mapDocumentListItem);
}

export function deleteDocument(documentId: string, reason: string, signal?: AbortSignal) {
  return apiRequest<void>(`/api/v1/documents/${documentId}`, {
    method: "DELETE",
    body: { reason },
    signal,
  });
}

export function createDocumentVersion(
  payload: { documentId: string; file: File; fileName?: string; mimeType?: string },
  signal?: AbortSignal,
) {
  const formData = new FormData();
  formData.append("file", payload.file);
  formData.append("fileName", payload.fileName ?? payload.file.name);
  formData.append("mimeType", payload.mimeType ?? payload.file.type);

  return apiRequest<Omit<DocumentVersionListItem, "revision" | "versionCode" | "contentType" | "sizeBytes" | "uploadedByUserId" | "isPublished">>(`/api/v1/documents/${payload.documentId}/versions`, {
    method: "POST",
    body: formData,
    signal,
  }).then(mapDocumentVersion);
}

export function listDocumentVersions(documentId: string, input?: DocumentVersionListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<Omit<DocumentVersionListItem, "revision" | "versionCode" | "contentType" | "sizeBytes" | "uploadedByUserId" | "isPublished">>>(
    `/api/v1/documents/${documentId}/versions${toQuery(input)}`,
    { signal },
  ).then((result) => ({ ...result, items: result.items.map(mapDocumentVersion) }));
}

export function listDocumentHistory(documentId: string, input?: DocumentHistoryListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<DocumentHistoryItem>>(
    `/api/v1/documents/${documentId}/history${toQuery(input)}`,
    { signal },
  );
}

export function deleteDocumentVersion(documentId: string, versionId: string, signal?: AbortSignal) {
  return apiRequest<void>(`/api/v1/documents/${documentId}/versions/${versionId}`, {
    method: "DELETE",
    signal,
  });
}

function applyDocumentAction(
  documentId: string,
  action: "submit" | "approve" | "reject" | "baseline" | "archive",
  body?: object,
  signal?: AbortSignal,
) {
  return apiRequest<DocumentDetailResponse>(`/api/v1/documents/${documentId}/${action}`, {
    method: "PUT",
    body,
    signal,
  });
}

export function submitDocument(documentId: string, payload: DocumentApprovalDecisionRequest, signal?: AbortSignal) {
  return applyDocumentAction(documentId, "submit", payload, signal);
}

export function approveDocument(documentId: string, payload: DocumentApprovalDecisionRequest, signal?: AbortSignal) {
  return applyDocumentAction(documentId, "approve", payload, signal);
}

export function rejectDocument(documentId: string, payload: DocumentApprovalDecisionRequest, signal?: AbortSignal) {
  return applyDocumentAction(documentId, "reject", payload, signal);
}

export function baselineDocument(documentId: string, signal?: AbortSignal) {
  return applyDocumentAction(documentId, "baseline", undefined, signal);
}

export function archiveDocument(documentId: string, reason: string, signal?: AbortSignal) {
  return applyDocumentAction(documentId, "archive", { reason }, signal);
}

export function createDocumentLink(documentId: string, payload: DocumentLinkRequest, signal?: AbortSignal) {
  return apiRequest<DocumentLinkItem>(`/api/v1/documents/${documentId}/links`, {
    method: "POST",
    body: payload,
    signal,
  });
}

export function downloadDocument(documentId: string, signal?: AbortSignal) {
  return apiDownload(`/api/v1/documents/${documentId}/download`, { signal });
}

export function listDocumentTypes(input?: DocumentTypeListInput, signal?: AbortSignal) {
  return apiRequest<PaginatedResult<DocumentTypeListItem>>(`/api/v1/documents/types${toQuery(input)}`, { signal });
}

export function getDocumentType(documentTypeId: string, signal?: AbortSignal) {
  return apiRequest<DocumentTypeResponse>(`/api/v1/documents/types/${documentTypeId}`, { signal });
}

export function createDocumentType(payload: DocumentTypeCreateRequest, signal?: AbortSignal) {
  return apiRequest<DocumentTypeResponse>("/api/v1/documents/types", {
    method: "POST",
    body: payload,
    signal,
  });
}

export function updateDocumentType(documentTypeId: string, payload: DocumentTypeUpdateRequest, signal?: AbortSignal) {
  return apiRequest<DocumentTypeResponse>(`/api/v1/documents/types/${documentTypeId}`, {
    method: "PUT",
    body: payload,
    signal,
  });
}
