# Page → API → Query → Cache Mapping

This document maps each route to the primary data calls and whether those calls are backed by Redis cache.

Legend:
- Cache: `none` = direct DB or external API
- Cache: `Redis (<key>)` = read path uses Redis

---

## Public routes

### /login
Page: `LoginPage`
1. Hook: `useAuth` → Keycloak/OIDC login flow → Cache: none

### /register
Page: `PublicRegistrationPage`
1. Hook: `usePublicRegistration` → API: `POST /api/v1/users/register`
   Backend: `UsersModule.CreateRegistrationRequestAsync` → `UserRegistrationCommands.CreateRegistrationRequestAsync` → Cache: none
2. Hook: `useDivisionOptions` → API: `GET /api/v1/users/divisions` (public) → `UserReferenceDataQueries.ListDivisionsAsync` → Cache: Redis `reference-data:divisions`
3. Hook: `useDepartmentOptions` → API: `GET /api/v1/users/departments?divisionId=...` (public) → `UserReferenceDataQueries.ListDepartmentsAsync` → Cache: Redis `reference-data:departments`
4. Hook: `useJobTitleOptions` → API: `GET /api/v1/users/job-titles?departmentId=...` (public) → `UserReferenceDataQueries.ListJobTitlesAsync` → Cache: Redis `reference-data:job-titles`

### /register/setup-password/:token
Page: `RegistrationPasswordSetupPage`
1. Hook: `useRegistrationPasswordSetup` → API: `GET /api/v1/users/registration-requests/{token}/setup-password`
   Backend: `UsersModule.GetRegistrationPasswordSetupAsync` → `UserRegistrationQueries.GetPasswordSetupAsync` → Cache: none
2. Hook: `useRegistrationPasswordSetup` → API: `POST /api/v1/users/registration-requests/{token}/setup-password`
   Backend: `UsersModule.CompleteRegistrationPasswordSetupAsync` → `UserRegistrationCommands.CompletePasswordSetupAsync` → Cache: none

### /invite/:token
Page: `InvitationAcceptPage`
1. Hook: `useInvitationAcceptance` → API: `GET /api/v1/users/invitations/{token}`
   Backend: `UsersModule.GetInvitationByTokenAsync` → `UserInvitationQueries.GetByTokenAsync` → Cache: none
2. Hook: `useInvitationAcceptance` → API: `POST /api/v1/users/invitations/{token}/accept`
   Backend: `UsersModule.AcceptInvitationAsync` → `UserInvitationCommands.AcceptInvitationAsync` → Cache: none

---

## App routes

### /app/profile
Page: `UserProfilePage`
1. Hook: `useCurrentUserProfile` → API: `GET /api/v1/users/me?includeIdentity=true`
   Backend: `UsersModule.GetCurrentUserAsync` → `UserQueries.GetUserAsync` → Cache: Redis `reference-data:*` (divisions/departments/job-titles) + Redis `keycloak:user:{id}`

### /app/change-password
Page: `ChangePasswordPage`
1. API: `POST /api/v1/users/me/change-password` → `UsersModule.ChangeCurrentUserPasswordAsync` → `UserSelfServiceCommands.ChangePasswordAsync` → Cache: none

### /app/documents
Page: `DocumentDashboardPage`
1. Hook: `useDocumentDashboard` / `useDocuments` → API: `GET /api/v1/documents`
   Backend: `DocumentsModule.ListDocumentsAsync` → `DocumentQueries.ListDocumentsAsync` → Cache: none
2. Hook: `useUpdateDocument` → API: `PUT /api/v1/documents/{documentId}` → `DocumentCommands.UpdateDocumentAsync` → Cache: none
3. Hook: `useDeleteDocument` → API: `DELETE /api/v1/documents/{documentId}` → `DocumentCommands.DeleteDocumentAsync` → Cache: none

### /app/documents/upload
Page: `DocumentUploadPage`
1. Hook: `useCreateDocument` → API: `POST /api/v1/documents` → `DocumentCommands.CreateDocumentAsync` → Cache: none

