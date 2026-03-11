import type { PropsWithChildren } from "react";
import { ConfigProvider } from "antd";

export function AppProviders({ children }: PropsWithChildren) {
  return <ConfigProvider>{children}</ConfigProvider>;
}
