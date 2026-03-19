import { App, Alert, Button, Card, Form, Input, Select, Space, Typography, Flex, Grid, Skeleton } from "antd";
import { ArrowLeftOutlined, EditOutlined, SaveOutlined } from "@ant-design/icons";
import { useEffect, useMemo, useRef } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useDivisionOptions } from "../hooks/useDivisionOptions";
import { useDepartmentOptions } from "../hooks/useDepartmentOptions";
import { useJobTitleOptions } from "../hooks/useJobTitleOptions";
import { useAdminRoles } from "../hooks/useAdminRoles";
import { useAdminUserDetail } from "../hooks/useAdminUserDetail";
import { useUpdateAdminUser } from "../hooks/useUpdateAdminUser";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";

type LocationState = {
  from?: string;
};

type FormValues = {
  email: string;
  firstName: string;
  lastName: string;
  divisionId?: string;
  departmentId?: string;
  jobTitleId?: string;
  roles?: string[];
};

export function AdminUserEditPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const { userId } = useParams<{ userId: string }>();

  const permissionState = usePermissions();
  const canUpdateUsers = permissionState.hasPermission(permissions.users.update);
  const canReadMasterData =
    permissionState.hasPermission(permissions.masterData.managePermanentOrg) || permissionState.hasPermission(permissions.users.read);

  const navigate = useNavigate();
  const location = useLocation();
  const locationState = location.state as LocationState | null;
  const backTarget = locationState?.from ?? "/app/admin/users";

  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;

  const [form] = Form.useForm<FormValues>();
  const divisionId = Form.useWatch("divisionId", form) as string | undefined;
  const departmentId = Form.useWatch("departmentId", form) as string | undefined;

  const userQuery = useAdminUserDetail(userId);
  const rolesState = useAdminRoles({ enabled: canUpdateUsers });
  const updateUserMutation = useUpdateAdminUser();

  const divisionOptionsState = useDivisionOptions({ enabled: canReadMasterData, pageSize: 5 });
  const departmentsState = useDepartmentOptions({ enabled: canReadMasterData, divisionId, pageSize: 5 });
  const jobTitlesState = useJobTitleOptions({ enabled: canReadMasterData, departmentId, pageSize: 5 });

  const roleOptions = useMemo(
    () =>
      rolesState.options.map((item) => ({
        value: item.value,
        label: (
          <Space direction="vertical" size={0}>
            <Typography.Text>{item.label}</Typography.Text>
            {item.description ? <Typography.Text type="secondary">{item.description}</Typography.Text> : null}
          </Space>
        ),
      })),
    [rolesState.options],
  );

  const initializedRef = useRef(false);
  useEffect(() => {
    if (initializedRef.current) return;
    if (!userQuery.data) return;
    if (rolesState.loading) return;

    const matchedRoleIds = rolesState.options
      .filter((item) => userQuery.data?.roles?.includes(item.label))
      .map((item) => item.value);

    form.setFieldsValue({
      email: userQuery.data.keycloak?.email ?? "",
      firstName: userQuery.data.keycloak?.firstName ?? "",
      lastName: userQuery.data.keycloak?.lastName ?? "",
      divisionId: userQuery.data.divisionId ?? undefined,
      departmentId: userQuery.data.departmentId ?? undefined,
      jobTitleId: userQuery.data.jobTitleId ?? undefined,
      roles: matchedRoleIds,
    });
    initializedRef.current = true;
  }, [form, rolesState.loading, rolesState.options, userQuery.data]);

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      if (!userId) return;

      updateUserMutation.mutate(
        {
          id: userId,
          email: values.email,
          firstName: values.firstName,
          lastName: values.lastName,
          divisionId: values.divisionId,
          departmentId: values.departmentId,
          jobTitleId: values.jobTitleId,
          roleIds: values.roles,
        },
        {
          onSuccess: () => {
            notification.success({ message: t("admin_users.messages.user_updated", { email: values.email }) });
            navigate(backTarget);
          },
          onError: (error) => {
            const presentation = getApiErrorPresentation(error, t("errors.update_user_failed"));
            notification.error({ message: presentation.title, description: presentation.description });
          },
        },
      );
    } catch {
      // validation errors
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Space style={{ width: "100%", justifyContent: "flex-start" }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(backTarget)} block={isMobile}>
          {t("admin_users.directory.edit_page_back")}
        </Button>
      </Space>

      <Card variant="borderless">
        <Space align="start" size={16}>
          <div
            style={{
              width: 48,
              height: 48,
              borderRadius: 14,
              display: "grid",
              placeItems: "center",
              background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)",
              color: "#fff",
            }}
          >
            <EditOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("admin_users.directory.edit_page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("admin_users.directory.edit_page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canUpdateUsers ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
        ) : userQuery.loading && !userQuery.data ? (
          <Skeleton active paragraph={{ rows: 8 }} />
        ) : !userQuery.data ? (
          <Alert type="error" showIcon message={t("errors.title_not_found")} />
        ) : (
          <>
            <Form<FormValues> form={form} layout="vertical" disabled={!canUpdateUsers}>
              <Space direction="vertical" size={20} style={{ width: "100%" }}>
                <Card size="small" variant="borderless" style={{ background: "rgba(14, 165, 233, 0.06)" }}>
                  <Typography.Title level={5} style={{ marginTop: 0 }}>
                    {t("admin_users.sections.account_information")}
                  </Typography.Title>
                  <Form.Item label={t("admin_users.fields.email")} name="email" rules={[{ required: true, type: "email" }]}>
                    <Input placeholder={t("admin_users.placeholders.user_email")} />
                  </Form.Item>
                  <Form.Item
                    label={t("admin_users.fields.first_name")}
                    name="firstName"
                    rules={[{ required: true, message: t("invitation_page.first_name_required") }]}
                  >
                    <Input placeholder={t("invitation_page.first_name_placeholder")} />
                  </Form.Item>
                  <Form.Item
                    label={t("admin_users.fields.last_name")}
                    name="lastName"
                    rules={[{ required: true, message: t("invitation_page.last_name_required") }]}
                  >
                    <Input placeholder={t("invitation_page.last_name_placeholder")} />
                  </Form.Item>
                </Card>

                <Card size="small" variant="borderless" style={{ background: "rgba(15, 23, 42, 0.03)" }}>
                  <Typography.Title level={5} style={{ marginTop: 0 }}>
                    {t("admin_users.sections.organization_structure")}
                  </Typography.Title>
                  <Form.Item label={t("admin_users.fields.division")} name="divisionId">
                    <Select
                      allowClear
                      showSearch
                      filterOption={false}
                      placeholder={t("admin_users.placeholders.select_division")}
                      options={divisionOptionsState.options}
                      loading={divisionOptionsState.loading}
                      onSearch={divisionOptionsState.onSearch}
                      dropdownRender={(menu) => (
                        <>
                          {menu}
                          {divisionOptionsState.hasMore ? (
                            <div style={{ padding: 8 }}>
                              <button
                                type="button"
                                onMouseDown={(event) => event.preventDefault()}
                                onClick={divisionOptionsState.onLoadMore}
                                style={{
                                  width: "100%",
                                  border: "none",
                                  background: "transparent",
                                  color: "#1677ff",
                                  cursor: "pointer",
                                  padding: 4,
                                }}
                              >
                                {t("admin_users.load_more_divisions")}
                              </button>
                            </div>
                          ) : null}
                        </>
                      )}
                      onChange={() => {
                        form.setFieldValue("departmentId", undefined);
                        form.setFieldValue("jobTitleId", undefined);
                      }}
                    />
                  </Form.Item>
                  <Form.Item label={t("admin_users.fields.department")} name="departmentId">
                    <Select
                      allowClear
                      showSearch
                      filterOption={false}
                      placeholder={t("admin_users.placeholders.select_department")}
                      disabled={!divisionId}
                      loading={departmentsState.loading}
                      options={departmentsState.options}
                      onSearch={departmentsState.onSearch}
                      dropdownRender={(menu) => (
                        <>
                          {menu}
                          {departmentsState.hasMore ? (
                            <div style={{ padding: 8 }}>
                              <button
                                type="button"
                                onMouseDown={(event) => event.preventDefault()}
                                onClick={departmentsState.onLoadMore}
                                style={{
                                  width: "100%",
                                  border: "none",
                                  background: "transparent",
                                  color: "#1677ff",
                                  cursor: "pointer",
                                  padding: 4,
                                }}
                              >
                                {t("admin_users.load_more_departments")}
                              </button>
                            </div>
                          ) : null}
                        </>
                      )}
                      onChange={() => form.setFieldValue("jobTitleId", undefined)}
                    />
                  </Form.Item>
                  <Form.Item label={t("admin_users.fields.job_title")} name="jobTitleId">
                    <Select
                      allowClear
                      showSearch
                      filterOption={false}
                      disabled={!departmentId}
                      placeholder={t("admin_users.placeholders.select_job_title")}
                      loading={jobTitlesState.loading}
                      options={jobTitlesState.options}
                      onSearch={jobTitlesState.onSearch}
                      dropdownRender={(menu) => (
                        <>
                          {menu}
                          {jobTitlesState.hasMore ? (
                            <div style={{ padding: 8 }}>
                              <button
                                type="button"
                                onMouseDown={(event) => event.preventDefault()}
                                onClick={jobTitlesState.onLoadMore}
                                style={{
                                  width: "100%",
                                  border: "none",
                                  background: "transparent",
                                  color: "#1677ff",
                                  cursor: "pointer",
                                  padding: 4,
                                }}
                              >
                                {t("admin_users.load_more_job_titles")}
                              </button>
                            </div>
                          ) : null}
                        </>
                      )}
                    />
                  </Form.Item>
                </Card>

                <Card size="small" variant="borderless" style={{ background: "rgba(22, 163, 74, 0.05)" }}>
                  <Typography.Title level={5} style={{ marginTop: 0 }}>
                    {t("admin_users.sections.access_rights")}
                  </Typography.Title>
                  <Form.Item label={t("admin_users.fields.roles")} name="roles" extra={t("admin_users.fields.roles_help")}>
                    <Select
                      mode="multiple"
                      allowClear
                      placeholder={t("admin_users.placeholders.select_roles")}
                      loading={rolesState.loading}
                      options={roleOptions}
                    />
                  </Form.Item>
                </Card>
              </Space>
            </Form>

            <Flex
              gap={12}
              wrap={!isMobile}
              vertical={isMobile}
              align={isMobile ? "stretch" : "center"}
              style={{ width: "100%", marginTop: 16 }}
            >
              <Button
                type="primary"
                icon={<SaveOutlined />}
                loading={updateUserMutation.isPending}
                onClick={() => void handleSubmit()}
                block={isMobile}
              >
                {t("common.actions.save")}
              </Button>
              <Button onClick={() => navigate(backTarget)} block={isMobile}>
                {t("common.actions.cancel")}
              </Button>
            </Flex>
          </>
        )}
      </Card>
    </Space>
  );
}

