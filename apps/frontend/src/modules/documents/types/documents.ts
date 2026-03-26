export interface DocumentListItemView {
  id: string;
  title: string;
  documentName: string;
  fileName?: string | null;
  contentType?: string | null;
  sizeBytes?: number | null;
  documentTypeName: string | null;
  projectName: string | null;
  phaseCode: string | null;
  ownerUserId: string | null;
  status: string;
  classification: string;
  retentionClass: string;
  currentVersionNumber: number | null;
  publishedRevision?: number | null;
  publishedVersionCode?: string | null;
  currentVersionStatus: string | null;
  currentFileName: string | null;
  currentFileSize: number | null;
  updatedAt: string;
}
