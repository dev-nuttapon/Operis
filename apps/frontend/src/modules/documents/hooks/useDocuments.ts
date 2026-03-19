import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createDocument,
  createDocumentVersion,
  deleteDocument,
  deleteDocumentVersion,
  listDocumentHistory,
  listDocumentVersions,
  listDocuments,
  lookupDocumentsByIds,
  publishDocumentVersion,
  unpublishDocumentVersion,
  updateDocument,
  type DocumentHistoryListInput,
  type DocumentListInput,
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
    mutationFn: ({ documentName }: { documentName: string }) => createDocument({ documentName }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "list"] });
    },
  });
}

export function useCreateDocumentVersion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ documentId, versionCode, file }: { documentId: string; versionCode: string; file: File }) =>
      createDocumentVersion({ documentId, versionCode, file }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "list"] });
    },
  });
}

export function useDocumentVersions(documentId: string | null, input?: DocumentVersionListInput, enabled = true) {
  return useQuery({
    queryKey: ["documents", "versions", documentId, input],
    queryFn: ({ signal }) => (documentId ? listDocumentVersions(documentId, input, signal) : Promise.resolve({ items: [], total: 0, page: 1, pageSize: 10 })),
    enabled: enabled && Boolean(documentId),
  });
}

export function useDocumentHistory(documentId: string | null, input?: DocumentHistoryListInput, enabled = true) {
  return useQuery({
    queryKey: ["documents", "history", documentId, input],
    queryFn: ({ signal }) => (documentId ? listDocumentHistory(documentId, input, signal) : Promise.resolve({ items: [], total: 0, page: 1, pageSize: 10 })),
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
      await queryClient.invalidateQueries({ queryKey: ["documents", "list"] });
    },
  });
}

export function usePublishDocumentVersion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ documentId, versionId }: { documentId: string; versionId: string }) =>
      publishDocumentVersion(documentId, versionId),
    onSuccess: async (_, variables) => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "versions", variables.documentId] });
      await queryClient.invalidateQueries({ queryKey: ["documents", "list"] });
    },
  });
}

export function useUnpublishDocumentVersion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ documentId }: { documentId: string }) => unpublishDocumentVersion(documentId),
    onSuccess: async (_, variables) => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "versions", variables.documentId] });
      await queryClient.invalidateQueries({ queryKey: ["documents", "list"] });
    },
  });
}

export function useUpdateDocument() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ documentId, documentName }: { documentId: string; documentName: string }) =>
      updateDocument(documentId, documentName),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "list"] });
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
