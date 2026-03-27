import { apiRequest } from "../../../shared/lib/apiClient";
import type {
  AssessmentFindingDetail,
  AssessmentFindingListInput,
  AssessmentFindingListResult,
  AssessmentPackageDetail,
  AssessmentPackageListInput,
  AssessmentPackageListResult,
  AssessmentPackageNote,
  ControlCatalogItem,
  ControlCatalogListInput,
  ControlCatalogListResult,
  ControlCoverageListInput,
  ControlCoverageListResult,
  ControlMappingDetail,
  ControlMappingListInput,
  ControlMappingListResult,
  CreateControlCatalogItemInput,
  CreateControlMappingInput,
  CreateAssessmentFindingInput,
  CreateAssessmentNoteInput,
  CreateAssessmentPackageInput,
  TransitionControlMappingInput,
  TransitionAssessmentFindingInput,
  TransitionAssessmentPackageInput,
  UpdateControlCatalogItemInput,
} from "../types/assessment";

function toQuery(input: Record<string, string | number | boolean | undefined | null>) {
  const params = new URLSearchParams();
  Object.entries(input).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      params.set(key, String(value));
    }
  });

  const query = params.toString();
  return query ? `?${query}` : "";
}

export const listAssessmentPackages = (input: AssessmentPackageListInput, signal?: AbortSignal) =>
  apiRequest<AssessmentPackageListResult>(`/api/v1/assessment/packages${toQuery(input as Record<string, string | number | boolean | undefined | null>)}`, { signal });

export const getAssessmentPackage = (packageId: string, signal?: AbortSignal) =>
  apiRequest<AssessmentPackageDetail>(`/api/v1/assessment/packages/${packageId}`, { signal });

export const createAssessmentPackage = (input: CreateAssessmentPackageInput) =>
  apiRequest<AssessmentPackageDetail>("/api/v1/assessment/packages", { method: "POST", body: input });

export const transitionAssessmentPackage = (packageId: string, input: TransitionAssessmentPackageInput) =>
  apiRequest<AssessmentPackageDetail>(`/api/v1/assessment/packages/${packageId}/transition`, { method: "POST", body: input });

export const createAssessmentNote = (packageId: string, input: CreateAssessmentNoteInput) =>
  apiRequest<AssessmentPackageNote>(`/api/v1/assessment/packages/${packageId}/notes`, { method: "POST", body: input });

export const listAssessmentFindings = (input: AssessmentFindingListInput, signal?: AbortSignal) =>
  apiRequest<AssessmentFindingListResult>(`/api/v1/assessment/findings${toQuery(input as Record<string, string | number | boolean | undefined | null>)}`, { signal });

export const getAssessmentFinding = (findingId: string, signal?: AbortSignal) =>
  apiRequest<AssessmentFindingDetail>(`/api/v1/assessment/findings/${findingId}`, { signal });

export const createAssessmentFinding = (input: CreateAssessmentFindingInput) =>
  apiRequest<AssessmentFindingDetail>("/api/v1/assessment/findings", { method: "POST", body: input });

export const transitionAssessmentFinding = (findingId: string, input: TransitionAssessmentFindingInput) =>
  apiRequest<AssessmentFindingDetail>(`/api/v1/assessment/findings/${findingId}/transition`, { method: "POST", body: input });

export const listControlCatalog = (input: ControlCatalogListInput, signal?: AbortSignal) =>
  apiRequest<ControlCatalogListResult>(`/api/v1/assessment/control-catalog${toQuery(input as Record<string, string | number | boolean | undefined | null>)}`, { signal });

export const getControlCatalogItem = (controlId: string, signal?: AbortSignal) =>
  apiRequest<ControlCatalogItem>(`/api/v1/assessment/control-catalog/${controlId}`, { signal });

export const createControlCatalogItem = (input: CreateControlCatalogItemInput) =>
  apiRequest<ControlCatalogItem>("/api/v1/assessment/control-catalog", { method: "POST", body: input });

export const updateControlCatalogItem = (controlId: string, input: UpdateControlCatalogItemInput) =>
  apiRequest<ControlCatalogItem>(`/api/v1/assessment/control-catalog/${controlId}`, { method: "PUT", body: input });

export const listControlMappings = (input: ControlMappingListInput, signal?: AbortSignal) =>
  apiRequest<ControlMappingListResult>(`/api/v1/assessment/control-mappings${toQuery(input as Record<string, string | number | boolean | undefined | null>)}`, { signal });

export const getControlMapping = (mappingId: string, signal?: AbortSignal) =>
  apiRequest<ControlMappingDetail>(`/api/v1/assessment/control-mappings/${mappingId}`, { signal });

export const createControlMapping = (input: CreateControlMappingInput) =>
  apiRequest<ControlMappingDetail>("/api/v1/assessment/control-mappings", { method: "POST", body: input });

export const transitionControlMapping = (mappingId: string, input: TransitionControlMappingInput) =>
  apiRequest<ControlMappingDetail>(`/api/v1/assessment/control-mappings/${mappingId}/transition`, { method: "POST", body: input });

export const listControlCoverage = (input: ControlCoverageListInput, signal?: AbortSignal) =>
  apiRequest<ControlCoverageListResult>(`/api/v1/assessment/control-coverage${toQuery(input as Record<string, string | number | boolean | undefined | null>)}`, { signal });
