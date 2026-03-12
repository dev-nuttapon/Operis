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
  const tr = (key: string, fallback: string) => i18n.t(key, { lng: language, defaultValue: fallback });

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
        label={tr("documents.form.title_label", "Document Title")}
        rules={[{ required: true, message: tr("documents.form.title_required", "Please input document title") }]}
      >
        <Input placeholder={tr("documents.form.title_placeholder", "e.g. Monthly Inventory Report")} />
      </Form.Item>

      <Form.Item
        name="documentCode"
        label={tr("documents.form.code_label", "Document Code")}
        rules={[{ required: true, message: tr("documents.form.code_required", "Please input document code") }]}
      >
        <Input placeholder={tr("documents.form.code_placeholder", "e.g. DOC-2026-001")} />
      </Form.Item>

      <Form.Item
        name="ownerEmail"
        label={tr("documents.form.owner_email_label", "Owner Email")}
        rules={[
          { required: true, message: tr("documents.form.owner_email_required", "Please input owner email") },
          { type: "email", message: tr("documents.form.owner_email_invalid", "Please input valid email") },
        ]}
      >
        <Input placeholder={tr("documents.form.owner_email_placeholder", "owner@example.com")} />
      </Form.Item>

      <Form.Item
        name="effectiveDate"
        label={tr("documents.form.effective_date_label", "Effective Date")}
        rules={[{ required: true, message: tr("documents.form.effective_date_required", "Please select effective date") }]}
      >
        <DatePicker style={{ width: "100%" }} />
      </Form.Item>

      <Form.Item name="description" label={tr("documents.form.description_label", "Description")}>
        <Input.TextArea rows={4} placeholder={tr("documents.form.description_placeholder", "Short description for test form")} />
      </Form.Item>

      <Space>
        <Button type="primary" htmlType="submit">
          {tr("documents.form.submit", "Submit Test Form")}
        </Button>
        <Button
          onClick={() => {
            form.resetFields();
          }}
        >
          {tr("documents.form.reset", "Reset")}
        </Button>
      </Space>

      <Typography.Paragraph type="secondary" style={{ marginTop: 12, marginBottom: 0 }}>
        {tr("documents.form.note", "This is a local test form in documents module (no API call yet).")}
      </Typography.Paragraph>
    </Form>
  );
}
