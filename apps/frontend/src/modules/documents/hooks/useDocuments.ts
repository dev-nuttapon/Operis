import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createDocument, createDocumentVersion, deleteDocument, listDocumentVersions, listDocuments, updateDocument } from "../api/documentsApi";

export function useDocuments(enabled = true) {
  return useQuery({
    queryKey: ["documents", "list"],
    queryFn: ({ signal }) => listDocuments(signal),
    staleTime: 15_000,
    enabled,
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

export function useDocumentVersions(documentId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["documents", "versions", documentId],
    queryFn: ({ signal }) => (documentId ? listDocumentVersions(documentId, signal) : Promise.resolve([])),
    enabled: enabled && Boolean(documentId),
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
