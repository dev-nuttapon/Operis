import { useMemo, useState } from "react";
import { Alert, Card, Empty, Select, Space, Spin, Tag, Tree, Typography } from "antd";
import type { DataNode } from "antd/es/tree";
import { ApartmentOutlined, TeamOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import { useProjectOptions } from "../hooks/useProjectOptions";
import type { ProjectOrgChartNode } from "../types/users";
import { formatDate } from "../utils/adminUsersPresentation";

function toTreeNode(node: ProjectOrgChartNode, language: string, t: ReturnType<typeof useTranslation>["t"]): DataNode {
  const title = (
    <Space direction="vertical" size={2}>
      <Space size={8} wrap>
        <Typography.Text strong>{node.userDisplayName ?? node.userEmail ?? node.userId}</Typography.Text>
        <Tag color={node.status === "Active" ? "green" : "gold"}>{node.status}</Tag>
        {node.isPrimary ? <Tag color="blue">{t("project_org_chart.primary_member")}</Tag> : null}
      </Space>
      <Typography.Text type="secondary">
        {node.projectRoleName}
        {node.startAt ? ` • ${formatDate(node.startAt, language)}` : ""}
      </Typography.Text>
    </Space>
  );

  return {
    key: node.assignmentId,
    title,
    children: node.children.map((child) => toTreeNode(child, language, t)),
  };
}

export function ProjectOrgChartPage() {
  const { t, i18n } = useTranslation();
  const [selectedProjectId, setSelectedProjectId] = useState<string>();

  const { projectOrgChartQuery } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 1 },
    projectRoles: { page: 1, pageSize: 10 },
    projectAssignments: null,
    projectOrgChartProjectId: selectedProjectId,
  });

  const projectOptionsState = useProjectOptions({ enabled: true });

  const treeData = useMemo(
    () => (projectOrgChartQuery.data ?? []).map((node) => toTreeNode(node, i18n.language, t)),
    [projectOrgChartQuery.data, i18n.language, t],
  );

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)", color: "#fff" }}>
            <ApartmentOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("project_org_chart.page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("project_org_chart.page_description")}
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
            placeholder={t("project_org_chart.select_project_placeholder")}
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
            <Alert type="info" showIcon message={t("project_org_chart.select_project_message")} />
          ) : projectOrgChartQuery.isLoading ? (
            <div style={{ minHeight: 240, display: "grid", placeItems: "center" }}>
              <Spin size="large" />
            </div>
          ) : treeData.length === 0 ? (
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description={t("project_org_chart.empty")}
            />
          ) : (
            <Space direction="vertical" size={16} style={{ width: "100%" }}>
              <Space size={8}>
                <TeamOutlined />
                <Typography.Text type="secondary">
                  {t("project_org_chart.summary", { count: projectOrgChartQuery.data?.length ?? 0 })}
                </Typography.Text>
              </Space>
              <Tree
                showLine
                selectable={false}
                defaultExpandAll
                treeData={treeData}
              />
            </Space>
          )}
        </Space>
      </Card>
    </Space>
  );
}
