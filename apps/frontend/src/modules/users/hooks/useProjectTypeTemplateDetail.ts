import { useQuery } from "@tanstack/react-query";
import { getProjectTypeTemplate } from "../api/usersApi";

const projectTypeTemplateDetailQueryKey = ["admin", "project-type-templates", "detail"];

export function useProjectTypeTemplateDetail(templateId?: string) {
  return useQuery({
    queryKey: [...projectTypeTemplateDetailQueryKey, templateId],
    enabled: Boolean(templateId),
    queryFn: ({ signal }) => getProjectTypeTemplate(templateId!, signal),
    staleTime: 15_000,
  });
}
