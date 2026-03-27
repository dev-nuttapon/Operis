import { beforeEach, describe, expect, it, vi } from "vitest";
import { MemoryRouter } from "react-router-dom";
import { render, screen } from "@testing-library/react";
import { EvidenceCompletenessPage } from "./EvidenceCompletenessPage";

vi.mock("../../../shared/authz/usePermissions", () => ({
  usePermissions: vi.fn(),
}));

vi.mock("../../users/public", () => ({
  useProjectList: vi.fn(),
}));

vi.mock("../hooks/useAuditLogs", () => ({
  useEvidenceRules: vi.fn(),
  useEvidenceRuleResults: vi.fn(),
  useCreateEvidenceRule: vi.fn(),
  useUpdateEvidenceRule: vi.fn(),
  useEvaluateEvidenceRules: vi.fn(),
}));

import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectList } from "../../users/public";
import {
  useCreateEvidenceRule,
  useEvaluateEvidenceRules,
  useEvidenceRuleResults,
  useEvidenceRules,
  useUpdateEvidenceRule,
} from "../hooks/useAuditLogs";

describe("EvidenceCompletenessPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(usePermissions).mockReturnValue({
      roles: [],
      permissions: [],
      hasAnyPermission: () => true,
      hasPermission: () => true,
      hasAllPermissions: () => true,
    } as ReturnType<typeof usePermissions>);
    vi.mocked(useProjectList).mockReturnValue({
      data: { items: [{ id: "project-1", code: "PRJ-001", name: "Alpha" }] },
      isLoading: false,
    } as ReturnType<typeof useProjectList>);
    vi.mocked(useEvidenceRules).mockReturnValue({
      data: {
        items: [
          {
            id: "rule-1",
            ruleCode: "EV-001",
            title: "Requirement baseline required",
            processArea: "requirements-traceability",
            artifactType: "requirement_baseline",
            projectId: null,
            status: "active",
            expressionType: "required",
            updatedAt: "2026-03-27T00:00:00Z",
          },
        ],
      },
      isLoading: false,
    } as unknown as ReturnType<typeof useEvidenceRules>);
    vi.mocked(useEvidenceRuleResults).mockReturnValue({
      data: {
        items: [
          {
            id: "result-1",
            scopeType: "project",
            scopeRef: "project-1",
            projectId: "project-1",
            projectCode: "PRJ-001",
            processArea: "requirements-traceability",
            status: "completed",
            evaluatedRuleCount: 1,
            missingItemCount: 1,
            startedAt: "2026-03-27T00:00:00Z",
            completedAt: "2026-03-27T00:05:00Z",
          },
        ],
      },
      isLoading: false,
    } as unknown as ReturnType<typeof useEvidenceRuleResults>);
    vi.mocked(useCreateEvidenceRule).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as unknown as ReturnType<typeof useCreateEvidenceRule>);
    vi.mocked(useUpdateEvidenceRule).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as unknown as ReturnType<typeof useUpdateEvidenceRule>);
    vi.mocked(useEvaluateEvidenceRules).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as unknown as ReturnType<typeof useEvaluateEvidenceRules>);
  });

  it("renders evidence rule and evaluation result tables", () => {
    render(
      <MemoryRouter>
        <EvidenceCompletenessPage />
      </MemoryRouter>,
    );

    expect(screen.getByText("Evidence Completeness")).toBeInTheDocument();
    expect(screen.getByText("Evidence Rules")).toBeInTheDocument();
    expect(screen.getByText("Evaluation Results")).toBeInTheDocument();
    expect(screen.getByText("EV-001")).toBeInTheDocument();
    expect(screen.getByText("Requirement baseline required")).toBeInTheDocument();
    expect(screen.getByText("PRJ-001 · project")).toBeInTheDocument();
  }, 15000);
});
