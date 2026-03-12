import { appEnv } from "../config/env";
import { getAccessToken, refreshToken } from "../../modules/auth/services/keycloakAuth";
import i18n from "../i18n/config";

type HttpMethod = "GET" | "POST" | "PUT" | "DELETE";

interface RequestOptions {
  method?: HttpMethod;
  body?: unknown;
  signal?: AbortSignal;
  auth?: boolean;
}

export class ApiError extends Error {
  readonly status: number;
  readonly category: "network" | "bad_request" | "unauthorized" | "forbidden" | "not_found" | "conflict" | "server" | "unknown";

  constructor(
    message: string,
    status: number,
    category: "network" | "bad_request" | "unauthorized" | "forbidden" | "not_found" | "conflict" | "server" | "unknown" = "unknown"
  ) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.category = category;
  }
}

const messageKeyMap: Record<string, string> = {
  "User already exists.": "errors.user_exists",
  "Pending invitation already exists.": "errors.pending_invitation_exists",
  "Pending registration request already exists.": "errors.pending_registration_exists",
  "Department already exists.": "errors.department_exists",
  "Job title already exists.": "errors.job_title_exists",
  "Department name is required.": "errors.department_required",
  "Job title name is required.": "errors.job_title_required",
  "Email is required.": "errors.email_required",
  "Department does not exist.": "errors.department_not_found",
  "Job title does not exist.": "errors.job_title_not_found",
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

function localizeApiMessage(message: string, status: number) {
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

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  if (options.auth !== false) {
    await refreshToken(30);
  }

  const token = options.auth === false ? null : getAccessToken();
  const headers = new Headers({
    "Content-Type": "application/json",
  });

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  let response: Response;
  try {
    response = await fetch(`${appEnv.apiBaseUrl}${path}`, {
      method: options.method ?? "GET",
      headers,
      body: options.body === undefined ? undefined : JSON.stringify(options.body),
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

    try {
      const contentType = response.headers.get("content-type") ?? "";
      if (contentType.includes("application/json")) {
        const payload = await response.json() as { detail?: string; title?: string };
        message = payload.detail || payload.title || message;
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
      localizeApiMessage(message, response.status),
      response.status,
      getErrorCategory(response.status)
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return await response.json() as T;
}

export async function publicApiRequest<T>(path: string, options: Omit<RequestOptions, "auth"> = {}): Promise<T> {
  return apiRequest<T>(path, { ...options, auth: false });
}
