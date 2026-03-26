import { App, Button, Card, Checkbox, Form, Input, Space, Spin, Table, Typography } from "antd";
import { SaveOutlined, SafetyCertificateOutlined } from "@ant-design/icons";
import { useEffect, useMemo } from "react";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { permissions } from "../../../shared/authz/permissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useApplyPermissionMatrix, usePermissionMatrix } from "../hooks/usePermissionMatrix";

type FormValues = {
  reason: string;
  grants: Record<string, Record<string, boolean>>;
};

export function PermissionMatrixPage() {
  const { notification } = App.useApp();
  const permissionState = usePermissions();
  const canApply = permissionState.hasPermission(permissions.admin.permissionMatrixApply);
  const matrixQuery = usePermissionMatrix();
  const applyMutation = useApplyPermissionMatrix();
  const [form] = Form.useForm<FormValues>();

  const permissionsCatalog = matrixQuery.data?.permissions ?? [];
  const roles = matrixQuery.data?.roles ?? [];

  const columns = useMemo(
    () => [
      {
        title: "Role",
        dataIndex: "roleName",
        key: "roleName",
        width: 220,
        render: (_: string, row: (typeof roles)[number]) => (
          <Space direction="vertical" size={0}>
            <Typography.Text strong>{row.roleName}</Typography.Text>
            <Typography.Text type="secondary">{row.roleKeycloakName}</Typography.Text>
          </Space>
        ),
      },
      ...permissionsCatalog.map((permissionItem) => ({
        title: permissionItem.label,
        key: permissionItem.key,
        width: 180,
        render: (_: unknown, row: (typeof roles)[number]) => (
          <Form.Item name={["grants", row.roleId, permissionItem.key]} valuePropName="checked" noStyle>
            <Checkbox disabled={!canApply} />
          </Form.Item>
        ),
      })),
    ],
    [canApply, permissionsCatalog, roles],
  );

  if (matrixQuery.isLoading && !matrixQuery.data) {
    return <Spin size="large" />;
  }

  useEffect(() => {
    form.setFieldsValue({
      reason: "",
      grants: Object.fromEntries(
        roles.map((role) => [
          role.roleId,
          Object.fromEntries(
            permissionsCatalog.map((permissionItem) => [
              permissionItem.key,
              role.grantedPermissions.includes(permissionItem.key),
            ]),
          ),
        ]),
      ) as never,
    });
  }, [form, permissionsCatalog, roles]);

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div
            style={{
              width: 48,
              height: 48,
              borderRadius: 14,
              display: "grid",
              placeItems: "center",
              background: "linear-gradient(135deg, #0f766e, #0f172a)",
              color: "#fff",
            }}
          >
            <SafetyCertificateOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              Permission Matrix
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Phase 0 access control matrix. Applying changes records the reason and updates action-level authorization.
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Form<FormValues> form={form} layout="vertical">
          <Form.Item
            label="Apply reason"
            name="reason"
            rules={[{ required: true, message: "Reason is required." }]}
          >
            <Input.TextArea rows={3} disabled={!canApply} />
          </Form.Item>

          <Table
            rowKey="roleId"
            columns={columns}
            dataSource={roles}
            loading={matrixQuery.isFetching}
            pagination={false}
            scroll={{ x: "max-content" }}
          />

          <Space style={{ marginTop: 16, justifyContent: "space-between", width: "100%" }}>
            <Typography.Text type="secondary">
              Current state: {matrixQuery.data?.state ?? "draft"}
              {matrixQuery.data?.appliedAt ? ` • last applied ${new Date(matrixQuery.data.appliedAt).toLocaleString()}` : ""}
            </Typography.Text>
            <Button
              type="primary"
              icon={<SaveOutlined />}
              loading={applyMutation.isPending}
              disabled={!canApply}
              onClick={() => {
                void form.validateFields().then((values) => {
                  const rolesPayload = roles.map((role) => {
                    const permissionMap = values.grants?.[role.roleId] ?? {};
                    const permissionKeys = permissionsCatalog
                      .filter((permissionItem) => permissionMap[permissionItem.key])
                      .map((permissionItem) => permissionItem.key);
                    return { roleId: role.roleId, permissionKeys };
                  });

                  applyMutation.mutate(
                    {
                      reason: values.reason,
                      roles: rolesPayload,
                    },
                    {
                      onSuccess: (response) => {
                        notification.success({
                          message: `Permission matrix applied by ${response.appliedBy ?? "system"}`,
                        });
                      },
                      onError: (error) => {
                        const presentation = getApiErrorPresentation(error, "Failed to apply permission matrix");
                        notification.error({ message: presentation.title, description: presentation.description });
                      },
                    },
                  );
                });
              }}
            >
              Apply matrix
            </Button>
          </Space>
        </Form>
      </Card>
    </Space>
  );
}
