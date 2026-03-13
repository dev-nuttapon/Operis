import { Alert, Card, Divider, List, Typography } from "antd";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { WorkflowDefinitionCreateForm } from "../components/WorkflowDefinitionCreateForm";
import { useCreateWorkflowDefinition } from "../hooks/useCreateWorkflowDefinition";
import { useWorkflowDefinitions } from "../hooks/useWorkflowDefinitions";

const { Paragraph, Title } = Typography;

export function WorkflowDefinitionsPage() {
  const definitionsQuery = useWorkflowDefinitions();
  const createDefinitionMutation = useCreateWorkflowDefinition();
  const createErrorPresentation = createDefinitionMutation.isError
    ? getApiErrorPresentation(createDefinitionMutation.error, "Unable to create workflow definition")
    : null;

  return (
    <Card variant="borderless" style={{ borderRadius: 16 }}>
      <Title level={2} style={{ marginTop: 0 }}>
        Workflow Definitions
      </Title>
      <Paragraph type="secondary">
        Workflow management will live in this module as the feature surface grows.
      </Paragraph>

      {createErrorPresentation ? (
        <Alert
          type="error"
          showIcon
          style={{ marginBottom: 16 }}
          message={createErrorPresentation.title}
          description={createErrorPresentation.description}
        />
      ) : null}

      <WorkflowDefinitionCreateForm
        isSubmitting={createDefinitionMutation.isPending}
        onSubmit={(values) => createDefinitionMutation.mutate(values)}
      />

      <Divider />

      <List
        dataSource={definitionsQuery.data ?? []}
        loading={definitionsQuery.isLoading}
        locale={{ emptyText: "No workflow definitions yet." }}
        renderItem={(item) => <List.Item>{item.name} ({item.status})</List.Item>}
      />
    </Card>
  );
}
