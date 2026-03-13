import { Button, Form, Input } from "antd";

interface WorkflowDefinitionEditFormProps {
  initialName: string;
  isSubmitting: boolean;
  onCancel: () => void;
  onSubmit: (name: string) => void;
}

export function WorkflowDefinitionEditForm({
  initialName,
  isSubmitting,
  onCancel,
  onSubmit,
}: WorkflowDefinitionEditFormProps) {
  return (
    <Form
      layout="inline"
      initialValues={{ name: initialName }}
      onFinish={(values: { name: string }) => onSubmit(values.name)}
    >
      <Form.Item
        name="name"
        rules={[
          { required: true, message: "Definition name is required." },
          { max: 200, message: "Definition name must be 200 characters or fewer." },
        ]}
      >
        <Input placeholder="Definition name" />
      </Form.Item>
      <Button htmlType="submit" type="primary" loading={isSubmitting}>
        Save
      </Button>
      <Button onClick={onCancel} disabled={isSubmitting}>
        Cancel
      </Button>
    </Form>
  );
}
