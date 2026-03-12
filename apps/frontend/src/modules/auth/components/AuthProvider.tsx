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
  roles: string[];
  [key: string]: unknown;
}

interface AuthContextValue {
  isReady: boolean;
  isAuthenticated: boolean;
  user?: AuthUser | null;
  login: () => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: PropsWithChildren) {
  const [state, setState] = useState<AuthContextValue>({
    isReady: false,
    isAuthenticated: false,
    user: null,
    login,
    logout,
  });

  useEffect(() => {
    let mounted = true;
    let refreshTimeoutId: ReturnType<typeof setTimeout> | null = null;

    const setAnonymous = () => {
      if (!mounted) return;
      setState({ isReady: true, isAuthenticated: false, user: null, login, logout });
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

    const buildUser = async (): Promise<AuthUser | null> => {
      const tokenParsed = getTokenParsed() as Record<string, unknown> | undefined;
      const roles = extractRoles(tokenParsed);

      try {
        const profile = await getUserProfile();
        return {
          ...profile,
          email: profile?.email || (typeof tokenParsed?.email === "string" ? tokenParsed.email : undefined),
          name: profile?.firstName
            ? `${profile.firstName} ${profile.lastName}`
            : (typeof tokenParsed?.name === "string" ? tokenParsed.name : (typeof tokenParsed?.preferred_username === "string" ? tokenParsed.preferred_username : undefined)),
          roles,
        };
      } catch (err) {
        console.error("Failed to load user profile:", err);
        return {
          email: typeof tokenParsed?.email === "string" ? tokenParsed.email : undefined,
          name: typeof tokenParsed?.name === "string" ? tokenParsed.name : (typeof tokenParsed?.preferred_username === "string" ? tokenParsed.preferred_username : undefined),
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
        setAnonymous();
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
        setAnonymous();
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
