import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  archiveMasterDataCatalog,
  createMasterDataCatalog,
  getMasterDataCatalog,
  listMasterDataCatalog,
  updateMasterDataCatalog,
} from "../api/usersApi";
import type {
  CreateMasterDataCatalogInput,
  ListMasterDataCatalogInput,
  SoftDeleteInput,
  UpdateMasterDataCatalogInput,
} from "../types/users";

const masterDataCatalogKey = ["admin", "master-data-catalog"];

export function useMasterDataCatalog(input: { list: ListMasterDataCatalogInput; detailId?: string }) {
  const queryClient = useQueryClient();

  const listQuery = useQuery({
    queryKey: [...masterDataCatalogKey, input.list],
    queryFn: ({ signal }) => listMasterDataCatalog(input.list, signal),
    staleTime: 15_000,
  });

  const detailQuery = useQuery({
    queryKey: [...masterDataCatalogKey, "detail", input.detailId],
    enabled: Boolean(input.detailId),
    queryFn: ({ signal }) => getMasterDataCatalog(input.detailId!, signal),
    staleTime: 15_000,
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: masterDataCatalogKey });

  const createMutation = useMutation({
    mutationFn: (payload: CreateMasterDataCatalogInput) => createMasterDataCatalog(payload),
    onSuccess: invalidate,
  });

  const updateMutation = useMutation({
    mutationFn: (payload: UpdateMasterDataCatalogInput) => updateMasterDataCatalog(payload),
    onSuccess: invalidate,
  });

  const archiveMutation = useMutation({
    mutationFn: ({ id, input: payload }: { id: string; input: SoftDeleteInput }) => archiveMasterDataCatalog(id, payload),
    onSuccess: invalidate,
  });

  return {
    listQuery,
    detailQuery,
    createMutation,
    updateMutation,
    archiveMutation,
  };
}
