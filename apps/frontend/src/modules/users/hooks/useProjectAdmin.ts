import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createProject,
  createProjectAssignment,
  createProjectRole,
  deleteProject,
  deleteProjectAssignment,
  getProjectCompliance,
  exportProjectEvidence,
  deleteProjectRole,
  getProjectOrgChart,
  listProjectEvidenceAssignmentHistory,
  listProjectEvidenceRoleResponsibilities,
  listProjectEvidenceTeamRegister,
  listProjectAssignments,
  listProjectRoles,
  listProjects,
  updateProject,
  updateProjectAssignment,
  updateProjectRole,
} from "../api/usersApi";
import type {
  CreateProjectAssignmentInput,
  CreateProjectInput,
  CreateProjectRoleInput,
  ListProjectAssignmentsInput,
  ListProjectsInput,
  SoftDeleteInput,
  UpdateProjectAssignmentInput,
  UpdateProjectInput,
  UpdateProjectRoleInput,
} from "../types/users";

const projectsQueryKey = ["admin", "projects"];
const projectRolesQueryKey = ["admin", "project-roles"];
const projectAssignmentsQueryKey = ["admin", "project-assignments"];

export function useProjectAdmin(input: {
  projectsEnabled?: boolean;
  projects: ListProjectsInput;
  projectRoles: { projectId?: string; search?: string; sortBy?: string; sortOrder?: "asc" | "desc"; page?: number; pageSize?: number };
  projectAssignments: ListProjectAssignmentsInput | null;
  projectOrgChartProjectId?: string;
  projectEvidenceTeamRegister?: { projectId?: string; page?: number; pageSize?: number };
  projectEvidenceRoleResponsibilities?: { projectId?: string; page?: number; pageSize?: number };
  projectEvidenceAssignmentHistory?: { projectId?: string; page?: number; pageSize?: number };
  projectComplianceProjectId?: string;
}) {
  const queryClient = useQueryClient();

  const projectsQuery = useQuery({
    queryKey: [...projectsQueryKey, input.projects],
    enabled: input.projectsEnabled ?? true,
    queryFn: ({ signal }) => listProjects(input.projects, signal),
    staleTime: 15_000,
  });

  const projectRolesQuery = useQuery({
    queryKey: [...projectRolesQueryKey, input.projectRoles],
    enabled: Boolean(input.projectRoles.projectId),
    queryFn: ({ signal }) =>
      listProjectRoles(
        {
          page: input.projectRoles.page,
          pageSize: input.projectRoles.pageSize,
          search: input.projectRoles.search,
          sortBy: input.projectRoles.sortBy,
          sortOrder: input.projectRoles.sortOrder,
          divisionId: input.projectRoles.projectId,
        },
        signal,
      ),
    staleTime: 15_000,
  });

  const projectAssignmentsQuery = useQuery({
    queryKey: [...projectAssignmentsQueryKey, input.projectAssignments],
    enabled: Boolean(input.projectAssignments?.projectId),
    queryFn: ({ signal }) => listProjectAssignments(input.projectAssignments!, signal),
    staleTime: 15_000,
  });

  const projectOrgChartQuery = useQuery({
    queryKey: [...projectAssignmentsQueryKey, "org-chart", input.projectOrgChartProjectId],
    enabled: Boolean(input.projectOrgChartProjectId),
    queryFn: ({ signal }) => getProjectOrgChart(input.projectOrgChartProjectId!, signal),
    staleTime: 15_000,
  });

  const projectEvidenceTeamRegisterQuery = useQuery({
    queryKey: [...projectAssignmentsQueryKey, "evidence", "team-register", input.projectEvidenceTeamRegister],
    enabled: Boolean(input.projectEvidenceTeamRegister?.projectId),
    queryFn: ({ signal }) =>
      listProjectEvidenceTeamRegister(
        input.projectEvidenceTeamRegister!.projectId!,
        { page: input.projectEvidenceTeamRegister!.page, pageSize: input.projectEvidenceTeamRegister!.pageSize },
        signal,
      ),
    staleTime: 15_000,
  });

  const projectEvidenceRoleResponsibilitiesQuery = useQuery({
    queryKey: [...projectAssignmentsQueryKey, "evidence", "role-responsibilities", input.projectEvidenceRoleResponsibilities],
    enabled: Boolean(input.projectEvidenceRoleResponsibilities?.projectId),
    queryFn: ({ signal }) =>
      listProjectEvidenceRoleResponsibilities(
        input.projectEvidenceRoleResponsibilities!.projectId!,
        { page: input.projectEvidenceRoleResponsibilities!.page, pageSize: input.projectEvidenceRoleResponsibilities!.pageSize },
        signal,
      ),
    staleTime: 15_000,
  });

  const projectEvidenceAssignmentHistoryQuery = useQuery({
    queryKey: [...projectAssignmentsQueryKey, "evidence", "assignment-history", input.projectEvidenceAssignmentHistory],
    enabled: Boolean(input.projectEvidenceAssignmentHistory?.projectId),
    queryFn: ({ signal }) =>
      listProjectEvidenceAssignmentHistory(
        input.projectEvidenceAssignmentHistory!.projectId!,
        { page: input.projectEvidenceAssignmentHistory!.page, pageSize: input.projectEvidenceAssignmentHistory!.pageSize },
        signal,
      ),
    staleTime: 15_000,
  });

  const projectComplianceQuery = useQuery({
    queryKey: [...projectAssignmentsQueryKey, "compliance", input.projectComplianceProjectId],
    enabled: Boolean(input.projectComplianceProjectId),
    queryFn: ({ signal }) => getProjectCompliance(input.projectComplianceProjectId!, signal),
    staleTime: 15_000,
  });

  const invalidateProjects = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: projectsQueryKey }),
      queryClient.invalidateQueries({ queryKey: projectRolesQueryKey }),
      queryClient.invalidateQueries({ queryKey: projectAssignmentsQueryKey }),
    ]);
  };

  const createProjectMutation = useMutation({
    mutationFn: (payload: CreateProjectInput) => createProject(payload),
    onSuccess: invalidateProjects,
  });

  const updateProjectMutation = useMutation({
    mutationFn: (payload: UpdateProjectInput) => updateProject(payload),
    onSuccess: invalidateProjects,
  });

  const deleteProjectMutation = useMutation({
    mutationFn: ({ id, input: payload }: { id: string; input: SoftDeleteInput }) => deleteProject(id, payload),
    onSuccess: invalidateProjects,
  });

  const createProjectRoleMutation = useMutation({
    mutationFn: (payload: CreateProjectRoleInput) => createProjectRole(payload),
    onSuccess: invalidateProjects,
  });

  const updateProjectRoleMutation = useMutation({
    mutationFn: (payload: UpdateProjectRoleInput) => updateProjectRole(payload),
    onSuccess: invalidateProjects,
  });

  const deleteProjectRoleMutation = useMutation({
    mutationFn: ({ id, input: payload }: { id: string; input: SoftDeleteInput }) => deleteProjectRole(id, payload),
    onSuccess: invalidateProjects,
  });

  const createProjectAssignmentMutation = useMutation({
    mutationFn: (payload: CreateProjectAssignmentInput) => createProjectAssignment(payload),
    onSuccess: invalidateProjects,
  });

  const updateProjectAssignmentMutation = useMutation({
    mutationFn: (payload: UpdateProjectAssignmentInput) => updateProjectAssignment(payload),
    onSuccess: invalidateProjects,
  });

  const deleteProjectAssignmentMutation = useMutation({
    mutationFn: ({ id, input }: { id: string; input: SoftDeleteInput }) => deleteProjectAssignment(id, input),
    onSuccess: invalidateProjects,
  });

  const exportProjectEvidenceCsv = (projectId: string, signal?: AbortSignal) => exportProjectEvidence(projectId, signal);

  return {
    projectsQuery,
    projectRolesQuery,
    projectAssignmentsQuery,
    projectOrgChartQuery,
    projectEvidenceTeamRegisterQuery,
    projectEvidenceRoleResponsibilitiesQuery,
    projectEvidenceAssignmentHistoryQuery,
    projectComplianceQuery,
    createProjectMutation,
    updateProjectMutation,
    deleteProjectMutation,
    createProjectRoleMutation,
    updateProjectRoleMutation,
    deleteProjectRoleMutation,
    createProjectAssignmentMutation,
    updateProjectAssignmentMutation,
    deleteProjectAssignmentMutation,
    exportProjectEvidenceCsv,
  };
}
