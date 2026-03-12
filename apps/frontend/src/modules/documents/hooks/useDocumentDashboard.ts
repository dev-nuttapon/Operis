import { useMemo, useState } from "react";
import { useDocuments } from "./useDocuments";
import type { DocumentFormValues } from "../types/documentForm";

export function useDocumentDashboard() {
  const documentsQuery = useDocuments();
  const [submittedData, setSubmittedData] = useState<DocumentFormValues | null>(null);

  const latestDocuments = useMemo(
    () => (documentsQuery.data ?? []).slice(0, 5),
    [documentsQuery.data],
  );

  return {
    documentsQuery,
    latestDocuments,
    submittedData,
    setSubmittedData,
  };
}
