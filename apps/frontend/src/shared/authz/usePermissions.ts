import { useMemo } from "react";
import { useAuth } from "../../modules/auth";
import { getPermissionsForRoles, type Permission } from "./permissions";

export function usePermissions() {
  const { user } = useAuth();
  const roles = Array.isArray(user?.roles) ? user.roles : [];

  const granted = useMemo(() => getPermissionsForRoles(roles), [roles]);

  return {
    roles,
    permissions: granted,
    hasPermission: (permission: Permission) => granted.includes(permission),
    hasAnyPermission: (...requested: Permission[]) => requested.some((permission) => granted.includes(permission)),
    hasAllPermissions: (...requested: Permission[]) => requested.every((permission) => granted.includes(permission)),
  };
}
