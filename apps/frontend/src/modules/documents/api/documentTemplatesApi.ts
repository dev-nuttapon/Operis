import { apiRequest } from "../../../shared/lib/apiClient";
import type { PaginatedResult } from "../../../shared/types/pagination";
import type { DocumentTemplateCreateInput, DocumentTemplateListItem } from "../types/documentTemplates";

export type DocumentTemplateListInput = {
  search?: string;
  page?: number;
  pageSize?: number;
};

export function listDocumentTemplates(input: DocumentTemplateListInput, signal?: AbortSignal) {
  const params = new URLSearchParams();
  if (input.search) params.set("search", input.search);
  if (input.page) params.set("page", String(input.page));
  if (input.pageSize) params.set("pageSize", String(input.pageSize));
  const query = params.toString();
  return apiRequest<PaginatedResult<DocumentTemplateListItem>>(`/api/v1/documents/templates${query ? `?${query}` : ""}`, { signal });
}

export function createDocumentTemplate(input: DocumentTemplateCreateInput) {
  return apiRequest(`/api/v1/documents/templates`, {
    method: "POST",
    body: input,
  });
}

export function getDocumentTemplate(templateId: string, signal?: AbortSignal) {
  return apiRequest(`/api/v1/documents/templates/${templateId}`, { signal });
}

export function updateDocumentTemplate(templateId: string, input: DocumentTemplateCreateInput) {
  return apiRequest(`/api/v1/documents/templates/${templateId}`, {
    method: "PUT",
    body: input,
  });
}
