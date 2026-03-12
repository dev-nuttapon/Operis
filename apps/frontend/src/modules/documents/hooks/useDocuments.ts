import { useQuery } from "@tanstack/react-query";
import { listDocuments } from "../api/documentsApi";

export function useDocuments() {
  return useQuery({
    queryKey: ["documents", "list"],
    queryFn: ({ signal }) => listDocuments(signal),
    staleTime: 15_000,
  });
}
