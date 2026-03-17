export interface DocumentListItemView {
  id: string;
  documentName: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedByUserId: string | null;
  uploadedAt: string;
  versionCode: string | null;
  revision: number | null;
  publishedVersionCode: string | null;
  publishedRevision: number | null;
}
