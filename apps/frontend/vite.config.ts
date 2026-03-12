import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  build: {
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (!id.includes("node_modules")) {
            return undefined;
          }

          if (id.includes("/rc-")) {
            return "rc-vendor";
          }

          if (id.includes("/antd/") || id.includes("/@ant-design/")) {
            return "antd-core-vendor";
          }

          if (id.includes("/dayjs/")) {
            return "date-vendor";
          }

          if (id.includes("/react-query/") || id.includes("/@tanstack/")) {
            return "query-vendor";
          }

          if (id.includes("/react-i18next/") || id.includes("/i18next") || id.includes("/i18next-browser-languagedetector/")) {
            return "i18n-vendor";
          }

          if (id.includes("/keycloak-js/")) {
            return "auth-vendor";
          }

          if (id.includes("/react/") || id.includes("/react-dom/") || id.includes("/react-router")) {
            return "react-vendor";
          }

          return undefined;
        },
      },
    },
  },
  server: {
    headers: {
      "X-Content-Type-Options": "nosniff",
      "X-Frame-Options": "SAMEORIGIN",
      "X-XSS-Protection": "1; mode=block",
      "Strict-Transport-Security": "max-age=31536000; includeSubDomains"
    }
  },
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: "./src/tests/setup.ts",
    css: true,
  }
});
