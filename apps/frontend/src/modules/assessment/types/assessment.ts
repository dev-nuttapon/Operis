import type { PaginationInput, PaginatedResult } from "../../../shared/types/pagination";

export interface AssessmentPackageListInput extends PaginationInput {
  projectId?: string;
  processArea?: string;
  status?: string;
  search?: string;
}

export interface AssessmentFindingListInput extends PaginationInput {
  packageId?: string;
  status?: string;
  search?: string;
}

export interface ControlCatalogListInput extends PaginationInput {
  projectId?: string;
  controlSet?: string;
  processArea?: string;
  status?: string;
  search?: string;
}

export interface ControlMappingListInput extends PaginationInput {
  controlId?: string;
  projectId?: string;
  status?: string;
  targetModule?: string;
  search?: string;
}

export interface ControlCoverageListInput extends PaginationInput {
  projectId?: string;
  controlSet?: string;
  processArea?: string;
  coverageStatus?: string;
  search?: string;
}

export interface AssessmentEvidenceReference {
  sourceModule: string;
  entityType: string;
  entityId: string;
  title: string;
  status: string;
  processArea: string;
  route: string;
  capturedAt?: string | null;
  metadataSummary?: string | null;
}

export interface AssessmentPackageNote {
  id: string;
  noteType: string;
  note: string;
  createdByUserId: string;
  createdAt: string;
}

export interface AssessmentFindingListItem {
  id: string;
  packageId: string;
  packageCode: string;
  title: string;
  severity: string;
  status: string;
  evidenceEntityType: string;
  evidenceEntityId: string;
  ownerUserId?: string | null;
  updatedAt: string;
}

export interface AssessmentPackageListItem {
  id: string;
  packageCode: string;
  projectId?: string | null;
  projectName?: string | null;
  projectCode?: string | null;
  processArea?: string | null;
  scopeSummary: string;
  status: string;
  evidenceCount: number;
  openFindingCount: number;
  updatedAt: string;
}

export interface AssessmentPackageDetail extends AssessmentPackageListItem {
  createdByUserId: string;
  preparedAt?: string | null;
  preparedByUserId?: string | null;
  sharedAt?: string | null;
  sharedByUserId?: string | null;
  archivedAt?: string | null;
  archivedByUserId?: string | null;
  evidenceReferences: AssessmentEvidenceReference[];
  notes: AssessmentPackageNote[];
  findings: AssessmentFindingListItem[];
  createdAt: string;
}

export interface AssessmentFindingDetail extends AssessmentFindingListItem {
  description: string;
  evidenceRoute?: string | null;
  acceptanceSummary?: string | null;
  closureSummary?: string | null;
  createdByUserId: string;
  acceptedAt?: string | null;
  acceptedByUserId?: string | null;
  closedAt?: string | null;
  closedByUserId?: string | null;
  createdAt: string;
}

export interface CreateAssessmentPackageInput {
  projectId?: string | null;
  processArea?: string | null;
  scopeSummary: string;
}

export interface TransitionAssessmentPackageInput {
  targetStatus: string;
  reason?: string | null;
}

export interface CreateAssessmentNoteInput {
  noteType: string;
  note: string;
}

export interface CreateAssessmentFindingInput {
  packageId: string;
  title: string;
  description: string;
  severity: string;
  evidenceEntityType: string;
  evidenceEntityId: string;
  evidenceRoute?: string | null;
  ownerUserId?: string | null;
}

export interface TransitionAssessmentFindingInput {
  targetStatus: string;
  summary?: string | null;
}

export interface ControlCatalogItem {
  id: string;
  controlCode: string;
  title: string;
  controlSet: string;
  processArea?: string | null;
  status: string;
  description?: string | null;
  projectId?: string | null;
  projectName?: string | null;
  activeMappingCount: number;
  updatedAt: string;
}

export interface CreateControlCatalogItemInput {
  controlCode: string;
  title: string;
  controlSet: string;
  processArea?: string | null;
  description?: string | null;
  projectId?: string | null;
}

export interface UpdateControlCatalogItemInput extends CreateControlCatalogItemInput {
  status: string;
}

export interface ControlMappingDetail {
  id: string;
  controlId: string;
  controlCode: string;
  controlTitle: string;
  projectId?: string | null;
  projectName?: string | null;
  targetModule: string;
  targetEntityType: string;
  targetEntityId: string;
  targetRoute: string;
  evidenceStatus: string;
  status: string;
  notes?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateControlMappingInput {
  controlId: string;
  projectId?: string | null;
  targetModule: string;
  targetEntityType: string;
  targetEntityId: string;
  targetRoute: string;
  evidenceStatus?: string | null;
  notes?: string | null;
}

export interface TransitionControlMappingInput {
  targetStatus: string;
  reason?: string | null;
}

export interface ControlCoverageItem {
  controlId: string;
  controlCode: string;
  title: string;
  controlSet: string;
  processArea?: string | null;
  projectId?: string | null;
  projectName?: string | null;
  coverageStatus: string;
  activeMappingCount: number;
  evidenceCount: number;
  gapCount: number;
  generatedAt: string;
}

export type AssessmentPackageListResult = PaginatedResult<AssessmentPackageListItem>;
export type AssessmentFindingListResult = PaginatedResult<AssessmentFindingListItem>;
export type ControlCatalogListResult = PaginatedResult<ControlCatalogItem>;
export type ControlMappingListResult = PaginatedResult<ControlMappingDetail>;
export type ControlCoverageListResult = PaginatedResult<ControlCoverageItem>;
