import { describe, expect, it, vi } from "vitest";
import { apiRequest } from "../../../shared/lib/apiClient";
import { activateWorkflowDefinition, archiveWorkflowDefinition, createWorkflowDefinition, listWorkflowDefinitions, updateWorkflowDefinition } from "./workflowsApi";

vi.mock("../../../shared/lib/apiClient", () => ({
  apiRequest: vi.fn(),
}));

describe("listWorkflowDefinitions", () => {
  it("calls the workflows definitions endpoint", async () => {
    vi.mocked(apiRequest).mockResolvedValueOnce({
      items: [],
      total: 0,
      page: 1,
      pageSize: 10,
      statusSummary: { all: 0, draft: 0, active: 0, archived: 0 },
    });

    await listWorkflowDefinitions();

    expect(apiRequest).toHaveBeenCalledWith("/api/v1/steps/definitions", { signal: undefined });
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

    await createWorkflowDefinition({
      name: "Document Review",
      steps: [
        {
          name: "Submit",
          stepType: "submit",
          displayOrder: 1,
          isRequired: true,
          roleIds: ["role-1"],
        },
      ],
    });

    expect(apiRequest).toHaveBeenCalledWith("/api/v1/steps/definitions", {
      method: "POST",
      body: {
        name: "Document Review",
        steps: [
          {
            name: "Submit",
            stepType: "submit",
            displayOrder: 1,
            isRequired: true,
            roleIds: ["role-1"],
          },
        ],
      },
    });
  });
});

describe("activateWorkflowDefinition", () => {
  it("posts to the activate endpoint", async () => {
    vi.mocked(apiRequest).mockResolvedValueOnce({
      id: "1",
      code: "document-review",
      name: "Document Review",
      status: "active",
    });

    await activateWorkflowDefinition("1");

    expect(apiRequest).toHaveBeenCalledWith("/api/v1/steps/definitions/1/activate", {
      method: "POST",
    });
  });
});

describe("updateWorkflowDefinition", () => {
  it("puts to the update endpoint", async () => {
    vi.mocked(apiRequest).mockResolvedValueOnce({
      id: "1",
      code: "policy-approval",
      name: "Policy Approval",
      status: "draft",
    });

    await updateWorkflowDefinition({
      workflowDefinitionId: "1",
      name: "Policy Approval",
      steps: [
        {
          name: "Review",
          stepType: "review",
          displayOrder: 1,
          isRequired: true,
          roleIds: ["role-1"],
        },
      ],
    });

    expect(apiRequest).toHaveBeenCalledWith("/api/v1/steps/definitions/1", {
      method: "PUT",
      body: {
        name: "Policy Approval",
        steps: [
          {
            name: "Review",
            stepType: "review",
            displayOrder: 1,
            isRequired: true,
            roleIds: ["role-1"],
          },
        ],
      },
    });
  });
});

describe("archiveWorkflowDefinition", () => {
  it("posts to the archive endpoint", async () => {
    vi.mocked(apiRequest).mockResolvedValueOnce({
      id: "1",
      code: "document-review",
      name: "Document Review",
      status: "archived",
    });

    await archiveWorkflowDefinition("1");

    expect(apiRequest).toHaveBeenCalledWith("/api/v1/steps/definitions/1/archive", {
      method: "POST",
    });
  });
});
