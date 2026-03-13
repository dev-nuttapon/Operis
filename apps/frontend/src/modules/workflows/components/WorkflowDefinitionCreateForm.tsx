import { Button, Form, Input } from "antd";
import { useTranslation } from "react-i18next";
import type { CreateWorkflowDefinitionInput } from "../types/workflows";

interface WorkflowDefinitionCreateFormProps {
  canManage: boolean;
  isSubmitting: boolean;
  onSubmit: (values: CreateWorkflowDefinitionInput) => void;
}

export function WorkflowDefinitionCreateForm({ canManage, isSubmitting, onSubmit }: WorkflowDefinitionCreateFormProps) {
  const { t } = useTranslation();
  const [form] = Form.useForm<CreateWorkflowDefinitionInput>();

  return (
    <Form<CreateWorkflowDefinitionInput>
      form={form}
      layout="vertical"
      onFinish={(values) => {
        onSubmit(values);
        form.resetFields();
      }}
    >
      <Form.Item
        label={t("workflow_definitions.form.name")}
        name="name"
        rules={[
          { required: true, message: t("workflow_definitions.validation.name_required") },
          { max: 200, message: t("workflow_definitions.validation.name_max_length") },
        ]}
      >
        <Input placeholder={t("workflow_definitions.placeholders.name")} />
      </Form.Item>
      <Button htmlType="submit" type="primary" loading={isSubmitting} disabled={!canManage}>
        {t("workflow_definitions.actions.create_draft")}
      </Button>
    </Form>
  );
}
