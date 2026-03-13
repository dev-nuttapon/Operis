import { Form, Modal, Select } from "antd";
import { useMemo, type ReactNode } from "react";
import type { User } from "../../types/users";

interface OptionItem {
  label: ReactNode;
  value: string;
  divisionId?: string | null;
  departmentId?: string | null;
}

interface AdminUserAffiliationModalProps {
  departmentOptions: OptionItem[];
  divisionOptions: OptionItem[];
  form: any;
  jobTitleOptions: OptionItem[];
  loading: boolean;
  onClose: () => void;
  onSubmit: () => void;
  openUser: User | null;
  t: (key: string, options?: Record<string, unknown>) => string;
}

export function AdminUserAffiliationModal({
  departmentOptions,
  divisionOptions,
  form,
  jobTitleOptions,
  loading,
  onClose,
  onSubmit,
  openUser,
  t,
}: AdminUserAffiliationModalProps) {
  const divisionId = Form.useWatch("divisionId", form) as string | undefined;
  const departmentId = Form.useWatch("departmentId", form) as string | undefined;
  const filteredDepartments = useMemo(
    () => departmentOptions.filter((item) => !divisionId || item.divisionId === divisionId),
    [departmentOptions, divisionId]
  );
  const filteredPositions = useMemo(
    () => jobTitleOptions.filter((item) => !departmentId || item.departmentId === departmentId),
    [departmentId, jobTitleOptions]
  );

  return (
    <Modal
      title={openUser ? t("admin_users.affiliations.modal_title", { email: openUser.keycloak?.email || openUser.id }) : t("admin_users.affiliations.modal_title_fallback")}
      open={openUser !== null}
      onCancel={onClose}
      onOk={onSubmit}
      confirmLoading={loading}
      okText={t("common.actions.save")}
      cancelText={t("common.actions.cancel")}
      destroyOnHidden
    >
      <Form form={form} layout="vertical">
        <Form.Item label={t("admin_users.fields.division")} name="divisionId">
          <Select
            allowClear
            placeholder={t("admin_users.placeholders.select_division")}
            options={divisionOptions}
            onChange={() => {
              form.setFieldValue("departmentId", undefined);
              form.setFieldValue("positionId", undefined);
            }}
          />
        </Form.Item>
        <Form.Item label={t("admin_users.fields.department")} name="departmentId">
          <Select
            allowClear
            placeholder={t("admin_users.placeholders.select_department")}
            options={filteredDepartments}
            onChange={() => {
              form.setFieldValue("positionId", undefined);
            }}
          />
        </Form.Item>
        <Form.Item label={t("admin_users.fields.position")} name="positionId">
          <Select allowClear placeholder={t("admin_users.placeholders.select_position")} options={filteredPositions} />
        </Form.Item>
      </Form>
    </Modal>
  );
}
