import { Button, Card, Descriptions, Flex, Typography } from "antd";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../auth";
import { useCurrentUserProfile } from "../hooks/useCurrentUserProfile";
import i18n from "../../../shared/i18n/config";
import { useI18nLanguage } from "../../../shared/i18n/hooks/useI18nLanguage";

const { Title, Text } = Typography;

export function UserProfilePage() {
  const language = useI18nLanguage();
  const tr = (key: string) => i18n.t(key, { lng: language });
  const navigate = useNavigate();
  const { user } = useAuth();
  const currentUserQuery = useCurrentUserProfile({ includeIdentity: true });

  const profile = currentUserQuery.data ?? user;
  const fallbackText = tr("common.position_empty");
  const firstName = profile?.keycloak?.firstName ?? fallbackText;
  const lastName = profile?.keycloak?.lastName ?? fallbackText;
  const fullName = [profile?.keycloak?.firstName, profile?.keycloak?.lastName]
    .filter(Boolean)
    .join(" ")
    .trim();
  const displayName = fullName || profile?.name || profile?.email || tr("common.user_fallback");
  const divisionName = profile?.divisionName ?? fallbackText;
  const departmentName = profile?.departmentName ?? fallbackText;

  return (
    <Card variant="borderless" style={{ borderRadius: 16 }}>
      <Flex align="center" justify="space-between" wrap="wrap" gap={12}>
        <div>
          <Title level={2} style={{ marginTop: 0 }}>
            {tr("common.profile")}
          </Title>
          <Text type="secondary">{displayName}</Text>
        </div>
        <Button
          type="default"
          onClick={() => navigate("/app/change-password")}
        >
          {tr("common.change_password")}
        </Button>
      </Flex>

      <Descriptions
        style={{ marginTop: 16 }}
        column={1}
        size="small"
        labelStyle={{ width: 160 }}
      >
        <Descriptions.Item label={tr("admin_users.fields.first_name")}>
          {firstName}
        </Descriptions.Item>
        <Descriptions.Item label={tr("admin_users.fields.last_name")}>
          {lastName}
        </Descriptions.Item>
        <Descriptions.Item label={tr("admin_users.fields.division")}>
          {divisionName}
        </Descriptions.Item>
        <Descriptions.Item label={tr("admin_users.fields.department")}>
          {departmentName}
        </Descriptions.Item>
      </Descriptions>
    </Card>
  );
}
