import { useEffect, useMemo, useState } from "react";
import {
  Alert,
  Button,
  Card,
  Col,
  Descriptions,
  Empty,
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
  DeploymentUnitOutlined,
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
import { useProjectOptions } from "../hooks/useProjectOptions";
import { formatDate } from "../utils/adminUsersPresentation";
import type { ProjectOrgChartNode } from "../types/users";
import type {
  ProjectWorkspacePrototypeAuditEvent,
  ProjectWorkspacePrototypeComplianceCheck,
  ProjectWorkspacePrototypeEvidenceItem,
  ProjectWorkspacePrototypeMember,
  ProjectWorkspacePrototypeOrgNode,
  ProjectWorkspacePrototypeRequiredDocument,
  ProjectWorkspacePrototypeRole,
  ProjectWorkspacePrototypeSection,
  ProjectWorkspacePrototypeSummaryCard,
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

function mapOrgChartNodes(nodes: ProjectOrgChartNode[]): ProjectWorkspacePrototypeOrgNode[] {
  return nodes.map((node) => ({
    id: node.assignmentId,
    label: node.userDisplayName ?? node.userEmail ?? node.userId,
    subtitle: node.projectRoleName,
    children: mapOrgChartNodes(node.children ?? []),
  }));
}

function flattenOrgChart(nodes: ProjectOrgChartNode[]): ProjectOrgChartNode[] {
  const result: ProjectOrgChartNode[] = [];
  for (const node of nodes) {
    result.push(node);
    if (node.children?.length) {
      result.push(...flattenOrgChart(node.children));
    }
  }
  return result;
}

export function ProjectWorkspacePrototypePage() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const { projectId } = useParams<{ projectId: string }>();
  const [selectedRole, setSelectedRole] = useState<ProjectWorkspacePrototypeRole | null>(null);
  const [selectedEvidenceItem, setSelectedEvidenceItem] = useState<ProjectWorkspacePrototypeEvidenceItem | null>(null);
  const [section, setSection] = useState<ProjectWorkspacePrototypeSection>("overview");
  const [orgChartView, setOrgChartView] = useState<"tree" | "roleGroups">("tree");
  const [auditorMode, setAuditorMode] = useState(false);
  const [teamPage, setTeamPage] = useState(1);
  const [teamPageSize, setTeamPageSize] = useState(10);
  const [rolePage, setRolePage] = useState(1);
  const [rolePageSize, setRolePageSize] = useState(10);
  const [auditPage, setAuditPage] = useState(1);
  const [auditPageSize, setAuditPageSize] = useState(10);
  const permissionState = usePermissions();
  const canReadProjects = permissionState.hasPermission(permissions.projects.read);
  const canManageProjects = permissionState.hasPermission(permissions.projects.manage);
  const quickActions = useMemo(
    () => [
      { id: "role-gap", labelKey: "project_workspace.quick_actions.role_gap", targetSection: "roles" as const },
      { id: "org-chart", labelKey: "project_workspace.quick_actions.org_chart", targetSection: "orgChart" as const },
      { id: "workflow", labelKey: "project_workspace.quick_actions.workflow", targetSection: "workflow" as const },
      { id: "compliance", labelKey: "project_workspace.quick_actions.compliance", targetSection: "compliance" as const },
      { id: "evidence", labelKey: "project_workspace.quick_actions.evidence", targetSection: "evidence" as const },
    ],
    [],
  );

  const { projectOrgChartQuery, projectEvidenceTeamRegisterQuery, projectEvidenceRoleResponsibilitiesQuery, projectEvidenceAssignmentHistoryQuery } =
    useProjectAdmin({
      projectsEnabled: false,
      projects: { page: 1, pageSize: 1 },
      projectRoles: { page: 1, pageSize: 1 },
      projectAssignments: null,
      projectOrgChartProjectId: projectId,
      projectEvidenceTeamRegister: { projectId, page: teamPage, pageSize: teamPageSize },
      projectEvidenceRoleResponsibilities: { projectId, page: rolePage, pageSize: rolePageSize },
      projectEvidenceAssignmentHistory: { projectId, page: auditPage, pageSize: auditPageSize },
    });
  const projectOptionsState = useProjectOptions({ enabled: true, assignedOnly: !canReadProjects });

  const selectedProject = useMemo(
    () => (projectId ? projectOptionsState.itemsById.get(projectId) ?? null : null),
    [projectId, projectOptionsState.itemsById],
  );
  const hasWorkspaceAccess = canReadProjects || selectedProject !== null;

  useEffect(() => {
    if (!projectId && projectOptionsState.items.length > 0) {
      navigate(`/app/projects/${projectOptionsState.items[0]!.id}/workspace`, { replace: true });
    }
  }, [navigate, projectId, projectOptionsState.items]);

  useEffect(() => {
    if (auditorMode && !["compliance", "evidence", "auditTrail", "workflow"].includes(section)) {
      setSection("compliance");
    }
  }, [auditorMode, section, setSection]);

  useEffect(() => {
    setTeamPage(1);
    setRolePage(1);
    setAuditPage(1);
  }, [projectId]);

  const sectionOptions = useMemo(
    () => [
      { label: t("project_workspace.sections.overview"), value: "overview" },
      { label: t("project_workspace.sections.team"), value: "team" },
      { label: t("project_workspace.sections.org_chart"), value: "orgChart" },
      { label: t("project_workspace.sections.roles"), value: "roles" },
      { label: t("project_workspace.sections.workflow"), value: "workflow" },
      { label: t("project_workspace.sections.compliance"), value: "compliance" },
      { label: t("project_workspace.sections.evidence"), value: "evidence" },
      { label: t("project_workspace.sections.audit_trail"), value: "auditTrail" },
    ],
    [t],
  );

  const orgChartNodes = useMemo(() => mapOrgChartNodes(projectOrgChartQuery.data ?? []), [projectOrgChartQuery.data]);
  const orgChartTree = useMemo(() => buildOrgChartTree(orgChartNodes), [orgChartNodes]);
  const orgChartFlat = useMemo(() => flattenOrgChart(projectOrgChartQuery.data ?? []), [projectOrgChartQuery.data]);
  const orgChartMembers = useMemo(() => {
    const displayById = new Map<string, string>();
    for (const node of orgChartFlat) {
      displayById.set(node.userId, node.userDisplayName ?? node.userEmail ?? node.userId);
    }
    return orgChartFlat.map((node) => ({
      id: node.assignmentId,
      name: node.userDisplayName ?? node.userEmail ?? node.userId,
      email: node.userEmail ?? "-",
      roleCode: "-",
      roleName: node.projectRoleName,
      reportsTo: node.reportsToUserId ? displayById.get(node.reportsToUserId) : undefined,
      primary: node.isPrimary,
      status: node.status,
      period: `${formatDate(node.startAt, i18n.language)} - ${formatDate(node.endAt, i18n.language)}`,
    }));
  }, [orgChartFlat, i18n.language]);
  const membersByRole = useMemo(() => {
    const groups = new Map<string, ProjectWorkspacePrototypeMember[]>();
    for (const member of orgChartMembers) {
      const key = `${member.roleName}`;
      const current = groups.get(key) ?? [];
      current.push(member);
      groups.set(key, current);
    }
    return Array.from(groups.entries()).map(([roleKey, members]) => ({ roleKey, members }));
  }, [orgChartMembers]);

  const activeMembers = projectEvidenceTeamRegisterQuery.data?.total ?? 0;
  const activeRoles = projectEvidenceRoleResponsibilitiesQuery.data?.total ?? 0;
  const reportingRoots = projectOrgChartQuery.data?.length ?? 0;
  const summaryCards: ProjectWorkspacePrototypeSummaryCard[] = useMemo(
    () => [
      {
        id: "project-type",
        tone: "blue",
        titleKey: "project_workspace.summary.project_type.title",
        helperKey: "project_workspace.summary.project_type.helper",
        value: selectedProject?.projectType ?? "-",
      },
      {
        id: "active-members",
        tone: "green",
        titleKey: "project_workspace.summary.active_members.title",
        helperKey: "project_workspace.summary.active_members.helper",
        value: activeMembers.toString(),
      },
      {
        id: "active-roles",
        tone: "gold",
        titleKey: "project_workspace.summary.active_roles.title",
        helperKey: "project_workspace.summary.active_roles.helper",
        value: activeRoles.toString(),
      },
      {
        id: "reporting-roots",
        tone: "red",
        titleKey: "project_workspace.summary.reporting_roots.title",
        helperKey: "project_workspace.summary.reporting_roots.helper",
        value: reportingRoots.toString(),
      },
    ],
    [activeMembers, activeRoles, reportingRoots, selectedProject?.projectType],
  );

  const complianceChecks: ProjectWorkspacePrototypeComplianceCheck[] = useMemo(() => {
    const ownerOk = Boolean(selectedProject?.ownerUserId);
    const rolesOk = activeRoles > 0;
    const teamOk = activeMembers > 0;
    const orgOk = reportingRoots > 0;
    return [
      {
        id: "owner",
        titleKey: "project_workspace.compliance.owner.title",
        detailKey: "project_workspace.compliance.owner.detail",
        severity: ownerOk ? "passed" : "warning",
        targetSection: "overview",
      },
      {
        id: "role-gap",
        titleKey: "project_workspace.compliance.role_gap.title",
        detailKey: "project_workspace.compliance.role_gap.detail",
        severity: rolesOk ? "passed" : "warning",
        targetSection: "roles",
      },
      {
        id: "team",
        titleKey: "project_workspace.compliance.org_chart.title",
        detailKey: "project_workspace.compliance.org_chart.detail",
        severity: teamOk && orgOk ? "passed" : "warning",
        targetSection: "orgChart",
      },
      {
        id: "evidence",
        titleKey: "project_workspace.compliance.evidence.title",
        detailKey: "project_workspace.compliance.evidence.detail",
        severity: "warning",
        targetSection: "evidence",
      },
    ];
  }, [activeMembers, activeRoles, reportingRoots, selectedProject?.ownerUserId]);

  const evidenceItems: ProjectWorkspacePrototypeEvidenceItem[] = [];
  const evidenceSnapshots: { id: string; title: string; detail: string; meta: string }[] = [];
  const processStages = useMemo(() => (selectedProject?.phase ? [selectedProject.phase] : []), [selectedProject?.phase]);
  const workspacePrompts = useMemo(() => [], []);
  const teamTableRows = useMemo(
    () =>
      (projectEvidenceTeamRegisterQuery.data?.items ?? []).map((row) => ({
        id: row.assignmentId,
        name: row.userDisplayName ?? row.userEmail ?? row.userId,
        email: row.userEmail ?? "-",
        roleCode: "-",
        roleName: row.projectRoleName,
        reportsTo: row.reportsToDisplayName ?? "-",
        primary: row.isPrimary,
        status: row.status,
        period: `${formatDate(row.startAt, i18n.language)} - ${formatDate(row.endAt, i18n.language)}`,
      })),
    [projectEvidenceTeamRegisterQuery.data?.items, i18n.language],
  );
  const roleTableRows = useMemo(
    () =>
      (projectEvidenceRoleResponsibilitiesQuery.data?.items ?? []).map((row) => ({
        id: row.projectRoleId,
        name: row.projectRoleName,
        code: row.code ?? "-",
        responsibility: row.responsibilities ?? row.description ?? "-",
        authority: row.authorityScope ?? "-",
        review: row.canReviewDocuments,
        approval: row.canApproveDocuments,
        release: row.canReleaseDocuments,
        memberCount: row.memberCount,
      })),
    [projectEvidenceRoleResponsibilitiesQuery.data?.items],
  );
  const auditTableRows = useMemo(
    () =>
      (projectEvidenceAssignmentHistoryQuery.data?.items ?? []).map((row) => ({
        id: row.assignmentId,
        timestamp: formatDate(row.createdAt, i18n.language),
        actor: row.userDisplayName ?? row.userEmail ?? row.userId,
        action: row.status,
        target: row.projectRoleName,
        detail: row.changeReason ?? "-",
      })),
    [projectEvidenceAssignmentHistoryQuery.data?.items, i18n.language],
  );
  const workflowSteps = useMemo(() => {
    const steps: { roleCode: string; action: string; output: string }[] = [];
    for (const role of roleTableRows) {
      if (role.review) {
        steps.push({ roleCode: role.code, action: t("project_workspace.roles.permissions.review"), output: role.name });
      }
      if (role.approval) {
        steps.push({ roleCode: role.code, action: t("project_workspace.roles.permissions.approval"), output: role.name });
      }
      if (role.release) {
        steps.push({ roleCode: role.code, action: t("project_workspace.roles.permissions.release"), output: role.name });
      }
    }
    return steps;
  }, [roleTableRows, t]);
  const workflowPreviewSteps = useMemo(
    () =>
      workflowSteps.map((step, index) => ({
        title: `${index + 1}. ${step.roleCode}`,
        description: `${step.action} · ${step.output}`,
        status: (index === workflowSteps.length - 1 ? "wait" : index === 0 ? "process" : "finish") as
          | "wait"
          | "process"
          | "finish"
          | "error",
      })),
    [workflowSteps],
  );
  const workflowStateSteps = useMemo(
    () =>
      workflowSteps.map((step) => ({
        title: step.action,
      })),
    [workflowSteps],
  );
  const blockedFlowAlerts = useMemo(() => [], []);
  const requiredDocuments: ProjectWorkspacePrototypeRequiredDocument[] = [];
  const roleDependencies: Array<{ fromRoleCode: string; toRoleCode: string; relation: string; rationale: string }> = [];

  const teamColumns: ColumnsType<ProjectWorkspacePrototypeMember> = [
    {
      title: t("project_workspace.team.columns.member"),
      dataIndex: "name",
      render: (_, record) => (
        <Space direction="vertical" size={2}>
          <Typography.Text strong>{record.name}</Typography.Text>
          <Typography.Text type="secondary">{record.email}</Typography.Text>
        </Space>
      ),
    },
    {
      title: t("project_workspace.team.columns.role"),
      dataIndex: "roleName",
      render: (_, record) => (
        <Space>
          <Tag>{record.roleCode}</Tag>
          <Typography.Text>{record.roleName}</Typography.Text>
        </Space>
      ),
    },
    {
      title: t("project_workspace.team.columns.reports_to"),
      dataIndex: "reportsTo",
      render: (value: string | undefined) => value ?? "-",
    },
    {
      title: t("project_workspace.team.columns.primary"),
      dataIndex: "primary",
      render: (value: boolean) => <Tag color={value ? "blue" : "default"}>{value ? t("common.actions.yes") : t("common.actions.no")}</Tag>,
    },
    {
      title: t("project_workspace.team.columns.status"),
      dataIndex: "status",
      render: (value: string) => <Tag color={value === "Active" ? "green" : "gold"}>{value}</Tag>,
    },
    {
      title: t("project_workspace.team.columns.period"),
      dataIndex: "period",
    },
  ];

  const roleColumns: ColumnsType<ProjectWorkspacePrototypeRole> = [
    { title: t("project_workspace.roles.columns.role"), dataIndex: "name" },
    { title: t("project_workspace.roles.columns.code"), dataIndex: "code" },
    {
      title: t("project_workspace.roles.columns.permissions"),
      key: "permissions",
      render: (_, record) => (
        <Space wrap size={[4, 4]}>
          {record.review ? <Tag color="processing">{t("project_workspace.roles.permissions.review")}</Tag> : null}
          {record.approval ? <Tag color="success">{t("project_workspace.roles.permissions.approval")}</Tag> : null}
          {record.release ? <Tag color="purple">{t("project_workspace.roles.permissions.release")}</Tag> : null}
        </Space>
      ),
    },
    { title: t("project_workspace.roles.columns.members"), dataIndex: "memberCount" },
    {
      title: t("project_workspace.roles.columns.actions"),
      key: "actions",
      render: (_, record) => (
        <Button type="link" onClick={() => setSelectedRole(record)}>
          {t("project_workspace.roles.inspect_role")}
        </Button>
      ),
    },
  ];

  const auditColumns: ColumnsType<ProjectWorkspacePrototypeAuditEvent> = [
    { title: t("project_workspace.audit.columns.when"), dataIndex: "timestamp" },
    { title: t("project_workspace.audit.columns.actor"), dataIndex: "actor" },
    { title: t("project_workspace.audit.columns.action"), dataIndex: "action" },
    { title: t("project_workspace.audit.columns.target"), dataIndex: "target" },
    { title: t("project_workspace.audit.columns.detail"), dataIndex: "detail" },
  ];

  const renderSection = (current: ProjectWorkspacePrototypeSection) => {
    switch (current) {
      case "team":
        return (
          <Card size="small" title={t("project_workspace.team.title")}>
            <Table
              rowKey="id"
              columns={teamColumns}
              dataSource={teamTableRows}
              loading={projectEvidenceTeamRegisterQuery.isLoading}
              pagination={{
                current: teamPage,
                pageSize: teamPageSize,
                total: projectEvidenceTeamRegisterQuery.data?.total ?? 0,
                showSizeChanger: true,
                pageSizeOptions: ["10", "25", "50", "100"],
                onChange: (page, pageSize) => {
                  setTeamPage(pageSize === teamPageSize ? page : 1);
                  setTeamPageSize(pageSize);
                },
              }}
            />
          </Card>
        );
      case "orgChart":
        return (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Card size="small" title={t("project_workspace.org_chart.title")}>
              <Flex justify="space-between" align="center" wrap="wrap" gap={12}>
                <Typography.Text type="secondary">{t("project_workspace.org_chart.description")}</Typography.Text>
                <Segmented
                  options={[
                    { label: t("project_workspace.org_chart.views.tree"), value: "tree" },
                    { label: t("project_workspace.org_chart.views.role_groups"), value: "roleGroups" },
                  ]}
                  value={orgChartView}
                  onChange={(value) => setOrgChartView(value as "tree" | "roleGroups")}
                />
              </Flex>
            </Card>
            {orgChartView === "tree" ? (
              <Card size="small" title={t("project_workspace.org_chart.tree_title")}>
                <Tree defaultExpandAll showLine selectable={false} treeData={orgChartTree} />
              </Card>
            ) : (
              <Row gutter={[16, 16]}>
                {membersByRole.map((group) => (
                  <Col key={group.roleKey} xs={24} lg={12}>
                    <Card size="small" title={group.roleKey}>
                      <List
                        dataSource={group.members}
                        renderItem={(member) => (
                          <List.Item>
                            <Space direction="vertical" size={2}>
                              <Typography.Text strong>{member.name}</Typography.Text>
                              <Typography.Text type="secondary">
                                {t("project_workspace.org_chart.role_group_meta", {
                                  reportsTo: member.reportsTo ?? "-",
                                  primary: member.primary ? t("common.actions.yes") : t("common.actions.no"),
                                })}
                              </Typography.Text>
                            </Space>
                          </List.Item>
                        )}
                      />
                    </Card>
                  </Col>
                ))}
              </Row>
            )}
          </Space>
        );
      case "roles":
        return (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Card size="small" title={t("project_workspace.roles.title")}>
              <Table
                rowKey="id"
                columns={roleColumns}
                dataSource={roleTableRows}
                loading={projectEvidenceRoleResponsibilitiesQuery.isLoading}
                pagination={{
                  current: rolePage,
                  pageSize: rolePageSize,
                  total: projectEvidenceRoleResponsibilitiesQuery.data?.total ?? 0,
                  showSizeChanger: true,
                  pageSizeOptions: ["10", "25", "50", "100"],
                  onChange: (page, pageSize) => {
                    setRolePage(pageSize === rolePageSize ? page : 1);
                    setRolePageSize(pageSize);
                  },
                }}
              />
            </Card>
            <Row gutter={[16, 16]}>
              {roleTableRows.map((role) => (
                <Col key={role.id} xs={24} lg={12}>
                  <Card size="small">
                    <Space direction="vertical" size={8} style={{ width: "100%" }}>
                      <Flex justify="space-between" align="center">
                        <Typography.Text strong>{role.name}</Typography.Text>
                        <Tag>{role.code}</Tag>
                      </Flex>
                      <Typography.Text type="secondary">{role.responsibility}</Typography.Text>
                      <Descriptions size="small" column={1}>
                        <Descriptions.Item label={t("project_workspace.roles.authority")}>
                          {role.authority}
                        </Descriptions.Item>
                      </Descriptions>
                      <Flex gap={8} wrap>
                        <Button size="small" type="link" onClick={() => setSection("workflow")}>
                          {t("project_workspace.roles.open_workflow")}
                        </Button>
                        <Button size="small" type="link" onClick={() => setSection("evidence")}>
                          {t("project_workspace.roles.open_evidence")}
                        </Button>
                        <Button size="small" type="link" onClick={() => setSection("compliance")}>
                          {t("project_workspace.roles.open_compliance")}
                        </Button>
                      </Flex>
                    </Space>
                  </Card>
                </Col>
              ))}
            </Row>
            <Card size="small" title={t("project_workspace.roles.dependencies_title")}>
              {roleDependencies.length === 0 ? (
                <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t("project_workspace.workflow.empty")} />
              ) : (
                <Timeline
                  items={roleDependencies.map((dependency) => ({
                    color: "blue",
                    children: (
                      <Space direction="vertical" size={2}>
                        <Typography.Text strong>{`${dependency.fromRoleCode} → ${dependency.toRoleCode}`}</Typography.Text>
                        <Typography.Text>{dependency.relation}</Typography.Text>
                        <Typography.Text type="secondary">{dependency.rationale}</Typography.Text>
                      </Space>
                    ),
                  }))}
                />
              )}
            </Card>
          </Space>
        );
      case "workflow":
        return (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Card size="small" title={t("project_workspace.workflow.title")}>
              <Space direction="vertical" size={8} style={{ width: "100%" }}>
                <Typography.Text type="secondary">
                  {t("project_workspace.workflow.description")}
                </Typography.Text>
                <Steps current={0} responsive items={workflowPreviewSteps} />
              </Space>
            </Card>
            <Row gutter={[16, 16]}>
              <Col xs={24}>
                <Card size="small" title={t("project_workspace.workflow.state_flow_title")}>
                  <Space direction="vertical" size={8} style={{ width: "100%" }}>
                    <Typography.Text type="secondary">
                      {t("project_workspace.workflow.state_flow_description")}
                    </Typography.Text>
                    <Steps current={Math.max(0, workflowStateSteps.length - 2)} responsive items={workflowStateSteps} />
                  </Space>
                </Card>
              </Col>
              <Col xs={24} xl={12}>
                <Card size="small" title={t("project_workspace.workflow.required_evidence_title")}>
                  {requiredDocuments.length === 0 ? (
                    <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t("project_workspace.evidence.empty")} />
                  ) : (
                    <List
                      dataSource={requiredDocuments}
                      renderItem={(item) => (
                        <List.Item>
                          <Space direction="vertical" size={2} style={{ width: "100%" }}>
                            <Flex justify="space-between" align="center" wrap="wrap" gap={8}>
                              <Space>
                                <Tag>{item.code}</Tag>
                                <Typography.Text strong>{item.name}</Typography.Text>
                              </Space>
                              <Tag color={item.status === "Ready" ? "green" : item.status === "Draft" ? "gold" : "red"}>
                                {item.status}
                              </Tag>
                            </Flex>
                            <Typography.Text type="secondary">
                              {t("project_workspace.workflow.required_evidence_meta", {
                                owner: item.ownerRoleCode,
                                stage: item.stage,
                              })}
                            </Typography.Text>
                          </Space>
                        </List.Item>
                      )}
                    />
                  )}
                  <Flex justify="flex-end" style={{ marginTop: 12 }}>
                    <Button type="link" onClick={() => setSection("evidence")}>
                      {t("project_workspace.workflow.open_evidence")}
                    </Button>
                  </Flex>
                </Card>
              </Col>
              <Col xs={24} xl={12}>
                <Card size="small" title={t("project_workspace.workflow.gates_title")}>
                  {workflowSteps.length === 0 ? (
                    <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t("project_workspace.workflow.empty")} />
                  ) : (
                    <Timeline
                      items={workflowSteps.map((step, index) => ({
                        color: index === workflowSteps.length - 1 ? "green" : "blue",
                        children: (
                          <Space direction="vertical" size={2}>
                            <Typography.Text strong={`${step.roleCode} · ${step.output}`} />
                            <Typography.Text>{step.action}</Typography.Text>
                          </Space>
                        ),
                      }))}
                    />
                  )}
                  <Flex justify="flex-end" style={{ marginTop: 12 }}>
                    <Button type="link" onClick={() => setSection("compliance")}>
                      {t("project_workspace.workflow.open_compliance")}
                    </Button>
                  </Flex>
                </Card>
              </Col>
            </Row>
            <Card size="small" title={t("project_workspace.workflow.live_questions_title")}>
              <List
                dataSource={[
                  t("project_workspace.workflow.questions.1"),
                  t("project_workspace.workflow.questions.2"),
                  t("project_workspace.workflow.questions.3"),
                ]}
                renderItem={(item) => (
                  <List.Item>
                    <Typography.Text>{item}</Typography.Text>
                  </List.Item>
                )}
              />
            </Card>
            <Card size="small" title={t("project_workspace.workflow.blocked_states_title")}>
              <Space direction="vertical" size={12} style={{ width: "100%" }}>
                {blockedFlowAlerts.length === 0 ? (
                  <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t("project_workspace.workflow.empty")} />
                ) : (
                  blockedFlowAlerts.map((item) => <Alert key={item} type="warning" showIcon message={item} />)
                )}
              </Space>
            </Card>
          </Space>
        );
      case "compliance":
        return (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Card size="small" title={t("project_workspace.compliance.title")}>
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
                          {t("project_workspace.compliance.drill_down")}
                        </Button>
                      </Space>
                    }
                  />
                ))}
              </Space>
            </Card>
            <Row gutter={[16, 16]}>
              <Col xs={24} xl={12}>
                <Card size="small" title={t("project_workspace.compliance.required_documents_title")}>
                  {requiredDocuments.length === 0 ? (
                    <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t("project_workspace.compliance.empty")} />
                  ) : (
                    <List
                      dataSource={requiredDocuments}
                      renderItem={(item) => (
                        <List.Item>
                          <Space direction="vertical" size={2} style={{ width: "100%" }}>
                            <Flex justify="space-between" align="center" wrap="wrap" gap={8}>
                              <Space>
                                <Tag>{item.code}</Tag>
                                <Typography.Text strong>{item.name}</Typography.Text>
                              </Space>
                              <Tag color={item.status === "Ready" ? "green" : item.status === "Draft" ? "gold" : "red"}>{item.status}</Tag>
                            </Flex>
                            <Typography.Text type="secondary">
                              {t("project_workspace.compliance.document_meta", {
                                owner: item.ownerRoleCode,
                                stage: item.stage,
                              })}
                            </Typography.Text>
                          </Space>
                        </List.Item>
                      )}
                    />
                  )}
                </Card>
              </Col>
              <Col xs={24} xl={12}>
                <Card size="small" title={t("project_workspace.compliance.approval_flow_title")}>
                  {workflowSteps.length === 0 ? (
                    <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t("project_workspace.workflow.empty")} />
                  ) : (
                    <Timeline
                      items={workflowSteps.map((step, index) => ({
                        color: "blue",
                        children: (
                          <Space direction="vertical" size={2}>
                            <Typography.Text strong>{`${index + 1}. ${step.roleCode}`}</Typography.Text>
                            <Typography.Text>{step.action}</Typography.Text>
                            <Typography.Text type="secondary">{step.output}</Typography.Text>
                          </Space>
                        ),
                      }))}
                    />
                  )}
                </Card>
              </Col>
            </Row>
          </Space>
        );
      case "evidence":
        return (
          <Card size="small" title={t("project_workspace.evidence.title")}>
            <Space direction="vertical" size={16} style={{ width: "100%" }}>
              <Card size="small" title={t("project_workspace.evidence.snapshot_preview_title")}>
                <Space direction="vertical" size={8} style={{ width: "100%" }}>
                  <Typography.Text type="secondary">
                    {t("project_workspace.evidence.snapshot_preview_description")}
                  </Typography.Text>
                  {evidenceSnapshots.length === 0 ? (
                    <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t("project_workspace.evidence.empty")} />
                  ) : (
                    <List
                      dataSource={evidenceSnapshots}
                      renderItem={(item) => (
                        <List.Item>
                          <Space direction="vertical" size={2}>
                            <Typography.Text strong>{item.title}</Typography.Text>
                            <Typography.Text>{item.detail}</Typography.Text>
                            <Typography.Text type="secondary">{item.meta}</Typography.Text>
                          </Space>
                        </List.Item>
                      )}
                    />
                  )}
                </Space>
              </Card>
              {evidenceItems.length === 0 ? (
                <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t("project_workspace.evidence.empty")} />
              ) : (
                <List
                  dataSource={evidenceItems}
                  renderItem={(item) => (
                    <List.Item
                      actions={[
                        <Button key={`${item.id}-preview`} onClick={() => setSelectedEvidenceItem(item)}>
                          {t("project_workspace.evidence.preview")}
                        </Button>,
                        <Button key={`${item.id}-history`} onClick={() => setSection("auditTrail")}>
                          {t("project_workspace.evidence.view_history")}
                        </Button>,
                      ]}
                    >
                      <List.Item.Meta
                        title={item.title}
                        description={
                          <Space direction="vertical" size={2}>
                            <Typography.Text>{item.description}</Typography.Text>
                            <Typography.Text type="secondary">
                              {t("project_workspace.evidence.meta", {
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
              )}
              <Card size="small" title={t("project_workspace.evidence.snapshots.title")}>
                {evidenceSnapshots.length === 0 ? (
                  <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t("project_workspace.evidence.empty")} />
                ) : (
                  <List
                    dataSource={evidenceSnapshots}
                    renderItem={(item) => (
                      <List.Item>
                        <Space direction="vertical" size={2}>
                          <Typography.Text strong>{item.title}</Typography.Text>
                          <Typography.Text>{item.detail}</Typography.Text>
                          <Typography.Text type="secondary">{item.meta}</Typography.Text>
                        </Space>
                      </List.Item>
                    )}
                  />
                )}
              </Card>
            </Space>
          </Card>
        );
      case "auditTrail":
        return (
          <Space direction="vertical" size={16} style={{ width: "100%" }}>
            <Card size="small" title={t("project_workspace.audit.timeline_title")}>
              <Timeline
                items={auditTableRows.map((event) => ({
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
            <Card size="small" title={t("project_workspace.audit.table_title")}>
              <Table
                rowKey="id"
                columns={auditColumns}
                dataSource={auditTableRows}
                loading={projectEvidenceAssignmentHistoryQuery.isLoading}
                pagination={{
                  current: auditPage,
                  pageSize: auditPageSize,
                  total: projectEvidenceAssignmentHistoryQuery.data?.total ?? 0,
                  showSizeChanger: true,
                  pageSizeOptions: ["10", "25", "50", "100"],
                  onChange: (page, pageSize) => {
                    setAuditPage(pageSize === auditPageSize ? page : 1);
                    setAuditPageSize(pageSize);
                  },
                }}
              />
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
                  label: t("project_workspace.overview.project_code"),
                  children: selectedProject?.code ?? "-",
                },
                {
                  key: "type",
                  label: t("project_workspace.overview.project_type"),
                  children: selectedProject?.projectType ?? "-",
                },
                {
                  key: "owner",
                  label: t("project_workspace.overview.owner"),
                  children: selectedProject?.ownerDisplayName ?? "-",
                },
                {
                  key: "sponsor",
                  label: t("project_workspace.overview.sponsor"),
                  children: selectedProject?.sponsorDisplayName ?? "-",
                },
              ]}
            />
            <Card size="small" title={t("project_workspace.overview.process_stages_title")}>
              {processStages.length === 0 ? (
                <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t("project_workspace.workflow.empty")} />
              ) : (
                <Steps
                  current={Math.max(0, processStages.length - 2)}
                  responsive
                  items={processStages.map((title) => ({ title }))}
                />
              )}
            </Card>
            <Card size="small" title={t("project_workspace.overview.workspace_prompts_title")}>
              {workspacePrompts.length === 0 ? (
                <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t("project_workspace.workflow.empty")} />
              ) : (
                <List
                  dataSource={workspacePrompts}
                  renderItem={(item) => (
                    <List.Item>
                      <Typography.Text>{item}</Typography.Text>
                    </List.Item>
                  )}
                />
              )}
            </Card>
            <Card size="small" title={t("project_workspace.overview.workflow_preview_title")}>
              <Steps direction="vertical" size="small" current={0} items={workflowPreviewSteps} />
            </Card>
            <Card size="small" title={t("project_workspace.overview.quick_actions_title")}>
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
              message={t("project_workspace.overview.callout_title")}
              description={t("project_workspace.overview.callout_description")}
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
              {t("project_workspace.page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("project_workspace.page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        {!hasWorkspaceAccess ? (
          <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
        ) : (
          <Space direction="vertical" size={20} style={{ width: "100%" }}>
            {auditorMode ? (
              <Alert
                type="info"
                showIcon
                message={t("project_workspace.auditor_mode.title")}
                description={t("project_workspace.auditor_mode.description")}
                action={
                  <Space wrap>
                    <Button size="small" onClick={() => setSection("compliance")}>
                      {t("project_workspace.sections.compliance")}
                    </Button>
                    <Button size="small" onClick={() => setSection("evidence")}>
                      {t("project_workspace.sections.evidence")}
                    </Button>
                    <Button size="small" onClick={() => setSection("auditTrail")}>
                      {t("project_workspace.sections.audit_trail")}
                    </Button>
                  </Space>
                }
              />
            ) : null}
            <Card size="small">
              <Flex justify="space-between" align="center" wrap="wrap" gap={12}>
                <Space wrap size={12}>
                  <Select
                    showSearch
                    filterOption={false}
                    optionFilterProp="label"
                    style={{ minWidth: 320, maxWidth: "100%" }}
                    placeholder={t("project_workspace.select_project_placeholder")}
                    options={projectOptionsState.options}
                    value={projectId}
                    loading={projectOptionsState.loading}
                    onSearch={projectOptionsState.onSearch}
                    onChange={(value: string) => navigate(`/app/projects/${value}/workspace`)}
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
                  <Segmented
                    options={[
                      { label: t("project_workspace.modes.standard"), value: "standard" },
                      { label: t("project_workspace.modes.auditor"), value: "auditor" },
                    ]}
                    value={auditorMode ? "auditor" : "standard"}
                    onChange={(value) => setAuditorMode(value === "auditor")}
                  />
                </Space>
                <Button onClick={() => navigate(canManageProjects ? "/app/admin/projects" : "/app/projects")}>
                  {t(canManageProjects ? "project_workspace.back_to_projects" : "projects.back_to_my_projects")}
                </Button>
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
              <Col xs={24} xl={auditorMode ? 24 : 17}>
                {renderSection(section)}
              </Col>
              {auditorMode ? null : (
                <Col xs={24} xl={7}>
                  <Space direction="vertical" size={16} style={{ width: "100%" }}>
                    <Card size="small" title={t("project_workspace.side_panels.project_controls")}>
                      <List
                        dataSource={[
                          { icon: <TeamOutlined />, label: t("project_workspace.side_panels.team_register") },
                          { icon: <ClusterOutlined />, label: t("project_workspace.side_panels.org_chart") },
                          { icon: <ProfileOutlined />, label: t("project_workspace.side_panels.role_matrix") },
                          { icon: <DeploymentUnitOutlined />, label: t("project_workspace.side_panels.workflow") },
                          { icon: <SafetyCertificateOutlined />, label: t("project_workspace.side_panels.compliance") },
                          { icon: <FileSearchOutlined />, label: t("project_workspace.side_panels.evidence") },
                          { icon: <AuditOutlined />, label: t("project_workspace.side_panels.audit") },
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

                    <Card size="small" title={t("project_workspace.side_panels.prototype_notes_title")}>
                      <Space direction="vertical" size={10}>
                        <Typography.Text type="secondary">
                          {t("project_workspace.side_panels.selected_project_meta", {
                            owner: selectedProject?.ownerDisplayName ?? "-",
                            phase: selectedProject?.phase ?? "-",
                          })}
                        </Typography.Text>
                      </Space>
                    </Card>
                  </Space>
                </Col>
              )}
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
              <Descriptions.Item label={t("project_workspace.roles.modal.responsibility")}>
                {selectedRole.responsibility}
              </Descriptions.Item>
              <Descriptions.Item label={t("project_workspace.roles.modal.authority")}>
                {selectedRole.authority}
              </Descriptions.Item>
              <Descriptions.Item label={t("project_workspace.roles.modal.members")}>
                {selectedRole.memberCount}
              </Descriptions.Item>
            </Descriptions>
            <Space wrap>
              {selectedRole.review ? <Tag color="processing">{t("project_workspace.roles.permissions.review")}</Tag> : null}
              {selectedRole.approval ? <Tag color="success">{t("project_workspace.roles.permissions.approval")}</Tag> : null}
              {selectedRole.release ? <Tag color="purple">{t("project_workspace.roles.permissions.release")}</Tag> : null}
            </Space>
            <Flex gap={8} wrap>
              <Button type="link" onClick={() => {
                setSelectedRole(null);
                setSection("workflow");
              }}>
                {t("project_workspace.roles.open_workflow")}
              </Button>
              <Button type="link" onClick={() => {
                setSelectedRole(null);
                setSection("evidence");
              }}>
                {t("project_workspace.roles.open_evidence")}
              </Button>
            </Flex>
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
              <Descriptions.Item label={t("project_workspace.evidence.modal.updated")}>
                {selectedEvidenceItem.lastUpdated}
              </Descriptions.Item>
              <Descriptions.Item label={t("project_workspace.evidence.modal.format")}>
                {selectedEvidenceItem.format}
              </Descriptions.Item>
              <Descriptions.Item label={t("project_workspace.evidence.modal.usage")}>
                {t("project_workspace.evidence.modal.usage_text")}
              </Descriptions.Item>
            </Descriptions>
          </Space>
        ) : null}
      </Modal>
    </Space>
  );
}
