import type { SortOrder } from "antd/es/table/interface";
import type { Dayjs } from "dayjs";

export const userStatusOptions = [
  { value: "Active", label: "Active" },
  { value: "Rejected", label: "Rejected" },
  { value: "Deleted", label: "Deleted" },
];

export const invitationStatusOptions = [
  { value: "Pending", label: "Pending" },
  { value: "Accepted", label: "Accepted" },
  { value: "Rejected", label: "Rejected" },
  { value: "Expired", label: "Expired" },
  { value: "Cancelled", label: "Cancelled" },
];

export const registrationStatusOptions = [
  { value: "Pending", label: "Pending" },
  { value: "Approved", label: "Approved" },
  { value: "Rejected", label: "Rejected" },
];

export function formatDate(value: string | null, language: string) {
  if (!value) {
    return "-";
  }

  return new Intl.DateTimeFormat(language.startsWith("th") ? "th-TH" : "en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

export function getDisplayActor(user: { name?: string | null; email?: string | null } | null | undefined) {
  return user?.email || user?.name || "admin@operis.local";
}

export function toApiSortOrder(order?: SortOrder): "asc" | "desc" | undefined {
  if (order === "ascend") return "asc";
  if (order === "descend") return "desc";
  return undefined;
}

export function toRange(range?: [Dayjs | null, Dayjs | null]) {
  return {
    from: range?.[0] ? range[0].startOf("day").toISOString() : undefined,
    to: range?.[1] ? range[1].endOf("day").toISOString() : undefined,
  };
}

export function getCurrentAdminUsersSection(pathname: string) {
  if (pathname.includes("/admin/invitations")) {
    return "invitations" as const;
  }

  if (pathname.includes("/admin/master/departments")) {
    return "master-departments" as const;
  }

  if (pathname.includes("/admin/master/job-titles")) {
    return "master-job-titles" as const;
  }

  if (pathname.includes("/admin/registrations")) {
    return "approvals" as const;
  }

  return "directory" as const;
}
