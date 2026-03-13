import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createProject,
  createProjectAssignment,
  createProjectRole,
  deleteProject,
  deleteProjectAssignment,
  getProjectCompliance,
  getProjectEvidence,
  exportProjectEvidence,
  deleteProjectRole,
  getProjectOrgChart,
  listProjectAssignments,
  listProjectRoles,
  listProjects,
  listUsers,
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
const projectMemberUsersQueryKey = ["admin", "project-member-users"];

export function useProjectAdmin(input: {
  projects: ListProjectsInput;
  projectRoles: { projectId?: string; search?: string; sortBy?: string; sortOrder?: "asc" | "desc"; page?: number; pageSize?: number };
  projectAssignments: ListProjectAssignmentsInput | null;
  projectOrgChartProjectId?: string;
  projectEvidenceProjectId?: string;
  projectComplianceProjectId?: string;
}) {
  const queryClient = useQueryClient();

  const projectsQuery = useQuery({
    queryKey: [...projectsQueryKey, input.projects],
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

  const projectEvidenceQuery = useQuery({
    queryKey: [...projectAssignmentsQueryKey, "evidence", input.projectEvidenceProjectId],
    enabled: Boolean(input.projectEvidenceProjectId),
    queryFn: ({ signal }) => getProjectEvidence(input.projectEvidenceProjectId!, signal),
    staleTime: 15_000,
  });

  const projectComplianceQuery = useQuery({
    queryKey: [...projectAssignmentsQueryKey, "compliance", input.projectComplianceProjectId],
    enabled: Boolean(input.projectComplianceProjectId),
    queryFn: ({ signal }) => getProjectCompliance(input.projectComplianceProjectId!, signal),
    staleTime: 15_000,
  });

  const projectMemberUsersQuery = useQuery({
    queryKey: projectMemberUsersQueryKey,
    queryFn: ({ signal }) => listUsers({ page: 1, pageSize: 100, sortBy: "createdAt", sortOrder: "desc" }, signal),
    staleTime: 5 * 60_000,
  });

  const invalidateProjects = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: projectsQueryKey }),
      queryClient.invalidateQueries({ queryKey: projectRolesQueryKey }),
      queryClient.invalidateQueries({ queryKey: projectAssignmentsQueryKey }),
      queryClient.invalidateQueries({ queryKey: projectMemberUsersQueryKey }),
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
    projectEvidenceQuery,
    projectComplianceQuery,
    projectMemberUsersQuery,
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
