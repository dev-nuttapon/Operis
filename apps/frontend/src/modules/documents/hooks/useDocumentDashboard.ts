import { useMemo } from "react";
import { useDocuments } from "./useDocuments";

export function useDocumentDashboard(canReadDocuments = true) {
  const documentsQuery = useDocuments(canReadDocuments);

  const latestDocuments = useMemo(
    () => (documentsQuery.data ?? []).slice(0, 5),
    [documentsQuery.data],
  );

  return {
    documentsQuery,
    latestDocuments,
  };
}
