import { useMemo } from "react";
import { useDocuments, useUploadDocument } from "./useDocuments";

export function useDocumentDashboard(canReadDocuments = true) {
  const documentsQuery = useDocuments(canReadDocuments);
  const uploadDocumentMutation = useUploadDocument();

  const latestDocuments = useMemo(
    () => (documentsQuery.data ?? []).slice(0, 5),
    [documentsQuery.data],
  );

  return {
    documentsQuery,
    uploadDocumentMutation,
    latestDocuments,
  };
}
