import { Typography, Card, Button, Space } from "antd";
import { LogoutOutlined } from "@ant-design/icons";
import { useAuth } from "../../../modules/auth/hooks/useAuth";

const { Title, Paragraph } = Typography;

export function DocumentDashboardPage() {
  const { logout, isAuthenticated } = useAuth();

  return (
    <Card bordered={false} style={{ borderRadius: 16 }}>
      <Space style={{ width: "100%", justifyContent: "space-between", marginBottom: 16 }}>
        <Title level={2} style={{ margin: 0 }}>Document Dashboard</Title>
        {isAuthenticated ? (
          <Button danger icon={<LogoutOutlined />} onClick={() => void logout()}>
            Logout
          </Button>
        ) : null}
      </Space>
      <Paragraph>
        Welcome to the Paperless Document System. Modifying and creating modules here.
      </Paragraph>
    </Card>
  );
}
