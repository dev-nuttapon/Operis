import { renderHook, act } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { useWorkflowDefinitions } from "./useWorkflowDefinitions";
import { useWorkflowDefinitionsScreen } from "./useWorkflowDefinitionsScreen";

vi.mock("./useWorkflowDefinitions", () => ({
  useWorkflowDefinitions: vi.fn(),
}));

describe("useWorkflowDefinitionsScreen", () => {
  it("builds status summary and filters definitions by selected status", () => {
    vi.mocked(useWorkflowDefinitions).mockReturnValue({
      data: [
        { id: "1", code: "document-review", name: "Document Review", status: "draft" },
        { id: "2", code: "policy-approval", name: "Policy Approval", status: "active" },
        { id: "3", code: "asset-onboarding", name: "Asset Onboarding", status: "archived" },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as ReturnType<typeof useWorkflowDefinitions>);

    const { result } = renderHook(() => useWorkflowDefinitionsScreen());

    expect(result.current.statusSummary).toEqual({
      all: 3,
      draft: 1,
      active: 1,
      archived: 1,
    });
    expect(result.current.filteredDefinitions).toHaveLength(3);

    act(() => {
      result.current.setStatusFilter("active");
    });

    expect(result.current.filteredDefinitions.map((item) => item.id)).toEqual(["2"]);
  });
});
