import { Form, Input, InputNumber, Modal, Select, Typography } from "antd";
import { useMemo } from "react";

interface EntityItem {
  id: string;
  name: string;
}

interface OptionItem {
  label: string;
  value: string;
  divisionId?: string | null;
  departmentId?: string | null;
}

interface AdminMasterDataModalsProps {
  createDivisionForm: any;
  createDivisionLoading: boolean;
  createDepartmentForm: any;
  createDepartmentLoading: boolean;
  createJobTitleForm: any;
  createJobTitleLoading: boolean;
  creatingDivision: boolean;
  creatingDepartment: boolean;
  creatingJobTitle: boolean;
  deleteDivisionForm: any;
  deleteDivisionLoading: boolean;
  deleteDepartmentForm: any;
  deleteDepartmentLoading: boolean;
  deleteJobTitleForm: any;
  deleteJobTitleLoading: boolean;
  deletingDivision: EntityItem | null;
  deletingDepartment: EntityItem | null;
  deletingJobTitle: EntityItem | null;
  divisionOptions: OptionItem[];
  departmentOptions: OptionItem[];
  editDivisionForm: any;
  editDivisionLoading: boolean;
  editDepartmentForm: any;
  editDepartmentLoading: boolean;
  editJobTitleForm: any;
  editJobTitleLoading: boolean;
  editingDivision: EntityItem | null;
  editingDepartment: EntityItem | null;
  editingJobTitle: EntityItem | null;
  onCloseCreateDivision: () => void;
  onCloseCreateDepartment: () => void;
  onCloseCreateJobTitle: () => void;
  onCloseDeleteDivision: () => void;
  onCloseDeleteDepartment: () => void;
  onCloseDeleteJobTitle: () => void;
  onCloseEditDivision: () => void;
  onCloseEditDepartment: () => void;
  onCloseEditJobTitle: () => void;
  onCreateDivision: () => void;
  onCreateDepartment: () => void;
  onCreateJobTitle: () => void;
  onDeleteDivision: () => void;
  onDeleteDepartment: () => void;
  onDeleteJobTitle: () => void;
  onEditDivision: () => void;
  onEditDepartment: () => void;
  onEditJobTitle: () => void;
  t: (key: string, options?: Record<string, unknown>) => string;
}

const { Paragraph } = Typography;

