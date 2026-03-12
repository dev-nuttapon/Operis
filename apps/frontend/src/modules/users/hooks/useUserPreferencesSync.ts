import { useEffect, useRef } from "react";
import { updateCurrentUserPreferences } from "../api/usersApi";
import type { UpdateCurrentUserPreferencesInput } from "../types/users";

export function useUserPreferencesSync(
  input: UpdateCurrentUserPreferencesInput,
  enabled: boolean
) {
  const lastSyncedRef = useRef<string | null>(null);
  const isTestMode = import.meta.env.MODE === "test";

  useEffect(() => {
    if (isTestMode) {
      return;
    }

    if (!enabled) {
      lastSyncedRef.current = null;
      return;
    }

    const preferenceKey = `${input.preferredLanguage ?? ""}:${input.preferredTheme ?? ""}`;
    if (lastSyncedRef.current === preferenceKey) {
      return;
    }

    void updateCurrentUserPreferences(input)
      .then(() => {
        lastSyncedRef.current = preferenceKey;
      })
      .catch((error: unknown) => {
        console.error("Unable to sync user preferences:", error);
      });
  }, [enabled, input, isTestMode]);
}
