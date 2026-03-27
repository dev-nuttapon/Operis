export interface TrainingCourseItem {
  id: string;
  courseCode: string | null;
  title: string;
  description: string | null;
  provider: string | null;
  deliveryMode: string | null;
  audienceScope: string | null;
  validityMonths: number;
  status: string;
  requirementCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface RoleTrainingRequirementItem {
  id: string;
  courseId: string;
  courseTitle: string;
  courseCode: string | null;
  courseStatus: string;
  projectRoleId: string;
  projectRoleName: string;
  projectId: string | null;
  projectName: string | null;
  requiredWithinDays: number;
  renewalIntervalMonths: number;
  status: string;
  notes: string | null;
  assignedUserCount: number;
  overdueUserCount: number;
  expiredUserCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface TrainingCompletionItem {
  id: string;
  courseId: string;
  courseTitle: string;
  courseCode: string | null;
  projectRoleId: string;
  projectRoleName: string;
  projectId: string;
  projectName: string;
  userId: string;
  status: string;
  isOverdue: boolean;
  isExpired: boolean;
  assignedAt: string;
  dueAt: string | null;
  completionDate: string | null;
  expiryDate: string | null;
  evidenceRef: string | null;
  notes: string | null;
  updatedAt: string;
}

export interface CompetencyReviewItem {
  id: string;
  userId: string;
  projectId: string | null;
  projectName: string | null;
  reviewPeriod: string;
  reviewerUserId: string;
  status: string;
  summary: string | null;
  plannedAt: string;
  completedAt: string | null;
  updatedAt: string;
}

export interface ProjectRoleOption {
  id: string;
  projectId: string | null;
  projectName: string | null;
  name: string;
  status: string;
}

export interface TrainingCourseListInput {
  search?: string;
  status?: string;
  page?: number;
  pageSize?: number;
}

export interface RoleTrainingMatrixInput {
  projectId?: string;
  projectRoleId?: string;
  courseId?: string;
  status?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface TrainingCompletionListInput {
  projectId?: string;
  projectRoleId?: string;
  courseId?: string;
  userId?: string;
  status?: string;
  onlyOverdue?: boolean;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface CompetencyReviewListInput {
  projectId?: string;
  userId?: string;
  status?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateTrainingCourseInput {
  courseCode?: string | null;
  title: string;
  description?: string | null;
  provider?: string | null;
  deliveryMode?: string | null;
  audienceScope?: string | null;
  validityMonths: number;
}

export interface UpdateTrainingCourseInput extends CreateTrainingCourseInput {}

export interface TransitionTrainingCourseInput {
  targetStatus: string;
  reason?: string | null;
}

export interface CreateRoleTrainingRequirementInput {
  courseId: string;
  projectRoleId: string;
  requiredWithinDays: number;
  renewalIntervalMonths: number;
  notes?: string | null;
}

export interface UpdateRoleTrainingRequirementInput extends CreateRoleTrainingRequirementInput {
  status: string;
}

export interface RecordTrainingCompletionInput {
  courseId: string;
  projectRoleId: string;
  projectId: string;
  userId: string;
  status: string;
  assignedAt?: string | null;
  dueAt?: string | null;
  completionDate?: string | null;
  evidenceRef?: string | null;
  notes?: string | null;
}

export interface UpdateTrainingCompletionInput {
  status: string;
  dueAt?: string | null;
  completionDate?: string | null;
  evidenceRef?: string | null;
  notes?: string | null;
}

export interface CreateCompetencyReviewInput {
  userId: string;
  projectId?: string | null;
  reviewPeriod: string;
  reviewerUserId: string;
  plannedAt: string;
  summary?: string | null;
}

export interface UpdateCompetencyReviewInput {
  reviewPeriod: string;
  reviewerUserId: string;
  status: string;
  plannedAt: string;
  completedAt?: string | null;
  summary?: string | null;
}
