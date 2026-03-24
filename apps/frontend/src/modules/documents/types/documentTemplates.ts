export interface DocumentTemplateListItem {
  id: string;
  name: string;
  documentCount: number;
  createdAt: string;
}

export interface DocumentTemplateDetail {
  id: string;
  name: string;
  documentIds: string[];
  items: DocumentTemplateItemDetail[];
  createdAt: string;
}

export interface DocumentTemplateItemDetail {
  documentId: string;
  documentVersionId: string | null;
  documentName: string | null;
  versionCode: string | null;
  revision: number | null;
}

export interface DocumentTemplateCreateInput {
  name: string;
  documentIds: string[];
}

export interface DocumentTemplateHistoryItem {
  id: string;
  templateId: string;
  eventType: string;
  summary: string | null;
  reason: string | null;
  actorUserId: string | null;
  actorEmail: string | null;
  actorDisplayName: string | null;
  beforeJson: string | null;
  afterJson: string | null;
  metadataJson: string | null;
  occurredAt: string;
}
