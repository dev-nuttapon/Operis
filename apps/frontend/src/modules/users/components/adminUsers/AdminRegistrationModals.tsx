import { Button, Form, Input, Modal, Select, Space, Typography } from "antd";

interface RegistrationLinkItem {
  divisionName?: string | null;
  departmentName?: string | null;
  email: string;
  jobTitleName?: string | null;
  passwordSetupExpiresAt?: string | null;
  passwordSetupLink?: string | null;
}

interface RegistrationRequestItem {
  email: string;
  id: string;
}

interface AdminRegistrationModalsProps {
  approveLoading: boolean;
  currentLanguage: string;
  formatDate: (value: string | null, language: string) => string;
  linkItem: RegistrationLinkItem | null;
  manageItem: RegistrationRequestItem | null;
  onApprove: () => void;
  onCloseLink: () => void;
  onCloseManage: () => void;
  onCopyLink: () => void;
  onReject: () => void;
  rejectLoading: boolean;
  reviewForm: any;
  t: (key: string, options?: Record<string, unknown>) => string;
}

const { Text } = Typography;

export function AdminRegistrationModals({
  approveLoading,
  currentLanguage,
  formatDate,
  linkItem,
  manageItem,
  onApprove,
  onCloseLink,
  onCloseManage,
  onCopyLink,
  onReject,
  rejectLoading,
  reviewForm,
  t,
}: AdminRegistrationModalsProps) {
  return (
    <>
      <Modal
        title={manageItem ? t("admin_users.registration.manage_modal_title_with_email", { email: manageItem.email }) : t("admin_users.registration.manage_modal_title")}
        open={manageItem !== null}
        okText={t("common.actions.confirm")}
        cancelText={t("common.actions.cancel")}
        confirmLoading={approveLoading || rejectLoading}
        onCancel={onCloseManage}
        onOk={() => {
          const action = reviewForm.getFieldValue("action");
          if (action === "reject") {
            onReject();
            return;
          }

          onApprove();
        }}
      >
        <Form form={reviewForm} layout="vertical" initialValues={{ action: "approve", reason: "" }}>
          <Form.Item label={t("admin_users.registration.manage_action_label")} name="action" rules={[{ required: true }]}>
            <Select
              options={[
                { value: "approve", label: t("common.actions.approve") },
                { value: "reject", label: t("common.actions.reject") },
              ]}
            />
          </Form.Item>
          <Form.Item noStyle shouldUpdate={(prevValues, currentValues) => prevValues.action !== currentValues.action}>
            {({ getFieldValue }) => (
              <Form.Item
                label={t("admin_users.fields.reason")}
                name="reason"
                rules={getFieldValue("action") === "reject" ? [{ required: true, message: t("admin_users.validation.reason_required") }] : []}
                extra={getFieldValue("action") === "approve" ? t("admin_users.registration.reason_optional_for_approve") : undefined}
              >
                <Input.TextArea rows={4} placeholder={t("admin_users.registration.reject_reason_placeholder")} />
              </Form.Item>
            )}
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={t("admin_users.registration.setup_link_title")}
        open={linkItem !== null}
        destroyOnHidden
        onCancel={onCloseLink}
        footer={[
          <Button key="copy" type="primary" onClick={onCopyLink}>
            {t("common.actions.copy_link")}
          </Button>,
          <Button key="close" onClick={onCloseLink}>
            {t("common.actions.close")}
          </Button>,
        ]}
      >
        <Space direction="vertical" size={12} style={{ width: "100%" }}>
          <Text strong>{linkItem?.email}</Text>
          <Text type="secondary">{t("admin_users.view.division")}: {linkItem?.divisionName || "-"}</Text>
          <Text type="secondary">{t("admin_users.view.department")}: {linkItem?.departmentName || "-"}</Text>
          <Text type="secondary">{t("admin_users.view.job_title")}: {linkItem?.jobTitleName || "-"}</Text>
          <Text type="secondary">
            {t("admin_users.registration.password_setup_expires_at")}: {formatDate(linkItem?.passwordSetupExpiresAt ?? null, currentLanguage)}
          </Text>
          <Input.TextArea
            value={linkItem?.passwordSetupLink ? `${window.location.origin}${linkItem.passwordSetupLink}` : ""}
            autoSize={{ minRows: 3, maxRows: 5 }}
            readOnly
          />
        </Space>
      </Modal>
    </>
  );
}
