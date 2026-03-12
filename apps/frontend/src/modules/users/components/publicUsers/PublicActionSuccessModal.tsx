import { Button, Flex, Modal, Typography } from "antd";
import { CheckCircleFilled } from "@ant-design/icons";

interface PublicActionSuccessModalProps {
  actionLabel: string;
  description: string;
  onConfirm: () => void;
  open: boolean;
  successColor: string;
  title: string;
}

const { Paragraph, Title } = Typography;

export function PublicActionSuccessModal({
  actionLabel,
  description,
  onConfirm,
  open,
  successColor,
  title,
}: PublicActionSuccessModalProps) {
  return (
    <Modal
      title={title}
      open={open}
      closable={false}
      maskClosable={false}
      keyboard={false}
      footer={[
        <Flex key="actions" justify="center">
          <Button type="primary" onClick={onConfirm}>
            {actionLabel}
          </Button>
        </Flex>,
      ]}
    >
      <Flex vertical align="center" gap={12} style={{ textAlign: "center", padding: "8px 0" }}>
        <CheckCircleFilled style={{ fontSize: 56, color: successColor }} />
        <Title level={4} style={{ margin: 0 }}>
          {title}
        </Title>
        <Paragraph style={{ marginBottom: 0, maxWidth: 360, whiteSpace: "pre-line" }}>
          {description}
        </Paragraph>
      </Flex>
    </Modal>
  );
}
