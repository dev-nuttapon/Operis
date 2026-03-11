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
  await keycloak.login({
    redirectUri: `${window.location.origin}${redirectPath}`,
  });
}

export async function logout(): Promise<void> {
  await keycloak.logout({
    redirectUri: `${window.location.origin}/login`,
  });
}

export function getAccessToken(): string | undefined {
  return keycloak.token;
}
