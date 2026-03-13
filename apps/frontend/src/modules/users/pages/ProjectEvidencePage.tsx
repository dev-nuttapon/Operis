import { useState } from "react";
import { App, Alert, Button, Card, Empty, Select, Space, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { AuditOutlined, DownloadOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import type { ProjectAssignmentHistoryRow, ProjectRoleResponsibilityRow, ProjectTeamRegisterRow } from "../types/users";
import { formatDate } from "../utils/adminUsersPresentation";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";

export function ProjectEvidencePage() {
  const { t, i18n } = useTranslation();
  const { notification } = App.useApp();
  const [selectedProjectId, setSelectedProjectId] = useState<string>();

  const { projectsQuery, projectEvidenceQuery, exportProjectEvidenceCsv } = useProjectAdmin({
    projects: { page: 1, pageSize: 100, sortBy: "name", sortOrder: "asc" },
    projectRoles: { page: 1, pageSize: 10 },
    projectAssignments: null,
    projectEvidenceProjectId: selectedProjectId,
  });

  const projectOptions = (projectsQuery.data?.items ?? []).map((item) => ({
    label: `${item.code} - ${item.name}`,
    value: item.id,
  }));

  const teamColumns: ColumnsType<ProjectTeamRegisterRow> = [
    {
      title: t("project_evidence.team_register.columns.member"),
      dataIndex: "userDisplayName",
      render: (_, record) => record.userDisplayName ?? record.userEmail ?? record.userId,
    },
    { title: t("project_evidence.team_register.columns.role"), dataIndex: "projectRoleName" },
    {
      title: t("project_evidence.team_register.columns.reports_to"),
      dataIndex: "reportsToDisplayName",
      render: (value: string | null) => value ?? "-",
    },
    {
      title: t("project_evidence.team_register.columns.primary"),
      dataIndex: "isPrimary",
      render: (value: boolean) => (value ? t("common.actions.yes") : t("common.actions.no")),
    },
    {
      title: t("project_evidence.team_register.columns.period"),
      key: "period",
      render: (_, record) => `${formatDate(record.startAt, i18n.language)} - ${formatDate(record.endAt, i18n.language)}`,
    },
  ];

  const roleColumns: ColumnsType<ProjectRoleResponsibilityRow> = [
    { title: t("project_evidence.role_responsibility.columns.role"), dataIndex: "projectRoleName" },
    { title: t("project_evidence.role_responsibility.columns.code"), dataIndex: "code", render: (value: string | null) => value ?? "-" },
    {
      title: t("project_evidence.role_responsibility.columns.permissions"),
      key: "permissions",
      render: (_, record) => {
        const labels = [
          record.canCreateDocuments ? t("project_roles.permissions.create") : null,
          record.canReviewDocuments ? t("project_roles.permissions.review") : null,
          record.canApproveDocuments ? t("project_roles.permissions.approve") : null,
          record.canReleaseDocuments ? t("project_roles.permissions.release") : null,
        ].filter(Boolean) as string[];
        return labels.length > 0 ? labels.join(", ") : "-";
      },
    },
    { title: t("project_evidence.role_responsibility.columns.members"), dataIndex: "memberCount" },
  ];

  const historyColumns: ColumnsType<ProjectAssignmentHistoryRow> = [
    {
      title: t("project_evidence.assignment_history.columns.member"),
      dataIndex: "userDisplayName",
      render: (_, record) => record.userDisplayName ?? record.userEmail ?? record.userId,
    },
    { title: t("project_evidence.assignment_history.columns.role"), dataIndex: "projectRoleName" },
    {
      title: t("project_evidence.assignment_history.columns.status"),
      dataIndex: "status",
      render: (value: string) => <Tag color={value === "Active" ? "green" : value === "Removed" ? "red" : "gold"}>{value}</Tag>,
    },
    {
      title: t("project_evidence.assignment_history.columns.reason"),
      dataIndex: "changeReason",
      render: (value: string | null) => value ?? "-",
    },
    {
      title: t("project_evidence.assignment_history.columns.period"),
      key: "period",
      render: (_, record) => `${formatDate(record.startAt, i18n.language)} - ${formatDate(record.endAt, i18n.language)}`,
    },
  ];

  const evidence = projectEvidenceQuery.data;

  const handleExport = async () => {
    if (!selectedProjectId) {
      return;
    }

    try {
      const result = await exportProjectEvidenceCsv(selectedProjectId);
      const url = URL.createObjectURL(result.blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = result.fileName ?? "project-evidence.csv";
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
      notification.success({ message: t("project_evidence.export.success") });
    } catch (error) {
      const presentation = getApiErrorPresentation(error, t("project_evidence.export.failed"));
      notification.error({ message: presentation.title, description: presentation.description });
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)", color: "#fff" }}>
            <AuditOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("project_evidence.page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("project_evidence.page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Space direction="vertical" size={16} style={{ width: "100%" }}>
          <Select
            allowClear
            showSearch
            placeholder={t("project_evidence.select_project_placeholder")}
            options={projectOptions}
            value={selectedProjectId}
            onChange={(value) => setSelectedProjectId(value)}
          />

          {!selectedProjectId ? (
            <Alert type="info" showIcon message={t("project_evidence.select_project_message")} />
          ) : !evidence ? (
            <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t("project_evidence.empty")} />
          ) : (
            <Space direction="vertical" size={16} style={{ width: "100%" }}>
              <Typography.Title level={4} style={{ margin: 0 }}>
                {evidence.projectName}
              </Typography.Title>

              <Space style={{ width: "100%", justifyContent: "flex-end" }}>
                <Button icon={<DownloadOutlined />} onClick={handleExport}>
                  {t("project_evidence.export.action")}
                </Button>
              </Space>

              <Card size="small" title={t("project_evidence.team_register.title")}>
                <Table rowKey="assignmentId" columns={teamColumns} dataSource={evidence.teamRegister} pagination={false} />
              </Card>

              <Card size="small" title={t("project_evidence.role_responsibility.title")}>
                <Table rowKey="projectRoleId" columns={roleColumns} dataSource={evidence.roleResponsibilities} pagination={false} />
              </Card>

              <Card size="small" title={t("project_evidence.assignment_history.title")}>
                <Table rowKey="assignmentId" columns={historyColumns} dataSource={evidence.assignmentHistory} pagination={false} />
              </Card>
            </Space>
          )}
        </Space>
      </Card>
    </Space>
  );
}
