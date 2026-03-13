import { describe, expect, it, vi } from "vitest";
import { apiRequest } from "../../../shared/lib/apiClient";
import { createWorkflowDefinition, listWorkflowDefinitions } from "./workflowsApi";

vi.mock("../../../shared/lib/apiClient", () => ({
  apiRequest: vi.fn(),
}));

describe("listWorkflowDefinitions", () => {
  it("calls the workflows definitions endpoint", async () => {
    vi.mocked(apiRequest).mockResolvedValueOnce([]);

    await listWorkflowDefinitions();

    expect(apiRequest).toHaveBeenCalledWith("/api/v1/workflows/definitions", { signal: undefined });
  });
});

describe("createWorkflowDefinition", () => {
  it("posts to the workflows definitions endpoint", async () => {
    vi.mocked(apiRequest).mockResolvedValueOnce({
      id: "1",
      code: "document-review",
      name: "Document Review",
      status: "draft",
    });

    await createWorkflowDefinition({ name: "Document Review" });

    expect(apiRequest).toHaveBeenCalledWith("/api/v1/workflows/definitions", {
      method: "POST",
      body: { name: "Document Review" },
    });
  });
});
