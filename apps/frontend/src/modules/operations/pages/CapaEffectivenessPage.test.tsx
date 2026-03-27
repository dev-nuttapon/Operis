import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { App } from "antd";
import { CapaEffectivenessPage } from "./CapaEffectivenessPage";

vi.mock("../../../shared/authz/usePermissions", () => ({
  usePermissions: vi.fn(),
}));

vi.mock("../hooks/useOperations", () => ({
  useCapaEffectivenessReviews: vi.fn(),
  useCapaRecords: vi.fn(),
  useCreateCapaEffectivenessReview: vi.fn(),
  useReopenCapa: vi.fn(),
}));

import { usePermissions } from "../../../shared/authz/usePermissions";
import { useCapaEffectivenessReviews, useCapaRecords, useCreateCapaEffectivenessReview, useReopenCapa } from "../hooks/useOperations";

describe("CapaEffectivenessPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(usePermissions).mockReturnValue({
      roles: [],
      permissions: [],
      hasAnyPermission: () => true,
      hasPermission: () => true,
      hasAllPermissions: () => true,
    } as ReturnType<typeof usePermissions>);
    vi.mocked(useCapaEffectivenessReviews).mockReturnValue({
      data: {
        items: [
          {
            id: "review-1",
            capaRecordId: "capa-1",
            capaTitle: "Close approval gap",
            capaOwnerUserId: "owner@example.com",
            capaStatus: "closed",
            effectivenessResult: "ineffective",
            evidenceRef: "minio://evidence/capa/effectiveness-review.pdf",
            reviewSummary: "Follow-up sampling found residual routing defects.",
            status: "ineffective",
            reviewedBy: "qa@example.com",
            reviewedAt: "2026-03-27T10:00:00Z",
            reopenedAt: null,
            reopenedBy: null,
            reopenReason: null,
            createdAt: "2026-03-27T10:00:00Z",
            updatedAt: null,
          },
        ],
        total: 1,
        page: 1,
        pageSize: 25,
      },
      isLoading: false,
    } as unknown as ReturnType<typeof useCapaEffectivenessReviews>);
    vi.mocked(useCapaRecords).mockReturnValue({
      data: {
        items: [
          {
            id: "capa-1",
            sourceType: "audit_finding",
            sourceRef: "AF-101",
            title: "Close approval gap",
            ownerUserId: "owner@example.com",
            rootCauseSummary: "Role handoff issue",
            status: "closed",
            actions: [],
            effectivenessReviews: [],
            createdAt: "2026-03-27T10:00:00Z",
            updatedAt: null,
            verifiedAt: "2026-03-26T10:00:00Z",
            verifiedBy: "qa@example.com",
            closedAt: "2026-03-27T09:00:00Z",
            closedBy: "approver@example.com",
          },
        ],
        total: 1,
        page: 1,
        pageSize: 100,
      },
      isLoading: false,
    } as unknown as ReturnType<typeof useCapaRecords>);
    vi.mocked(useCreateCapaEffectivenessReview).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as unknown as ReturnType<typeof useCreateCapaEffectivenessReview>);
    vi.mocked(useReopenCapa).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as unknown as ReturnType<typeof useReopenCapa>);
  });

  it("renders CAPA effectiveness rows and reopen action", () => {
    render(
      <App>
        <CapaEffectivenessPage />
      </App>,
    );

    expect(screen.getByText("CAPA Effectiveness")).toBeInTheDocument();
    expect(screen.getByText("Close approval gap")).toBeInTheDocument();
    expect(screen.getAllByText("ineffective").length).toBeGreaterThan(0);
    expect(screen.getByRole("button", { name: /Reopen CAPA/i })).toBeInTheDocument();
  }, 15000);
});
