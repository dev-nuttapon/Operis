import { Button, Space, Typography } from "antd";
import { useTranslation } from "react-i18next";
import type { WorkflowDefinitionStatusSummary, WorkflowStatusFilter } from "../types/workflows";

const { Text } = Typography;

interface WorkflowDefinitionFiltersProps {
  selectedFilter: WorkflowStatusFilter;
  statusSummary: WorkflowDefinitionStatusSummary;
  onSelectFilter: (filter: WorkflowStatusFilter) => void;
}

const filterOrder: WorkflowStatusFilter[] = ["all", "draft", "active", "archived"];

export function WorkflowDefinitionFilters({
  selectedFilter,
  statusSummary,
  onSelectFilter,
}: WorkflowDefinitionFiltersProps) {
  const { t } = useTranslation();
  const filterLabels: Record<WorkflowStatusFilter, string> = {
    all: t("workflow_definitions.filters.all"),
    draft: t("workflow_definitions.filters.draft"),
    active: t("workflow_definitions.filters.active"),
    archived: t("workflow_definitions.filters.archived"),
  };

  return (
    <Space orientation="vertical" size={12} style={{ width: "100%" }}>
      <Space wrap>
        {filterOrder.map((filter) => (
          <Button
            key={filter}
            type={selectedFilter === filter ? "primary" : "default"}
            onClick={() => onSelectFilter(filter)}
          >
            {filterLabels[filter]} ({statusSummary[filter]})
          </Button>
        ))}
      </Space>
      <Text type="secondary">
        {t("workflow_definitions.filters.summary", {
          active: statusSummary.active,
          draft: statusSummary.draft,
          archived: statusSummary.archived,
        })}
      </Text>
    </Space>
  );
}
