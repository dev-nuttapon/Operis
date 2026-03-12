import { useSyncExternalStore } from "react";
import i18n from "../config";

function subscribe(onStoreChange: () => void) {
  const handler = () => onStoreChange();
  i18n.on("languageChanged", handler);
  return () => {
    i18n.off("languageChanged", handler);
  };
}

function getSnapshot() {
  return i18n.language;
}

export function useI18nLanguage(): string {
  return useSyncExternalStore(subscribe, getSnapshot, getSnapshot);
}
