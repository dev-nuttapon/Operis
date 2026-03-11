import Keycloak from "keycloak-js";
import { appEnv } from "../../../shared/config/env";

const keycloak = new Keycloak({
  url: appEnv.keycloakUrl,
  realm: appEnv.keycloakRealm,
  clientId: appEnv.keycloakClientId,
});

let initialized = false;
let initPromise: Promise<boolean> | null = null;
const AUTH_FLAG_KEY = "operis.authenticated";

interface AuthEventHandlers {
  onAuthenticatedChanged?: (authenticated: boolean) => void;
  onTokenExpired?: () => void;
}

export function isLoginCallback(): boolean {
  const hash = window.location.hash;
  return hash.includes("code=") || hash.includes("state=") || hash.includes("session_state=") || hash.includes("iss=");
}

export function hasAuthCallbackParams(): boolean {
  const url = new URL(window.location.href);
  return (
    url.searchParams.has("code") ||
    url.searchParams.has("session_state") ||
    url.searchParams.has("iss") ||
    url.searchParams.has("state") ||
    isLoginCallback()
  );
}

export function clearAuthCallbackParams(): void {
  const url = new URL(window.location.href);
  url.search = "";
  url.hash = "";
  window.history.replaceState({}, document.title, url.pathname);
}

export function getAuthFlag(): boolean {
  try {
    return sessionStorage.getItem(AUTH_FLAG_KEY) === "1";
  } catch {
    return false;
  }
}

export function setAuthFlag(value: boolean): void {
  try {
    if (value) {
      sessionStorage.setItem(AUTH_FLAG_KEY, "1");
    } else {
      sessionStorage.removeItem(AUTH_FLAG_KEY);
    }
  } catch {
    // ignore storage errors
  }
}

export async function initKeycloak(): Promise<boolean> {
  if (initialized) {
    return Boolean(keycloak.authenticated);
  }
  if (initPromise) {
    return initPromise;
  }

  initPromise = keycloak
    .init({
      onLoad: "check-sso",
      silentCheckSsoRedirectUri: `${window.location.origin}/silent-check-sso.html`,
      checkLoginIframe: false,
      pkceMethod: "S256",
    })
    .then((authenticated) => {
      initialized = true;
      return authenticated;
    })
    .catch((err) => {
      initPromise = null;
      throw err;
    });

  return initPromise;
}

export function bindAuthEvents(handlers: AuthEventHandlers): void {
  keycloak.onAuthSuccess = () => {
    handlers.onAuthenticatedChanged?.(true);
  };
  keycloak.onAuthLogout = () => {
    handlers.onAuthenticatedChanged?.(false);
    handlers.onTokenExpired?.();
  };
  keycloak.onTokenExpired = () => {
    handlers.onAuthenticatedChanged?.(false);
    handlers.onTokenExpired?.();
  };
  keycloak.onAuthRefreshError = () => {
    handlers.onAuthenticatedChanged?.(false);
    handlers.onTokenExpired?.();
  };
}

export async function refreshToken(minValidity = 30): Promise<boolean> {
  try {
    return await keycloak.updateToken(minValidity);
  } catch {
    return false;
  }
}

export async function login(redirectPath = "/app"): Promise<void> {
  const redirectUri = `${window.location.origin}${redirectPath}`;
  const realmUrl = `${appEnv.keycloakUrl.replace(/\/$/, "")}/realms/${encodeURIComponent(appEnv.keycloakRealm)}`;
  const authUrl =
    `${realmUrl}/protocol/openid-connect/auth` +
    `?client_id=${encodeURIComponent(appEnv.keycloakClientId)}` +
    `&redirect_uri=${encodeURIComponent(redirectUri)}` +
    `&response_type=code` +
    `&scope=openid`;
  window.location.href = authUrl;
}

export async function logout(): Promise<void> {
  setAuthFlag(false);
  window.location.href = `${window.location.origin}/login`;
}

export function getAccessToken(): string | undefined {
  return keycloak.token;
}
