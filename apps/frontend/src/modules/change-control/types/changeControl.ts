import type { PaginatedResult, PaginationInput } from "../../../shared/types/pagination";

export interface ChangeControlListInput extends PaginationInput {
  search?: string;
  status?: string;
  priority?: string;
  projectId?: string;
}

export interface ChangeImpact {
  id?: string;
  changeRequestId?: string;
  scopeImpact: string;
  scheduleImpact: string;
  qualityImpact: string;
  securityImpact: string;
  performanceImpact: string;
  riskImpact: string;
}

export interface ChangeRequestListItem {
  id: string;
  projectId: string;
  projectName: string;
  code: string;
  title: string;
  priority: string;
  requestedBy: string;
  status: string;
  targetBaselineName?: string | null;
  approvalStatus?: string | null;
  updatedAt: string;
}

export interface ChangeRequestDetail {
  id: string;
  projectId: string;
  projectName: string;
  code: string;
  title: string;
  requestedBy: string;
  reason: string;
  status: string;
  priority: string;
  targetBaselineId?: string | null;
  targetBaselineName?: string | null;
  linkedRequirementIds: string[];
  linkedConfigurationItemIds: string[];
  impact: ChangeImpact;
  decisionRationale?: string | null;
  implementationSummary?: string | null;
  approvedBy?: string | null;
  approvedAt?: string | null;
  history: Array<{ id: string; eventType: string; summary?: string | null; reason?: string | null; actorUserId?: string | null; occurredAt: string }>;
  createdAt: string;
  updatedAt: string;
}

export interface ChangeRequestFormInput {
  projectId: string;
  code: string;
  title: string;
  requestedBy: string;
  reason: string;
  priority: string;
  targetBaselineId?: string | null;
  impact: ChangeImpact;
  linkedRequirementIds?: string[];
  linkedConfigurationItemIds?: string[];
}

export interface ChangeRequestUpdateInput extends Omit<ChangeRequestFormInput, "projectId" | "code"> {}

export interface ConfigurationItem {
  id: string;
  projectId: string;
  projectName: string;
  code: string;
  name: string;
  itemType: string;
  ownerModule: string;
  status: string;
  baselineRef?: string | null;
  createdAt?: string;
  updatedAt: string;
}

export interface ConfigurationItemFormInput {
  projectId: string;
  code: string;
  name: string;
  itemType: string;
  ownerModule: string;
}

export interface ConfigurationItemUpdateInput {
  name: string;
  itemType: string;
  ownerModule: string;
}

export interface BaselineRegistryItem {
  id: string;
  projectId: string;
  projectName: string;
  baselineName: string;
  baselineType: string;
  sourceEntityType: string;
  sourceEntityId: string;
  status: string;
  approvedBy?: string | null;
  approvedAt?: string | null;
  changeRequestId?: string | null;
  supersededByBaselineId?: string | null;
  updatedAt: string;
}

export interface BaselineRegistryDetail extends BaselineRegistryItem {
  overrideReason?: string | null;
  createdAt: string;
}

export interface BaselineRegistryFormInput {
  projectId: string;
  baselineName: string;
  baselineType: string;
  sourceEntityType: string;
  sourceEntityId: string;
  changeRequestId: string;
}

export interface BaselineOverrideInput {
  supersededByBaselineId?: string | null;
  emergencyOverride: boolean;
  reason?: string | null;
}

export type ChangeRequestListResult = PaginatedResult<ChangeRequestListItem>;
export type ConfigurationItemListResult = PaginatedResult<ConfigurationItem>;
export type BaselineRegistryListResult = PaginatedResult<BaselineRegistryItem>;
