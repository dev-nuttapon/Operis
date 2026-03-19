import { App, Alert, Button, Card, Form, Space, Typography, Flex, Grid, Skeleton } from "antd";
import { ArrowLeftOutlined, EditOutlined, SaveOutlined } from "@ant-design/icons";
import { useEffect, useMemo, useRef } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import dayjs from "dayjs";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectAssignmentDetail } from "../hooks/useProjectAssignmentDetail";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import { useProjectRoleOptions } from "../hooks/useProjectRoleOptions";
import { useProjectUserOptions } from "../hooks/useProjectUserOptions";
import type { User } from "../types/users";
import { ProjectMemberForm, type ProjectMemberFormValues } from "../components/projectMembers/ProjectMemberForm";

type LocationState = {
  from?: string;
};

function toUserLabel(user: User) {
  return user.keycloak?.email ?? user.keycloak?.username ?? user.id;
}

export function ProjectMemberEditPage() {
  const { t } = useTranslation();
  const { notification } = App.useApp();
  const { assignmentId } = useParams<{ assignmentId: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const locationState = location.state as LocationState | null;
  const backTarget = locationState?.from ?? "/app/admin/project-members";

  const permissionState = usePermissions();
  const canManageProjectMembers = permissionState.hasPermission(permissions.projects.manageMembers);

  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;

  const [form] = Form.useForm<ProjectMemberFormValues>();

  const assignmentQuery = useProjectAssignmentDetail(assignmentId);
  const assignment = assignmentQuery.data;

  const { updateProjectAssignmentMutation } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 1 },
    projectRoles: { page: 1, pageSize: 1 },
    projectAssignments: null,
  });

  const projectRoleOptionsState = useProjectRoleOptions({ enabled: canManageProjectMembers, projectId: assignment?.projectId });
  const userOptionsState = useProjectUserOptions(canManageProjectMembers, toUserLabel);

  const projectRoleOptions = useMemo(() => {
    const base = [...projectRoleOptionsState.options];
    const seen = new Set(base.map((item) => item.value));
    if (assignment && !seen.has(assignment.projectRoleId)) {
      base.push({ value: assignment.projectRoleId, label: assignment.projectRoleName ?? assignment.projectRoleId });
    }
    return base;
  }, [assignment, projectRoleOptionsState.options]);

  const userOptions = useMemo(() => {
    const base = [...userOptionsState.options];
    const seen = new Set(base.map((item) => item.value));
    const ensureOption = (value?: string, label?: string) => {
      if (!value || seen.has(value)) return;
      base.push({ value, label: label ?? value });
      seen.add(value);
    };
    if (assignment) {
      ensureOption(assignment.userId, assignment.userDisplayName ?? assignment.userEmail ?? assignment.userId);
      ensureOption(assignment.reportsToUserId ?? undefined, assignment.reportsToDisplayName ?? assignment.reportsToUserId ?? undefined);
    }
    return base;
  }, [assignment, userOptionsState.options]);

  const reportingOptions = userOptions;
  const projectOptions = useMemo(
    () =>
      assignment
        ? [{ value: assignment.projectId, label: assignment.projectName }]
        : [],
    [assignment],
  );

  const initializedRef = useRef(false);
  useEffect(() => {
    if (initializedRef.current) return;
    if (!assignment) return;

    form.setFieldsValue({
      projectId: assignment.projectId,
      userId: assignment.userId,
      projectRoleId: assignment.projectRoleId,
      reportsToUserId: assignment.reportsToUserId ?? undefined,
      isPrimary: assignment.isPrimary,
      period: [assignment.startAt ? dayjs(assignment.startAt) : null, assignment.endAt ? dayjs(assignment.endAt) : null],
    });
    initializedRef.current = true;
  }, [assignment, form]);

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      if (!assignment) return;

      updateProjectAssignmentMutation.mutate(
        {
          id: assignment.id,
          userId: values.userId,
          projectId: assignment.projectId,
          projectRoleId: values.projectRoleId,
          reportsToUserId: values.reportsToUserId,
          isPrimary: Boolean(values.isPrimary),
          startAt: values.period?.[0]?.startOf("day").toISOString(),
          endAt: values.period?.[1]?.endOf("day").toISOString(),
          reason: values.reason ?? "",
        },
        {
          onSuccess: () => {
            notification.success({ message: t("project_members.messages.updated") });
            navigate(backTarget);
          },
          onError: (error) => {
            const presentation = getApiErrorPresentation(error, t("project_members.messages.update_failed"));
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
          {t("project_members.edit_page_back")}
        </Button>
      </Space>

      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)", color: "#fff" }}>
            <EditOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("project_members.edit_page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("project_members.edit_page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canManageProjectMembers ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
        ) : assignmentQuery.isLoading && !assignment ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : !assignment ? (
          <Alert type="error" showIcon message={t("errors.title_not_found")} />
        ) : (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <ProjectMemberForm
              form={form}
              t={t}
              showProjectField
              disableProjectField
              projectOptions={projectOptions}
              userOptions={userOptions}
              projectRoleOptions={projectRoleOptions}
              reportingOptions={reportingOptions}
              includeReason
              userOptionsLoading={userOptionsState.loading}
              onUserSearch={userOptionsState.onSearch}
              onUserLoadMore={userOptionsState.onLoadMore}
              userHasMore={userOptionsState.hasMore}
              roleOptionsLoading={projectRoleOptionsState.loading}
              onRoleSearch={projectRoleOptionsState.onSearch}
              onRoleLoadMore={projectRoleOptionsState.onLoadMore}
              roleHasMore={projectRoleOptionsState.hasMore}
            />

            <Flex justify="flex-start">
              <Button type="primary" icon={<SaveOutlined />} onClick={handleSubmit} loading={updateProjectAssignmentMutation.isPending} block={isMobile}>
                {t("project_members.edit_page_submit")}
              </Button>
            </Flex>
          </Space>
        )}
      </Card>
    </Space>
  );
}
