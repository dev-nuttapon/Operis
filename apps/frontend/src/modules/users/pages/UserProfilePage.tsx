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
  const { user: authUser } = useAuth();
  const currentUserQuery = useCurrentUserProfile({ includeIdentity: true });

  const profile = currentUserQuery.data;
  const fallbackText = tr("common.position_empty");
  
  // Use data from API profile if available, otherwise from auth user
  const firstName = profile?.keycloak?.firstName ?? (authUser?.firstName as string | undefined) ?? fallbackText;
  const lastName = profile?.keycloak?.lastName ?? (authUser?.lastName as string | undefined) ?? fallbackText;
  
  const fullName = [firstName, lastName]
    .filter(val => val && val !== fallbackText)
    .join(" ")
    .trim();
    
  const displayName = fullName || profile?.keycloak?.username || authUser?.name || authUser?.email || tr("common.user_fallback");
  const divisionName = (profile?.divisionName as string | undefined) ?? (authUser?.divisionName as string | undefined) ?? fallbackText;
  const departmentName = (profile?.departmentName as string | undefined) ?? (authUser?.departmentName as string | undefined) ?? fallbackText;

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
        bordered
        column={{ xxl: 2, xl: 2, lg: 2, md: 1, sm: 1, xs: 1 }}
        style={{ marginTop: 24 }}
      >
        <Descriptions.Item label={tr("common.email")}>{profile?.email ?? authUser?.email ?? "-"}</Descriptions.Item>
        <Descriptions.Item label={tr("common.roles")}>
          {(profile?.roles ?? authUser?.roles ?? []).join(", ")}
        </Descriptions.Item>
        <Descriptions.Item label={tr("common.division")}>{divisionName}</Descriptions.Item>
        <Descriptions.Item label={tr("common.department")}>{departmentName}</Descriptions.Item>
        <Descriptions.Item label={tr("common.job_title")}>
          {profile?.jobTitleName ?? (authUser?.jobTitleName as string | undefined) ?? fallbackText}
        </Descriptions.Item>
        <Descriptions.Item label={tr("common.status")}>{profile?.status ?? "Active"}</Descriptions.Item>
      </Descriptions>
    </Card>
  );
}
