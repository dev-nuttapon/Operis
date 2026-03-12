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

interface AuthContextValue {
  isReady: boolean;
  isAuthenticated: boolean;
  user?: any | null;
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
    let refreshIntervalId: ReturnType<typeof setInterval> | null = null;

    const setAnonymous = () => {
      if (!mounted) return;
      setState({ isReady: true, isAuthenticated: false, user: null, login, logout });
    };

    const ensureFreshSession = async (minValidity = 60) => {
      const refreshed = await refreshToken(minValidity);
      if (!refreshed && !isAuthenticated()) {
        setAnonymous();
      }
    };

    const startRefreshLoop = () => {
      if (refreshIntervalId) return;
      refreshIntervalId = setInterval(() => {
        void ensureFreshSession();
      }, 30000);
    };

    const stopRefreshLoop = () => {
      if (!refreshIntervalId) return;
      clearInterval(refreshIntervalId);
      refreshIntervalId = null;
    };

    const updateAuthState = async (authenticated: boolean) => {
      if (!mounted) return;
      
      let user = null;
      if (authenticated) {
        try {
          const profile = await getUserProfile();
          const tokenParsed = getTokenParsed();
          user = {
            ...profile,
            email: profile?.email || tokenParsed?.email,
            name: profile?.firstName ? `${profile.firstName} ${profile.lastName}` : (tokenParsed?.name || tokenParsed?.preferred_username),
          };
        } catch (err) {
          console.error("Failed to load user profile:", err);
        }
      }

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

    bindAuthEvents({
      onAuthenticatedChanged: (authenticated) => {
        void updateAuthState(authenticated);
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
