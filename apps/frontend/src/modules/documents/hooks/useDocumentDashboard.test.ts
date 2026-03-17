import { renderHook } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { useDocumentDashboard } from "./useDocumentDashboard";
import { useDocuments } from "./useDocuments";

vi.mock("./useDocuments", () => ({
  useDocuments: vi.fn(),
}));

describe("useDocumentDashboard", () => {
  it("returns only the latest five documents from the query result", () => {
    vi.mocked(useDocuments).mockReturnValue({
      data: [
        { id: "1", documentName: "Doc 1", fileName: "doc-1.pdf", contentType: "application/pdf", sizeBytes: 1024, uploadedByUserId: "u1", uploadedAt: "2026-03-01T00:00:00Z", versionCode: "1.0", revision: 1 },
        { id: "2", documentName: "Doc 2", fileName: "doc-2.pdf", contentType: "application/pdf", sizeBytes: 1024, uploadedByUserId: "u1", uploadedAt: "2026-03-02T00:00:00Z", versionCode: "1.0", revision: 1 },
        { id: "3", documentName: "Doc 3", fileName: "doc-3.pdf", contentType: "application/pdf", sizeBytes: 1024, uploadedByUserId: "u1", uploadedAt: "2026-03-03T00:00:00Z", versionCode: "1.0", revision: 1 },
        { id: "4", documentName: "Doc 4", fileName: "doc-4.pdf", contentType: "application/pdf", sizeBytes: 1024, uploadedByUserId: "u1", uploadedAt: "2026-03-04T00:00:00Z", versionCode: "1.0", revision: 1 },
        { id: "5", documentName: "Doc 5", fileName: "doc-5.pdf", contentType: "application/pdf", sizeBytes: 1024, uploadedByUserId: "u1", uploadedAt: "2026-03-05T00:00:00Z", versionCode: "1.0", revision: 1 },
        { id: "6", documentName: "Doc 6", fileName: "doc-6.pdf", contentType: "application/pdf", sizeBytes: 1024, uploadedByUserId: "u1", uploadedAt: "2026-03-06T00:00:00Z", versionCode: "1.0", revision: 1 },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as ReturnType<typeof useDocuments>);

    const { result } = renderHook(() => useDocumentDashboard());

    expect(result.current.latestDocuments).toHaveLength(5);
    expect(result.current.latestDocuments.map((item) => item.id)).toEqual(["1", "2", "3", "4", "5"]);
  });
});
