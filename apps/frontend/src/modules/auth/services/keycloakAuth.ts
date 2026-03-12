import Keycloak from "keycloak-js";
import { appEnv } from "../../../shared/config/env";

const keycloak = new Keycloak({
  url: appEnv.keycloakUrl,
  realm: appEnv.keycloakRealm,
  clientId: appEnv.keycloakClientId,
});

let initialized = false;
let initPromise: Promise<boolean> | null = null;

interface AuthEventHandlers {
  onAuthenticatedChanged?: (authenticated: boolean) => void;
  onTokenExpired?: () => void;
  onTokenRefreshed?: () => void;
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
      flow: "standard",
      onLoad: "check-sso",
      scope: "openid profile email",
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
    handlers.onTokenExpired?.();
  };
  keycloak.onAuthRefreshError = () => {
    handlers.onTokenExpired?.();
  };
  keycloak.onAuthRefreshSuccess = () => {
    handlers.onTokenRefreshed?.();
  };
}

export async function refreshToken(minValidity = 30): Promise<boolean> {
  try {
    if (!keycloak.authenticated) {
      return false;
    }
    return await keycloak.updateToken(minValidity);
  } catch {
    return false;
  }
}

export async function login(redirectPath = "/app"): Promise<void> {
  await keycloak.login({
    redirectUri: `${window.location.origin}${redirectPath}`,
  });
}

export async function logout(): Promise<void> {
  await keycloak.logout({
    redirectUri: `${window.location.origin}/login`,
  });
}

export async function getUserProfile() {
  if (!keycloak.authenticated) return null;
  return await keycloak.loadUserProfile();
}

export function getTokenParsed() {
  return keycloak.tokenParsed;
}

export function getAccessToken(): string | undefined {
  return keycloak.token;
}

export function isAuthenticated(): boolean {
  return Boolean(keycloak.authenticated);
}