### /app/documents/:documentId/versions
Page: `DocumentVersionsPage`
1. Hook: `useDocumentVersions` → API: `GET /api/v1/documents/{documentId}/versions` → `DocumentQueries.ListDocumentVersionsAsync` → Cache: none
2. Hook: `useDeleteDocumentVersion` → API: `DELETE /api/v1/documents/{documentId}/versions/{versionId}` → `DocumentCommands.DeleteDocumentVersionAsync` → Cache: none
3. Hook: `usePublishDocumentVersion` → API: `POST /api/v1/documents/{documentId}/versions/{versionId}/publish` → `DocumentCommands.PublishDocumentVersionAsync` → Cache: none
4. Hook: `useUnpublishDocumentVersion` → API: `POST /api/v1/documents/{documentId}/versions/unpublish` → `DocumentCommands.UnpublishDocumentVersionAsync` → Cache: none

### /app/documents/:documentId/versions/new
Page: `DocumentVersionUploadPage`
1. Hook: `useCreateDocumentVersion` → API: `POST /api/v1/documents/{documentId}/versions` → `DocumentCommands.CreateDocumentVersionAsync` → Cache: none

### /app/documents/:documentId/history
Page: `DocumentHistoryPage`
1. Hook: `useDocumentHistory` → API: `GET /api/v1/documents/{documentId}/history` → `DocumentHistoryQueries.ListAsync` → Cache: none

### /app/documents/templates (alias: /app/document-templates)
Page: `DocumentTemplatesPage`
1. Hook: `useDocumentTemplates` → API: `GET /api/v1/documents/templates`
   Backend: `DocumentTemplateQueries.ListTemplatesAsync` → Cache: Redis `document-templates:list`

### /app/document-templates/new
Page: `DocumentTemplateCreatePage`
1. Hook: `useDocumentOptions` → API: `GET /api/v1/documents` → `DocumentQueries.ListDocumentsAsync` → Cache: none
2. Hook: `useCreateDocumentTemplate` → API: `POST /api/v1/documents/templates` → `DocumentTemplateCommands.CreateTemplateAsync` → Cache invalidation only

### /app/document-templates/:templateId/edit
Page: `DocumentTemplateEditPage`
1. Hook: `useDocumentTemplate` → API: `GET /api/v1/documents/templates/{templateId}` → `DocumentTemplateQueries.GetTemplateAsync` → Cache: none
2. Hook: `useDocumentOptions` → API: `GET /api/v1/documents` → `DocumentQueries.ListDocumentsAsync` → Cache: none
3. Hook: `useDocumentVersions` → API: `GET /api/v1/documents/{documentId}/versions` → Cache: none
4. Hook: `useUpdateDocumentTemplate` → API: `PUT /api/v1/documents/templates/{templateId}` → Cache invalidation only
5. Hook: `useRefreshDocumentTemplateItemVersion` → API: `POST /api/v1/documents/templates/{templateId}/items/{documentId}/refresh-version` → Cache: none

### /app/document-templates/:templateId/history
Page: `DocumentTemplateHistoryPage`
1. Hook: `useDocumentTemplateHistory` → API: `GET /api/v1/documents/templates/{templateId}/history` → `DocumentTemplateHistoryQueries.ListAsync` → Cache: none

### /app/projects
Page: `ProjectsPage`
1. Hook: `useProjectAdmin` → API: `GET /api/v1/users/projects` → `ProjectQueries.ListProjectsAsync` → Cache: none

### /app/projects/new
Page: `ProjectCreatePage`
1. Hook: `useProjectAdmin` → API: `POST /api/v1/users/projects` → `ProjectCommands.CreateProjectAsync` → Cache: none
2. Hook: `useProjectRoleOptions` → API: `GET /api/v1/users/project-roles` → `UserReferenceDataQueries.ListProjectRolesAsync` → Cache: Redis `reference-data:project-roles`
3. Hook: `useWorkflowDefinitionOptions` → API: `GET /api/v1/steps/definitions` → `WorkflowQueries.ListDefinitionsAsync` → Cache: Redis `workflows:definitions`
4. Hook: `useProjectUserOptions` → API: `GET /api/v1/users` → `UserQueries.ListUsersAsync` → Cache: Redis `reference-data:*`

