import type { ReactNode } from "react";
import { Button, DatePicker, Form, Input, Modal, Select, Space, Typography } from "antd";
import { MailOutlined } from "@ant-design/icons";
import { useDepartmentOptions } from "../../hooks/useDepartmentOptions";
import { useJobTitleOptions } from "../../hooks/useJobTitleOptions";
import type { Invitation } from "../../types/users";

interface OptionItem {
  label: ReactNode;
  value: string;
}

interface AdminInvitationModalsProps {
  createLoading: boolean;
  creatingInvitation: boolean;
  divisionHasMore: boolean;
  divisionLoading: boolean;
  divisionOptions: OptionItem[];
  editForm: any;
  editingInvitation: Invitation | null;
  invitationForm: any;
  onDivisionLoadMore: () => void;
  onDivisionSearch: (value: string) => void;
  onCloseEdit: () => void;
  onCloseView: () => void;
  onCopyViewLink: () => void;
  onCreate: () => void;
  onEdit: () => void;
  onOpenChangeCreate: (open: boolean) => void;
  t: (key: string, options?: Record<string, unknown>) => string;
  updateLoading: boolean;
  viewingInvitation: Invitation | null;
}

export function AdminInvitationModals({
  createLoading,
  creatingInvitation,
  divisionHasMore,
  divisionLoading,
  divisionOptions,
  editForm,
  editingInvitation,
  invitationForm,
  onDivisionLoadMore,
  onDivisionSearch,
  onCloseEdit,
  onCloseView,
  onCopyViewLink,
  onCreate,
  onEdit,
  onOpenChangeCreate,
  t,
  updateLoading,
  viewingInvitation,
}: AdminInvitationModalsProps) {
  const createDivisionId = Form.useWatch("divisionId", invitationForm) as string | undefined;
  const createDepartmentId = Form.useWatch("departmentId", invitationForm) as string | undefined;
  const editDivisionId = Form.useWatch("divisionId", editForm) as string | undefined;
  const editDepartmentId = Form.useWatch("departmentId", editForm) as string | undefined;
  const editJobTitleId = Form.useWatch("jobTitleId", editForm) as string | undefined;
  const createDepartments = useDepartmentOptions({ enabled: creatingInvitation, divisionId: createDivisionId, pageSize: 5 });
  const editDepartments = useDepartmentOptions({ enabled: Boolean(editingInvitation), divisionId: editDivisionId, pageSize: 5 });
  const createJobTitles = useJobTitleOptions({ enabled: creatingInvitation, departmentId: createDepartmentId, pageSize: 5 });
  const editJobTitles = useJobTitleOptions({ enabled: Boolean(editingInvitation), departmentId: editDepartmentId, pageSize: 5 });

  const ensureOption = (options: OptionItem[], value?: string, label?: string | null) => {
    if (!value || options.some((option) => option.value === value)) {
      return options;
    }
    return [{ label: label ?? value, value }, ...options];
  };

  const editDivisionOptions = ensureOption(divisionOptions, editDivisionId, editingInvitation?.divisionName ?? undefined);
  const createDepartmentOptions = createDepartments.options;
  const editDepartmentOptions = ensureOption(
    editDepartments.options,
    editDepartmentId,
    editingInvitation?.departmentName ?? undefined
  );
  const createJobTitleOptions = createJobTitles.options;
  const editJobTitleOptions = ensureOption(
    editJobTitles.options,
    editJobTitleId,
    editingInvitation?.jobTitleName ?? undefined
  );
  return (
    <>
      <Modal
        title={t("admin_users.invitations.view_title")}
        open={viewingInvitation !== null}
        destroyOnHidden
        onCancel={onCloseView}
        footer={[
          <Button key="copy" type="primary" onClick={onCopyViewLink}>
            {t("common.actions.copy_link")}
          </Button>,
          <Button key="close" onClick={onCloseView}>
            {t("common.actions.close")}
          </Button>,
        ]}
      >
        <Space direction="vertical" size={12} style={{ width: "100%" }}>
          <Typography.Text strong>{viewingInvitation?.email}</Typography.Text>
          <Typography.Text type="secondary">{t("admin_users.view.division")}: {viewingInvitation?.divisionName || "-"}</Typography.Text>
          <Typography.Text type="secondary">{t("admin_users.view.department")}: {viewingInvitation?.departmentName || "-"}</Typography.Text>
          <Typography.Text type="secondary">{t("admin_users.view.job_title")}: {viewingInvitation?.jobTitleName || "-"}</Typography.Text>
          <Input.TextArea
            value={viewingInvitation ? `${window.location.origin}${viewingInvitation.invitationLink}` : ""}
            autoSize={{ minRows: 3, maxRows: 5 }}
            readOnly
          />
        </Space>
      </Modal>

      <Modal
        title={t("admin_users.invitations.create_modal_title")}
        open={creatingInvitation}
        destroyOnHidden
        okText={t("common.actions.send_invitation")}
        cancelText={t("common.actions.cancel")}
        confirmLoading={createLoading}
        onCancel={() => {
          onOpenChangeCreate(false);
          invitationForm.resetFields();
        }}
        onOk={onCreate}
      >
        <Form layout="vertical" form={invitationForm}>
          <Form.Item label={t("admin_users.fields.email")} name="email" rules={[{ required: true, type: "email" }]}>
            <Input prefix={<MailOutlined />} placeholder={t("admin_users.placeholders.invitee_email")} />
          </Form.Item>
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
                invitationForm.setFieldValue("departmentId", undefined);
                invitationForm.setFieldValue("jobTitleId", undefined);
              }}
            />
          </Form.Item>
          <Form.Item label={t("admin_users.fields.department")} name="departmentId">
                <Select
                  allowClear
                  disabled={!createDivisionId}
                  showSearch
                  filterOption={false}
                  placeholder={t("admin_users.placeholders.select_department")}
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
                invitationForm.setFieldValue("jobTitleId", undefined);
              }}
            />
          </Form.Item>
          <Form.Item label={t("admin_users.fields.job_title")} name="jobTitleId">
            <Select
              allowClear
              disabled={!createDepartmentId}
              showSearch
              filterOption={false}
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
          <Form.Item label={t("admin_users.fields.expires_at")} name="expiresAt">
            <DatePicker
              style={{ width: "100%" }}
              format="DD/MM/YYYY"
              placeholder={t("admin_users.placeholders.select_expiration_date")}
              disabledDate={(current) => Boolean(current && current.endOf("day").valueOf() <= Date.now())}
            />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={t("admin_users.invitations.edit_modal_title")}
        open={editingInvitation !== null}
        destroyOnHidden
        okText={t("common.actions.save")}
        cancelText={t("common.actions.cancel")}
        confirmLoading={updateLoading}
        onCancel={onCloseEdit}
        onOk={onEdit}
      >
        <Form layout="vertical" form={editForm}>
          <Form.Item label={t("admin_users.fields.email")} name="email" rules={[{ required: true, type: "email" }]}>
            <Input prefix={<MailOutlined />} placeholder={t("admin_users.placeholders.invitee_email")} />
          </Form.Item>
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
                  disabled={!editDivisionId}
                  showSearch
                  filterOption={false}
                  placeholder={t("admin_users.placeholders.select_department")}
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
              disabled={!editDepartmentId}
              showSearch
              filterOption={false}
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
          <Form.Item label={t("admin_users.fields.expires_at")} name="expiresAt">
            <DatePicker
              style={{ width: "100%" }}
              format="DD/MM/YYYY"
              placeholder={t("admin_users.placeholders.select_expiration_date")}
              disabledDate={(current) => Boolean(current && current.endOf("day").valueOf() <= Date.now())}
            />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
