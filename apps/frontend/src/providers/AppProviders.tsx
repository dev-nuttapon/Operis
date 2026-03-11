import { useEffect, useState } from "react";
import type { PropsWithChildren } from "react";
import { ConfigProvider, theme as antdTheme } from "antd";
import { useThemeStore } from "../shared/store/useThemeStore";
import "../shared/i18n/config"; // Initialize i18n

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
    <ConfigProvider
      theme={{
        algorithm: isDarkMode ? antdTheme.darkAlgorithm : antdTheme.defaultAlgorithm,
        token: {
          // You can also add global token overrides here later (e.g. fontFamily)
          fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial",
        }
      }}
    >
      {children}
    </ConfigProvider>
  );
}
