import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { PolicyAcknowledgementsPage } from "./PolicyAcknowledgementsPage";

vi.mock("../../../shared/authz/usePermissions", () => ({
  usePermissions: vi.fn(),
}));

vi.mock("../hooks/useGovernance", () => ({
  usePolicies: vi.fn(),
  usePolicyCampaigns: vi.fn(),
  usePolicyAcknowledgements: vi.fn(),
  useCreatePolicyAcknowledgement: vi.fn(),
}));

import { usePermissions } from "../../../shared/authz/usePermissions";
import { useCreatePolicyAcknowledgement, usePolicies, usePolicyAcknowledgements, usePolicyCampaigns } from "../hooks/useGovernance";

describe("PolicyAcknowledgementsPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(usePermissions).mockReturnValue({
      roles: [],
      permissions: [],
      hasAnyPermission: () => true,
      hasPermission: () => true,
      hasAllPermissions: () => true,
    } as ReturnType<typeof usePermissions>);
    vi.mocked(usePolicies).mockReturnValue({
      data: { items: [{ id: "policy-1", policyCode: "POL-001", title: "Security Policy" }] },
      isLoading: false,
    } as unknown as ReturnType<typeof usePolicies>);
    vi.mocked(usePolicyCampaigns).mockReturnValue({
      data: { items: [{ id: "campaign-1", campaignCode: "CMP-001", title: "Q1 Attestation" }] },
      isLoading: false,
    } as unknown as ReturnType<typeof usePolicyCampaigns>);
    vi.mocked(usePolicyAcknowledgements).mockReturnValue({
      data: {
        items: [
          {
            id: "ack-1",
            policyId: "policy-1",
            policyTitle: "Security Policy",
            policyCampaignId: "campaign-1",
            campaignTitle: "Q1 Attestation",
            userId: "user@example.com",
            status: "pending",
            isOverdue: true,
            requiresAttestation: true,
            dueAt: "2026-03-27T09:00:00Z",
            acknowledgedAt: null,
            attestationText: null,
            updatedAt: "2026-03-27T09:00:00Z",
          },
        ],
        total: 1,
        page: 1,
        pageSize: 25,
      },
      isLoading: false,
    } as unknown as ReturnType<typeof usePolicyAcknowledgements>);
    vi.mocked(useCreatePolicyAcknowledgement).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as unknown as ReturnType<typeof useCreatePolicyAcknowledgement>);
  });

  it("renders pending and overdue acknowledgement rows", () => {
    render(
      <MemoryRouter>
        <PolicyAcknowledgementsPage />
      </MemoryRouter>,
    );

    expect(screen.getByText("Policy Acknowledgements")).toBeInTheDocument();
    expect(screen.getByText("Security Policy")).toBeInTheDocument();
    expect(screen.getByText("Q1 Attestation")).toBeInTheDocument();
    expect(screen.getByText("overdue")).toBeInTheDocument();
  }, 15000);
});
