import { Button, Form, Input } from "antd";
import type { CreateWorkflowDefinitionInput } from "../types/workflows";

interface WorkflowDefinitionCreateFormProps {
  isSubmitting: boolean;
  onSubmit: (values: CreateWorkflowDefinitionInput) => void;
}

export function WorkflowDefinitionCreateForm({ isSubmitting, onSubmit }: WorkflowDefinitionCreateFormProps) {
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
        label="Definition name"
        name="name"
        rules={[
          { required: true, message: "Definition name is required." },
          { max: 200, message: "Definition name must be 200 characters or fewer." },
        ]}
      >
        <Input placeholder="Enter workflow definition name" />
      </Form.Item>
      <Button htmlType="submit" type="primary" loading={isSubmitting}>
        Create draft
      </Button>
    </Form>
  );
}
