import type { ReactNode } from "react";
import { Card, Form, Input, Modal, Select, Space, Typography } from "antd";
import { useDepartmentOptions } from "../../hooks/useDepartmentOptions";
import { useJobTitleOptions } from "../../hooks/useJobTitleOptions";
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
  divisionHasMore: boolean;
  divisionLoading: boolean;
  divisionOptions: OptionItem[];
  editForm: any;
  editLoading: boolean;
  editingUser: User | null;
  onDivisionLoadMore: () => void;
  onDivisionSearch: (value: string) => void;
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
  divisionHasMore,
  divisionLoading,
  divisionOptions,
  editForm,
  editLoading,
  editingUser,
  onDivisionLoadMore,
  onDivisionSearch,
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
  const createDivisionId = Form.useWatch("divisionId", createForm) as string | undefined;
  const createDepartmentId = Form.useWatch("departmentId", createForm) as string | undefined;
  const editDivisionId = Form.useWatch("divisionId", editForm) as string | undefined;
  const editDepartmentId = Form.useWatch("departmentId", editForm) as string | undefined;
  const editJobTitleId = Form.useWatch("jobTitleId", editForm) as string | undefined;
  const createDepartments = useDepartmentOptions({ enabled: creatingUser, divisionId: createDivisionId, pageSize: 5 });
  const editDepartments = useDepartmentOptions({ enabled: Boolean(editingUser), divisionId: editDivisionId, pageSize: 5 });
  const createJobTitles = useJobTitleOptions({ enabled: creatingUser, departmentId: createDepartmentId, pageSize: 5 });
  const editJobTitles = useJobTitleOptions({ enabled: Boolean(editingUser), departmentId: editDepartmentId, pageSize: 5 });

  const ensureOption = (options: OptionItem[], value?: string, label?: string | null) => {
    if (!value || options.some((option) => option.value === value)) {
      return options;
    }
    return [{ label: label ?? value, value }, ...options];
  };

  const editDivisionOptions = ensureOption(divisionOptions, editDivisionId, editingUser?.divisionName ?? undefined);
  const createDepartmentOptions = createDepartments.options;
  const editDepartmentOptions = ensureOption(
    editDepartments.options,
    editDepartmentId,
    editingUser?.departmentName ?? undefined
  );
  const createJobTitleOptions = createJobTitles.options;
  const editJobTitleOptions = ensureOption(
    editJobTitles.options,
    editJobTitleId,
    editingUser?.jobTitleName ?? undefined
  );

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
              <Form.Item label={t("admin_users.fields.division")} name="divisionId">
                <Select
                  allowClear
                  showSearch
                  filterOption={false}
                  placeholder={t("admin_users.placeholders.select_division")}
                  options={divisionOptions}
                  loading={divisionLoading}
                  onSearch={onDivisionSearch}
                  dropdownRender={(menu) => (
                    <>
                      {menu}
                      {divisionHasMore ? (
                        <div style={{ padding: 8 }}>
                          <button
                            type="button"
                            onMouseDown={(event) => event.preventDefault()}
                            onClick={onDivisionLoadMore}
                            style={{
                              width: "100%",
                              border: "none",
                              background: "transparent",
                              color: "#1677ff",
                              cursor: "pointer",
                              padding: 4,
                            }}
                          >
                            {t("admin_users.load_more_divisions")}
                          </button>
                        </div>
                      ) : null}
                    </>
                  )}
                  onChange={() => {
                    createForm.setFieldValue("departmentId", undefined);
                    createForm.setFieldValue("jobTitleId", undefined);
                  }}
                />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.department")} name="departmentId">
                <Select
                  allowClear
                  showSearch
                  filterOption={false}
                  placeholder={t("admin_users.placeholders.select_department")}
                  disabled={!createDivisionId}
                  loading={createDepartments.loading}
                  options={createDepartmentOptions}
                  onSearch={createDepartments.onSearch}
                  dropdownRender={(menu) => (
                    <>
                      {menu}
                      {createDepartments.hasMore ? (
                        <div style={{ padding: 8 }}>
                          <button
                            type="button"
                            onMouseDown={(event) => event.preventDefault()}
                            onClick={createDepartments.onLoadMore}
                            style={{
                              width: "100%",
                              border: "none",
                              background: "transparent",
                              color: "#1677ff",
                              cursor: "pointer",
                              padding: 4,
                            }}
                          >
                            {t("admin_users.load_more_departments")}
                          </button>
                        </div>
                      ) : null}
                    </>
                  )}
                  onChange={() => {
                    createForm.setFieldValue("jobTitleId", undefined);
                  }}
                />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.job_title")} name="jobTitleId">
                <Select
                  allowClear
                  showSearch
                  filterOption={false}
                  disabled={!createDepartmentId}
                  placeholder={t("admin_users.placeholders.select_job_title")}
                  loading={createJobTitles.loading}
                  options={createJobTitleOptions}
                  onSearch={createJobTitles.onSearch}
                  dropdownRender={(menu) => (
                    <>
                      {menu}
                      {createJobTitles.hasMore ? (
                        <div style={{ padding: 8 }}>
                          <button
                            type="button"
                            onMouseDown={(event) => event.preventDefault()}
                            onClick={createJobTitles.onLoadMore}
                            style={{
                              width: "100%",
                              border: "none",
                              background: "transparent",
                              color: "#1677ff",
                              cursor: "pointer",
                              padding: 4,
                            }}
                          >
                            {t("admin_users.load_more_job_titles")}
                          </button>
                        </div>
                      ) : null}
                    </>
                  )}
                />
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
              <Form.Item label={t("admin_users.fields.division")} name="divisionId">
                <Select
                  allowClear
                  showSearch
                  filterOption={false}
                  placeholder={t("admin_users.placeholders.select_division")}
                  options={editDivisionOptions}
                  loading={divisionLoading}
                  onSearch={onDivisionSearch}
                  dropdownRender={(menu) => (
                    <>
                      {menu}
                      {divisionHasMore ? (
                        <div style={{ padding: 8 }}>
                          <button
                            type="button"
                            onMouseDown={(event) => event.preventDefault()}
                            onClick={onDivisionLoadMore}
                            style={{
                              width: "100%",
                              border: "none",
                              background: "transparent",
                              color: "#1677ff",
                              cursor: "pointer",
                              padding: 4,
                            }}
                          >
                            {t("admin_users.load_more_divisions")}
                          </button>
                        </div>
                      ) : null}
                    </>
                  )}
                  onChange={() => {
                    editForm.setFieldValue("departmentId", undefined);
                    editForm.setFieldValue("jobTitleId", undefined);
                  }}
                />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.department")} name="departmentId">
                <Select
                  allowClear
                  showSearch
                  filterOption={false}
                  placeholder={t("admin_users.placeholders.select_department")}
                  disabled={!editDivisionId}
                  loading={editDepartments.loading}
                  options={editDepartmentOptions}
                  onSearch={editDepartments.onSearch}
                  dropdownRender={(menu) => (
                    <>
                      {menu}
                      {editDepartments.hasMore ? (
                        <div style={{ padding: 8 }}>
                          <button
                            type="button"
                            onMouseDown={(event) => event.preventDefault()}
                            onClick={editDepartments.onLoadMore}
                            style={{
                              width: "100%",
                              border: "none",
                              background: "transparent",
                              color: "#1677ff",
                              cursor: "pointer",
                              padding: 4,
                            }}
                          >
                            {t("admin_users.load_more_departments")}
                          </button>
                        </div>
                      ) : null}
                    </>
                  )}
                  onChange={() => {
                    editForm.setFieldValue("jobTitleId", undefined);
                  }}
                />
              </Form.Item>
              <Form.Item label={t("admin_users.fields.job_title")} name="jobTitleId">
                <Select
                  allowClear
                  showSearch
                  filterOption={false}
                  disabled={!editDepartmentId}
                  placeholder={t("admin_users.placeholders.select_job_title")}
                  loading={editJobTitles.loading}
                  options={editJobTitleOptions}
                  onSearch={editJobTitles.onSearch}
                  dropdownRender={(menu) => (
                    <>
                      {menu}
                      {editJobTitles.hasMore ? (
                        <div style={{ padding: 8 }}>
                          <button
                            type="button"
                            onMouseDown={(event) => event.preventDefault()}
                            onClick={editJobTitles.onLoadMore}
                            style={{
                              width: "100%",
                              border: "none",
                              background: "transparent",
                              color: "#1677ff",
                              cursor: "pointer",
                              padding: 4,
                            }}
                          >
                            {t("admin_users.load_more_job_titles")}
                          </button>
                        </div>
                      ) : null}
                    </>
                  )}
                />
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
