import { useEffect, useMemo, useState } from "react";
import {
  Alert,
  Button,
  Card,
  Col,
  Descriptions,
  Flex,
  List,
  Modal,
  Row,
  Segmented,
  Select,
  Space,
  Steps,
  Table,
  Tag,
  Timeline,
  Tree,
  Typography,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import type { DataNode } from "antd/es/tree";
import {
  AuditOutlined,
  ClusterOutlined,
  FileSearchOutlined,
  ProfileOutlined,
  ProjectOutlined,
  SafetyCertificateOutlined,
  TeamOutlined,
} from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import { useProjectWorkspacePrototype } from "../hooks/useProjectWorkspacePrototype";
import type {
  ProjectWorkspacePrototypeAuditEvent,
  ProjectWorkspacePrototypeComplianceCheck,
  ProjectWorkspacePrototypeEvidenceItem,
  ProjectWorkspacePrototypeMember,
  ProjectWorkspacePrototypeOrgNode,
  ProjectWorkspacePrototypeRole,
  ProjectWorkspacePrototypeSection,
} from "../types/projectWorkspacePrototype";

function toneToStyle(tone: "blue" | "green" | "gold" | "red") {
  switch (tone) {
    case "green":
      return { background: "linear-gradient(135deg, #16a34a, #15803d)" };
    case "gold":
      return { background: "linear-gradient(135deg, #d97706, #b45309)" };
    case "red":
      return { background: "linear-gradient(135deg, #dc2626, #b91c1c)" };
    default:
      return { background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)" };
  }
}

function complianceColor(severity: ProjectWorkspacePrototypeComplianceCheck["severity"]) {
  switch (severity) {
    case "passed":
      return "success";
    case "warning":
      return "warning";
    default:
      return "error";
  }
}

function buildOrgChartTree(nodes: ProjectWorkspacePrototypeOrgNode[]): DataNode[] {
  return nodes.map((node) => ({
    key: node.id,
    title: (
      <Space direction="vertical" size={2}>
        <Typography.Text strong>{node.label}</Typography.Text>
        <Typography.Text type="secondary">{node.subtitle}</Typography.Text>
      </Space>
    ),
    children: node.children ? buildOrgChartTree(node.children) : undefined,
  }));
}

export function ProjectWorkspacePrototypePage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { projectId } = useParams<{ projectId: string }>();
  const [selectedRole, setSelectedRole] = useState<ProjectWorkspacePrototypeRole | null>(null);
  const [selectedEvidenceItem, setSelectedEvidenceItem] = useState<ProjectWorkspacePrototypeEvidenceItem | null>(null);
  const permissionState = usePermissions();
  const canReadProjects = permissionState.hasPermission(permissions.projects.read);
  const {
    section,
    setSection,
    templateId,
    setTemplateId,
    scenarioId,
    setScenarioId,
    templateOptions,
    scenarioOptions,
    quickActions,
    summaryCards,
    teamMembers,
    roles,
    complianceChecks,
    evidenceItems,
    auditTrail,
    orgChart,
  } = useProjectWorkspacePrototype();

  const { projectsQuery } = useProjectAdmin({
    projects: { page: 1, pageSize: 100, sortBy: "name", sortOrder: "asc" },
    projectRoles: { page: 1, pageSize: 100, sortBy: "displayOrder", sortOrder: "asc" },
    projectAssignments: null,
  });

  const projectOptions = useMemo(
    () =>
      (projectsQuery.data?.items ?? []).map((item) => ({
        label: `${item.code} - ${item.name}`,
        value: item.id,
      })),
    [projectsQuery.data?.items],
  );

  const selectedProject = useMemo(
    () => (projectsQuery.data?.items ?? []).find((item) => item.id === projectId) ?? null,
    [projectId, projectsQuery.data?.items],
  );

  useEffect(() => {
    if (!projectId && (projectsQuery.data?.items?.length ?? 0) > 0) {
      navigate(`/app/projects/${projectsQuery.data!.items[0]!.id}/workspace`, { replace: true });
    }
  }, [navigate, projectId, projectsQuery.data?.items]);

  const sectionOptions = useMemo(
    () => [
      { label: t("project_workspace_prototype.sections.overview"), value: "overview" },
      { label: t("project_workspace_prototype.sections.team"), value: "team" },
      { label: t("project_workspace_prototype.sections.org_chart"), value: "orgChart" },
      { label: t("project_workspace_prototype.sections.roles"), value: "roles" },
      { label: t("project_workspace_prototype.sections.compliance"), value: "compliance" },
      { label: t("project_workspace_prototype.sections.evidence"), value: "evidence" },
      { label: t("project_workspace_prototype.sections.audit_trail"), value: "auditTrail" },
    ],
    [t],
  );

  const selectedTemplate = templateOptions.find((option) => option.id === templateId) ?? null;
  const selectedScenario = scenarioOptions.find((option) => option.id === scenarioId) ?? null;
  const orgChartTree = useMemo(() => buildOrgChartTree(orgChart), [orgChart]);
  const processStages = useMemo(() => {
    switch (templateId) {
      case "compliance_audit":
        return [
          t("project_workspace_prototype.process_stages.compliance_audit.prepare"),
          t("project_workspace_prototype.process_stages.compliance_audit.collect"),
          t("project_workspace_prototype.process_stages.compliance_audit.review"),
          t("project_workspace_prototype.process_stages.compliance_audit.close"),
        ];
      case "process_improvement":
        return [
          t("project_workspace_prototype.process_stages.process_improvement.assess"),
          t("project_workspace_prototype.process_stages.process_improvement.design"),
          t("project_workspace_prototype.process_stages.process_improvement.pilot"),
          t("project_workspace_prototype.process_stages.process_improvement.rollout"),
        ];
      default:
        return [
          t("project_workspace_prototype.process_stages.software_delivery.plan"),
          t("project_workspace_prototype.process_stages.software_delivery.build"),
          t("project_workspace_prototype.process_stages.software_delivery.verify"),
          t("project_workspace_prototype.process_stages.software_delivery.release"),
        ];
    }
  }, [t, templateId]);

  const workspacePrompts = useMemo(() => {
    switch (scenarioId) {
      case "release_readiness":
        return [
          t("project_workspace_prototype.workspace_prompts.release_readiness.1"),
          t("project_workspace_prototype.workspace_prompts.release_readiness.2"),
          t("project_workspace_prototype.workspace_prompts.release_readiness.3"),
        ];
      case "audit_preparation":
        return [
          t("project_workspace_prototype.workspace_prompts.audit_preparation.1"),
          t("project_workspace_prototype.workspace_prompts.audit_preparation.2"),
          t("project_workspace_prototype.workspace_prompts.audit_preparation.3"),
        ];
      case "corrective_action":
        return [
          t("project_workspace_prototype.workspace_prompts.corrective_action.1"),
          t("project_workspace_prototype.workspace_prompts.corrective_action.2"),
          t("project_workspace_prototype.workspace_prompts.corrective_action.3"),
        ];
      default:
        return [
          t("project_workspace_prototype.workspace_prompts.default.1"),
          t("project_workspace_prototype.workspace_prompts.default.2"),
          t("project_workspace_prototype.workspace_prompts.default.3"),
        ];
    }
  }, [scenarioId, t]);

  const teamColumns: ColumnsType<ProjectWorkspacePrototypeMember> = [
    {
      title: t("project_workspace_prototype.team.columns.member"),
      dataIndex: "name",
      render: (_, record) => (
        <Space direction="vertical" size={2}>
          <Typography.Text strong>{record.name}</Typography.Text>
          <Typography.Text type="secondary">{record.email}</Typography.Text>
        </Space>
      ),
    },
    {
      title: t("project_workspace_prototype.team.columns.role"),
      dataIndex: "roleName",
                  render: (_, record) => (
                    <Space>
                      <Tag>{record.roleCode}</Tag>
                      <Typography.Text>{record.roleName}</Typography.Text>
                    </Space>
      ),
    },
    {
      title: t("project_workspace_prototype.team.columns.reports_to"),
      dataIndex: "reportsTo",
      render: (value: string | undefined) => value ?? "-",
    },
    {
      title: t("project_workspace_prototype.team.columns.primary"),
      dataIndex: "primary",
      render: (value: boolean) => <Tag color={value ? "blue" : "default"}>{value ? t("common.actions.yes") : t("common.actions.no")}</Tag>,
    },
    {
      title: t("project_workspace_prototype.team.columns.status"),
      dataIndex: "status",
      render: (value: string) => <Tag color={value === "Active" ? "green" : "gold"}>{value}</Tag>,
    },
    {
      title: t("project_workspace_prototype.team.columns.period"),
      dataIndex: "period",
    },
  ];

  const roleColumns: ColumnsType<ProjectWorkspacePrototypeRole> = [
    { title: t("project_workspace_prototype.roles.columns.role"), dataIndex: "name" },
    { title: t("project_workspace_prototype.roles.columns.code"), dataIndex: "code" },
    {
      title: t("project_workspace_prototype.roles.columns.permissions"),
      key: "permissions",
      render: (_, record) => (
        <Space wrap size={[4, 4]}>
          {record.review ? <Tag color="processing">{t("project_workspace_prototype.roles.permissions.review")}</Tag> : null}
          {record.approval ? <Tag color="success">{t("project_workspace_prototype.roles.permissions.approval")}</Tag> : null}
          {record.release ? <Tag color="purple">{t("project_workspace_prototype.roles.permissions.release")}</Tag> : null}
        </Space>
      ),
    },
    { title: t("project_workspace_prototype.roles.columns.members"), dataIndex: "memberCount" },
    {
      title: t("project_workspace_prototype.roles.columns.actions"),
      key: "actions",
      render: (_, record) => (
        <Button type="link" onClick={() => setSelectedRole(record)}>
          {t("project_workspace_prototype.roles.inspect_role")}
        </Button>
      ),
    },
  ];

  const auditColumns: ColumnsType<ProjectWorkspacePrototypeAuditEvent> = [
    { title: t("project_workspace_prototype.audit.columns.when"), dataIndex: "timestamp" },
    { title: t("project_workspace_prototype.audit.columns.actor"), dataIndex: "actor" },
    { title: t("project_workspace_prototype.audit.columns.action"), dataIndex: "action" },
    { title: t("project_workspace_prototype.audit.columns.target"), dataIndex: "target" },
    { title: t("project_workspace_prototype.audit.columns.detail"), dataIndex: "detail" },
  ];

  const renderSection = (current: ProjectWorkspacePrototypeSection) => {
    switch (current) {
      case "team":
        return (
          <Card size="small" title={t("project_workspace_prototype.team.title")}>
            <Table rowKey="id" columns={teamColumns} dataSource={teamMembers} pagination={false} />
          </Card>
        );
      case "orgChart":
        return (
          <Card size="small" title={t("project_workspace_prototype.org_chart.title")}>
            <Tree defaultExpandAll showLine selectable={false} treeData={orgChartTree} />
          </Card>
        );
      case "roles":
        return (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Card size="small" title={t("project_workspace_prototype.roles.title")}>
              <Table rowKey="id" columns={roleColumns} dataSource={roles} pagination={false} />
            </Card>
            <Row gutter={[16, 16]}>
              {roles.map((role) => (
                <Col key={role.id} xs={24} lg={12}>
                  <Card size="small">
                    <Space direction="vertical" size={8} style={{ width: "100%" }}>
                      <Flex justify="space-between" align="center">
                        <Typography.Text strong>{role.name}</Typography.Text>
                        <Tag>{role.code}</Tag>
                      </Flex>
                      <Typography.Text type="secondary">{role.responsibility}</Typography.Text>
                      <Descriptions size="small" column={1}>
                        <Descriptions.Item label={t("project_workspace_prototype.roles.authority")}>
                          {role.authority}
                        </Descriptions.Item>
                      </Descriptions>
                    </Space>
                  </Card>
                </Col>
              ))}
            </Row>
          </Space>
        );
      case "compliance":
        return (
          <Card size="small" title={t("project_workspace_prototype.compliance.title")}>
            <Space direction="vertical" size={12} style={{ width: "100%" }}>
              {complianceChecks.map((check) => (
                <Alert
                  key={check.id}
                  type={complianceColor(check.severity)}
                  showIcon
                  message={t(check.titleKey)}
                  description={
                    <Space direction="vertical" size={8}>
                      <Typography.Text>{t(check.detailKey)}</Typography.Text>
                      <Button type="link" style={{ paddingInline: 0 }} onClick={() => setSection(check.targetSection)}>
                        {t("project_workspace_prototype.compliance.drill_down")}
                      </Button>
                    </Space>
                  }
                />
              ))}
            </Space>
          </Card>
        );
      case "evidence":
        return (
          <Card size="small" title={t("project_workspace_prototype.evidence.title")}>
            <List
              dataSource={evidenceItems}
              renderItem={(item) => (
                <List.Item
                  actions={[
                    <Button key={`${item.id}-preview`} onClick={() => setSelectedEvidenceItem(item)}>
                      {t("project_workspace_prototype.evidence.preview")}
                    </Button>,
                    <Button key={`${item.id}-history`} onClick={() => setSection("auditTrail")}>
                      {t("project_workspace_prototype.evidence.view_history")}
                    </Button>,
                  ]}
                >
                  <List.Item.Meta
                    title={item.title}
                    description={
                      <Space direction="vertical" size={2}>
                        <Typography.Text>{item.description}</Typography.Text>
                        <Typography.Text type="secondary">
                          {t("project_workspace_prototype.evidence.meta", {
                            updated: item.lastUpdated,
                            format: item.format,
                          })}
                        </Typography.Text>
                      </Space>
                    }
                  />
                </List.Item>
              )}
            />
          </Card>
        );
      case "auditTrail":
        return (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Card size="small" title={t("project_workspace_prototype.audit.timeline_title")}>
              <Timeline
                items={auditTrail.map((event) => ({
                  color: "blue",
                  children: (
                    <Space direction="vertical" size={0}>
                      <Typography.Text strong>{`${event.actor} · ${event.action}`}</Typography.Text>
                      <Typography.Text>{event.detail}</Typography.Text>
                      <Typography.Text type="secondary">{`${event.timestamp} · ${event.target}`}</Typography.Text>
                    </Space>
                  ),
                }))}
              />
            </Card>
            <Card size="small" title={t("project_workspace_prototype.audit.table_title")}>
              <Table rowKey="id" columns={auditColumns} dataSource={auditTrail} pagination={false} />
            </Card>
          </Space>
        );
      case "overview":
      default:
        return (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Descriptions
              bordered
              size="small"
              column={{ xs: 1, md: 2 }}
              items={[
                {
                  key: "code",
                  label: t("project_workspace_prototype.overview.project_code"),
                  children: selectedProject?.code ?? "-",
                },
                {
                  key: "type",
                  label: t("project_workspace_prototype.overview.project_type"),
                  children: selectedProject?.projectType ?? (selectedTemplate ? t(selectedTemplate.labelKey) : "-"),
                },
                {
                  key: "owner",
                  label: t("project_workspace_prototype.overview.owner"),
                  children: selectedProject?.ownerDisplayName ?? "-",
                },
                {
                  key: "sponsor",
                  label: t("project_workspace_prototype.overview.sponsor"),
                  children: selectedProject?.sponsorDisplayName ?? "-",
                },
                {
                  key: "template",
                  label: t("project_workspace_prototype.overview.template"),
                  children: selectedTemplate ? t(selectedTemplate.labelKey) : "-",
                },
                {
                  key: "scenario",
                  label: t("project_workspace_prototype.overview.scenario"),
                  children: selectedScenario ? t(selectedScenario.labelKey) : "-",
                },
              ]}
            />
            <Card size="small" title={t("project_workspace_prototype.overview.process_stages_title")}>
              <Steps
                current={Math.max(0, processStages.length - 2)}
                responsive
                items={processStages.map((title) => ({ title }))}
              />
            </Card>
            <Card size="small" title={t("project_workspace_prototype.overview.workspace_prompts_title")}>
              <List
                dataSource={workspacePrompts}
                renderItem={(item) => (
                  <List.Item>
                    <Typography.Text>{item}</Typography.Text>
                  </List.Item>
                )}
              />
            </Card>
            <Card size="small" title={t("project_workspace_prototype.overview.quick_actions_title")}>
              <Flex gap={12} wrap="wrap">
                {quickActions.map((action) => (
                  <Button key={action.id} onClick={() => setSection(action.targetSection)}>
                    {t(action.labelKey)}
                  </Button>
                ))}
              </Flex>
            </Card>
            <Alert
              type="info"
              showIcon
              message={t("project_workspace_prototype.overview.callout_title")}
              description={t("project_workspace_prototype.overview.callout_description")}
            />
          </Space>
        );
    }
  };

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
              background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)",
              color: "#fff",
            }}
          >
            <ProjectOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("project_workspace_prototype.page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("project_workspace_prototype.page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!canReadProjects ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
        ) : (
          <Space direction="vertical" size={20} style={{ width: "100%" }}>
            <Card size="small">
              <Flex justify="space-between" align="center" wrap="wrap" gap={12}>
                <Space wrap size={12}>
                  <Select
                    showSearch
                    optionFilterProp="label"
                    style={{ minWidth: 320, maxWidth: "100%" }}
                    placeholder={t("project_workspace_prototype.select_project_placeholder")}
                    options={projectOptions}
                    value={projectId}
                    onChange={(value: string) => navigate(`/app/projects/${value}/workspace`)}
                  />
                  <Select
                    style={{ minWidth: 220 }}
                    options={templateOptions.map((option) => ({ label: t(option.labelKey), value: option.id }))}
                    value={templateId}
                    onChange={(value: string) => setTemplateId(value)}
                    placeholder={t("project_workspace_prototype.select_template_placeholder")}
                  />
                  <Select
                    style={{ minWidth: 220 }}
                    options={scenarioOptions.map((option) => ({ label: t(option.labelKey), value: option.id }))}
                    value={scenarioId}
                    onChange={(value: string) => setScenarioId(value)}
                    placeholder={t("project_workspace_prototype.select_scenario_placeholder")}
                  />
                </Space>
                <Button onClick={() => navigate("/app/admin/projects")}>{t("project_workspace_prototype.back_to_projects")}</Button>
              </Flex>
            </Card>

            <Flex gap={16} wrap="wrap">
              {summaryCards.map((card) => (
                <Card
                  key={card.id}
                  size="small"
                  style={{
                    minWidth: 220,
                    flex: "1 1 220px",
                    ...toneToStyle(card.tone),
                    color: "#fff",
                  }}
                  styles={{ body: { padding: 18 } }}
                >
                  <Space direction="vertical" size={2}>
                    <Typography.Text style={{ color: "rgba(255,255,255,0.9)" }}>{t(card.titleKey)}</Typography.Text>
                    <Typography.Title level={2} style={{ margin: 0, color: "#fff" }}>
                      {card.value}
                    </Typography.Title>
                    <Typography.Text style={{ color: "rgba(255,255,255,0.9)" }}>{t(card.helperKey)}</Typography.Text>
                  </Space>
                </Card>
              ))}
            </Flex>

            <Card size="small">
              <Flex justify="space-between" align="center" wrap="wrap" gap={12}>
                <Space size={10} wrap>
                  <Tag color="processing">{selectedProject?.code ?? "-"}</Tag>
                  <Tag color="green">{selectedProject?.status ?? "-"}</Tag>
                  <Typography.Text strong>{selectedProject?.name ?? "-"}</Typography.Text>
                </Space>
                <Segmented options={sectionOptions} value={section} onChange={(value) => setSection(value as ProjectWorkspacePrototypeSection)} />
              </Flex>
            </Card>

            <Row gutter={[16, 16]}>
              <Col xs={24} xl={17}>
                {renderSection(section)}
              </Col>
              <Col xs={24} xl={7}>
                <Space direction="vertical" size={16} style={{ width: "100%" }}>
                  <Card size="small" title={t("project_workspace_prototype.side_panels.project_controls")}>
                    <List
                      dataSource={[
                        { icon: <TeamOutlined />, label: t("project_workspace_prototype.side_panels.team_register") },
                        { icon: <ClusterOutlined />, label: t("project_workspace_prototype.side_panels.org_chart") },
                        { icon: <ProfileOutlined />, label: t("project_workspace_prototype.side_panels.role_matrix") },
                        { icon: <SafetyCertificateOutlined />, label: t("project_workspace_prototype.side_panels.compliance") },
                        { icon: <FileSearchOutlined />, label: t("project_workspace_prototype.side_panels.evidence") },
                        { icon: <AuditOutlined />, label: t("project_workspace_prototype.side_panels.audit") },
                      ]}
                      renderItem={(item) => (
                        <List.Item>
                          <Space>
                            {item.icon}
                            <Typography.Text>{item.label}</Typography.Text>
                          </Space>
                        </List.Item>
                      )}
                    />
                  </Card>

                  <Card size="small" title={t("project_workspace_prototype.side_panels.prototype_notes_title")}>
                    <Space direction="vertical" size={10}>
                      <Typography.Text type="secondary">
                        {t("project_workspace_prototype.side_panels.selected_project_meta", {
                          owner: selectedProject?.ownerDisplayName ?? "-",
                          phase: selectedProject?.phase ?? "-",
                        })}
                      </Typography.Text>
                      {selectedTemplate ? <Typography.Paragraph style={{ margin: 0 }}>{t(selectedTemplate.descriptionKey)}</Typography.Paragraph> : null}
                      {selectedScenario ? <Typography.Paragraph style={{ margin: 0 }}>{t(selectedScenario.descriptionKey)}</Typography.Paragraph> : null}
                      <Typography.Paragraph style={{ margin: 0 }}>{t("project_workspace_prototype.side_panels.prototype_notes_1")}</Typography.Paragraph>
                      <Typography.Paragraph style={{ margin: 0 }}>{t("project_workspace_prototype.side_panels.prototype_notes_2")}</Typography.Paragraph>
                      <Typography.Paragraph style={{ margin: 0 }}>{t("project_workspace_prototype.side_panels.prototype_notes_3")}</Typography.Paragraph>
                    </Space>
                  </Card>
                </Space>
              </Col>
            </Row>
          </Space>
        )}
      </Card>

      <Modal
        open={selectedRole !== null}
        title={selectedRole ? `${selectedRole.code} · ${selectedRole.name}` : ""}
        footer={null}
        onCancel={() => setSelectedRole(null)}
      >
        {selectedRole ? (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Descriptions size="small" column={1}>
              <Descriptions.Item label={t("project_workspace_prototype.roles.modal.responsibility")}>
                {selectedRole.responsibility}
              </Descriptions.Item>
              <Descriptions.Item label={t("project_workspace_prototype.roles.modal.authority")}>
                {selectedRole.authority}
              </Descriptions.Item>
              <Descriptions.Item label={t("project_workspace_prototype.roles.modal.members")}>
                {selectedRole.memberCount}
              </Descriptions.Item>
            </Descriptions>
            <Space wrap>
              {selectedRole.review ? <Tag color="processing">{t("project_workspace_prototype.roles.permissions.review")}</Tag> : null}
              {selectedRole.approval ? <Tag color="success">{t("project_workspace_prototype.roles.permissions.approval")}</Tag> : null}
              {selectedRole.release ? <Tag color="purple">{t("project_workspace_prototype.roles.permissions.release")}</Tag> : null}
            </Space>
          </Space>
        ) : null}
      </Modal>

      <Modal
        open={selectedEvidenceItem !== null}
        title={selectedEvidenceItem?.title ?? ""}
        footer={null}
        onCancel={() => setSelectedEvidenceItem(null)}
      >
        {selectedEvidenceItem ? (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Typography.Paragraph style={{ margin: 0 }}>{selectedEvidenceItem.description}</Typography.Paragraph>
            <Descriptions size="small" column={1}>
              <Descriptions.Item label={t("project_workspace_prototype.evidence.modal.updated")}>
                {selectedEvidenceItem.lastUpdated}
              </Descriptions.Item>
              <Descriptions.Item label={t("project_workspace_prototype.evidence.modal.format")}>
                {selectedEvidenceItem.format}
              </Descriptions.Item>
              <Descriptions.Item label={t("project_workspace_prototype.evidence.modal.usage")}>
                {t("project_workspace_prototype.evidence.modal.usage_text")}
              </Descriptions.Item>
            </Descriptions>
          </Space>
        ) : null}
      </Modal>
    </Space>
  );
}
