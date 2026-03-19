import { renderHook } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { useDocumentDashboard } from "./useDocumentDashboard";
import { useDocuments } from "./useDocuments";

vi.mock("./useDocuments", () => ({
  useDocuments: vi.fn(),
}));

describe("useDocumentDashboard", () => {
  it("returns the documents query result", () => {
    vi.mocked(useDocuments).mockReturnValue({
      data: {
        items: [
          { id: "1", documentName: "Doc 1", fileName: "doc-1.pdf", contentType: "application/pdf", sizeBytes: 1024, uploadedByUserId: "u1", uploadedAt: "2026-03-01T00:00:00Z", versionCode: "1.0", revision: 1, publishedVersionCode: null, publishedRevision: null },
          { id: "2", documentName: "Doc 2", fileName: "doc-2.pdf", contentType: "application/pdf", sizeBytes: 1024, uploadedByUserId: "u1", uploadedAt: "2026-03-02T00:00:00Z", versionCode: "1.0", revision: 1, publishedVersionCode: null, publishedRevision: null },
          { id: "3", documentName: "Doc 3", fileName: "doc-3.pdf", contentType: "application/pdf", sizeBytes: 1024, uploadedByUserId: "u1", uploadedAt: "2026-03-03T00:00:00Z", versionCode: "1.0", revision: 1, publishedVersionCode: null, publishedRevision: null },
          { id: "4", documentName: "Doc 4", fileName: "doc-4.pdf", contentType: "application/pdf", sizeBytes: 1024, uploadedByUserId: "u1", uploadedAt: "2026-03-04T00:00:00Z", versionCode: "1.0", revision: 1, publishedVersionCode: null, publishedRevision: null },
          { id: "5", documentName: "Doc 5", fileName: "doc-5.pdf", contentType: "application/pdf", sizeBytes: 1024, uploadedByUserId: "u1", uploadedAt: "2026-03-05T00:00:00Z", versionCode: "1.0", revision: 1, publishedVersionCode: null, publishedRevision: null },
          { id: "6", documentName: "Doc 6", fileName: "doc-6.pdf", contentType: "application/pdf", sizeBytes: 1024, uploadedByUserId: "u1", uploadedAt: "2026-03-06T00:00:00Z", versionCode: "1.0", revision: 1, publishedVersionCode: null, publishedRevision: null },
        ],
        total: 6,
        page: 1,
        pageSize: 10,
      },
      isLoading: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof useDocuments>);

    const { result } = renderHook(() => useDocumentDashboard());

    expect(result.current.documentsQuery.data?.items).toHaveLength(6);
  });
});
