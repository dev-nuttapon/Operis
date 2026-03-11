import Keycloak from "keycloak-js";
import { appEnv } from "../../../shared/config/env";

const keycloak = new Keycloak({
  url: appEnv.keycloakUrl,
  realm: appEnv.keycloakRealm,
  clientId: appEnv.keycloakClientId,
});

let initialized = false;

export async function initKeycloak(): Promise<boolean> {
  if (initialized) {
    return Boolean(keycloak.authenticated);
  }

  const authenticated = await keycloak.init({
    onLoad: "check-sso",
    checkLoginIframe: false,
    pkceMethod: "S256",
  });

  initialized = true;
  return authenticated;
}

export async function login(): Promise<void> {
  await keycloak.login({
    redirectUri: window.location.origin,
  });
}

export async function logout(): Promise<void> {
  await keycloak.logout({
    redirectUri: window.location.origin,
  });
}

export function getAccessToken(): string | undefined {
  return keycloak.token;
}
