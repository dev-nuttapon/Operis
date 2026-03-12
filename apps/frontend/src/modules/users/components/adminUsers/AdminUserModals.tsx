import type { ReactNode } from "react";
import { Card, Form, Input, Modal, Select, Space, Typography } from "antd";
import type { User } from "../../types/users";

interface OptionItem {
  label: ReactNode;
  value: string;
}

interface AdminUserModalsProps {
  createForm: any;
  createLoading: boolean;
  creatingUser: boolean;
  deleteForm: any;
  deleteLoading: boolean;
  deletingUser: User | null;
  departmentOptions: OptionItem[];
  departmentsLoading: boolean;
  editForm: any;
  editLoading: boolean;
  editingUser: User | null;
  jobTitleOptions: OptionItem[];
  jobTitlesLoading: boolean;
  onCloseCreate: () => void;
  onCloseDelete: () => void;
  onCloseEdit: () => void;
  onCreate: () => void;
  onDelete: () => void;
  onEdit: () => void;
  roleOptions: OptionItem[];
  rolesLoading: boolean;
  t: (key: string, options?: Record<string, unknown>) => string;
}

export function AdminUserModals({
  createForm,
  createLoading,
  creatingUser,
  deleteForm,
  deleteLoading,
  deletingUser,
  departmentOptions,
  departmentsLoading,
  editForm,
  editLoading,
  editingUser,
  jobTitleOptions,
  jobTitlesLoading,
  onCloseCreate,
  onCloseDelete,
  onCloseEdit,
  onCreate,
  onDelete,
  onEdit,
  roleOptions,
  rolesLoading,
  t,
}: AdminUserModalsProps) {
  return (
    <>
      <Modal
        title={t("admin_users.directory.create_modal_title")}
        open={creatingUser}
        width={820}
        destroyOnHidden
        okText={t("common.actions.save")}
        cancelText={t("common.actions.cancel")}
        confirmLoading={createLoading}
        onCancel={onCloseCreate}
        onOk={onCreate}
      >
        <Form layout="vertical" form={createForm}>
          <Space direction="vertical" size={20} style={{ width: "100%" }}>
            <Card size="small" variant="borderless" style={{ background: "rgba(14, 165, 233, 0.06)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                {t("admin_users.sections.account_information")}
              </Typography.Title>
              <Form.Item label={t("admin_users.fields.email")} name="email" rules={[{ required: true, type: "email" }]}>
                <Input placeholder={t("admin_users.placeholders.user_email")} />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.first_name")} name="firstName" rules={[{ required: true, message: t("invitation_page.first_name_required") }]}>
                <Input placeholder={t("invitation_page.first_name_placeholder")} />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.last_name")} name="lastName" rules={[{ required: true, message: t("invitation_page.last_name_required") }]}>
                <Input placeholder={t("invitation_page.last_name_placeholder")} />
              </Form.Item>
              <Form.Item
                label={t("invitation_page.password_label")}
                name="password"
                rules={[
                  { required: true, message: t("errors.password_required") },
                  { min: 8, message: t("errors.password_min_length") },
                ]}
              >
                <Input.Password placeholder={t("invitation_page.password_placeholder")} />
              </Form.Item>
              <Form.Item
                label={t("invitation_page.confirm_password_label")}
                name="confirmPassword"
                dependencies={["password"]}
                rules={[
                  { required: true, message: t("invitation_page.confirm_password_required") },
                  ({ getFieldValue }) => ({
                    validator(_, value) {
                      if (!value || getFieldValue("password") === value) {
                        return Promise.resolve();
                      }

                      return Promise.reject(new Error(t("errors.password_mismatch")));
                    },
                  }),
                ]}
              >
                <Input.Password placeholder={t("invitation_page.confirm_password_placeholder")} />
              </Form.Item>
            </Card>

            <Card size="small" variant="borderless" style={{ background: "rgba(15, 23, 42, 0.03)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                {t("admin_users.sections.organization_structure")}
              </Typography.Title>
              <Form.Item label={t("admin_users.fields.department")} name="departmentId">
                <Select allowClear placeholder={t("admin_users.placeholders.select_department")} loading={departmentsLoading} options={departmentOptions} />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.job_title")} name="jobTitleId">
                <Select allowClear placeholder={t("admin_users.placeholders.select_job_title")} loading={jobTitlesLoading} options={jobTitleOptions} />
              </Form.Item>
            </Card>

            <Card size="small" variant="borderless" style={{ background: "rgba(22, 163, 74, 0.05)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                {t("admin_users.sections.access_rights")}
              </Typography.Title>
              <Form.Item label={t("admin_users.fields.roles")} name="roles" extra={t("admin_users.fields.roles_help")}>
                <Select mode="multiple" allowClear placeholder={t("admin_users.placeholders.select_roles")} loading={rolesLoading} options={roleOptions} />
              </Form.Item>
            </Card>
          </Space>
        </Form>
      </Modal>

      <Modal
        title={t("admin_users.directory.edit_modal_title")}
        open={editingUser !== null}
        width={820}
        destroyOnHidden
        okText={t("common.actions.save")}
        cancelText={t("common.actions.cancel")}
        confirmLoading={editLoading}
        onCancel={onCloseEdit}
        onOk={onEdit}
      >
        <Form layout="vertical" form={editForm}>
          <Space direction="vertical" size={20} style={{ width: "100%" }}>
            <Card size="small" variant="borderless" style={{ background: "rgba(14, 165, 233, 0.06)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                {t("admin_users.sections.account_information")}
              </Typography.Title>
              <Form.Item label={t("admin_users.fields.email")} name="email" rules={[{ required: true, type: "email" }]}>
                <Input placeholder={t("admin_users.placeholders.user_email")} />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.first_name")} name="firstName" rules={[{ required: true, message: t("invitation_page.first_name_required") }]}>
                <Input placeholder={t("invitation_page.first_name_placeholder")} />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.last_name")} name="lastName" rules={[{ required: true, message: t("invitation_page.last_name_required") }]}>
                <Input placeholder={t("invitation_page.last_name_placeholder")} />
              </Form.Item>
            </Card>

            <Card size="small" variant="borderless" style={{ background: "rgba(15, 23, 42, 0.03)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                {t("admin_users.sections.organization_structure")}
              </Typography.Title>
              <Form.Item label={t("admin_users.fields.department")} name="departmentId">
                <Select allowClear placeholder={t("admin_users.placeholders.select_department")} loading={departmentsLoading} options={departmentOptions} />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.job_title")} name="jobTitleId">
                <Select allowClear placeholder={t("admin_users.placeholders.select_job_title")} loading={jobTitlesLoading} options={jobTitleOptions} />
              </Form.Item>
            </Card>

            <Card size="small" variant="borderless" style={{ background: "rgba(22, 163, 74, 0.05)" }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                {t("admin_users.sections.access_rights")}
              </Typography.Title>
              <Form.Item label={t("admin_users.fields.roles")} name="roleIds" extra={t("admin_users.fields.roles_help")}>
                <Select mode="multiple" allowClear placeholder={t("admin_users.placeholders.select_roles")} loading={rolesLoading} options={roleOptions} />
              </Form.Item>
            </Card>
          </Space>
        </Form>
      </Modal>

      <Modal
        title={deletingUser ? t("admin_users.directory.delete_modal_title_with_email", { email: deletingUser.keycloak?.email || deletingUser.id }) : t("admin_users.directory.delete_modal_title")}
        open={deletingUser !== null}
        destroyOnHidden
        okText={t("common.actions.confirm_delete")}
        cancelText={t("common.actions.cancel")}
        okButtonProps={{ danger: true }}
        confirmLoading={deleteLoading}
        onCancel={onCloseDelete}
        onOk={onDelete}
      >
        <Form layout="vertical" form={deleteForm}>
          <Typography.Paragraph type="secondary">{t("admin_users.directory.delete_description")}</Typography.Paragraph>
          <Form.Item name="reason" label={t("admin_users.fields.delete_reason")} rules={[{ required: true, message: t("admin_users.validation.delete_reason_required") }]}>
            <Input.TextArea rows={4} placeholder={t("admin_users.placeholders.user_delete_reason")} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
