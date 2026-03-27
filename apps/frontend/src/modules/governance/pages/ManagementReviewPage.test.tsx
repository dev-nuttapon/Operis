import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { ManagementReviewPage } from "./ManagementReviewPage";

vi.mock("../../../shared/authz/usePermissions", () => ({
  usePermissions: vi.fn(),
}));

vi.mock("../../users/public", () => ({
  useProjectList: vi.fn(),
}));

vi.mock("../hooks/useGovernance", () => ({
  useManagementReviews: vi.fn(),
  useCreateManagementReview: vi.fn(),
  useUpdateManagementReview: vi.fn(),
}));

import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectList } from "../../users/public";
import { useCreateManagementReview, useManagementReviews, useUpdateManagementReview } from "../hooks/useGovernance";

describe("ManagementReviewPage", () => {
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
    vi.mocked(useManagementReviews).mockReturnValue({
      data: {
        items: [
          {
            id: "review-1",
            projectId: "project-1",
            projectName: "Alpha",
            reviewCode: "MR-001",
            title: "Monthly management review",
            reviewPeriod: "2026-03",
            scheduledAt: "2026-03-27T09:00:00Z",
            facilitatorUserId: "director@example.com",
            status: "scheduled",
            openActionCount: 2,
            updatedAt: "2026-03-27T09:00:00Z",
          },
        ],
        total: 1,
        page: 1,
        pageSize: 25,
      },
      isLoading: false,
    } as unknown as ReturnType<typeof useManagementReviews>);
    vi.mocked(useCreateManagementReview).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as unknown as ReturnType<typeof useCreateManagementReview>);
    vi.mocked(useUpdateManagementReview).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as unknown as ReturnType<typeof useUpdateManagementReview>);
  });

  it("renders the management review register and rows", () => {
    render(
      <MemoryRouter>
        <ManagementReviewPage />
      </MemoryRouter>,
    );

    expect(screen.getByText("Management Reviews")).toBeInTheDocument();
    expect(screen.getByText("Monthly management review")).toBeInTheDocument();
    expect(screen.getByText("MR-001")).toBeInTheDocument();
    expect(screen.getByText("Open Actions")).toBeInTheDocument();
  }, 15000);
});
