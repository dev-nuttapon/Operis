import { describe, expect, it, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { ComplianceDashboardPage } from "./ComplianceDashboardPage";

vi.mock("../../../shared/authz/usePermissions", () => ({
  usePermissions: vi.fn(),
}));

vi.mock("../../users/public", () => ({
  useProjectList: vi.fn(),
}));

vi.mock("../hooks/useGovernance", () => ({
  useComplianceDashboard: vi.fn(),
  useComplianceDrilldown: vi.fn(),
  useUpdateComplianceDashboardPreferences: vi.fn(),
}));

import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectList } from "../../users/public";
import { useComplianceDashboard, useComplianceDrilldown, useUpdateComplianceDashboardPreferences } from "../hooks/useGovernance";

describe("ComplianceDashboardPage", () => {
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
    vi.mocked(useComplianceDashboard).mockReturnValue({
      data: {
        summary: {
          projectsInGoodStanding: 1,
          projectsWithMissingArtifacts: 0,
          overdueApprovals: 2,
          staleBaselines: 1,
          openCapa: 3,
          openAuditFindings: 1,
          openSecurityItems: 2,
        },
        projects: [
          {
            projectId: "project-1",
            projectCode: "PRJ-001",
            projectName: "Alpha",
            projectStatus: "active",
            projectPhase: "verification",
            readinessScore: 78,
            readinessState: "at_risk",
            missingArtifactCount: 1,
            overdueApprovalCount: 2,
            staleBaselineCount: 1,
            openCapaCount: 0,
            openAuditFindingCount: 0,
            openSecurityItemCount: 0,
          },
        ],
        processAreas: [
          {
            processArea: "requirements-traceability",
            label: "Requirements & Traceability",
            projectCount: 1,
            atRiskProjectCount: 1,
            missingArtifactCount: 1,
            overdueApprovalCount: 0,
            staleBaselineCount: 0,
            openCapaCount: 0,
            openAuditFindingCount: 0,
            openSecurityItemCount: 0,
          },
        ],
        generatedAt: "2026-03-27T00:00:00Z",
        filters: {
          periodDays: 30,
          showOnlyAtRisk: false,
        },
      },
      isLoading: false,
    } as ReturnType<typeof useComplianceDashboard>);
    vi.mocked(useComplianceDrilldown).mockReturnValue({
      data: { issueType: "missing-artifact", generatedAt: "2026-03-27T00:00:00Z", rows: [] },
      isLoading: false,
    } as unknown as ReturnType<typeof useComplianceDrilldown>);
    vi.mocked(useUpdateComplianceDashboardPreferences).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as unknown as ReturnType<typeof useUpdateComplianceDashboardPreferences>);
  });

  it("renders the compliance summary and readiness tables", async () => {
    render(
      <MemoryRouter>
        <ComplianceDashboardPage />
      </MemoryRouter>,
    );

    expect(screen.getByText("Compliance Dashboard")).toBeInTheDocument();
    expect(screen.getByText("Project Readiness")).toBeInTheDocument();
    expect(screen.getByText("Process Area Readiness")).toBeInTheDocument();
    expect(screen.getByText("PRJ-001")).toBeInTheDocument();
    expect(screen.getByText("Requirements & Traceability")).toBeInTheDocument();
    expect(screen.getByText("Overdue Approvals")).toBeInTheDocument();
  }, 15000);
});
