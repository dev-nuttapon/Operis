import { createContext, useContext, useEffect, useState, type PropsWithChildren } from "react";
import { bindAuthEvents, initKeycloak, login, logout } from "../services/keycloakAuth";

interface AuthContextValue {
  isReady: boolean;
  isAuthenticated: boolean;
  user: any | null;
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

    const updateAuthState = async (authenticated: boolean) => {
      if (!mounted) return;
      
      let user = null;
      if (authenticated) {
        try {
          const { getUserProfile, getTokenParsed } = await import("../services/keycloakAuth");
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
    };

    bindAuthEvents({
      onAuthenticatedChanged: (authenticated) => {
        updateAuthState(authenticated);
      },
      onTokenExpired: () => {
        if (mounted) {
          setState({ isReady: true, isAuthenticated: false, user: null, login, logout });
        }
      },
    });

    initKeycloak()
      .then((authenticated) => {
        updateAuthState(authenticated);
      })
      .catch((err) => {
        console.error("Keycloak init failed:", err);
        if (mounted) {
          setState({ isReady: true, isAuthenticated: false, user: null, login, logout });
        }
      });

    return () => {
      mounted = false;
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
