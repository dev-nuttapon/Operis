import { useAuthContext } from "../components/AuthProvider";

/**
 * Hook to access authentication state and actions.
 * Uses shared AuthContext initialized once at app root via <AuthProvider>.
 */
export function useAuth() {
  return useAuthContext();
}
