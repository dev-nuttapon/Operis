import { Form, Input, InputNumber, Modal, Typography } from "antd";

interface EntityItem {
  id: string;
  name: string;
}

interface AdminMasterDataModalsProps {
  createDepartmentForm: any;
  createDepartmentLoading: boolean;
  createJobTitleForm: any;
  createJobTitleLoading: boolean;
  creatingDepartment: boolean;
  creatingJobTitle: boolean;
  deleteDepartmentForm: any;
  deleteDepartmentLoading: boolean;
  deleteJobTitleForm: any;
  deleteJobTitleLoading: boolean;
  deletingDepartment: EntityItem | null;
  deletingJobTitle: EntityItem | null;
  editDepartmentForm: any;
  editDepartmentLoading: boolean;
  editJobTitleForm: any;
  editJobTitleLoading: boolean;
  editingDepartment: EntityItem | null;
  editingJobTitle: EntityItem | null;
  onCloseCreateDepartment: () => void;
  onCloseCreateJobTitle: () => void;
  onCloseDeleteDepartment: () => void;
  onCloseDeleteJobTitle: () => void;
  onCloseEditDepartment: () => void;
  onCloseEditJobTitle: () => void;
  onCreateDepartment: () => void;
  onCreateJobTitle: () => void;
  onDeleteDepartment: () => void;
  onDeleteJobTitle: () => void;
  onEditDepartment: () => void;
  onEditJobTitle: () => void;
  t: (key: string, options?: Record<string, unknown>) => string;
}

const { Paragraph } = Typography;

export function AdminMasterDataModals({
  createDepartmentForm,
  createDepartmentLoading,
  createJobTitleForm,
  createJobTitleLoading,
  creatingDepartment,
  creatingJobTitle,
  deleteDepartmentForm,
  deleteDepartmentLoading,
  deleteJobTitleForm,
  deleteJobTitleLoading,
  deletingDepartment,
  deletingJobTitle,
  editDepartmentForm,
  editDepartmentLoading,
  editJobTitleForm,
  editJobTitleLoading,
  editingDepartment,
  editingJobTitle,
  onCloseCreateDepartment,
  onCloseCreateJobTitle,
  onCloseDeleteDepartment,
  onCloseDeleteJobTitle,
  onCloseEditDepartment,
  onCloseEditJobTitle,
  onCreateDepartment,
  onCreateJobTitle,
  onDeleteDepartment,
  onDeleteJobTitle,
  onEditDepartment,
  onEditJobTitle,
  t,
}: AdminMasterDataModalsProps) {
  return (
    <>
      <Modal
        title={t("admin_users.master.create_department_modal_title")}
        open={creatingDepartment}
        okText={t("common.actions.create")}
        cancelText={t("common.actions.cancel")}
        confirmLoading={createDepartmentLoading}
        onCancel={onCloseCreateDepartment}
        onOk={onCreateDepartment}
      >
        <Form form={createDepartmentForm} layout="vertical">
          <Form.Item name="name" label={t("admin_users.master.department_name")} rules={[{ required: true, message: t("errors.department_required") }]}>
            <Input placeholder={t("admin_users.placeholders.department_name")} />
          </Form.Item>
          <Form.Item name="displayOrder" label={t("admin_users.master.display_order")} rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={t("admin_users.master.edit_department_modal_title")}
        open={editingDepartment !== null}
        okText={t("common.actions.save")}
        cancelText={t("common.actions.cancel")}
        confirmLoading={editDepartmentLoading}
        onCancel={onCloseEditDepartment}
        onOk={onEditDepartment}
      >
        <Form form={editDepartmentForm} layout="vertical">
          <Form.Item name="editName" label={t("admin_users.master.department_name")} rules={[{ required: true, message: t("errors.department_required") }]}>
            <Input />
          </Form.Item>
          <Form.Item name="editDisplayOrder" label={t("admin_users.master.display_order")} rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={deletingDepartment ? t("admin_users.master.delete_department_title_with_name", { name: deletingDepartment.name }) : t("admin_users.master.delete_department_title")}
        open={deletingDepartment !== null}
        okText={t("common.actions.delete")}
        cancelText={t("common.actions.cancel")}
        okButtonProps={{ danger: true }}
        confirmLoading={deleteDepartmentLoading}
        onCancel={onCloseDeleteDepartment}
        onOk={onDeleteDepartment}
      >
        <Form form={deleteDepartmentForm} layout="vertical">
          <Paragraph type="secondary">{t("admin_users.master.delete_soft_delete_description")}</Paragraph>
          <Form.Item name="reason" label={t("admin_users.fields.reason")} rules={[{ required: true, message: t("admin_users.validation.reason_required") }]}>
            <Input.TextArea rows={4} placeholder={t("admin_users.placeholders.department_delete_reason")} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={t("admin_users.master.create_job_title_modal_title")}
        open={creatingJobTitle}
        okText={t("common.actions.create")}
        cancelText={t("common.actions.cancel")}
        confirmLoading={createJobTitleLoading}
        onCancel={onCloseCreateJobTitle}
        onOk={onCreateJobTitle}
      >
        <Form form={createJobTitleForm} layout="vertical">
          <Form.Item name="name" label={t("admin_users.master.job_title_name")} rules={[{ required: true, message: t("errors.job_title_required") }]}>
            <Input placeholder={t("admin_users.placeholders.job_title_name")} />
          </Form.Item>
          <Form.Item name="displayOrder" label={t("admin_users.master.display_order")} rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={t("admin_users.master.edit_job_title_modal_title")}
        open={editingJobTitle !== null}
        okText={t("common.actions.save")}
        cancelText={t("common.actions.cancel")}
        confirmLoading={editJobTitleLoading}
        onCancel={onCloseEditJobTitle}
        onOk={onEditJobTitle}
      >
        <Form form={editJobTitleForm} layout="vertical">
          <Form.Item name="editName" label={t("admin_users.master.job_title_name")} rules={[{ required: true, message: t("errors.job_title_required") }]}>
            <Input />
          </Form.Item>
          <Form.Item name="editDisplayOrder" label={t("admin_users.master.display_order")} rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={deletingJobTitle ? t("admin_users.master.delete_job_title_title_with_name", { name: deletingJobTitle.name }) : t("admin_users.master.delete_job_title_title")}
        open={deletingJobTitle !== null}
        okText={t("common.actions.delete")}
        cancelText={t("common.actions.cancel")}
        okButtonProps={{ danger: true }}
        confirmLoading={deleteJobTitleLoading}
        onCancel={onCloseDeleteJobTitle}
        onOk={onDeleteJobTitle}
      >
        <Form form={deleteJobTitleForm} layout="vertical">
          <Paragraph type="secondary">{t("admin_users.master.delete_soft_delete_description")}</Paragraph>
          <Form.Item name="reason" label={t("admin_users.fields.reason")} rules={[{ required: true, message: t("admin_users.validation.reason_required") }]}>
            <Input.TextArea rows={4} placeholder={t("admin_users.placeholders.job_title_delete_reason")} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
