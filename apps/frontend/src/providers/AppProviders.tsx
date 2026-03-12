import { useEffect, useRef, useState } from "react";
import type { PropsWithChildren } from "react";
import { App, ConfigProvider, theme as antdTheme } from "antd";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useThemeStore } from "../shared/store/useThemeStore";
import { AuthProvider } from "../modules/auth/components/AuthProvider";
import { useAuth } from "../modules/auth";
import { useI18nLanguage } from "../shared/i18n/hooks/useI18nLanguage";
import { updateCurrentUserPreferences } from "../modules/users/api/usersApi";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      gcTime: 5 * 60_000,
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

function UserPreferenceSync() {
  const { isAuthenticated } = useAuth();
  const language = useI18nLanguage();
  const { theme } = useThemeStore();
  const lastSyncedRef = useRef<string | null>(null);
  const isTestMode = import.meta.env.MODE === "test";

  useEffect(() => {
    if (isTestMode) {
      return;
    }

    if (!isAuthenticated) {
      lastSyncedRef.current = null;
      return;
    }

    const preferenceKey = `${language}:${theme}`;
    if (lastSyncedRef.current === preferenceKey) {
      return;
    }

    void updateCurrentUserPreferences({
      preferredLanguage: language || null,
      preferredTheme: theme,
    })
      .then(() => {
        lastSyncedRef.current = preferenceKey;
      })
      .catch((error: unknown) => {
        console.error("Unable to sync user preferences:", error);
      });
  }, [isAuthenticated, isTestMode, language, theme]);

  return null;
}

export function AppProviders({ children }: PropsWithChildren) {
  const { theme } = useThemeStore();
  const [isSystemDark, setIsSystemDark] = useState(false);

  useEffect(() => {
    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
    setIsSystemDark(mediaQuery.matches);

    const handler = (e: MediaQueryListEvent) => {
      setIsSystemDark(e.matches);
    };

    mediaQuery.addEventListener("change", handler);
    return () => mediaQuery.removeEventListener("change", handler);
  }, []);

  const isDarkMode = theme === "dark" || (theme === "system" && isSystemDark);

  return (
    <AuthProvider>
      <QueryClientProvider client={queryClient}>
        <ConfigProvider
          theme={{
            algorithm: isDarkMode ? antdTheme.darkAlgorithm : antdTheme.defaultAlgorithm,
            token: {
              fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial",
              colorPrimary: '#0284c7', // sky-600
              colorBgBase: isDarkMode ? '#020617' : '#f8fafc', // slate-950 / slate-50
              colorBgContainer: isDarkMode ? '#0f172a' : '#ffffff', // slate-900 / white
              colorBgElevated: isDarkMode ? '#0f172a' : '#ffffff',
              colorTextBase: isDarkMode ? '#f1f5f9' : '#0f172a', // slate-100 / slate-900
              colorTextSecondary: isDarkMode ? '#94a3b8' : '#64748b', // slate-400 / slate-500
              colorBorder: isDarkMode ? '#1e293b' : '#e2e8f0', // slate-800 / slate-200
              colorLink: '#0284c7',
              colorLinkHover: '#0369a1', // sky-700
              borderRadius: 6,
            },
            components: {
              Card: {
                borderRadiusLG: 12,
              },
              Button: {
                controlHeight: 40,
              }
            }
          }}
        >
          <App>
            <UserPreferenceSync />
            {children}
          </App>
        </ConfigProvider>
      </QueryClientProvider>
    </AuthProvider>
  );
}
