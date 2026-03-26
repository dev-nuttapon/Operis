import { createContext, useContext, useEffect, useState, type PropsWithChildren } from "react";
import {
  bindAuthEvents,
  getTokenParsed,
  getUserProfile,
  initKeycloak,
  isAuthenticated,
  login,
  logout,
  refreshToken,
} from "../services/keycloakAuth";

interface AuthUser {
  email?: string | null;
  name?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  departmentName?: string | null;
  jobTitleName?: string | null;
  roles: string[];
  [key: string]: unknown;
}

interface AuthContextValue {
  isReady: boolean;
  isAuthenticated: boolean;
  authState: "loading" | "anonymous" | "authenticated" | "expired";
  user?: AuthUser | null;
  login: () => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: PropsWithChildren) {
  const [state, setState] = useState<AuthContextValue>({
    isReady: false,
    isAuthenticated: false,
    authState: "loading",
    user: null,
    login,
    logout,
  });

  useEffect(() => {
    let mounted = true;
    let refreshTimeoutId: ReturnType<typeof setTimeout> | null = null;

    const setAnonymous = (authState: AuthContextValue["authState"] = "anonymous") => {
      if (!mounted) return;
      setState({ isReady: true, isAuthenticated: false, authState, user: null, login, logout });
    };

    const extractRoles = (tokenParsed: Record<string, unknown> | undefined) => {
      if (!tokenParsed) {
        return [];
      }

      const roles = new Set<string>();

      const realmAccess = tokenParsed.realm_access as { roles?: unknown } | undefined;
      if (Array.isArray(realmAccess?.roles)) {
        for (const role of realmAccess.roles) {
          if (typeof role === "string" && role.trim()) {
            roles.add(role);
          }
        }
      }

      const resourceAccess = tokenParsed.resource_access as Record<string, { roles?: unknown } | undefined> | undefined;
      if (resourceAccess) {
        for (const resource of Object.values(resourceAccess)) {
          if (!Array.isArray(resource?.roles)) {
            continue;
          }

          for (const role of resource.roles) {
            if (typeof role === "string" && role.trim()) {
              roles.add(role);
            }
          }
        }
      }

      return Array.from(roles);
    };

    const resolveAttributeValue = (source: Record<string, unknown> | undefined, key: string) => {
      if (!source) return undefined;
      const value = source[key];
      if (Array.isArray(value)) {
        const first = value.find((item) => typeof item === "string" && item.trim());
        return typeof first === "string" ? first.trim() : undefined;
      }
      if (typeof value === "string" && value.trim()) {
        return value.trim();
      }
      return undefined;
    };

    const resolveProfileAttribute = (profile: Record<string, unknown> | null, key: string) => {
      if (!profile) return undefined;
      const attributes = profile.attributes as Record<string, unknown> | undefined;
      return resolveAttributeValue(attributes, key) ?? resolveAttributeValue(profile, key);
    };

    const buildUser = async (): Promise<AuthUser | null> => {
      const tokenParsed = getTokenParsed() as Record<string, unknown> | undefined;
      const roles = extractRoles(tokenParsed);

      try {
        const profile = await getUserProfile();
        const jobTitleName =
          resolveProfileAttribute(profile as Record<string, unknown> | null, "jobTitleName") ??
          resolveProfileAttribute(profile as Record<string, unknown> | null, "job_title_name") ??
          resolveProfileAttribute(profile as Record<string, unknown> | null, "jobTitle") ??
          resolveProfileAttribute(profile as Record<string, unknown> | null, "job_title") ??
          resolveProfileAttribute(profile as Record<string, unknown> | null, "position") ??
          resolveAttributeValue(tokenParsed, "jobTitleName") ??
          resolveAttributeValue(tokenParsed, "job_title_name") ??
          resolveAttributeValue(tokenParsed, "jobTitle") ??
          resolveAttributeValue(tokenParsed, "job_title") ??
          resolveAttributeValue(tokenParsed, "position");
        const departmentName =
          resolveProfileAttribute(profile as Record<string, unknown> | null, "departmentName") ??
          resolveProfileAttribute(profile as Record<string, unknown> | null, "department_name") ??
          resolveProfileAttribute(profile as Record<string, unknown> | null, "department") ??
          resolveAttributeValue(tokenParsed, "departmentName") ??
          resolveAttributeValue(tokenParsed, "department_name") ??
          resolveAttributeValue(tokenParsed, "department");

        return {
          ...profile,
          email: profile?.email || (typeof tokenParsed?.email === "string" ? tokenParsed.email : undefined),
          name: profile?.firstName
            ? `${profile.firstName} ${profile.lastName}`
            : (typeof tokenParsed?.name === "string" ? tokenParsed.name : (typeof tokenParsed?.preferred_username === "string" ? tokenParsed.preferred_username : undefined)),
          jobTitleName,
          departmentName,
          roles,
        };
      } catch (err) {
        console.error("Failed to load user profile:", err);
        const jobTitleName =
          resolveAttributeValue(tokenParsed, "jobTitleName") ??
          resolveAttributeValue(tokenParsed, "job_title_name") ??
          resolveAttributeValue(tokenParsed, "jobTitle") ??
          resolveAttributeValue(tokenParsed, "job_title") ??
          resolveAttributeValue(tokenParsed, "position");
        const departmentName =
          resolveAttributeValue(tokenParsed, "departmentName") ??
          resolveAttributeValue(tokenParsed, "department_name") ??
          resolveAttributeValue(tokenParsed, "department");
        return {
          email: typeof tokenParsed?.email === "string" ? tokenParsed.email : undefined,
          name: typeof tokenParsed?.name === "string" ? tokenParsed.name : (typeof tokenParsed?.preferred_username === "string" ? tokenParsed.preferred_username : undefined),
          jobTitleName,
          departmentName,
          roles,
        };
      }
    };

    const updateAuthState = async (authenticated: boolean) => {
      if (!mounted) return;

      const user = authenticated ? await buildUser() : null;

      setState({
        isReady: true,
        isAuthenticated: authenticated,
        authState: authenticated ? "authenticated" : "anonymous",
        user,
        login,
        logout
      });

      if (authenticated) {
        startRefreshLoop();
      } else {
        stopRefreshLoop();
      }
    };

    const getTokenRefreshDelayMs = () => {
      const tokenParsed = getTokenParsed() as { exp?: number } | undefined;
      if (!tokenParsed?.exp) {
        return 30000;
      }

      const expiresAtMs = tokenParsed.exp * 1000;
      const refreshAtMs = expiresAtMs - 60000;
      return Math.max(5000, refreshAtMs - Date.now());
    };

    const stopRefreshLoop = () => {
      if (!refreshTimeoutId) return;
      clearTimeout(refreshTimeoutId);
      refreshTimeoutId = null;
    };

    const ensureFreshSession = async (minValidity = 60) => {
      const refreshed = await refreshToken(minValidity);
      if (!refreshed && !isAuthenticated()) {
        stopRefreshLoop();
        setAnonymous("expired");
        return;
      }

      if (isAuthenticated()) {
        void updateAuthState(true);
      }
    };

    const startRefreshLoop = () => {
      stopRefreshLoop();
      refreshTimeoutId = setTimeout(() => {
        void ensureFreshSession(60);
      }, getTokenRefreshDelayMs());
    };

    bindAuthEvents({
      onAuthenticatedChanged: (authenticated) => {
        void updateAuthState(authenticated);
      },
      onTokenRefreshed: () => {
        startRefreshLoop();
        void updateAuthState(true);
      },
      onTokenExpired: () => {
        void ensureFreshSession(0);
      },
    });

    const onWindowFocus = () => {
      void ensureFreshSession();
    };
    window.addEventListener("focus", onWindowFocus);

    initKeycloak()
      .then((authenticated) => {
        void updateAuthState(authenticated);
      })
      .catch((err) => {
        console.error("Keycloak init failed:", err);
        setAnonymous("expired");
      });

    return () => {
      mounted = false;
      stopRefreshLoop();
      window.removeEventListener("focus", onWindowFocus);
    };
  }, []);

  return (
    <AuthContext.Provider value={state}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuthContext(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuthContext must be used within <AuthProvider>");
  }
  return ctx;
}