### /app/projects/:projectId/edit
Page: `ProjectEditPage`
1. Hook: `useProjectAdmin` → API: `GET /api/v1/users/projects/{projectId}` → `ProjectQueries.GetProjectAsync` → Cache: none
2. Hook: `useProjectAdmin` → API: `GET /api/v1/users/project-assignments?projectId=...` → `ProjectQueries.ListProjectAssignmentsAsync` → Cache: Redis `keycloak:user:{id}` for display name/email
3. Hook: `useProjectRoleOptions` → API: `GET /api/v1/users/project-roles` → Cache: Redis `reference-data:project-roles`
4. Hook: `useWorkflowDefinitionOptions` → API: `GET /api/v1/steps/definitions` → Cache: Redis `workflows:definitions`
5. Hook: `useWorkflowDefinition` → API: `GET /api/v1/steps/definitions/{id}` → Cache: none
6. Hook: `useDocumentsByIds` → API: `POST /api/v1/documents/lookup` → Cache: none

### /app/projects/:projectId/history
Page: `ProjectHistoryPage`
1. Hook: `useProjectHistory` → API: `GET /api/v1/users/projects/{projectId}/history` → `ProjectHistoryQueries.ListAsync` → Cache: none

### /app/projects/roles
Page: `ProjectRolesPage`
1. Hook: `useProjectAdmin` → API: `GET /api/v1/users/project-roles` → `UserReferenceDataQueries.ListProjectRolesAsync` → Cache: Redis `reference-data:project-roles`

### /app/projects/roles/new
Page: `ProjectRoleCreatePage`
1. Hook: `useProjectAdmin` → API: `POST /api/v1/users/project-roles` → `ProjectCommands.CreateProjectRoleAsync` → Cache invalidation only

### /app/projects/roles/:projectRoleId/edit
Page: `ProjectRoleEditPage`
1. Hook: `useProjectRoleDetail` → API: `GET /api/v1/users/project-roles/{projectRoleId}` → `ProjectQueries.GetProjectRoleAsync` → Cache: none
2. Hook: `useProjectAdmin` → API: `PUT /api/v1/users/project-roles/{projectRoleId}` → `ProjectCommands.UpdateProjectRoleAsync` → Cache invalidation only

### /app/admin/project-members
Page: `ProjectMembersPage`
1. Hook: `useProjectAdmin` → API: `GET /api/v1/users/project-assignments` → `ProjectQueries.ListProjectAssignmentsAsync` → Cache: Redis `keycloak:user:{id}` for display name/email
2. Hook: `useProjectOptions` → API: `GET /api/v1/users/projects` → Cache: none

### /app/admin/project-members/new
Page: `ProjectMemberCreatePage`
1. Hook: `useProjectAdmin` → API: `POST /api/v1/users/project-assignments` → `ProjectCommands.CreateProjectAssignmentAsync` → Cache: none
2. Hook: `useProjectRoleOptions` → API: `GET /api/v1/users/project-roles` → Cache: Redis `reference-data:project-roles`
3. Hook: `useProjectUserOptions` → API: `GET /api/v1/users` → Cache: Redis `reference-data:*`

### /app/admin/project-members/:assignmentId/edit
Page: `ProjectMemberEditPage`
1. Hook: `useProjectAssignmentDetail` → API: `GET /api/v1/users/project-assignments/{assignmentId}` → `ProjectQueries.GetProjectAssignmentAsync` → Cache: Redis `keycloak:user:{id}` for display name/email
2. Hook: `useProjectAdmin` → API: `PUT /api/v1/users/project-assignments/{assignmentId}` → `ProjectCommands.UpdateProjectAssignmentAsync` → Cache: none
3. Hook: `useProjectRoleOptions` → API: `GET /api/v1/users/project-roles` → Cache: Redis `reference-data:project-roles`
4. Hook: `useProjectUserOptions` → API: `GET /api/v1/users` → Cache: Redis `reference-data:*`

### /app/admin/project-org-chart
Page: `ProjectOrgChartPage`
1. Hook: `useProjectAdmin` → API: `GET /api/v1/users/projects/{projectId}/org-chart` → `ProjectQueries.GetOrgChartAsync` → Cache: Redis `keycloak:user:{id}` for display name/email

### /app/projects/:projectId/workspace
Page: `ProjectWorkspacePrototypePage`
1. Hook: `useProjectAdmin` → API: `GET /api/v1/users/projects/{projectId}` → Cache: none
2. Hook: `useProjectAdmin` → API: `GET /api/v1/users/project-assignments?projectId=...` → Cache: Redis `keycloak:user:{id}`

