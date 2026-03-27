export interface LessonLearnedItem {
  id: string;
  projectId: string;
  projectName: string;
  title: string;
  summary: string;
  lessonType: string;
  ownerUserId: string;
  status: string;
  sourceRef: string | null;
  context: string | null;
  whatHappened: string | null;
  whatToRepeat: string | null;
  whatToAvoid: string | null;
  linkedEvidence: string[];
  publishedAt: string | null;
  updatedAt: string;
}

export interface LessonLearnedListInput {
  projectId?: string;
  lessonType?: string;
  ownerUserId?: string;
  status?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateLessonLearnedInput {
  projectId: string;
  title: string;
  summary: string;
  lessonType: string;
  ownerUserId: string;
  sourceRef?: string | null;
  context?: string | null;
  whatHappened?: string | null;
  whatToRepeat?: string | null;
  whatToAvoid?: string | null;
  linkedEvidence?: string[] | null;
}

export interface UpdateLessonLearnedInput extends CreateLessonLearnedInput {
  status: string;
}

export interface PublishLessonLearnedInput {
  sourceRef?: string | null;
  context?: string | null;
  summary?: string | null;
  linkedEvidence?: string[] | null;
}
