import { useMemo, useState } from "react";
import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { listDocuments } from "../api/documentsApi";
import {
  createDocumentTemplate,
  getDocumentTemplate,
  listDocumentTemplates,
  updateDocumentTemplate,
  type DocumentTemplateListInput,
} from "../api/documentTemplatesApi";
import type { DocumentTemplateCreateInput } from "../types/documentTemplates";
import type { DocumentListItemView } from "../types/documents";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";

export function useDocumentTemplates(input: DocumentTemplateListInput, enabled = true) {
  return useQuery({
    queryKey: ["documents", "templates", input],
    queryFn: ({ signal }) => listDocumentTemplates(input, signal),
    staleTime: 15_000,
    enabled,
  });
}

export function useCreateDocumentTemplate() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: DocumentTemplateCreateInput) => createDocumentTemplate(payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "templates"] });
    },
  });
}

export function useDocumentTemplate(templateId: string | null, enabled = true) {
  return useQuery({
    queryKey: ["documents", "templates", templateId],
    queryFn: ({ signal }) => (templateId ? getDocumentTemplate(templateId, signal) : Promise.resolve(null)),
    enabled: enabled && Boolean(templateId),
  });
}

export function useUpdateDocumentTemplate() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ templateId, payload }: { templateId: string; payload: DocumentTemplateCreateInput }) =>
      updateDocumentTemplate(templateId, payload),
    onSuccess: async (_, variables) => {
      await queryClient.invalidateQueries({ queryKey: ["documents", "templates"] });
      await queryClient.invalidateQueries({ queryKey: ["documents", "templates", variables.templateId] });
    },
  });
}

type DocumentOption = { label: string; value: string; meta: DocumentListItemView };

export function useDocumentOptions(enabled: boolean) {
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search, 300);
  const pageSize = 10;

  const documentsQuery = useInfiniteQuery({
    queryKey: ["documents", "options", { search: debouncedSearch }],
    enabled,
    queryFn: ({ signal, pageParam }) =>
      listDocuments({ page: pageParam as number, pageSize, search: debouncedSearch }, signal),
    initialPageParam: 1,
    getNextPageParam: (lastPage, allPages) => {
      const loaded = allPages.reduce((sum, page) => sum + page.items.length, 0);
      if (lastPage.total && loaded < lastPage.total) {
        return allPages.length + 1;
      }
      return lastPage.items.length === pageSize ? allPages.length + 1 : undefined;
    },
    staleTime: 60_000,
  });

  const options = useMemo<DocumentOption[]>(() => {
    const items = documentsQuery.data?.pages.flatMap((page) => page.items) ?? [];
  return items.map((item: DocumentListItemView) => ({
    label: item.documentName,
    value: item.id,
    meta: item,
  }));
  }, [documentsQuery.data]);

  const handleSearch = (value: string) => {
    setSearch(value);
  };

  const loadMore = () => {
    if (!documentsQuery.isFetching && documentsQuery.hasNextPage) {
      void documentsQuery.fetchNextPage();
    }
  };

  return useMemo(
    () => ({
      options,
      search,
      hasMore: Boolean(documentsQuery.hasNextPage),
      loading: documentsQuery.isFetching,
      onSearch: handleSearch,
      onLoadMore: loadMore,
    }),
    [documentsQuery.hasNextPage, documentsQuery.isFetching, options, search],
  );
}
