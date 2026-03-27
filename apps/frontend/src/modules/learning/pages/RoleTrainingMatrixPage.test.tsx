import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { RoleTrainingMatrixPage } from "./RoleTrainingMatrixPage";

vi.mock("../../../shared/authz/usePermissions", () => ({
  usePermissions: vi.fn(),
}));

vi.mock("../../users/public", () => ({
  useProjectList: vi.fn(),
}));

vi.mock("../hooks/useLearning", () => ({
  useCreateRoleTrainingRequirement: vi.fn(),
  useUpdateRoleTrainingRequirement: vi.fn(),
  useRoleTrainingRequirements: vi.fn(),
  useProjectRoleOptions: vi.fn(),
  useTrainingCourses: vi.fn(),
}));

import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectList } from "../../users/public";
import {
  useCreateRoleTrainingRequirement,
  useProjectRoleOptions,
  useRoleTrainingRequirements,
  useTrainingCourses,
  useUpdateRoleTrainingRequirement,
} from "../hooks/useLearning";

describe("RoleTrainingMatrixPage", () => {
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
    vi.mocked(useProjectRoleOptions).mockReturnValue({
      data: [{ id: "role-1", projectId: "project-1", projectName: "Alpha", name: "Project Manager", status: "Active" }],
      isLoading: false,
    } as unknown as ReturnType<typeof useProjectRoleOptions>);
    vi.mocked(useTrainingCourses).mockReturnValue({
      data: { items: [{ id: "course-1", courseCode: "CMMI", title: "CMMI Foundations" }] },
      isLoading: false,
    } as unknown as ReturnType<typeof useTrainingCourses>);
    vi.mocked(useRoleTrainingRequirements).mockReturnValue({
      data: {
        items: [{
          id: "req-1",
          courseId: "course-1",
          courseTitle: "CMMI Foundations",
          courseCode: "CMMI",
          courseStatus: "active",
          projectRoleId: "role-1",
          projectRoleName: "Project Manager",
          projectId: "project-1",
          projectName: "Alpha",
          requiredWithinDays: 30,
          renewalIntervalMonths: 12,
          status: "active",
          notes: null,
          assignedUserCount: 2,
          overdueUserCount: 1,
          expiredUserCount: 0,
          createdAt: "2026-03-27T10:00:00Z",
          updatedAt: "2026-03-27T10:00:00Z",
        }],
        total: 1,
        page: 1,
        pageSize: 25,
      },
      isLoading: false,
    } as unknown as ReturnType<typeof useRoleTrainingRequirements>);
    vi.mocked(useCreateRoleTrainingRequirement).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as unknown as ReturnType<typeof useCreateRoleTrainingRequirement>);
    vi.mocked(useUpdateRoleTrainingRequirement).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as unknown as ReturnType<typeof useUpdateRoleTrainingRequirement>);
  });

  it("renders the role training matrix and requirement rows", () => {
    render(
      <MemoryRouter>
        <RoleTrainingMatrixPage />
      </MemoryRouter>,
    );

    expect(screen.getByText("Role Training Matrix")).toBeInTheDocument();
    expect(screen.getByText("Project Manager")).toBeInTheDocument();
    expect(screen.getByText("CMMI Foundations")).toBeInTheDocument();
    expect(screen.getByText("Overdue")).toBeInTheDocument();
  }, 15000);
});
