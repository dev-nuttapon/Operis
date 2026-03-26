import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

export type ThemeMode = "light" | "dark" | "system";

interface ThemeState {
  theme: ThemeMode;
  setTheme: (theme: ThemeMode) => void;
}

export const useThemeStore = create<ThemeState>()(
  persist(
    (set) => ({
      theme: "system",
      setTheme: (theme) => set({ theme }),
    }),
    {
      name: "operis-theme-storage", // prefix for localStorage key
      storage: createJSONStorage(() => {
        if (typeof window !== "undefined" && typeof window.localStorage !== "undefined") {
          const candidate = window.localStorage as Partial<Storage>;
          if (
            typeof candidate.getItem === "function"
            && typeof candidate.setItem === "function"
            && typeof candidate.removeItem === "function"
          ) {
            return candidate as Storage;
          }
        }

        const memory = new Map<string, string>();
        return {
          getItem: (name) => memory.get(name) ?? null,
          setItem: (name, value) => {
            memory.set(name, value);
          },
          removeItem: (name) => {
            memory.delete(name);
          },
          clear: () => {
            memory.clear();
          },
          key: (index) => Array.from(memory.keys())[index] ?? null,
          get length() {
            return memory.size;
          },
        } satisfies Storage;
      }),
    }
  )
);