### /app/steps
Page: `WorkflowDefinitionsPage`
1. Hook: `useWorkflowDefinitionsScreen` → API: `GET /api/v1/steps/definitions` → `WorkflowQueries.ListDefinitionsAsync` → Cache: Redis `workflows:definitions`
2. Hook: `useWorkflowDefinitionActions` → API: `POST /api/v1/steps/definitions/{id}/activate` / `.../archive` → `WorkflowCommands.*` → Cache invalidation only

### /app/steps/new
Page: `WorkflowDefinitionCreatePage`
1. Hook: `useCreateWorkflowDefinition` → API: `POST /api/v1/steps/definitions` → `WorkflowCommands.CreateDefinitionAsync` → Cache invalidation only
2. Hook: `useProjectRoleOptions` → API: `GET /api/v1/users/project-roles` → Cache: Redis `reference-data:project-roles`
3. Hook: `useDocumentTemplates` → API: `GET /api/v1/documents/templates` → `DocumentTemplateQueries.ListTemplatesAsync` → Cache: Redis `document-templates:list`
4. Hook: `useDocumentTemplate` → API: `GET /api/v1/documents/templates/{id}` → Cache: none
5. Hook: `useDocumentsByIds` → API: `POST /api/v1/documents/lookup` → Cache: none

### /app/steps/:workflowDefinitionId/edit
Page: `WorkflowDefinitionEditPage`
1. Hook: `useWorkflowDefinition` → API: `GET /api/v1/steps/definitions/{id}` → Cache: none
2. Hook: `useUpdateWorkflowDefinition` → API: `PUT /api/v1/steps/definitions/{id}` → Cache invalidation only
3. Hook: `useProjectRoleOptions` → API: `GET /api/v1/users/project-roles` → Cache: Redis `reference-data:project-roles`
4. Hook: `useDocumentTemplates` → API: `GET /api/v1/documents/templates` → Cache: Redis `document-templates:list`
5. Hook: `useDocumentTemplate` → API: `GET /api/v1/documents/templates/{id}` → Cache: none
6. Hook: `useDocumentsByIds` → API: `POST /api/v1/documents/lookup` → Cache: none

### /app/workspace
Page: `WorkspaceProjectsPage`
1. Hook: `useProjectList` → API: `GET /api/v1/users/projects` → `ProjectQueries.ListProjectsAsync` → Cache: none

### /app/workspace/:projectId
Page: `WorkflowTasksPage`
1. Hook: `useProjectDetail` → API: `GET /api/v1/users/projects/{projectId}` → Cache: none
2. Hook: `useWorkflowTasks` → API: `GET /api/v1/steps/tasks?projectId=...` → `WorkflowTaskQueries.ListTasksAsync` → Cache: none

### /app/workspace/:projectId/tasks/:workflowInstanceStepId
Page: `WorkflowTaskDetailPage`
1. Hook: `useWorkflowTasks` → API: `GET /api/v1/steps/tasks?projectId=...` → Cache: none
2. Hook: `useWorkflowInstance` → API: `GET /api/v1/steps/instances/{workflowInstanceId}` → `WorkflowInstanceQueries.GetInstanceAsync` → Cache: none
3. Hook: `useDocumentVersions` → API: `GET /api/v1/documents/{documentId}/versions` → Cache: none
4. Action: `downloadDocument` → API: `GET /api/v1/documents/{documentId}/download` → Cache: none

### /app/workspace/:projectId/tasks/:workflowInstanceStepId/upload
Page: `WorkflowTaskUploadPage`
1. Hook: `useWorkflowTasks` → API: `GET /api/v1/steps/tasks?projectId=...` → Cache: none
2. Hook: `useCreateDocumentVersion` → API: `POST /api/v1/documents/{documentId}/versions` → Cache: none

### /app/notifications
Page: `NotificationsPage`
1. Hook: `useNotifications` → API: `GET /api/v1/notifications` → `NotificationsModule.ListAsync` → Cache: none
2. Hook: `useNotificationDetail` → API: `GET /api/v1/notifications/{id}` → Cache: none
3. Hook: `useNotificationActions` → API: `POST /api/v1/notifications/{id}/read` / `POST /api/v1/notifications/read-all` → Cache: none

