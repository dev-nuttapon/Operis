import { useDocuments } from "./useDocuments";

export function useDocumentDashboard(canReadDocuments = true, page = 1, pageSize = 10, search = "") {
  const documentsQuery = useDocuments({ page, pageSize, search }, canReadDocuments);

  return {
    documentsQuery,
  };
}
