import { Alert, Button, Card, Divider, Flex, Typography, Space } from "antd";
import { FolderOpenOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { WorkflowDefinitionFilters } from "../components/WorkflowDefinitionFilters";
import { WorkflowDefinitionList } from "../components/WorkflowDefinitionList";
import { useWorkflowDefinitionActions } from "../hooks/useWorkflowDefinitionActions";
import { useWorkflowDefinitionsScreen } from "../hooks/useWorkflowDefinitionsScreen";

const { Paragraph, Title } = Typography;

export function WorkflowDefinitionsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const permissionState = usePermissions();
  const canReadWorkflows = permissionState.hasPermission(permissions.workflows.read);
  const canManageDefinitions = permissionState.hasPermission(permissions.workflows.manageDefinitions);
  const {
    definitionsQuery,
    filteredDefinitions,
    setStatusFilter,
    statusFilter,
    statusSummary,
    paging,
    setPaging,
  } = useWorkflowDefinitionsScreen();
  const { activateMutation, archiveMutation } = useWorkflowDefinitionActions();
  const actionErrorPresentation = activateMutation.isError
    ? getApiErrorPresentation(activateMutation.error, t("workflow_definitions.notifications.activate_failed_title"))
    : archiveMutation.isError
      ? getApiErrorPresentation(archiveMutation.error, t("workflow_definitions.notifications.archive_failed_title"))
      : null;

  return (
    <Space direction="vertical" size={16} style={{ width: "100%" }}>
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
            <FolderOpenOutlined />
          </div>
          <div>
            <Title level={2} style={{ margin: 0 }}>
              {t("workflow_definitions.page_title")}
            </Title>
            <Paragraph type="secondary" style={{ marginTop: 4 }}>
              {t("workflow_definitions.page_description")}
            </Paragraph>
          </div>
        </Space>
      </Card>

    <Card variant="borderless" style={{ borderRadius: 16 }}>

      {!canReadWorkflows ? (
        <Alert
          type="warning"
          showIcon
          style={{ marginBottom: 16 }}
          message={t("errors.title_forbidden")}
        />
      ) : null}

      {actionErrorPresentation ? (
        <Alert
          type="error"
          showIcon
          style={{ marginBottom: 16 }}
          message={actionErrorPresentation.title}
          description={actionErrorPresentation.description}
        />
      ) : null}

      {canManageDefinitions ? (
        <Flex justify="flex-end" style={{ marginBottom: 16 }}>
          <Button type="primary" onClick={() => navigate("/app/workflows/new")}>
            {t("workflow_definitions.actions.create")}
          </Button>
        </Flex>
      ) : null}

      <Divider />

      {canReadWorkflows ? (
        <WorkflowDefinitionFilters
          selectedFilter={statusFilter}
          statusSummary={statusSummary}
          onSelectFilter={setStatusFilter}
        />
      ) : null}

      <Divider />

      {canReadWorkflows ? (
        <WorkflowDefinitionList
          canManage={canManageDefinitions}
          definitions={filteredDefinitions}
          isLoading={definitionsQuery.isLoading}
          isMutating={activateMutation.isPending || archiveMutation.isPending}
          pagination={{
            page: definitionsQuery.data?.page ?? paging.page,
            pageSize: definitionsQuery.data?.pageSize ?? paging.pageSize,
            total: definitionsQuery.data?.total ?? 0,
          }}
          onPageChange={(page, pageSize) => {
            setPaging((current) => ({
              ...current,
              page: pageSize === current.pageSize ? page : 1,
              pageSize,
            }));
          }}
          onEdit={(workflowDefinitionId) => navigate(`/app/workflows/${workflowDefinitionId}/edit`)}
          onActivate={(workflowDefinitionId) => activateMutation.mutate({ workflowDefinitionId })}
          onArchive={(workflowDefinitionId) => archiveMutation.mutate({ workflowDefinitionId })}
        />
      ) : null}
    </Card>
    </Space>
  );
}