### /app/admin/users
Page: `AdminUsersPage`
1. Hook: `useAdminUsersScreen` → `useAdminUsers` → API: `GET /api/v1/users` → `UserQueries.ListUsersAsync` → Cache: Redis `reference-data:*`
2. Hook: `useAdminUsersScreen` → API: `GET /api/v1/users/registration-requests` → `UserRegistrationQueries.ListAsync` → Cache: none
3. Hook: `useAdminUsersScreen` → API: `GET /api/v1/users/invitations` → `UserInvitationQueries.ListAsync` → Cache: none
4. Hook: `useAdminUsersScreen` → API: `GET /api/v1/users/divisions` / `departments` / `job-titles` / `roles` → Cache: Redis `reference-data:*`
5. Mutations: create/update/delete user/invitation/registration → `UserManagementCommands` / `UserInvitationCommands` / `UserRegistrationCommands` → Cache invalidation only

### /app/admin/users/new
Page: `AdminUserCreatePage`
1. Hook: `useCreateAdminUser` → API: `POST /api/v1/users` → `UserManagementCommands.CreateUserAsync` → Cache invalidation only
2. Hook: `useDivisionOptions` / `useDepartmentOptions` / `useJobTitleOptions` / `useAdminRoles` → APIs: `/users/divisions|departments|job-titles|roles` → Cache: Redis `reference-data:*`

### /app/admin/users/:userId/edit
Page: `AdminUserEditPage`
1. Hook: `useAdminUserDetail` → API: `GET /api/v1/users/{id}` → `UserQueries.GetUserAsync` → Cache: Redis `reference-data:*` + Redis `keycloak:user:{id}`
2. Hook: `useUpdateAdminUser` → API: `PUT /api/v1/users/{id}` → `UserManagementCommands.UpdateUserAsync` → Cache invalidation only
3. Hook: `useDivisionOptions` / `useDepartmentOptions` / `useJobTitleOptions` / `useAdminRoles` → Cache: Redis `reference-data:*`

### /app/admin/settings
Page: `AdminSettingsPage`
1. API: `POST /api/v1/users/keycloak/refresh-cache` → `UserManagementCommands.RefreshAllKeycloakUsersAsync` → Cache: Redis `keycloak:user:{id}`
2. API: `POST /api/v1/steps/definitions/cache/refresh` → `WorkflowDefinitionCache.RefreshAsync` → Cache: Redis `workflows:definitions`
3. API: `POST /api/v1/documents/templates/cache/refresh` → `DocumentTemplateCache.RefreshAsync` → Cache: Redis `document-templates:list`
4. API: `POST /api/v1/users/departments|divisions|job-titles|project-roles/cache/refresh` → `ReferenceDataCache.Refresh*` → Cache: Redis `reference-data:*`

### /app/admin/master/divisions
Page: `AdminUsersPage` (master data section)
1. Hook: `useAdminUsers` → API: `GET /api/v1/users/divisions` → `UserReferenceDataQueries.ListDivisionsAsync` → Cache: Redis `reference-data:divisions`

### /app/admin/master/departments
Page: `AdminUsersPage` (master data section)
1. Hook: `useAdminUsers` → API: `GET /api/v1/users/departments` → `UserReferenceDataQueries.ListDepartmentsAsync` → Cache: Redis `reference-data:departments`

### /app/admin/master/positions
Page: `AdminUsersPage` (master data section)
1. Hook: `useAdminUsers` → API: `GET /api/v1/users/job-titles` → `UserReferenceDataQueries.ListJobTitlesAsync` → Cache: Redis `reference-data:job-titles`

### /app/admin/invitations
Page: `AdminUsersPage` (invitations section)
1. Hook: `useAdminUsers` → API: `GET /api/v1/users/invitations` → Cache: none

### /app/admin/registrations
Page: `AdminUsersPage` (registrations section)
1. Hook: `useAdminUsers` → API: `GET /api/v1/users/registration-requests` → Cache: none

### /app/admin/activity-logs
Page: `ActivityLogsPage`
1. Hook: `useActivityLogs` → API: `GET /api/v1/activity-logs` → `ActivityLogQueries.ListAsync` → Cache: none
2. Hook: `activityDetailQuery` → API: `GET /api/v1/activity-logs/{id}` → Cache: none

---

## Notes
- “Cache invalidation only” means write endpoints invalidate Redis but do not read from it.
- Detail endpoints (single entity) generally read from DB directly for freshness.
- If you want deeper verification for any specific page, call it out and I will trace the exact handlers and queries.
