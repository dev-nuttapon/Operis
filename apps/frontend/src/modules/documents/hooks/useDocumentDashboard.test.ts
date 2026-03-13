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
        { id: "1", fileName: "doc-1.pdf", uploadedAt: "2026-03-01T00:00:00Z" },
        { id: "2", fileName: "doc-2.pdf", uploadedAt: "2026-03-02T00:00:00Z" },
        { id: "3", fileName: "doc-3.pdf", uploadedAt: "2026-03-03T00:00:00Z" },
        { id: "4", fileName: "doc-4.pdf", uploadedAt: "2026-03-04T00:00:00Z" },
        { id: "5", fileName: "doc-5.pdf", uploadedAt: "2026-03-05T00:00:00Z" },
        { id: "6", fileName: "doc-6.pdf", uploadedAt: "2026-03-06T00:00:00Z" },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as ReturnType<typeof useDocuments>);

    const { result } = renderHook(() => useDocumentDashboard());

    expect(result.current.latestDocuments).toHaveLength(5);
    expect(result.current.latestDocuments.map((item) => item.id)).toEqual(["1", "2", "3", "4", "5"]);
    expect(result.current.submittedData).toBeNull();
  });
});
