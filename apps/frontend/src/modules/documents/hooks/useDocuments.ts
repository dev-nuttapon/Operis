import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  approveDocument,
  archiveDocument,
  baselineDocument,
  createDocument,
  createDocumentLink,
  createDocumentType,
  createDocumentVersion,
  deleteDocument,
  deleteDocumentVersion,
  getDocument,
  getDocumentType,
  listDocumentHistory,
  listDocumentTypes,
  listDocumentVersions,
  listDocuments,
  lookupDocumentsByIds,
  rejectDocument,
  submitDocument,
  updateDocument,
  updateDocumentType,
  type DocumentApprovalDecisionRequest,
  type DocumentCreateRequest,
  type DocumentHistoryListInput,
  type DocumentLinkRequest,
  type DocumentListInput,
  type DocumentTypeCreateRequest,
  type DocumentTypeListInput,
  type DocumentTypeUpdateRequest,
  type DocumentVersionListInput,
} from "../api/documentsApi";

export function useDocuments(input?: DocumentListInput, enabled = true) {
  return useQuery({
    queryKey: ["documents", "list", input],
    queryFn: ({ signal }) => listDocuments(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useDocument(documentId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["documents", "detail", documentId],
    queryFn: ({ signal }) => (documentId ? getDocument(documentId, signal) : Promise.reject(new Error("documentId is required"))),
    enabled: enabled && Boolean(documentId),
  });
}

export function useDocumentsByIds(documentIds: string[] | null, enabled = true) {
  return useQuery({
    queryKey: ["documents", "lookup", documentIds],
    queryFn: ({ signal }) =>
      documentIds && documentIds.length > 0 ? lookupDocumentsByIds(documentIds, signal) : Promise.resolve([]),
    enabled: enabled && Boolean(documentIds && documentIds.length > 0),
    staleTime: 15_000,
  });
}

export function useCreateDocument() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: DocumentCreateRequest) => createDocument(payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "list"] });
    },
  });
}

export function useUpdateDocument() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ documentId, payload }: { documentId: string; payload: DocumentCreateRequest }) =>
      updateDocument(documentId, payload),
    onSuccess: async (_, variables) => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "list"] });
      await queryClient.invalidateQueries({ queryKey: ["documents", "detail", variables.documentId] });
    },
  });
}

export function useDeleteDocument() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ documentId, reason }: { documentId: string; reason: string }) => deleteDocument(documentId, reason),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "list"] });
    },
  });
}

export function useCreateDocumentVersion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ documentId, file }: { documentId: string; file: File }) => createDocumentVersion({ documentId, file }),
    onSuccess: async (_, variables) => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "list"] });
      await queryClient.invalidateQueries({ queryKey: ["documents", "detail", variables.documentId] });
      await queryClient.invalidateQueries({ queryKey: ["documents", "versions", variables.documentId] });
    },
  });
}

export function useDocumentVersions(documentId: string | null, input?: DocumentVersionListInput, enabled = true) {
  return useQuery({
    queryKey: ["documents", "versions", documentId, input],
    queryFn: ({ signal }) =>
      documentId
        ? listDocumentVersions(documentId, input, signal)
        : Promise.resolve({ items: [], total: 0, page: 1, pageSize: 10 }),
    enabled: enabled && Boolean(documentId),
  });
}

export function useDocumentHistory(documentId: string | null, input?: DocumentHistoryListInput, enabled = true) {
  return useQuery({
    queryKey: ["documents", "history", documentId, input],
    queryFn: ({ signal }) =>
      documentId
        ? listDocumentHistory(documentId, input, signal)
        : Promise.resolve({ items: [], total: 0, page: 1, pageSize: 10 }),
    enabled: enabled && Boolean(documentId),
  });
}

export function useDeleteDocumentVersion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ documentId, versionId }: { documentId: string; versionId: string }) =>
      deleteDocumentVersion(documentId, versionId),
    onSuccess: async (_, variables) => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "versions", variables.documentId] });
      await queryClient.invalidateQueries({ queryKey: ["documents", "detail", variables.documentId] });
      await queryClient.invalidateQueries({ queryKey: ["documents", "list"] });
    },
  });
}

function createWorkflowMutation(
  action: (documentId: string, payload: DocumentApprovalDecisionRequest) => Promise<unknown>,
) {
  return function useWorkflowMutation() {
    const queryClient = useQueryClient();

    return useMutation({
      mutationFn: ({ documentId, payload }: { documentId: string; payload: DocumentApprovalDecisionRequest }) =>
        action(documentId, payload),
      onSuccess: async (_, variables) => {
        await queryClient.invalidateQueries({ queryKey: ["documents", "detail", variables.documentId] });
        await queryClient.invalidateQueries({ queryKey: ["documents", "list"] });
      },
    });
  };
}

export const useSubmitDocument = createWorkflowMutation(submitDocument);
export const useApproveDocument = createWorkflowMutation(approveDocument);
export const useRejectDocument = createWorkflowMutation(rejectDocument);

export function useBaselineDocument() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ documentId }: { documentId: string }) => baselineDocument(documentId),
    onSuccess: async (_, variables) => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "detail", variables.documentId] });
      await queryClient.invalidateQueries({ queryKey: ["documents", "list"] });
    },
  });
}

export function useArchiveDocument() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ documentId, reason }: { documentId: string; reason: string }) => archiveDocument(documentId, reason),
    onSuccess: async (_, variables) => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "detail", variables.documentId] });
      await queryClient.invalidateQueries({ queryKey: ["documents", "list"] });
    },
  });
}

export function useCreateDocumentLink() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ documentId, payload }: { documentId: string; payload: DocumentLinkRequest }) =>
      createDocumentLink(documentId, payload),
    onSuccess: async (_, variables) => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "detail", variables.documentId] });
    },
  });
}

export function useDocumentTypes(input?: DocumentTypeListInput, enabled = true) {
  return useQuery({
    queryKey: ["documents", "types", input],
    queryFn: ({ signal }) => listDocumentTypes(input, signal),
    staleTime: 60_000,
    enabled,
  });
}

export function useDocumentType(documentTypeId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["documents", "types", "detail", documentTypeId],
    queryFn: ({ signal }) =>
      documentTypeId ? getDocumentType(documentTypeId, signal) : Promise.reject(new Error("documentTypeId is required")),
    enabled: enabled && Boolean(documentTypeId),
  });
}

export function useCreateDocumentType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: DocumentTypeCreateRequest) => createDocumentType(payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "types"] });
    },
  });
}

export function useUpdateDocumentType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ documentTypeId, payload }: { documentTypeId: string; payload: DocumentTypeUpdateRequest }) =>
      updateDocumentType(documentTypeId, payload),
    onSuccess: async (_, variables) => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "types"] });
      await queryClient.invalidateQueries({ queryKey: ["documents", "types", "detail", variables.documentTypeId] });
    },
  });
}
