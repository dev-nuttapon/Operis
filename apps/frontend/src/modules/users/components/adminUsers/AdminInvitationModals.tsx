import type { ReactNode } from "react";
import { Button, DatePicker, Form, Input, Modal, Select, Space, Typography } from "antd";
import { MailOutlined } from "@ant-design/icons";
import type { Invitation } from "../../types/users";

interface OptionItem {
  label: ReactNode;
  value: string;
}

interface AdminInvitationModalsProps {
  createLoading: boolean;
  creatingInvitation: boolean;
  departmentOptions: OptionItem[];
  editForm: any;
  editingInvitation: Invitation | null;
  invitationForm: any;
  jobTitleOptions: OptionItem[];
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
  departmentOptions,
  editForm,
  editingInvitation,
  invitationForm,
  jobTitleOptions,
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
          <Form.Item label={t("admin_users.fields.department")} name="departmentId">
            <Select allowClear placeholder={t("admin_users.placeholders.select_department")} options={departmentOptions} />
          </Form.Item>
          <Form.Item label={t("admin_users.fields.job_title")} name="jobTitleId">
            <Select allowClear placeholder={t("admin_users.placeholders.select_job_title")} options={jobTitleOptions} />
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
          <Form.Item label={t("admin_users.fields.department")} name="departmentId">
            <Select allowClear placeholder={t("admin_users.placeholders.select_department")} options={departmentOptions} />
          </Form.Item>
          <Form.Item label={t("admin_users.fields.job_title")} name="jobTitleId">
            <Select allowClear placeholder={t("admin_users.placeholders.select_job_title")} options={jobTitleOptions} />
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
