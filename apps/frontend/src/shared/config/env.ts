type AppMode = "local" | "dev" | "prod";

function readRequiredEnv(name: string): string {
  const value = import.meta.env[name as keyof ImportMetaEnv];
  if (!value) {
    throw new Error(`Missing required environment variable: ${name}`);
  }
  return value;
}

function normalizeMode(mode: string): AppMode {
  if (mode === "app-local") {
    return "local";
  }
  if (mode === "dev" || mode === "prod") {
    return mode;
  }
  return "local";
}

export const appEnv = {
  mode: normalizeMode(import.meta.env.MODE),
  appName: readRequiredEnv("VITE_APP_NAME"),
  apiBaseUrl: readRequiredEnv("VITE_API_BASE_URL"),
  authBaseUrl: readRequiredEnv("VITE_AUTH_BASE_URL"),
  keycloakUrl: readRequiredEnv("VITE_KEYCLOAK_URL"),
  keycloakRealm: readRequiredEnv("VITE_KEYCLOAK_REALM"),
  keycloakClientId: readRequiredEnv("VITE_KEYCLOAK_CLIENT_ID"),
};
