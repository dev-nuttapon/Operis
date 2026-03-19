import type { ReactNode } from "react";
import { Button, Dropdown } from "antd";
import type { ButtonProps, DropdownProps, MenuProps } from "antd";
import { MoreOutlined } from "@ant-design/icons";

export type ActionMenuItem = {
  key: string;
  label: ReactNode;
  icon?: ReactNode;
  disabled?: boolean;
  danger?: boolean;
  onClick?: () => void;
};

type ActionMenuProps = {
  items: ActionMenuItem[];
  ariaLabel?: string;
  size?: ButtonProps["size"];
  loading?: boolean;
  disabled?: boolean;
  placement?: DropdownProps["placement"];
  stopPropagation?: boolean;
};

export function ActionMenu({
  items,
  ariaLabel = "actions",
  size = "small",
  loading,
  disabled,
  placement = "bottomRight",
  stopPropagation = true,
}: ActionMenuProps) {
  const itemMap = new Map(items.map((item) => [item.key, item]));
  const menuItems: MenuProps["items"] = items.map((item) => ({
    key: item.key,
    label: item.label,
    icon: item.icon,
    disabled: item.disabled,
    danger: item.danger,
  }));

  const handleMenuClick: MenuProps["onClick"] = (info) => {
    if (stopPropagation) {
      info.domEvent?.stopPropagation();
    }
    const target = itemMap.get(String(info.key));
    target?.onClick?.();
  };

  return (
    <Dropdown menu={{ items: menuItems, onClick: handleMenuClick }} trigger={["click"]} placement={placement}>
      <Button
        size={size}
        icon={<MoreOutlined />}
        loading={loading}
        disabled={disabled}
        aria-label={ariaLabel}
        onClick={(event) => {
          if (stopPropagation) {
            event.stopPropagation();
          }
        }}
      />
    </Dropdown>
  );
}
