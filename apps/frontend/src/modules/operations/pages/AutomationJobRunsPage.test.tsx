import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { App } from "antd";
import { AutomationJobRunsPage } from "./AutomationJobRunsPage";

vi.mock("../../../shared/authz/usePermissions", () => ({
  usePermissions: vi.fn(),
}));

vi.mock("../hooks/useOperations", () => ({
  useAutomationJobRuns: vi.fn(),
}));

import { usePermissions } from "../../../shared/authz/usePermissions";
import { useAutomationJobRuns } from "../hooks/useOperations";

describe("AutomationJobRunsPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(usePermissions).mockReturnValue({
      roles: [],
      permissions: [],
      hasAnyPermission: () => true,
      hasPermission: () => true,
      hasAllPermissions: () => true,
    } as ReturnType<typeof usePermissions>);
    vi.mocked(useAutomationJobRuns).mockReturnValue({
      data: {
        items: [
          {
            id: "run-1",
            jobId: "job-1",
            jobName: "Nightly Backup",
            jobType: "backup",
            status: "failed",
            triggeredBy: "ops@example.com",
            triggerReason: "Scheduled run",
            queuedAt: "2026-03-27T10:00:00Z",
            startedAt: "2026-03-27T10:01:00Z",
            completedAt: "2026-03-27T10:05:00Z",
            errorSummary: "MinIO bucket unavailable",
            remediationPath: "Re-run after storage recovery",
            evidenceRefs: [
              {
                id: "evidence-1",
                jobRunId: "run-1",
                entityType: "backup_evidence",
                entityId: "backup-1",
                route: "/app/operations/backup-evidence",
                evidenceRef: "minio://ops/backup/nightly-2026-03-27.json",
                createdAt: "2026-03-27T10:05:00Z",
              },
            ],
            createdAt: "2026-03-27T10:00:00Z",
          },
        ],
        total: 1,
        page: 1,
        pageSize: 25,
      },
      isLoading: false,
    } as unknown as ReturnType<typeof useAutomationJobRuns>);
  });

  it("renders automation runs and evidence count", () => {
    render(
      <App>
        <AutomationJobRunsPage />
      </App>,
    );

    expect(screen.getByText("Automation Runs")).toBeInTheDocument();
    expect(screen.getByText("Nightly Backup")).toBeInTheDocument();
    expect(screen.getByText("MinIO bucket unavailable")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /Open/i })).toBeInTheDocument();
  }, 15000);
});
