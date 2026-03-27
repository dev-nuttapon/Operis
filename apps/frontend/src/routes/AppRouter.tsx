import { lazy, Suspense } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { Flex, Spin } from "antd";
import { ProtectedRoute } from "../shared/components/ProtectedRoute";
import { AuthorizedRoute } from "../shared/components/AuthorizedRoute";
import { MainLayout } from "../shared/components/layouts/MainLayout";
import { AppErrorBoundary } from "../shared/components/AppErrorBoundary";
import { permissions } from "../shared/authz/permissions";

const DocumentDashboardPage = lazy(() =>
  import("../modules/documents/pages/DocumentDashboardPage").then((module) => ({ default: module.DocumentDashboardPage }))
);
const DocumentUploadPage = lazy(() =>
  import("../modules/documents/pages/DocumentUploadPage").then((module) => ({ default: module.DocumentUploadPage }))
);
const DocumentVersionUploadPage = lazy(() =>
  import("../modules/documents/pages/DocumentVersionUploadPage").then((module) => ({ default: module.DocumentVersionUploadPage }))
);
const DocumentVersionsPage = lazy(() =>
  import("../modules/documents/pages/DocumentVersionsPage").then((module) => ({ default: module.DocumentVersionsPage }))
);
const DocumentTypeSetupPage = lazy(() =>
  import("../modules/documents/pages/DocumentTypeSetupPage").then((module) => ({ default: module.DocumentTypeSetupPage }))
);
const DocumentHistoryPage = lazy(() =>
  import("../modules/documents/pages/DocumentHistoryPage").then((module) => ({ default: module.DocumentHistoryPage }))
);
const DocumentTemplatesPage = lazy(() =>
  import("../modules/documents/pages/DocumentTemplatesPage").then((module) => ({ default: module.DocumentTemplatesPage }))
);
const DocumentTemplateCreatePage = lazy(() =>
  import("../modules/documents/pages/DocumentTemplateCreatePage").then((module) => ({ default: module.DocumentTemplateCreatePage }))
);
const DocumentTemplateEditPage = lazy(() =>
  import("../modules/documents/pages/DocumentTemplateEditPage").then((module) => ({ default: module.DocumentTemplateEditPage }))
);
const DocumentTemplateHistoryPage = lazy(() =>
  import("../modules/documents/pages/DocumentTemplateHistoryPage").then((module) => ({ default: module.DocumentTemplateHistoryPage }))
);
const LoginPage = lazy(() =>
  import("../modules/auth/pages/LoginPage").then((module) => ({ default: module.LoginPage }))
);
const AdminUsersPage = lazy(() =>
  import("../modules/users/pages/AdminUsersPage").then((module) => ({ default: module.AdminUsersPage }))
);
const AdminUserCreatePage = lazy(() =>
  import("../modules/users/pages/AdminUserCreatePage").then((module) => ({ default: module.AdminUserCreatePage }))
);
const AdminUserEditPage = lazy(() =>
  import("../modules/users/pages/AdminUserEditPage").then((module) => ({ default: module.AdminUserEditPage }))
);
const AdminSettingsPage = lazy(() =>
  import("../modules/users/pages/AdminSettingsPage").then((module) => ({ default: module.AdminSettingsPage }))
);
const PermissionMatrixPage = lazy(() =>
  import("../modules/users/pages/PermissionMatrixPage").then((module) => ({ default: module.PermissionMatrixPage }))
);
const ProcessLibraryPage = lazy(() =>
  import("../modules/governance/pages/ProcessLibraryPage").then((module) => ({ default: module.ProcessLibraryPage }))
);
const QaReviewChecklistPage = lazy(() =>
  import("../modules/governance/pages/QaReviewChecklistPage").then((module) => ({ default: module.QaReviewChecklistPage }))
);
const ProjectPlanPage = lazy(() =>
  import("../modules/governance/pages/ProjectPlanPage").then((module) => ({ default: module.ProjectPlanPage }))
);
const StakeholderRegisterPage = lazy(() =>
  import("../modules/governance/pages/StakeholderRegisterPage").then((module) => ({ default: module.StakeholderRegisterPage }))
);
const TailoringRecordPage = lazy(() =>
  import("../modules/governance/pages/TailoringRecordPage").then((module) => ({ default: module.TailoringRecordPage }))
);
const RaciMapPage = lazy(() =>
  import("../modules/governance/pages/RaciMapPage").then((module) => ({ default: module.RaciMapPage }))
);
const ApprovalEvidenceLogPage = lazy(() =>
  import("../modules/governance/pages/ApprovalEvidenceLogPage").then((module) => ({ default: module.ApprovalEvidenceLogPage }))
);
const WorkflowOverrideLogPage = lazy(() =>
  import("../modules/governance/pages/WorkflowOverrideLogPage").then((module) => ({ default: module.WorkflowOverrideLogPage }))
);
const SlaEscalationRulesPage = lazy(() =>
  import("../modules/governance/pages/SlaEscalationRulesPage").then((module) => ({ default: module.SlaEscalationRulesPage }))
);
const RetentionPolicyPage = lazy(() =>
  import("../modules/governance/pages/RetentionPolicyPage").then((module) => ({ default: module.RetentionPolicyPage }))
);
const ArchitectureRegisterPage = lazy(() =>
  import("../modules/governance/pages/ArchitectureRegisterPage").then((module) => ({ default: module.ArchitectureRegisterPage }))
);
const DesignReviewPage = lazy(() =>
  import("../modules/governance/pages/DesignReviewPage").then((module) => ({ default: module.DesignReviewPage }))
);
const IntegrationReviewPage = lazy(() =>
  import("../modules/governance/pages/IntegrationReviewPage").then((module) => ({ default: module.IntegrationReviewPage }))
);
const ComplianceDashboardPage = lazy(() =>
  import("../modules/governance/pages/ComplianceDashboardPage").then((module) => ({ default: module.ComplianceDashboardPage }))
);
const ManagementReviewPage = lazy(() =>
  import("../modules/governance/pages/ManagementReviewPage").then((module) => ({ default: module.ManagementReviewPage }))
);
const ManagementReviewDetailPage = lazy(() =>
  import("../modules/governance/pages/ManagementReviewDetailPage").then((module) => ({ default: module.ManagementReviewDetailPage }))
);
const TrainingCatalogPage = lazy(() =>
  import("../modules/learning/pages/TrainingCatalogPage").then((module) => ({ default: module.TrainingCatalogPage }))
);
const RoleTrainingMatrixPage = lazy(() =>
  import("../modules/learning/pages/RoleTrainingMatrixPage").then((module) => ({ default: module.RoleTrainingMatrixPage }))
);
const TrainingCompletionsPage = lazy(() =>
  import("../modules/learning/pages/TrainingCompletionsPage").then((module) => ({ default: module.TrainingCompletionsPage }))
);
const RequirementRegisterPage = lazy(() =>
  import("../modules/requirements/pages/RequirementRegisterPage").then((module) => ({ default: module.RequirementRegisterPage }))
);
const RequirementDetailPage = lazy(() =>
  import("../modules/requirements/pages/RequirementDetailPage").then((module) => ({ default: module.RequirementDetailPage }))
);
const RequirementBaselinesPage = lazy(() =>
  import("../modules/requirements/pages/RequirementBaselinesPage").then((module) => ({ default: module.RequirementBaselinesPage }))
);
const TraceabilityMatrixPage = lazy(() =>
  import("../modules/requirements/pages/TraceabilityMatrixPage").then((module) => ({ default: module.TraceabilityMatrixPage }))
);
const ChangeRequestRegisterPage = lazy(() =>
  import("../modules/change-control/pages/ChangeRequestRegisterPage").then((module) => ({ default: module.ChangeRequestRegisterPage }))
);
const ChangeRequestDetailPage = lazy(() =>
  import("../modules/change-control/pages/ChangeRequestDetailPage").then((module) => ({ default: module.ChangeRequestDetailPage }))
);
const ConfigurationItemsPage = lazy(() =>
  import("../modules/change-control/pages/ConfigurationItemsPage").then((module) => ({ default: module.ConfigurationItemsPage }))
);
const BaselineRegistryPage = lazy(() =>
  import("../modules/change-control/pages/BaselineRegistryPage").then((module) => ({ default: module.BaselineRegistryPage }))
);
const RiskRegisterPage = lazy(() =>
  import("../modules/risks/pages/RiskRegisterPage").then((module) => ({ default: module.RiskRegisterPage }))
);
const RiskDetailPage = lazy(() =>
  import("../modules/risks/pages/RiskDetailPage").then((module) => ({ default: module.RiskDetailPage }))
);
const IssueLogPage = lazy(() =>
  import("../modules/risks/pages/IssueLogPage").then((module) => ({ default: module.IssueLogPage }))
);
const IssueDetailPage = lazy(() =>
  import("../modules/risks/pages/IssueDetailPage").then((module) => ({ default: module.IssueDetailPage }))
);
const MeetingRegisterPage = lazy(() =>
  import("../modules/meetings/pages/MeetingRegisterPage").then((module) => ({ default: module.MeetingRegisterPage }))
);
const MeetingDetailPage = lazy(() =>
  import("../modules/meetings/pages/MeetingDetailPage").then((module) => ({ default: module.MeetingDetailPage }))
);
const DecisionLogPage = lazy(() =>
  import("../modules/meetings/pages/DecisionLogPage").then((module) => ({ default: module.DecisionLogPage }))
);
const DecisionDetailPage = lazy(() =>
  import("../modules/meetings/pages/DecisionDetailPage").then((module) => ({ default: module.DecisionDetailPage }))
);
const TestPlanPage = lazy(() =>
  import("../modules/verification/pages/TestPlanPage").then((module) => ({ default: module.TestPlanPage }))
);
const TestCaseExecutionPage = lazy(() =>
  import("../modules/verification/pages/TestCaseExecutionPage").then((module) => ({ default: module.TestCaseExecutionPage }))
);
const UatSignoffPage = lazy(() =>
  import("../modules/verification/pages/UatSignoffPage").then((module) => ({ default: module.UatSignoffPage }))
);
const AuditLogsPage = lazy(() =>
  import("../modules/audits/pages/AuditLogsPage").then((module) => ({ default: module.AuditLogsPage }))
);
const EvidenceExportsPage = lazy(() =>
  import("../modules/audits/pages/EvidenceExportsPage").then((module) => ({ default: module.EvidenceExportsPage }))
);
const AuditPlansPage = lazy(() =>
  import("../modules/audits/pages/AuditPlansPage").then((module) => ({ default: module.AuditPlansPage }))
);
const EvidenceCompletenessPage = lazy(() =>
  import("../modules/audits/pages/EvidenceCompletenessPage").then((module) => ({ default: module.EvidenceCompletenessPage }))
);
const EvidenceCompletenessDetailPage = lazy(() =>
  import("../modules/audits/pages/EvidenceCompletenessDetailPage").then((module) => ({ default: module.EvidenceCompletenessDetailPage }))
);
const MetricDefinitionsPage = lazy(() =>
  import("../modules/metrics/pages/MetricDefinitionsPage").then((module) => ({ default: module.MetricDefinitionsPage }))
);
const MetricsDashboardPage = lazy(() =>
  import("../modules/metrics/pages/MetricsDashboardPage").then((module) => ({ default: module.MetricsDashboardPage }))
);
const QualityGatesPage = lazy(() =>
  import("../modules/metrics/pages/QualityGatesPage").then((module) => ({ default: module.QualityGatesPage }))
);
const MetricReviewsPage = lazy(() =>
  import("../modules/metrics/pages/MetricReviewsPage").then((module) => ({ default: module.MetricReviewsPage }))
);
const TrendReportsPage = lazy(() =>
  import("../modules/metrics/pages/TrendReportsPage").then((module) => ({ default: module.TrendReportsPage }))
);
const PerformanceBaselinePage = lazy(() =>
  import("../modules/metrics/pages/PerformanceBaselinePage").then((module) => ({ default: module.PerformanceBaselinePage }))
);
const CapacityReviewPage = lazy(() =>
  import("../modules/metrics/pages/CapacityReviewPage").then((module) => ({ default: module.CapacityReviewPage }))
);
const SlowOperationReviewPage = lazy(() =>
  import("../modules/metrics/pages/SlowOperationReviewPage").then((module) => ({ default: module.SlowOperationReviewPage }))
);
const PerformanceRegressionGatePage = lazy(() =>
  import("../modules/metrics/pages/PerformanceRegressionGatePage").then((module) => ({ default: module.PerformanceRegressionGatePage }))
);
const ReleaseRegisterPage = lazy(() =>
  import("../modules/releases/pages/ReleaseRegisterPage").then((module) => ({ default: module.ReleaseRegisterPage }))
);
const DeploymentChecklistPage = lazy(() =>
  import("../modules/releases/pages/DeploymentChecklistPage").then((module) => ({ default: module.DeploymentChecklistPage }))
);
const ReleaseNotesPage = lazy(() =>
  import("../modules/releases/pages/ReleaseNotesPage").then((module) => ({ default: module.ReleaseNotesPage }))
);
const DefectLogPage = lazy(() =>
  import("../modules/defects/pages/DefectLogPage").then((module) => ({ default: module.DefectLogPage }))
);
const DefectDetailPage = lazy(() =>
  import("../modules/defects/pages/DefectDetailPage").then((module) => ({ default: module.DefectDetailPage }))
);
const NonConformanceLogPage = lazy(() =>
  import("../modules/defects/pages/NonConformanceLogPage").then((module) => ({ default: module.NonConformanceLogPage }))
);
const NonConformanceDetailPage = lazy(() =>
  import("../modules/defects/pages/NonConformanceDetailPage").then((module) => ({ default: module.NonConformanceDetailPage }))
);
const AccessReviewsPage = lazy(() =>
  import("../modules/operations/pages/AccessReviewsPage").then((module) => ({ default: module.AccessReviewsPage }))
);
const AccessRecertificationsPage = lazy(() =>
  import("../modules/operations/pages/AccessRecertificationsPage").then((module) => ({ default: module.AccessRecertificationsPage }))
);
const SecurityReviewsPage = lazy(() =>
  import("../modules/operations/pages/SecurityReviewsPage").then((module) => ({ default: module.SecurityReviewsPage }))
);
const ExternalDependenciesPage = lazy(() =>
  import("../modules/operations/pages/ExternalDependenciesPage").then((module) => ({ default: module.ExternalDependenciesPage }))
);
const ConfigurationAuditsPage = lazy(() =>
  import("../modules/operations/pages/ConfigurationAuditsPage").then((module) => ({ default: module.ConfigurationAuditsPage }))
);
const SupplierRegisterPage = lazy(() =>
  import("../modules/operations/pages/SupplierRegisterPage").then((module) => ({ default: module.SupplierRegisterPage }))
);
const SupplierAgreementsPage = lazy(() =>
  import("../modules/operations/pages/SupplierAgreementsPage").then((module) => ({ default: module.SupplierAgreementsPage }))
);
const SecurityIncidentRegisterPage = lazy(() =>
  import("../modules/operations/pages/SecurityIncidentRegisterPage").then((module) => ({ default: module.SecurityIncidentRegisterPage }))
);
const VulnerabilityRegisterPage = lazy(() =>
  import("../modules/operations/pages/VulnerabilityRegisterPage").then((module) => ({ default: module.VulnerabilityRegisterPage }))
);
const SecretRotationRegisterPage = lazy(() =>
  import("../modules/operations/pages/SecretRotationRegisterPage").then((module) => ({ default: module.SecretRotationRegisterPage }))
);
const PrivilegedAccessLogPage = lazy(() =>
  import("../modules/operations/pages/PrivilegedAccessLogPage").then((module) => ({ default: module.PrivilegedAccessLogPage }))
);
const ClassificationPolicyPage = lazy(() =>
  import("../modules/operations/pages/ClassificationPolicyPage").then((module) => ({ default: module.ClassificationPolicyPage }))
);
const BackupEvidencePage = lazy(() =>
  import("../modules/operations/pages/BackupEvidencePage").then((module) => ({ default: module.BackupEvidencePage }))
);
const RestoreVerificationPage = lazy(() =>
  import("../modules/operations/pages/RestoreVerificationPage").then((module) => ({ default: module.RestoreVerificationPage }))
);
const DrDrillLogPage = lazy(() =>
  import("../modules/operations/pages/DrDrillLogPage").then((module) => ({ default: module.DrDrillLogPage }))
);
const LegalHoldRegisterPage = lazy(() =>
  import("../modules/operations/pages/LegalHoldRegisterPage").then((module) => ({ default: module.LegalHoldRegisterPage }))
);
const LessonsLearnedPage = lazy(() =>
  import("../modules/knowledge/pages/LessonsLearnedPage").then((module) => ({ default: module.LessonsLearnedPage }))
);
const InvitationAcceptPage = lazy(() =>
  import("../modules/users/pages/InvitationAcceptPage").then((module) => ({ default: module.InvitationAcceptPage }))
);
const PublicRegistrationPage = lazy(() =>
  import("../modules/users/pages/PublicRegistrationPage").then((module) => ({ default: module.PublicRegistrationPage }))
);
const RegistrationPasswordSetupPage = lazy(() =>
  import("../modules/users/pages/RegistrationPasswordSetupPage").then((module) => ({ default: module.RegistrationPasswordSetupPage }))
);
const ProjectsPage = lazy(() =>
  import("../modules/users/pages/ProjectsPage").then((module) => ({ default: module.ProjectsPage }))
);
const ProjectCreatePage = lazy(() =>
  import("../modules/users/pages/ProjectCreatePage").then((module) => ({ default: module.ProjectCreatePage }))
);
const ProjectEditPage = lazy(() =>
  import("../modules/users/pages/ProjectEditPage").then((module) => ({ default: module.ProjectEditPage }))
);
const ProjectHistoryPage = lazy(() =>
  import("../modules/users/pages/ProjectHistoryPage").then((module) => ({ default: module.ProjectHistoryPage }))
);
const ProjectRolesPage = lazy(() =>
  import("../modules/users/pages/ProjectRolesPage").then((module) => ({ default: module.ProjectRolesPage }))
);
const ProjectRoleCreatePage = lazy(() =>
  import("../modules/users/pages/ProjectRoleCreatePage").then((module) => ({ default: module.ProjectRoleCreatePage }))
);
const ProjectRoleEditPage = lazy(() =>
  import("../modules/users/pages/ProjectRoleEditPage").then((module) => ({ default: module.ProjectRoleEditPage }))
);
const ProjectMembersPage = lazy(() =>
  import("../modules/users/pages/ProjectMembersPage").then((module) => ({ default: module.ProjectMembersPage }))
);
const ProjectMemberCreatePage = lazy(() =>
  import("../modules/users/pages/ProjectMemberCreatePage").then((module) => ({ default: module.ProjectMemberCreatePage }))
);
const ProjectMemberEditPage = lazy(() =>
  import("../modules/users/pages/ProjectMemberEditPage").then((module) => ({ default: module.ProjectMemberEditPage }))
);
const ProjectOrgChartPage = lazy(() =>
  import("../modules/users/pages/ProjectOrgChartPage").then((module) => ({ default: module.ProjectOrgChartPage }))
);
const ProjectPhaseApprovalsPage = lazy(() =>
  import("../modules/users/pages/ProjectPhaseApprovalsPage").then((module) => ({ default: module.ProjectPhaseApprovalsPage }))
);
const MasterDataCatalogPage = lazy(() =>
  import("../modules/users/pages/MasterDataCatalogPage").then((module) => ({ default: module.MasterDataCatalogPage }))
);
const ProjectWorkspacePrototypePage = lazy(() =>
  import("../modules/users/pages/ProjectWorkspacePrototypePage").then((module) => ({ default: module.ProjectWorkspacePrototypePage }))
);
const UserProfilePage = lazy(() =>
  import("../modules/users/pages/UserProfilePage").then((module) => ({ default: module.UserProfilePage }))
);
const ChangePasswordPage = lazy(() =>
  import("../modules/users/pages/ChangePasswordPage").then((module) => ({ default: module.ChangePasswordPage }))
);
const ActivityLogsPage = lazy(() =>
  import("../modules/activities/pages/ActivityLogsPage").then((module) => ({ default: module.ActivityLogsPage }))
);
const WorkflowDefinitionsPage = lazy(() =>
  import("../modules/workflows/pages/WorkflowDefinitionsPage").then((module) => ({ default: module.WorkflowDefinitionsPage }))
);
const WorkflowDefinitionCreatePage = lazy(() =>
  import("../modules/workflows/pages/WorkflowDefinitionCreatePage").then((module) => ({ default: module.WorkflowDefinitionCreatePage }))
);
const WorkflowDefinitionEditPage = lazy(() =>
  import("../modules/workflows/pages/WorkflowDefinitionEditPage").then((module) => ({ default: module.WorkflowDefinitionEditPage }))
);
const WorkflowTasksPage = lazy(() =>
  import("../modules/workflows/pages/WorkflowTasksPage").then((module) => ({ default: module.WorkflowTasksPage }))
);
const WorkflowTaskDetailPage = lazy(() =>
  import("../modules/workflows/pages/WorkflowTaskDetailPage").then((module) => ({ default: module.WorkflowTaskDetailPage }))
);
const WorkflowTaskUploadPage = lazy(() =>
  import("../modules/workflows/pages/WorkflowTaskUploadPage").then((module) => ({ default: module.WorkflowTaskUploadPage }))
);
const WorkspaceProjectsPage = lazy(() =>
  import("../modules/workflows/pages/WorkspaceProjectsPage").then((module) => ({ default: module.WorkspaceProjectsPage }))
);
const NotificationsPage = lazy(() =>
  import("../modules/notifications/pages/NotificationsPage").then((module) => ({ default: module.NotificationsPage }))
);
const NotificationQueuePage = lazy(() =>
  import("../modules/notifications/pages/NotificationQueuePage").then((module) => ({ default: module.NotificationQueuePage }))
);
const DataCollectionSchedulePage = lazy(() =>
  import("../modules/metrics/pages/DataCollectionSchedulePage").then((module) => ({ default: module.DataCollectionSchedulePage }))
);
const ChangeLogPage = lazy(() =>
  import("../modules/change-control/pages/ChangeLogPage").then((module) => ({ default: module.ChangeLogPage }))
);
const CapaRegisterPage = lazy(() =>
  import("../modules/operations/pages/CapaRegisterPage").then((module) => ({ default: module.CapaRegisterPage }))
);
const EscalationHistoryPage = lazy(() =>
  import("../modules/operations/pages/EscalationHistoryPage").then((module) => ({ default: module.EscalationHistoryPage }))
);

