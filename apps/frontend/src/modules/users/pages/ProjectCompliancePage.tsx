import { useState } from "react";
import { Alert, Card, Empty, List, Progress, Select, Space, Tag, Typography } from "antd";
import { SafetyCertificateOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import { useProjectOptions } from "../hooks/useProjectOptions";

function getStatusColor(status: string) {
  switch (status.toLowerCase()) {
    case "passed":
      return "green";
    case "warning":
      return "gold";
    case "failed":
      return "red";
    default:
      return "default";
  }
}

export function ProjectCompliancePage() {
  const { t } = useTranslation();
  const [selectedProjectId, setSelectedProjectId] = useState<string>();

  const { projectComplianceQuery } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 1 },
    projectRoles: { page: 1, pageSize: 10 },
    projectAssignments: null,
    projectComplianceProjectId: selectedProjectId,
  });

  const projectOptionsState = useProjectOptions({ enabled: true });

  const compliance = projectComplianceQuery.data;
  const totalChecks = compliance?.checks.length ?? 0;
  const readinessPercent = totalChecks === 0 ? 0 : Math.round(((compliance?.passedChecks ?? 0) / totalChecks) * 100);

  const getCheckTitle = (code: string, fallback: string) => t(`project_compliance.checks.${code}.title`, { defaultValue: fallback });
  const getCheckDescription = (code: string, fallback: string) => t(`project_compliance.checks.${code}.description`, { defaultValue: fallback });
  const getCheckDetail = (code: string, status: string, fallback: string | null) =>
    t(`project_compliance.checks.${code}.detail.${status.toLowerCase()}`, { defaultValue: fallback ?? "" });
  const getStatusLabel = (status: string) => t(`project_compliance.status.${status.toLowerCase()}`, { defaultValue: status });
  const getSeverityLabel = (severity: string) => t(`project_compliance.severity.${severity.toLowerCase()}`, { defaultValue: severity });

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)", color: "#fff" }}>
            <SafetyCertificateOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("project_compliance.page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("project_compliance.page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Space direction="vertical" size={16} style={{ width: "100%" }}>
          <Select
            allowClear
            showSearch
            filterOption={false}
            placeholder={t("project_compliance.select_project_placeholder")}
            options={projectOptionsState.options}
            value={selectedProjectId}
            onSearch={projectOptionsState.onSearch}
            loading={projectOptionsState.loading}
            onChange={(value) => setSelectedProjectId(value)}
            dropdownRender={(menu) => (
              <>
                {menu}
                {projectOptionsState.hasMore ? (
                  <div style={{ padding: 8 }}>
                    <button
                      type="button"
                      onMouseDown={(event) => event.preventDefault()}
                      onClick={() => projectOptionsState.onLoadMore()}
                      style={{
                        width: "100%",
                        border: "none",
                        background: "transparent",
                        color: "#1677ff",
                        cursor: "pointer",
                        padding: 4,
                      }}
                    >
                      {t("projects.load_more_projects")}
                    </button>
                  </div>
                ) : null}
              </>
            )}
          />

          {!selectedProjectId ? (
            <Alert type="info" showIcon message={t("project_compliance.select_project_message")} />
          ) : !compliance ? (
            <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t("project_compliance.empty")} />
          ) : (
            <Space direction="vertical" size={16} style={{ width: "100%" }}>
              <Card size="small">
                <Space direction="vertical" size={8} style={{ width: "100%" }}>
                  <Typography.Title level={4} style={{ margin: 0 }}>
                    {compliance.projectName}
                  </Typography.Title>
                  <Space wrap size={8}>
                    <Tag>{compliance.projectType}</Tag>
                    <Tag color={compliance.status.toLowerCase() === "active" ? "green" : "default"}>{compliance.status}</Tag>
                    <Tag color="green">{t("project_compliance.summary.passed", { count: compliance.passedChecks })}</Tag>
                    <Tag color="gold">{t("project_compliance.summary.warning", { count: compliance.warningChecks })}</Tag>
                    <Tag color="red">{t("project_compliance.summary.failed", { count: compliance.failedChecks })}</Tag>
                  </Space>
                  <div>
                    <Typography.Text>{t("project_compliance.summary.readiness")}</Typography.Text>
                    <Progress percent={readinessPercent} status={compliance.failedChecks > 0 ? "exception" : compliance.warningChecks > 0 ? "active" : "success"} />
                  </div>
                </Space>
              </Card>

              <Card size="small" title={t("project_compliance.checks_title")}>
                <List
                  dataSource={compliance.checks}
                  renderItem={(item) => (
                    <List.Item>
                      <Space direction="vertical" size={4} style={{ width: "100%" }}>
                        <Space wrap size={8}>
                          <Typography.Text strong>{getCheckTitle(item.code, item.title)}</Typography.Text>
                          <Tag color={getStatusColor(item.status)}>{getStatusLabel(item.status)}</Tag>
                          <Tag>{getSeverityLabel(item.severity)}</Tag>
                        </Space>
                        <Typography.Text type="secondary">{getCheckDescription(item.code, item.description)}</Typography.Text>
                        {getCheckDetail(item.code, item.status, item.detail) ? <Typography.Text>{getCheckDetail(item.code, item.status, item.detail)}</Typography.Text> : null}
                      </Space>
                    </List.Item>
                  )}
                />
              </Card>
            </Space>
          )}
        </Space>
      </Card>
    </Space>
  );
}
