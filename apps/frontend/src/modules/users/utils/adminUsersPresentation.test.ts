import { describe, expect, it } from "vitest";
import dayjs from "dayjs";
import {
  getCurrentAdminUsersSection,
  getDisplayActor,
  toApiSortOrder,
  toRange,
} from "./adminUsersPresentation";

describe("adminUsersPresentation", () => {
  it("derives the current admin section from the pathname", () => {
    expect(getCurrentAdminUsersSection("/app/admin/invitations")).toBe("invitations");
    expect(getCurrentAdminUsersSection("/app/admin/master/divisions")).toBe("master-divisions");
    expect(getCurrentAdminUsersSection("/app/admin/master/departments")).toBe("master-departments");
    expect(getCurrentAdminUsersSection("/app/admin/master/positions")).toBe("master-positions");
    expect(getCurrentAdminUsersSection("/app/admin/master/project-roles")).toBe("master-project-roles");
    expect(getCurrentAdminUsersSection("/app/admin/registrations")).toBe("approvals");
    expect(getCurrentAdminUsersSection("/app/admin/users")).toBe("directory");
  });

  it("normalizes sorting and range filters for the API", () => {
    expect(toApiSortOrder("ascend")).toBe("asc");
    expect(toApiSortOrder("descend")).toBe("desc");
    expect(toApiSortOrder(undefined)).toBeUndefined();

    const start = dayjs("2026-03-12");
    const end = dayjs("2026-03-13");
    const range = toRange([start, end]);
    expect(range.from).toBe(start.startOf("day").toISOString());
    expect(range.to).toBe(end.endOf("day").toISOString());
  });

  it("prefers email over name for audit actor fallbacks", () => {
    expect(getDisplayActor({ email: "admin@example.com", name: "Admin" })).toBe("admin@example.com");
    expect(getDisplayActor({ name: "Admin" })).toBe("Admin");
    expect(getDisplayActor(null)).toBe("admin@operis.local");
  });
});
