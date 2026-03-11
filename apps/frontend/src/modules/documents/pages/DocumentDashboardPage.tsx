import { Typography, Card } from "antd";

const { Title, Paragraph } = Typography;

export function DocumentDashboardPage() {
  return (
    <Card bordered={false} style={{ borderRadius: 16 }}>
      <Title level={2}>Document Dashboard</Title>
      <Paragraph>
        Welcome to the Paperless Document System. Modifying and creating modules here.
      </Paragraph>
    </Card>
  );
}
