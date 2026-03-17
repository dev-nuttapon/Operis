import { appEnv } from "../config/env";
import { getAccessToken, refreshToken } from "../../modules/auth";
import i18n from "../i18n/config";

type HttpMethod = "GET" | "POST" | "PUT" | "DELETE";

interface RequestOptions {
  method?: HttpMethod;
  body?: unknown;
  signal?: AbortSignal;
  auth?: boolean;
}

interface FileRequestResult {
  blob: Blob;
  fileName?: string;
}

export class ApiError extends Error {
  readonly status: number;
  readonly category: "network" | "bad_request" | "unauthorized" | "forbidden" | "not_found" | "conflict" | "server" | "unknown";
  readonly code?: string;

  constructor(
    message: string,
    status: number,
    category: "network" | "bad_request" | "unauthorized" | "forbidden" | "not_found" | "conflict" | "server" | "unknown" = "unknown",
    code?: string
  ) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.category = category;
    this.code = code;
  }
}

const messageKeyMap: Record<string, string> = {
  "User already exists.": "errors.user_exists",
  "Pending invitation already exists.": "errors.pending_invitation_exists",
  "Pending registration request already exists.": "errors.pending_registration_exists",
  "Department already exists.": "errors.department_exists",
  "Division already exists.": "errors.division_exists",
  "Job title already exists.": "errors.job_title_exists",
  "Project role already exists.": "errors.project_role_exists",
  "Department name is required.": "errors.department_required",
  "Job title name is required.": "errors.job_title_required",
  "Email is required.": "errors.email_required",
  "Department does not exist.": "errors.department_not_found",
  "Department is required when division is selected.": "errors.department_required_for_division",
  "Department does not belong to the selected division.": "errors.department_division_mismatch",
  "Department is required when job title is selected.": "errors.department_required_for_job_title",
  "Job title does not exist.": "errors.job_title_not_found",
  "Job title does not belong to the selected department.": "errors.job_title_department_mismatch",
  "Division does not exist.": "errors.division_not_found",
  "One or more selected roles do not exist.": "errors.roles_not_found",
  "Registration request has already been reviewed.": "errors.registration_reviewed",
  "Expiration date must be in the future.": "errors.expiration_future",
  "Invitation has been cancelled.": "errors.invitation_cancelled",
  "Invitation has already been cancelled.": "errors.invitation_cancelled_already",
  "Invitation has already been accepted.": "errors.invitation_accepted",
  "Invitation has already been rejected.": "errors.invitation_rejected",
  "Invitation has expired.": "errors.invitation_expired",
  "Accepted invitation cannot be cancelled.": "errors.invitation_cancel_accepted",
  "Expired invitation cannot be cancelled.": "errors.invitation_cancel_expired",
  "Invited by is required.": "errors.invited_by_required",
  "Accepted invitation cannot be updated.": "errors.invitation_update_accepted",
  "Cancelled invitation cannot be updated.": "errors.invitation_update_cancelled",
  "Rejected invitation cannot be updated.": "errors.invitation_update_rejected",
  "Password is required.": "errors.password_required",
  "Password must be at least 8 characters.": "errors.password_min_length",
  "Password and confirmation do not match.": "errors.password_mismatch",
  "Password setup has already been completed.": "errors.password_setup_completed",
  "Password setup link has expired.": "errors.password_setup_expired",
  "Registration request is not approved.": "errors.registration_not_approved",
  "Invitation not found.": "errors.invitation_not_found",
  "Keycloak user already exists.": "errors.keycloak_user_exists",
  "Keycloak user already exists but cannot resolve id.": "errors.keycloak_user_exists",
};

function localizeApiMessage(message: string, status: number, code?: string) {
  if (code && i18n.exists(`errors.${code}`)) {
    return i18n.t(`errors.${code}`);
  }

  const key = messageKeyMap[message];
  if (key) {
    return i18n.t(key);
  }

  if (message.startsWith("Request failed with status")) {
    return i18n.t("errors.request_failed", { status });
  }

  return message || i18n.t("errors.unknown");
}

function getErrorCategory(status: number): ApiError["category"] {
  if (status <= 0) return "network";
  if (status === 400) return "bad_request";
  if (status === 401) return "unauthorized";
  if (status === 403) return "forbidden";
  if (status === 404) return "not_found";
  if (status === 409) return "conflict";
  if (status >= 500) return "server";
  return "unknown";
}

export function getApiErrorPresentation(error: unknown, fallbackTitle?: string) {
  if (error instanceof ApiError) {
    const titleKey = {
      network: "errors.title_network",
      bad_request: "errors.title_bad_request",
      unauthorized: "errors.title_unauthorized",
      forbidden: "errors.title_forbidden",
      not_found: "errors.title_not_found",
      conflict: "errors.title_conflict",
      server: "errors.title_server",
      unknown: "errors.title_generic",
    }[error.category];

    return {
      title: fallbackTitle || i18n.t(titleKey),
      description: error.message,
    };
  }

  if (error instanceof Error) {
    return {
      title: fallbackTitle || i18n.t("errors.title_generic"),
      description: error.message,
    };
  }

  return {
    title: fallbackTitle || i18n.t("errors.title_generic"),
    description: i18n.t("errors.unknown"),
  };
}