export function AdminMasterDataModals({
  createDivisionForm,
  createDivisionLoading,
  createDepartmentForm,
  createDepartmentLoading,
  createJobTitleForm,
  createJobTitleLoading,
  creatingDivision,
  creatingDepartment,
  creatingJobTitle,
  deleteDivisionForm,
  deleteDivisionLoading,
  deleteDepartmentForm,
  deleteDepartmentLoading,
  deleteJobTitleForm,
  deleteJobTitleLoading,
  deletingDivision,
  deletingDepartment,
  deletingJobTitle,
  divisionOptions,
  departmentOptions,
  editDivisionForm,
  editDivisionLoading,
  editDepartmentForm,
  editDepartmentLoading,
  editJobTitleForm,
  editJobTitleLoading,
  editingDivision,
  editingDepartment,
  editingJobTitle,
  onCloseCreateDivision,
  onCloseCreateDepartment,
  onCloseCreateJobTitle,
  onCloseDeleteDivision,
  onCloseDeleteDepartment,
  onCloseDeleteJobTitle,
  onCloseEditDivision,
  onCloseEditDepartment,
  onCloseEditJobTitle,
  onCreateDivision,
  onCreateDepartment,
  onCreateJobTitle,
  onDeleteDivision,
  onDeleteDepartment,
  onDeleteJobTitle,
  onEditDivision,
  onEditDepartment,
  onEditJobTitle,
  t,
}: AdminMasterDataModalsProps) {
  const createJobTitleDivisionId = Form.useWatch("divisionId", createJobTitleForm) as string | undefined;
  const editJobTitleDivisionId = Form.useWatch("editDivisionId", editJobTitleForm) as string | undefined;
  const createJobTitleDepartmentOptions = useMemo(
    () => departmentOptions.filter((item) => !createJobTitleDivisionId || item.divisionId === createJobTitleDivisionId),
    [createJobTitleDivisionId, departmentOptions]
  );
  const editJobTitleDepartmentOptions = useMemo(
    () => departmentOptions.filter((item) => !editJobTitleDivisionId || item.divisionId === editJobTitleDivisionId),
    [departmentOptions, editJobTitleDivisionId]
  );

  return (
    <>
      <Modal
        title={t("admin_users.master.create_division_modal_title")}
        open={creatingDivision}
        okText={t("common.actions.create")}
        cancelText={t("common.actions.cancel")}
        confirmLoading={createDivisionLoading}
        onCancel={onCloseCreateDivision}
        onOk={onCreateDivision}
      >
        <Form form={createDivisionForm} layout="vertical">
          <Form.Item name="name" label={t("admin_users.master.division_name")} rules={[{ required: true, message: t("errors.division_required") }]}>
            <Input placeholder={t("admin_users.placeholders.division_name")} />
          </Form.Item>
          <Form.Item name="displayOrder" label={t("admin_users.master.display_order")} rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={t("admin_users.master.edit_division_modal_title")}
        open={editingDivision !== null}
        okText={t("common.actions.save")}
        cancelText={t("common.actions.cancel")}
        confirmLoading={editDivisionLoading}
        onCancel={onCloseEditDivision}
        onOk={onEditDivision}
      >
        <Form form={editDivisionForm} layout="vertical">
          <Form.Item name="editName" label={t("admin_users.master.division_name")} rules={[{ required: true, message: t("errors.division_required") }]}>
            <Input />
          </Form.Item>
          <Form.Item name="editDisplayOrder" label={t("admin_users.master.display_order")} rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={deletingDivision ? t("admin_users.master.delete_division_title_with_name", { name: deletingDivision.name }) : t("admin_users.master.delete_division_title")}
        open={deletingDivision !== null}
        okText={t("common.actions.delete")}
        cancelText={t("common.actions.cancel")}
        okButtonProps={{ danger: true }}
        confirmLoading={deleteDivisionLoading}
        onCancel={onCloseDeleteDivision}
        onOk={onDeleteDivision}
      >
        <Form form={deleteDivisionForm} layout="vertical">
          <Paragraph type="secondary">{t("admin_users.master.delete_soft_delete_description")}</Paragraph>
          <Form.Item name="reason" label={t("admin_users.fields.reason")} rules={[{ required: true, message: t("admin_users.validation.reason_required") }]}>
            <Input.TextArea rows={4} placeholder={t("admin_users.placeholders.division_delete_reason")} />
          </Form.Item>
        </Form>
      </Modal>

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
          <Form.Item name="divisionId" label={t("admin_users.fields.division")}>
            <Select allowClear placeholder={t("admin_users.placeholders.select_division")} options={divisionOptions} />
          </Form.Item>
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
          <Form.Item name="editDivisionId" label={t("admin_users.fields.division")}>
            <Select allowClear placeholder={t("admin_users.placeholders.select_division")} options={divisionOptions} />
          </Form.Item>
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
          <Form.Item name="divisionId" label={t("admin_users.fields.division")}>
            <Select
              allowClear
              placeholder={t("admin_users.placeholders.select_division")}
              options={divisionOptions}
              onChange={() => {
                createJobTitleForm.setFieldValue("departmentId", undefined);
              }}
            />
          </Form.Item>
          <Form.Item name="departmentId" label={t("admin_users.fields.department")} rules={[{ required: true, message: t("errors.department_required_for_division") }]}>
            <Select allowClear placeholder={t("admin_users.placeholders.select_department")} options={createJobTitleDepartmentOptions} />
          </Form.Item>
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
          <Form.Item name="editDivisionId" label={t("admin_users.fields.division")}>
            <Select
              allowClear
              placeholder={t("admin_users.placeholders.select_division")}
              options={divisionOptions}
              onChange={() => {
                editJobTitleForm.setFieldValue("editDepartmentId", undefined);
              }}
            />
          </Form.Item>
          <Form.Item name="editDepartmentId" label={t("admin_users.fields.department")} rules={[{ required: true, message: t("errors.department_required_for_division") }]}>
            <Select allowClear placeholder={t("admin_users.placeholders.select_department")} options={editJobTitleDepartmentOptions} />
          </Form.Item>
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
