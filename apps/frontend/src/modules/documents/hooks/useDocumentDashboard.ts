import { useDocuments } from "./useDocuments";

export function useDocumentDashboard(canReadDocuments = true, page = 1, pageSize = 10) {
  const documentsQuery = useDocuments({ page, pageSize }, canReadDocuments);

  return {
    documentsQuery,
  };
}
