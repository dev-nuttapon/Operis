import { useEffect, useState } from "react";
import { initKeycloak, login, logout } from "../services/keycloakAuth";

export function useAuth() {
  const [isReady, setIsReady] = useState(false);
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    let mounted = true;

    initKeycloak()
      .then((authenticated) => {
        if (!mounted) {
          return;
        }
        setIsAuthenticated(authenticated);
        setIsReady(true);
      })
      .catch(() => {
        if (!mounted) {
          return;
        }
        setIsAuthenticated(false);
        setIsReady(true);
      });

    return () => {
      mounted = false;
    };
  }, []);

  return {
    isReady,
    isAuthenticated,
    login,
    logout,
  };
}
