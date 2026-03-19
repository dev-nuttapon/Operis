import { Form, Input, Modal, Typography } from "antd";
import type { User } from "../../types/users";

interface AdminUserDeleteModalProps {
  deletingUser: User | null;
  deleteForm: any;
  deleteLoading: boolean;
  onClose: () => void;
  onDelete: () => void;
  t: (key: string, options?: Record<string, unknown>) => string;
}

export function AdminUserDeleteModal({
  deletingUser,
  deleteForm,
  deleteLoading,
  onClose,
  onDelete,
  t,
}: AdminUserDeleteModalProps) {
  return (
    <Modal
      title={
        deletingUser
          ? t("admin_users.directory.delete_modal_title_with_email", { email: deletingUser.keycloak?.email || deletingUser.id })
          : t("admin_users.directory.delete_modal_title")
      }
      open={deletingUser !== null}
      destroyOnHidden
      okText={t("common.actions.confirm_delete")}
      cancelText={t("common.actions.cancel")}
      okButtonProps={{ danger: true }}
      confirmLoading={deleteLoading}
      onCancel={onClose}
      onOk={onDelete}
    >
      <Form layout="vertical" form={deleteForm}>
        <Typography.Paragraph type="secondary">{t("admin_users.directory.delete_description")}</Typography.Paragraph>
        <Form.Item
          name="reason"
          label={t("admin_users.fields.delete_reason")}
          rules={[{ required: true, message: t("admin_users.validation.delete_reason_required") }]}
        >
          <Input.TextArea rows={4} placeholder={t("admin_users.placeholders.user_delete_reason")} />
        </Form.Item>
      </Form>
    </Modal>
  );
}

