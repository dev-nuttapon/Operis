import { Button, DatePicker, Form, Input, Space, Typography } from "antd";
import dayjs from "dayjs";
import type { DocumentFormValues } from "../types/documentForm";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";

interface DocumentTestFormProps {
  onSubmit: (values: DocumentFormValues) => void;
}

export function DocumentTestForm({ onSubmit }: DocumentTestFormProps) {
  const [form] = Form.useForm();
  const language = useI18nLanguage();
  const tr = (key: string) => i18n.t(key, { lng: language });

  const handleFinish = (values: {
    title: string;
    documentCode: string;
    ownerEmail: string;
    effectiveDate: dayjs.Dayjs;
    description?: string;
  }) => {
    onSubmit({
      title: values.title,
      documentCode: values.documentCode,
      ownerEmail: values.ownerEmail,
      effectiveDate: values.effectiveDate.format("YYYY-MM-DD"),
      description: values.description,
    });
  };

  return (
    <Form
      form={form}
      layout="vertical"
      onFinish={handleFinish}
      initialValues={{
        title: "",
        documentCode: "",
        ownerEmail: "",
        description: "",
      }}
    >
      <Form.Item
        name="title"
        label={tr("documents.form.title_label")}
        rules={[{ required: true, message: tr("documents.form.title_required") }]}
      >
        <Input placeholder={tr("documents.form.title_placeholder")} />
      </Form.Item>

      <Form.Item
        name="documentCode"
        label={tr("documents.form.code_label")}
        rules={[{ required: true, message: tr("documents.form.code_required") }]}
      >
        <Input placeholder={tr("documents.form.code_placeholder")} />
      </Form.Item>

      <Form.Item
        name="ownerEmail"
        label={tr("documents.form.owner_email_label")}
        rules={[
          { required: true, message: tr("documents.form.owner_email_required") },
          { type: "email", message: tr("documents.form.owner_email_invalid") },
        ]}
      >
        <Input placeholder={tr("documents.form.owner_email_placeholder")} />
      </Form.Item>

      <Form.Item
        name="effectiveDate"
        label={tr("documents.form.effective_date_label")}
        rules={[{ required: true, message: tr("documents.form.effective_date_required") }]}
      >
        <DatePicker style={{ width: "100%" }} />
      </Form.Item>

      <Form.Item name="description" label={tr("documents.form.description_label")}>
        <Input.TextArea rows={4} placeholder={tr("documents.form.description_placeholder")} />
      </Form.Item>

      <Space>
        <Button type="primary" htmlType="submit">
          {tr("documents.form.submit")}
        </Button>
        <Button
          onClick={() => {
            form.resetFields();
          }}
        >
          {tr("documents.form.reset")}
        </Button>
      </Space>

      <Typography.Paragraph type="secondary" style={{ marginTop: 12, marginBottom: 0 }}>
        {tr("documents.form.note")}
      </Typography.Paragraph>
    </Form>
  );
}