export function hasApiErrorCode(error: unknown, ...codes: string[]) {
  return error instanceof ApiError && !!error.code && codes.includes(error.code);
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  if (options.auth !== false) {
    await refreshToken(30);
  }

  const token = options.auth === false ? null : getAccessToken();
  const isFormData = typeof FormData !== "undefined" && options.body instanceof FormData;
  const headers = new Headers();

  if (!isFormData) {
    headers.set("Content-Type", "application/json");
  }

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  let response: Response;
  try {
    response = await fetch(`${appEnv.apiBaseUrl}${path}`, {
      method: options.method ?? "GET",
      headers,
      body:
        options.body === undefined
          ? undefined
          : isFormData
            ? (options.body as FormData)
            : JSON.stringify(options.body),
      signal: options.signal,
    });
  } catch {
    throw new ApiError(
      i18n.t("errors.network_unreachable"),
      0,
      "network"
    );
  }

  if (!response.ok) {
    let message = i18n.t("errors.request_failed", { status: response.status });
    let code: string | undefined;

    try {
      const contentType = response.headers.get("content-type") ?? "";
      if (contentType.includes("application/json")) {
        const payload = await response.json() as { detail?: string; title?: string; code?: string };
        message = payload.detail || payload.title || message;
        code = payload.code;
      } else {
        const text = await response.text();
        if (text) {
          message = text;
        }
      }
    } catch {
      // Keep default message when parsing fails.
    }

    throw new ApiError(
      localizeApiMessage(message, response.status, code),
      response.status,
      getErrorCategory(response.status),
      code
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return await response.json() as T;
}

export async function apiDownload(path: string, options: RequestOptions = {}): Promise<FileRequestResult> {
  if (options.auth !== false) {
    await refreshToken(30);
  }

  const token = options.auth === false ? null : getAccessToken();
  const headers = new Headers();

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  let response: Response;
  try {
    response = await fetch(`${appEnv.apiBaseUrl}${path}`, {
      method: options.method ?? "GET",
      headers,
      signal: options.signal,
    });
  } catch {
    throw new ApiError(
      i18n.t("errors.network_unreachable"),
      0,
      "network"
    );
  }

  if (!response.ok) {
    let message = i18n.t("errors.request_failed", { status: response.status });
    let code: string | undefined;

    try {
      const contentType = response.headers.get("content-type") ?? "";
      if (contentType.includes("application/json")) {
        const payload = await response.json() as { detail?: string; title?: string; code?: string };
        message = payload.detail || payload.title || message;
        code = payload.code;
      } else {
        const text = await response.text();
        if (text) {
          message = text;
        }
      }
    } catch {
      // Keep default message when parsing fails.
    }

    throw new ApiError(localizeApiMessage(message, response.status, code), response.status, getErrorCategory(response.status), code);
  }

  const blob = await response.blob();
  const disposition = response.headers.get("content-disposition") ?? "";
  const fileNameMatch = disposition.match(/filename\\*=UTF-8''([^;]+)|filename=\"?([^\";]+)\"?/i);
  const encodedName = fileNameMatch?.[1] || fileNameMatch?.[2];
  const fileName = encodedName ? decodeURIComponent(encodedName) : undefined;

  return { blob, fileName };
}

export async function publicApiRequest<T>(path: string, options: Omit<RequestOptions, "auth"> = {}): Promise<T> {
  return apiRequest<T>(path, { ...options, auth: false });
}

export async function apiFileRequest(path: string, options: RequestOptions = {}): Promise<FileRequestResult> {
  if (options.auth !== false) {
    await refreshToken(30);
  }

  const token = options.auth === false ? null : getAccessToken();
  const headers = new Headers();

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  let response: Response;
  try {
    response = await fetch(`${appEnv.apiBaseUrl}${path}`, {
      method: options.method ?? "GET",
      headers,
      signal: options.signal,
    });
  } catch {
    throw new ApiError(i18n.t("errors.network_unreachable"), 0, "network");
  }

  if (!response.ok) {
    let message = i18n.t("errors.request_failed", { status: response.status });
    let code: string | undefined;

    try {
      const contentType = response.headers.get("content-type") ?? "";
      if (contentType.includes("application/json")) {
        const payload = await response.json() as { detail?: string; title?: string; code?: string };
        message = payload.detail || payload.title || message;
        code = payload.code;
      } else {
        const text = await response.text();
        if (text) {
          message = text;
        }
      }
    } catch {
      // Keep default message when parsing fails.
    }

    throw new ApiError(localizeApiMessage(message, response.status, code), response.status, getErrorCategory(response.status), code);
  }

  const disposition = response.headers.get("content-disposition") ?? "";
  const match = disposition.match(/filename=\"?([^\";]+)\"?/i);
  return { blob: await response.blob(), fileName: match?.[1] };
}