function RouteFallback() {
  return (
    <Flex align="center" justify="center" style={{ minHeight: "100vh" }}>
      <Spin size="large" />
    </Flex>
  );
}

export function AppRouter() {
  return (
    <BrowserRouter>
      <Suspense fallback={<RouteFallback />}>
        <AppErrorBoundary>
          <Routes>
            <Route path="/" element={<Navigate to="/login" replace />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<PublicRegistrationPage />} />
            <Route path="/register/setup-password/:token" element={<RegistrationPasswordSetupPage />} />
            <Route path="/invite/:token" element={<InvitationAcceptPage />} />

            <Route path="/app" element={<ProtectedRoute />}>
              <Route element={<MainLayout />}>
                <Route index element={<Navigate to="documents" replace />} />
                <Route path="profile" element={<UserProfilePage />} />
                <Route path="change-password" element={<ChangePasswordPage />} />
                <Route path="documents" element={<AuthorizedRoute anyOf={[permissions.documents.read, permissions.documents.upload, permissions.documents.manageVersions, permissions.documents.publish]}><DocumentDashboardPage /></AuthorizedRoute>} />
                <Route path="documents/types" element={<AuthorizedRoute anyOf={[permissions.documents.read, permissions.documents.deactivate]}><DocumentTypeSetupPage /></AuthorizedRoute>} />
                <Route path="documents/templates" element={<AuthorizedRoute anyOf={[permissions.documents.read, permissions.documents.upload]}><DocumentTemplatesPage /></AuthorizedRoute>} />
                <Route path="document-templates" element={<DocumentTemplatesPage />} />
                <Route path="document-templates/new" element={<DocumentTemplateCreatePage />} />
                <Route path="document-templates/:templateId/edit" element={<DocumentTemplateEditPage />} />
                <Route path="document-templates/:templateId/history" element={<DocumentTemplateHistoryPage />} />
                <Route path="documents/new" element={<AuthorizedRoute permission={permissions.documents.upload}><DocumentUploadPage /></AuthorizedRoute>} />
                <Route path="documents/:documentId" element={<AuthorizedRoute anyOf={[permissions.documents.read, permissions.documents.manageVersions, permissions.documents.publish]}><DocumentVersionsPage /></AuthorizedRoute>} />
                <Route path="documents/:documentId/versions" element={<AuthorizedRoute anyOf={[permissions.documents.read, permissions.documents.manageVersions, permissions.documents.publish]}><DocumentVersionsPage /></AuthorizedRoute>} />
                <Route path="documents/:documentId/history" element={<AuthorizedRoute permission={permissions.activityLogs.read}><DocumentHistoryPage /></AuthorizedRoute>} />
                <Route path="documents/:documentId/versions/new" element={<AuthorizedRoute permission={permissions.documents.manageVersions}><DocumentVersionUploadPage /></AuthorizedRoute>} />
                <Route path="projects" element={<AuthorizedRoute anyOf={[permissions.projects.read, permissions.projects.manage]}><ProjectsPage /></AuthorizedRoute>} />
                <Route path="projects/new" element={<AuthorizedRoute permission={permissions.projects.manage}><ProjectCreatePage /></AuthorizedRoute>} />
                <Route path="projects/:projectId" element={<AuthorizedRoute anyOf={[permissions.projects.read, permissions.projects.manage]}><ProjectWorkspacePrototypePage /></AuthorizedRoute>} />
                <Route path="projects/:projectId/team-assignment" element={<AuthorizedRoute anyOf={[permissions.projects.read, permissions.projects.manageMembers]}><ProjectMembersPage /></AuthorizedRoute>} />
                <Route path="projects/:projectId/edit" element={<AuthorizedRoute permission={permissions.projects.manage}><ProjectEditPage /></AuthorizedRoute>} />
                <Route path="projects/:projectId/history" element={<AuthorizedRoute permission={permissions.activityLogs.read}><ProjectHistoryPage /></AuthorizedRoute>} />
              <Route path="steps" element={<WorkflowDefinitionsPage />} />
              <Route path="steps/new" element={<WorkflowDefinitionCreatePage />} />
              <Route path="steps/:workflowDefinitionId/edit" element={<WorkflowDefinitionEditPage />} />
              <Route path="workspace" element={<WorkspaceProjectsPage />} />
              <Route path="workspace/:projectId" element={<WorkflowTasksPage />} />
              <Route path="workspace/:projectId/tasks/:workflowInstanceStepId" element={<WorkflowTaskDetailPage />} />
              <Route
                path="workspace/:projectId/tasks/:workflowInstanceStepId/upload"
                element={<WorkflowTaskUploadPage />}
              />
              <Route path="notifications" element={<NotificationsPage />} />
              <Route path="notifications/queue" element={<AuthorizedRoute anyOf={[permissions.notifications.read, permissions.notifications.manage]}><NotificationQueuePage /></AuthorizedRoute>} />
              <Route path="admin/users" element={<AuthorizedRoute permission={permissions.users.read}><AdminUsersPage /></AuthorizedRoute>} />
              <Route path="admin/users/new" element={<AuthorizedRoute permission={permissions.users.create}><AdminUserCreatePage /></AuthorizedRoute>} />
              <Route path="admin/users/:userId/edit" element={<AuthorizedRoute permission={permissions.users.update}><AdminUserEditPage /></AuthorizedRoute>} />
              <Route path="admin/permissions" element={<AuthorizedRoute permission={permissions.admin.permissionMatrixRead}><PermissionMatrixPage /></AuthorizedRoute>} />
              <Route path="admin/settings" element={<AuthorizedRoute anyOf={[permissions.admin.settingsRead, permissions.admin.settingsManage]}><AdminSettingsPage /></AuthorizedRoute>} />
              <Route path="process-library" element={<AuthorizedRoute anyOf={[permissions.governance.processLibraryRead, permissions.governance.processLibraryManage]}><ProcessLibraryPage /></AuthorizedRoute>} />
              <Route path="qa-review-checklists" element={<AuthorizedRoute anyOf={[permissions.governance.qaChecklistRead, permissions.governance.qaChecklistManage]}><QaReviewChecklistPage /></AuthorizedRoute>} />
              <Route path="project-plans" element={<AuthorizedRoute anyOf={[permissions.governance.projectPlanRead, permissions.governance.projectPlanManage, permissions.governance.projectPlanApprove]}><ProjectPlanPage /></AuthorizedRoute>} />
              <Route path="stakeholders" element={<AuthorizedRoute anyOf={[permissions.governance.stakeholderRead, permissions.governance.stakeholderManage]}><StakeholderRegisterPage /></AuthorizedRoute>} />
              <Route path="tailoring-records" element={<AuthorizedRoute anyOf={[permissions.governance.tailoringRead, permissions.governance.tailoringManage, permissions.governance.tailoringApprove]}><TailoringRecordPage /></AuthorizedRoute>} />
              <Route path="governance/raci-maps" element={<AuthorizedRoute anyOf={[permissions.governance.raciRead, permissions.governance.raciManage]}><RaciMapPage /></AuthorizedRoute>} />
              <Route path="governance/approval-evidence" element={<AuthorizedRoute permission={permissions.governance.approvalEvidenceRead}><ApprovalEvidenceLogPage /></AuthorizedRoute>} />
              <Route path="governance/workflow-overrides" element={<AuthorizedRoute permission={permissions.governance.overrideLogRead}><WorkflowOverrideLogPage /></AuthorizedRoute>} />
              <Route path="governance/sla-rules" element={<AuthorizedRoute anyOf={[permissions.governance.slaRead, permissions.governance.slaManage]}><SlaEscalationRulesPage /></AuthorizedRoute>} />
              <Route path="governance/retention-policies" element={<AuthorizedRoute anyOf={[permissions.governance.retentionRead, permissions.governance.retentionManage]}><RetentionPolicyPage /></AuthorizedRoute>} />
              <Route path="governance/architecture-records" element={<AuthorizedRoute anyOf={[permissions.governance.architectureRead, permissions.governance.architectureManage]}><ArchitectureRegisterPage /></AuthorizedRoute>} />
              <Route path="governance/design-reviews" element={<AuthorizedRoute anyOf={[permissions.governance.designReviewRead, permissions.governance.designReviewManage]}><DesignReviewPage /></AuthorizedRoute>} />
              <Route path="governance/integration-reviews" element={<AuthorizedRoute anyOf={[permissions.governance.integrationReviewRead, permissions.governance.integrationReviewManage]}><IntegrationReviewPage /></AuthorizedRoute>} />
              <Route path="governance/compliance-dashboard" element={<AuthorizedRoute anyOf={[permissions.governance.complianceRead, permissions.governance.complianceManage]}><ComplianceDashboardPage /></AuthorizedRoute>} />
              <Route path="governance/management-reviews" element={<AuthorizedRoute anyOf={[permissions.governance.managementReviewRead, permissions.governance.managementReviewManage, permissions.governance.managementReviewApprove]}><ManagementReviewPage /></AuthorizedRoute>} />
              <Route path="governance/management-reviews/:reviewId" element={<AuthorizedRoute anyOf={[permissions.governance.managementReviewRead, permissions.governance.managementReviewManage, permissions.governance.managementReviewApprove]}><ManagementReviewDetailPage /></AuthorizedRoute>} />
              <Route path="learning/training-catalog" element={<AuthorizedRoute anyOf={[permissions.learning.read, permissions.learning.manage, permissions.learning.approve]}><TrainingCatalogPage /></AuthorizedRoute>} />
              <Route path="learning/role-training-matrix" element={<AuthorizedRoute anyOf={[permissions.learning.read, permissions.learning.manage, permissions.learning.approve]}><RoleTrainingMatrixPage /></AuthorizedRoute>} />
              <Route path="learning/completions" element={<AuthorizedRoute anyOf={[permissions.learning.read, permissions.learning.manage, permissions.learning.approve]}><TrainingCompletionsPage /></AuthorizedRoute>} />
              <Route path="requirements" element={<AuthorizedRoute anyOf={[permissions.requirements.read, permissions.requirements.manage, permissions.requirements.approve, permissions.requirements.baseline]}><RequirementRegisterPage /></AuthorizedRoute>} />
              <Route path="requirements/:requirementId" element={<AuthorizedRoute anyOf={[permissions.requirements.read, permissions.requirements.manage, permissions.requirements.approve, permissions.requirements.baseline]}><RequirementDetailPage /></AuthorizedRoute>} />
              <Route path="requirements/baselines" element={<AuthorizedRoute anyOf={[permissions.requirements.read, permissions.requirements.baseline]}><RequirementBaselinesPage /></AuthorizedRoute>} />
              <Route path="requirements/traceability" element={<AuthorizedRoute permission={permissions.requirements.read}><TraceabilityMatrixPage /></AuthorizedRoute>} />
              <Route path="change-control/change-requests" element={<AuthorizedRoute anyOf={[permissions.changeControl.read, permissions.changeControl.manage, permissions.changeControl.approve]}><ChangeRequestRegisterPage /></AuthorizedRoute>} />
              <Route path="change-control/change-requests/:changeRequestId" element={<AuthorizedRoute anyOf={[permissions.changeControl.read, permissions.changeControl.manage, permissions.changeControl.approve]}><ChangeRequestDetailPage /></AuthorizedRoute>} />
              <Route path="change-control/change-log" element={<AuthorizedRoute anyOf={[permissions.changeControl.read, permissions.changeControl.manage, permissions.changeControl.approve]}><ChangeLogPage /></AuthorizedRoute>} />
              <Route path="change-control/configuration-items" element={<AuthorizedRoute anyOf={[permissions.changeControl.readConfiguration, permissions.changeControl.manageConfiguration]}><ConfigurationItemsPage /></AuthorizedRoute>} />
              <Route path="change-control/baseline-registry" element={<AuthorizedRoute anyOf={[permissions.changeControl.manageBaselines, permissions.changeControl.approveBaselines, permissions.changeControl.readConfiguration]}><BaselineRegistryPage /></AuthorizedRoute>} />
              <Route path="risks" element={<AuthorizedRoute anyOf={[permissions.risks.read, permissions.risks.manage]}><RiskRegisterPage /></AuthorizedRoute>} />
              <Route path="risks/:riskId" element={<AuthorizedRoute anyOf={[permissions.risks.read, permissions.risks.manage]}><RiskDetailPage /></AuthorizedRoute>} />
              <Route path="issues" element={<AuthorizedRoute anyOf={[permissions.risks.read, permissions.risks.manage]}><IssueLogPage /></AuthorizedRoute>} />
              <Route path="issues/:issueId" element={<AuthorizedRoute anyOf={[permissions.risks.read, permissions.risks.manage]}><IssueDetailPage /></AuthorizedRoute>} />
              <Route path="meetings" element={<AuthorizedRoute anyOf={[permissions.meetings.read, permissions.meetings.manage, permissions.meetings.approve]}><MeetingRegisterPage /></AuthorizedRoute>} />
              <Route path="meetings/:meetingId" element={<AuthorizedRoute anyOf={[permissions.meetings.read, permissions.meetings.manage, permissions.meetings.approve]}><MeetingDetailPage /></AuthorizedRoute>} />
              <Route path="decisions" element={<AuthorizedRoute anyOf={[permissions.meetings.read, permissions.meetings.manage, permissions.meetings.approve]}><DecisionLogPage /></AuthorizedRoute>} />
              <Route path="decisions/:decisionId" element={<AuthorizedRoute anyOf={[permissions.meetings.read, permissions.meetings.manage, permissions.meetings.approve]}><DecisionDetailPage /></AuthorizedRoute>} />
              <Route path="test-plans" element={<AuthorizedRoute anyOf={[permissions.verification.read, permissions.verification.manage, permissions.verification.approve]}><TestPlanPage /></AuthorizedRoute>} />
              <Route path="test-cases" element={<AuthorizedRoute anyOf={[permissions.verification.read, permissions.verification.manage]}><TestCaseExecutionPage /></AuthorizedRoute>} />
              <Route path="uat-signoffs" element={<AuthorizedRoute anyOf={[permissions.verification.read, permissions.verification.submitUat, permissions.verification.approve]}><UatSignoffPage /></AuthorizedRoute>} />
              <Route path="audit-logs" element={<AuthorizedRoute permission={permissions.auditLogs.read}><AuditLogsPage /></AuthorizedRoute>} />
              <Route path="evidence-exports" element={<AuthorizedRoute anyOf={[permissions.auditLogs.read, permissions.auditLogs.export]}><EvidenceExportsPage /></AuthorizedRoute>} />
              <Route path="audit-plans" element={<AuthorizedRoute anyOf={[permissions.auditLogs.read, permissions.auditLogs.manage]}><AuditPlansPage /></AuthorizedRoute>} />
              <Route path="audits/evidence-completeness" element={<AuthorizedRoute anyOf={[permissions.audits.evidenceRead, permissions.audits.evidenceManage]}><EvidenceCompletenessPage /></AuthorizedRoute>} />
              <Route path="audits/evidence-completeness/:resultId" element={<AuthorizedRoute anyOf={[permissions.audits.evidenceRead, permissions.audits.evidenceManage]}><EvidenceCompletenessDetailPage /></AuthorizedRoute>} />
              <Route path="metrics/definitions" element={<AuthorizedRoute anyOf={[permissions.metrics.read, permissions.metrics.manage]}><MetricDefinitionsPage /></AuthorizedRoute>} />
              <Route path="metrics/schedules" element={<AuthorizedRoute anyOf={[permissions.metrics.read, permissions.metrics.manage]}><DataCollectionSchedulePage /></AuthorizedRoute>} />
              <Route path="metrics/dashboard" element={<AuthorizedRoute permission={permissions.metrics.read}><MetricsDashboardPage /></AuthorizedRoute>} />
              <Route path="metrics/quality-gates" element={<AuthorizedRoute anyOf={[permissions.metrics.read, permissions.metrics.manage, permissions.metrics.overrideQualityGates]}><QualityGatesPage /></AuthorizedRoute>} />
              <Route path="metrics/reviews" element={<AuthorizedRoute anyOf={[permissions.metrics.read, permissions.metrics.manage]}><MetricReviewsPage /></AuthorizedRoute>} />
              <Route path="metrics/trend-reports" element={<AuthorizedRoute anyOf={[permissions.metrics.read, permissions.metrics.manage]}><TrendReportsPage /></AuthorizedRoute>} />
              <Route path="metrics/performance-baselines" element={<AuthorizedRoute anyOf={[permissions.metrics.read, permissions.metrics.manage]}><PerformanceBaselinePage /></AuthorizedRoute>} />
              <Route path="metrics/capacity-reviews" element={<AuthorizedRoute anyOf={[permissions.metrics.read, permissions.metrics.manage]}><CapacityReviewPage /></AuthorizedRoute>} />
              <Route path="metrics/slow-operations" element={<AuthorizedRoute anyOf={[permissions.metrics.read, permissions.metrics.manage]}><SlowOperationReviewPage /></AuthorizedRoute>} />
              <Route path="metrics/performance-gates" element={<AuthorizedRoute anyOf={[permissions.metrics.read, permissions.metrics.manage, permissions.metrics.overrideQualityGates]}><PerformanceRegressionGatePage /></AuthorizedRoute>} />
              <Route path="releases" element={<AuthorizedRoute anyOf={[permissions.releases.read, permissions.releases.manage, permissions.releases.approve]}><ReleaseRegisterPage /></AuthorizedRoute>} />
              <Route path="releases/checklists" element={<AuthorizedRoute anyOf={[permissions.releases.read, permissions.releases.manage]}><DeploymentChecklistPage /></AuthorizedRoute>} />
              <Route path="releases/notes" element={<AuthorizedRoute anyOf={[permissions.releases.read, permissions.releases.manage, permissions.releases.approve]}><ReleaseNotesPage /></AuthorizedRoute>} />
              <Route path="defects" element={<AuthorizedRoute anyOf={[permissions.defects.read, permissions.defects.manage]}><DefectLogPage /></AuthorizedRoute>} />
              <Route path="defects/:defectId" element={<AuthorizedRoute anyOf={[permissions.defects.read, permissions.defects.manage]}><DefectDetailPage /></AuthorizedRoute>} />
              <Route path="non-conformances" element={<AuthorizedRoute anyOf={[permissions.defects.read, permissions.defects.manage]}><NonConformanceLogPage /></AuthorizedRoute>} />
              <Route path="non-conformances/:nonConformanceId" element={<AuthorizedRoute anyOf={[permissions.defects.read, permissions.defects.manage]}><NonConformanceDetailPage /></AuthorizedRoute>} />
              <Route path="operations/access-reviews" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage, permissions.operations.approve]}><AccessReviewsPage /></AuthorizedRoute>} />
              <Route path="operations/access-recertifications" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage, permissions.operations.approve]}><AccessRecertificationsPage /></AuthorizedRoute>} />
              <Route path="operations/security-reviews" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage]}><SecurityReviewsPage /></AuthorizedRoute>} />
              <Route path="operations/external-dependencies" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage]}><ExternalDependenciesPage /></AuthorizedRoute>} />
              <Route path="operations/security-incidents" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage]}><SecurityIncidentRegisterPage /></AuthorizedRoute>} />
              <Route path="operations/vulnerabilities" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage]}><VulnerabilityRegisterPage /></AuthorizedRoute>} />
              <Route path="operations/secret-rotations" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage]}><SecretRotationRegisterPage /></AuthorizedRoute>} />
              <Route path="operations/privileged-access" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage]}><PrivilegedAccessLogPage /></AuthorizedRoute>} />
              <Route path="operations/classification-policies" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage]}><ClassificationPolicyPage /></AuthorizedRoute>} />
              <Route path="operations/backup-evidence" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage]}><BackupEvidencePage /></AuthorizedRoute>} />
              <Route path="operations/restore-verifications" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage]}><RestoreVerificationPage /></AuthorizedRoute>} />
              <Route path="operations/dr-drills" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage]}><DrDrillLogPage /></AuthorizedRoute>} />
              <Route path="operations/legal-holds" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage, permissions.operations.approve]}><LegalHoldRegisterPage /></AuthorizedRoute>} />
              <Route path="operations/capa" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage, permissions.operations.approve]}><CapaRegisterPage /></AuthorizedRoute>} />
              <Route path="operations/escalations" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage]}><EscalationHistoryPage /></AuthorizedRoute>} />
              <Route path="operations/suppliers" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage]}><SupplierRegisterPage /></AuthorizedRoute>} />
              <Route path="operations/supplier-agreements" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage]}><SupplierAgreementsPage /></AuthorizedRoute>} />
              <Route path="operations/configuration-audits" element={<AuthorizedRoute anyOf={[permissions.operations.read, permissions.operations.manage]}><ConfigurationAuditsPage /></AuthorizedRoute>} />
              <Route path="lessons-learned" element={<AuthorizedRoute anyOf={[permissions.knowledge.read, permissions.knowledge.manage]}><LessonsLearnedPage /></AuthorizedRoute>} />
              <Route path="admin/master" element={<Navigate to="/app/admin/master/divisions" replace />} />
                <Route path="admin/master/catalog" element={<AuthorizedRoute anyOf={[permissions.masterData.read, permissions.masterData.managePermanentOrg, permissions.masterData.manageProjectStructures]}><MasterDataCatalogPage /></AuthorizedRoute>} />
                <Route path="admin/master/divisions" element={<AdminUsersPage />} />
                <Route path="admin/master/departments" element={<AdminUsersPage />} />
                <Route path="admin/master/positions" element={<AdminUsersPage />} />
                <Route path="projects/roles" element={<ProjectRolesPage />} />
                <Route path="projects/roles/new" element={<ProjectRoleCreatePage />} />
                <Route path="projects/roles/:projectRoleId/edit" element={<ProjectRoleEditPage />} />
                <Route path="projects/phase-approvals" element={<AuthorizedRoute anyOf={[permissions.projects.read, permissions.projects.manage, permissions.projects.approvePhase]}><ProjectPhaseApprovalsPage /></AuthorizedRoute>} />
                <Route path="admin/project-members" element={<ProjectMembersPage />} />
                <Route path="admin/project-members/new" element={<ProjectMemberCreatePage />} />
                <Route path="admin/project-members/:assignmentId/edit" element={<ProjectMemberEditPage />} />
                <Route path="admin/project-org-chart" element={<ProjectOrgChartPage />} />
                <Route path="projects/:projectId/workspace" element={<ProjectWorkspacePrototypePage />} />
                <Route path="admin/invitations" element={<AdminUsersPage />} />
                <Route path="admin/registrations" element={<AdminUsersPage />} />
                <Route path="admin/activity-logs" element={<ActivityLogsPage />} />
              </Route>
            </Route>

            <Route path="*" element={<Navigate to="/login" replace />} />
          </Routes>
        </AppErrorBoundary>
      </Suspense>
    </BrowserRouter>
  );
}
