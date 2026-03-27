import { useMemo, useState } from "react";
import { Alert, Button, Card, Col, Drawer, Flex, InputNumber, Row, Select, Space, Statistic, Switch, Table, Tag, Typography, message } from "antd";
import type { ColumnsType } from "antd/es/table";
import { DashboardOutlined, SaveOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { useProjectList } from "../../users/public";
import { useComplianceDashboard, useComplianceDrilldown, useUpdateComplianceDashboardPreferences } from "../hooks/useGovernance";
import type { ComplianceProcessArea, ComplianceProjectReadiness } from "../types/governance";

const { Title, Paragraph, Text } = Typography;

const processAreaOptions = [
  { value: "process-assets-planning", label: "Process Assets & Planning" },
  { value: "requirements-traceability", label: "Requirements & Traceability" },
  { value: "document-governance", label: "Document Governance" },
  { value: "change-configuration", label: "Change & Configuration" },
  { value: "verification-release", label: "Verification & Release" },
  { value: "audit-capa", label: "Audit & CAPA" },
  { value: "security-resilience", label: "Security & Resilience" },
] as const;

const drilldownOptions = [
  { value: "missing-artifact", label: "Missing Artifacts" },
  { value: "overdue-approval", label: "Overdue Approvals" },
  { value: "stale-baseline", label: "Stale Baselines" },
  { value: "open-capa", label: "Open CAPA" },
  { value: "open-audit-finding", label: "Open Audit Findings" },
  { value: "open-security-item", label: "Open Security Items" },
] as const;

export function ComplianceDashboardPage() {
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canRead = permissionState.hasAnyPermission(permissions.governance.complianceRead, permissions.governance.complianceManage);
  const canManage = permissionState.hasPermission(permissions.governance.complianceManage);
  const [messageApi, contextHolder] = message.useMessage();
  const [filters, setFilters] = useState<{ projectId?: string; processArea?: string; periodDays: number; showOnlyAtRisk: boolean }>({
    projectId: undefined,
    processArea: undefined,
    periodDays: 30,
    showOnlyAtRisk: false,
  });
  const [drilldown, setDrilldown] = useState<{ issueType: string; projectId?: string; processArea?: string } | null>(null);

  const dashboardQuery = useComplianceDashboard(filters, canRead);
  const drilldownQuery = useComplianceDrilldown(drilldown, canRead && Boolean(drilldown));
  const projectsQuery = useProjectList({ page: 1, pageSize: 100 });
  const saveDefaultsMutation = useUpdateComplianceDashboardPreferences();

  const projectColumns = useMemo<ColumnsType<ComplianceProjectReadiness>>(
    () => [
      { title: "Project", key: "project", render: (_, item) => <Space direction="vertical" size={0}><Text strong>{item.projectCode}</Text><Text type="secondary">{item.projectName}</Text></Space> },
      { title: "Phase", dataIndex: "projectPhase", render: (value) => value ?? "-" },
      { title: "Readiness", dataIndex: "readinessScore", render: (value: number, item) => <Space><Tag color={item.readinessState === "good" ? "green" : item.readinessState === "at_risk" ? "gold" : "red"}>{item.readinessState}</Tag><Text>{value}</Text></Space> },
      { title: "Missing", dataIndex: "missingArtifactCount" },
      { title: "Overdue", dataIndex: "overdueApprovalCount" },
      { title: "Stale", dataIndex: "staleBaselineCount" },
      { title: "CAPA", dataIndex: "openCapaCount" },
      { title: "Audit", dataIndex: "openAuditFindingCount" },
      { title: "Security", dataIndex: "openSecurityItemCount" },
    ],
    [],
  );

  const processAreaColumns = useMemo<ColumnsType<ComplianceProcessArea>>(
    () => [
      { title: "Process Area", dataIndex: "label" },
      { title: "Projects", dataIndex: "projectCount" },
      { title: "At Risk", dataIndex: "atRiskProjectCount" },
      { title: "Missing", dataIndex: "missingArtifactCount" },
      { title: "Overdue", dataIndex: "overdueApprovalCount" },
      { title: "Stale", dataIndex: "staleBaselineCount" },
      {
        title: "Drilldown",
        key: "drilldown",
        render: (_, item) => (
          <Select
            placeholder="Open issue set"
            style={{ width: 200 }}
            options={drilldownOptions.map((option) => ({ value: option.value, label: option.label }))}
            onChange={(value) => setDrilldown({ issueType: value, projectId: filters.projectId, processArea: item.processArea })}
          />
        ),
      },
    ],
    [filters.projectId],
  );

  if (!canRead) {
    return <Alert type="warning" showIcon message="Compliance dashboard is not available for this account." />;
  }

  const summary = dashboardQuery.data?.summary;

  const saveDefaults = async () => {
    try {
      await saveDefaultsMutation.mutateAsync({
        defaultProjectId: filters.projectId,
        defaultProcessArea: filters.processArea,
        defaultPeriodDays: filters.periodDays,
        defaultShowOnlyAtRisk: filters.showOnlyAtRisk,
      });
      void messageApi.success("Compliance dashboard defaults saved.");
    } catch (error) {
      const presentation = getApiErrorPresentation(error, "Unable to save compliance defaults");
      void messageApi.error(presentation.description);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      {contextHolder}
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #1d4ed8)", color: "#fff" }}>
            <DashboardOutlined />
          </div>
          <div>
            <Title level={3} style={{ margin: 0 }}>Compliance Dashboard</Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              Review readiness, missing governed artifacts, overdue approvals, stale baselines, and unresolved operational controls across projects.
            </Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Flex gap={12} wrap="wrap" justify="space-between" align="center">
          <Flex gap={12} wrap="wrap">
            <Select
              allowClear
              placeholder="Project"
              style={{ width: 240 }}
              options={(projectsQuery.data?.items ?? []).map((project) => ({ value: project.id, label: `${project.code} · ${project.name}` }))}
              value={filters.projectId}
              onChange={(value) => setFilters((current) => ({ ...current, projectId: value }))}
              showSearch
              optionFilterProp="label"
            />
            <Select
              allowClear
              placeholder="Process area"
              style={{ width: 240 }}
              options={processAreaOptions.map((option) => ({ value: option.value, label: option.label }))}
              value={filters.processArea}
              onChange={(value) => setFilters((current) => ({ ...current, processArea: value }))}
            />
            <InputNumber min={7} max={365} value={filters.periodDays} onChange={(value) => setFilters((current) => ({ ...current, periodDays: Number(value) || 30 }))} addonBefore="Days" />
            <Space>
              <Switch checked={filters.showOnlyAtRisk} onChange={(value) => setFilters((current) => ({ ...current, showOnlyAtRisk: value }))} />
              <Text>At-risk only</Text>
            </Space>
          </Flex>
          <Button type="default" icon={<SaveOutlined />} disabled={!canManage} loading={saveDefaultsMutation.isPending} onClick={() => void saveDefaults()}>
            Save defaults
          </Button>
        </Flex>
        <Paragraph type="secondary" style={{ margin: "12px 0 0" }}>
          Generated at {dashboardQuery.data?.generatedAt ? new Date(dashboardQuery.data.generatedAt).toLocaleString() : "-"}
        </Paragraph>
      </Card>

      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={8}><Card><Statistic title="Projects In Good Standing" value={summary?.projectsInGoodStanding ?? 0} /></Card></Col>
        <Col xs={24} sm={12} lg={8}><Card><Statistic title="Projects Missing Artifacts" value={summary?.projectsWithMissingArtifacts ?? 0} /></Card></Col>
        <Col xs={24} sm={12} lg={8}><Card><Statistic title="Overdue Approvals" value={summary?.overdueApprovals ?? 0} /></Card></Col>
        <Col xs={24} sm={12} lg={8}><Card><Statistic title="Stale Baselines" value={summary?.staleBaselines ?? 0} /></Card></Col>
        <Col xs={24} sm={12} lg={8}><Card><Statistic title="Open CAPA" value={summary?.openCapa ?? 0} /></Card></Col>
        <Col xs={24} sm={12} lg={8}><Card><Statistic title="Open Security Items" value={summary?.openSecurityItems ?? 0} /></Card></Col>
      </Row>

      <Card variant="borderless" title="Project Readiness">
        <Table
          rowKey="projectId"
          loading={dashboardQuery.isLoading}
          columns={projectColumns}
          dataSource={dashboardQuery.data?.projects ?? []}
          expandable={{
            expandedRowRender: (item) => (
              <Space wrap>
                {drilldownOptions.map((option) => (
                  <Button key={option.value} size="small" onClick={() => setDrilldown({ issueType: option.value, projectId: item.projectId, processArea: filters.processArea })}>
                    {option.label}
                  </Button>
                ))}
                <Button size="small" type="link" onClick={() => navigate(`/app/projects/${item.projectId}`)}>Open project</Button>
              </Space>
            ),
          }}
          pagination={false}
        />
      </Card>

      <Card variant="borderless" title="Process Area Readiness">
        <Table rowKey="processArea" loading={dashboardQuery.isLoading} columns={processAreaColumns} dataSource={dashboardQuery.data?.processAreas ?? []} pagination={false} />
      </Card>

      <Drawer
        title="Compliance Drilldown"
        open={Boolean(drilldown)}
        width={720}
        onClose={() => setDrilldown(null)}
        destroyOnHidden
      >
        <Space direction="vertical" size={16} style={{ width: "100%" }}>
          <Text type="secondary">Generated at {drilldownQuery.data?.generatedAt ? new Date(drilldownQuery.data.generatedAt).toLocaleString() : "-"}</Text>
          <Table
            rowKey={(row) => `${row.entityType}-${row.entityId}-${row.title}`}
            loading={drilldownQuery.isLoading}
            pagination={false}
            columns={[
              { title: "Title", dataIndex: "title" },
              { title: "Status", dataIndex: "status", render: (value: string) => <Tag>{value}</Tag> },
              { title: "Scope", dataIndex: "scope" },
              { title: "Due / Age", dataIndex: "dueAt", render: (value?: string | null) => value ? new Date(value).toLocaleString() : "-" },
              { title: "Metadata", dataIndex: "metadata", render: (value?: string | null) => value ?? "-" },
              {
                title: "Open",
                key: "open",
                render: (_, row: { route: string }) => (
                  <Button type="link" onClick={() => navigate(row.route)}>Go to source</Button>
                ),
              },
            ]}
            dataSource={drilldownQuery.data?.rows ?? []}
          />
        </Space>
      </Drawer>
    </Space>
  );
}
