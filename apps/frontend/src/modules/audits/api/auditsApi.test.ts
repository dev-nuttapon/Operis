import { describe, expect, it, vi } from "vitest";
import { listAuditLogs } from "./auditsApi";
import { apiRequest } from "../../../shared/lib/apiClient";

vi.mock("../../../shared/lib/apiClient", () => ({
  apiRequest: vi.fn(),
}));

describe("listAuditLogs", () => {
  it("builds the query string from defined filters only", async () => {
    vi.mocked(apiRequest).mockResolvedValueOnce({
      items: [],
      total: 0,
      page: 1,
      pageSize: 10,
    });

    await listAuditLogs({
      module: "users",
      actor: "admin@example.com",
      eventType: "created",
      page: 2,
      pageSize: 25,
    });

    expect(apiRequest).toHaveBeenCalledWith(
      "/api/v1/audit-events?module=users&eventType=created&actor=admin%40example.com&page=2&pageSize=25",
      { signal: undefined },
    );
  });
});
