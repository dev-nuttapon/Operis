import { Button, Space, Typography } from "antd";
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
  return (
    <Space orientation="vertical" size={12} style={{ width: "100%" }}>
      <Space wrap>
        {filterOrder.map((filter) => (
          <Button
            key={filter}
            type={selectedFilter === filter ? "primary" : "default"}
            onClick={() => onSelectFilter(filter)}
          >
            {filter} ({statusSummary[filter]})
          </Button>
        ))}
      </Space>
      <Text type="secondary">
        {statusSummary.active} active / {statusSummary.draft} draft / {statusSummary.archived} archived
      </Text>
    </Space>
  );
}
