import { describe, expect, it, vi } from "vitest";
import { listAuditEvents } from "./auditsApi";
import { apiRequest } from "../../../shared/lib/apiClient";

vi.mock("../../../shared/lib/apiClient", () => ({
  apiRequest: vi.fn(),
}));

describe("listAuditEvents", () => {
  it("builds the query string from defined filters only", async () => {
    vi.mocked(apiRequest).mockResolvedValueOnce({
      items: [],
      total: 0,
      page: 1,
      pageSize: 10,
    });

    await listAuditEvents({
      entityType: "user",
      actorUserId: "admin@example.com",
      action: "create",
      page: 2,
      pageSize: 25,
    });

    expect(apiRequest).toHaveBeenCalledWith(
      "/api/v1/audit-events?entityType=user&action=create&actorUserId=admin%40example.com&page=2&pageSize=25",
      { signal: undefined },
    );
  });
});
