import type { ReactNode } from "react";
import { usePermissions } from "../authz/usePermissions";
import type { Permission } from "../authz/permissions";
import { AccessDeniedState } from "./AccessDeniedState";

interface AuthorizedRouteProps {
  permission?: Permission;
  anyOf?: Permission[];
  children: ReactNode;
}

export function AuthorizedRoute({ permission, anyOf, children }: AuthorizedRouteProps) {
  const permissionState = usePermissions();
  const allowed = permission
    ? permissionState.hasPermission(permission)
    : anyOf
      ? permissionState.hasAnyPermission(...anyOf)
      : true;

  if (!allowed) {
    return <AccessDeniedState />;
  }

  return <>{children}</>;
}
