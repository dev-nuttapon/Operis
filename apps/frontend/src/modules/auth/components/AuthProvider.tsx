import { createContext, useContext, useEffect, useState, type PropsWithChildren } from "react";
import { bindAuthEvents, initKeycloak, login, logout } from "../services/keycloakAuth";

interface AuthContextValue {
  isReady: boolean;
  isAuthenticated: boolean;
  login: () => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: PropsWithChildren) {
  const [state, setState] = useState<AuthContextValue>({
    isReady: false,
    isAuthenticated: false,
    login,
    logout,
  });

  useEffect(() => {
    let mounted = true;

    bindAuthEvents({
      onAuthenticatedChanged: (authenticated) => {
        if (!mounted) {
          return;
        }
        setState({ isReady: true, isAuthenticated: authenticated, login, logout });
      },
      onTokenExpired: () => {
        if (!mounted) {
          return;
        }
        setState({ isReady: true, isAuthenticated: false, login, logout });
      },
    });

    initKeycloak()
      .then((authenticated) => {
        if (mounted) {
          setState({ isReady: true, isAuthenticated: authenticated, login, logout });
        }
      })
      .catch((err) => {
        console.error("Keycloak init failed:", err);
        if (mounted) {
          setState({ isReady: true, isAuthenticated: false, login, logout });
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
