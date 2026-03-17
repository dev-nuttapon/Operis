import { useDocuments } from "./useDocuments";

export function useDocumentDashboard(canReadDocuments = true) {
  const documentsQuery = useDocuments(canReadDocuments);

  return {
    documentsQuery,
  };
}
