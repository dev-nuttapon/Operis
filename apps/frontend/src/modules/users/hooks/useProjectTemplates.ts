import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createProjectTypeRoleRequirement,
  createProjectTypeTemplate,
  deleteProjectTypeRoleRequirement,
  deleteProjectTypeTemplate,
  listProjectTypeRoleRequirements,
  listProjectTypeTemplates,
  updateProjectTypeRoleRequirement,
  updateProjectTypeTemplate,
} from "../api/usersApi";
import type {
  CreateProjectTypeRoleRequirementInput,
  CreateProjectTypeTemplateInput,
  SoftDeleteInput,
  UpdateProjectTypeRoleRequirementInput,
  UpdateProjectTypeTemplateInput,
} from "../types/users";

const projectTypeTemplatesQueryKey = ["admin", "project-type-templates"];
const projectTypeRoleRequirementsQueryKey = ["admin", "project-type-role-requirements"];

export function useProjectTemplates(input: {
  templates: { search?: string; sortBy?: string; sortOrder?: "asc" | "desc"; page?: number; pageSize?: number };
  roleRequirements: { templateId?: string; search?: string; sortBy?: string; sortOrder?: "asc" | "desc"; page?: number; pageSize?: number };
}) {
  const queryClient = useQueryClient();

  const templatesQuery = useQuery({
    queryKey: [...projectTypeTemplatesQueryKey, input.templates],
    queryFn: ({ signal }) => listProjectTypeTemplates(input.templates, signal),
    staleTime: 15_000,
  });

  const roleRequirementsQuery = useQuery({
    queryKey: [...projectTypeRoleRequirementsQueryKey, input.roleRequirements],
    enabled: Boolean(input.roleRequirements.templateId),
    queryFn: ({ signal }) =>
      listProjectTypeRoleRequirements(
        {
          templateId: input.roleRequirements.templateId!,
          page: input.roleRequirements.page,
          pageSize: input.roleRequirements.pageSize,
          search: input.roleRequirements.search,
          sortBy: input.roleRequirements.sortBy,
          sortOrder: input.roleRequirements.sortOrder,
        },
        signal,
      ),
    staleTime: 15_000,
  });

  const invalidate = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: projectTypeTemplatesQueryKey }),
      queryClient.invalidateQueries({ queryKey: projectTypeRoleRequirementsQueryKey }),
    ]);
  };

  return {
    templatesQuery,
    roleRequirementsQuery,
    createTemplateMutation: useMutation({ mutationFn: (payload: CreateProjectTypeTemplateInput) => createProjectTypeTemplate(payload), onSuccess: invalidate }),
    updateTemplateMutation: useMutation({ mutationFn: (payload: UpdateProjectTypeTemplateInput) => updateProjectTypeTemplate(payload), onSuccess: invalidate }),
    deleteTemplateMutation: useMutation({ mutationFn: ({ id, input: payload }: { id: string; input: SoftDeleteInput }) => deleteProjectTypeTemplate(id, payload), onSuccess: invalidate }),
    createRoleRequirementMutation: useMutation({ mutationFn: (payload: CreateProjectTypeRoleRequirementInput) => createProjectTypeRoleRequirement(payload), onSuccess: invalidate }),
    updateRoleRequirementMutation: useMutation({ mutationFn: (payload: UpdateProjectTypeRoleRequirementInput) => updateProjectTypeRoleRequirement(payload), onSuccess: invalidate }),
    deleteRoleRequirementMutation: useMutation({ mutationFn: ({ id, input: payload }: { id: string; input: SoftDeleteInput }) => deleteProjectTypeRoleRequirement(id, payload), onSuccess: invalidate }),
  };
}
