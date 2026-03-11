import { createContext, useContext, useEffect, useState, type PropsWithChildren } from "react";
import {
  clearAuthCallbackParams,
  getAuthFlag,
  hasAuthCallbackParams,
  login,
  logout,
  setAuthFlag,
} from "../services/keycloakAuth";

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
    if (hasAuthCallbackParams()) {
      setAuthFlag(true);
      clearAuthCallbackParams();
    }
    setState({ isReady: true, isAuthenticated: getAuthFlag(), login, logout });
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
