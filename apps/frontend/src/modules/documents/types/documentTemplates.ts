export interface DocumentTemplateListItem {
  id: string;
  name: string;
  documentCount: number;
  createdAt: string;
}

export interface DocumentTemplateCreateInput {
  name: string;
  documentIds: string[];
}
