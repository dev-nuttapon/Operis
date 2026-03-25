# CMMI L3 Phase + Menu + Role + Workflow Spec

This document combines:
- Menu structure and delivery phases
- Detailed screen roles/permissions
- Workflow states for each screen

Legend:
- C: Create
- R: Read
- U: Update
- A: Approve/Review
- X: Execute/Run
- E: Export

## 1. Menu Structure (Order + Screen Names)

1. Overview
2. Projects
   - Project Register
   - Project Detail
   - Project Roles
   - Team Assignment
   - Project Phase Approval
3. Requirements
   - Requirement Register
   - Requirement Detail
   - Requirement Baseline
   - Traceability Matrix
4. Documents
   - Document Type Setup
   - Document Register
   - Document Detail
5. Change Control
   - Change Request Register
   - Change Request Detail
   - Change Log
6. Meetings & Decisions
   - MOM Register
   - MOM Detail
   - Decision Log
7. Test & Validation
   - Test Plan
   - Test Case & Execution
   - UAT Sign-off
8. Audit & Evidence
   - Audit Log
   - Evidence Export
9. Metrics & Quality
   - Metrics Dashboard
   - Quality Gate Status
10. Process & Organization
    - Process Library
    - Training & Competency
11. Risk & Issue
    - Risk Register
    - Issue / Action Log
12. Configuration & Baseline
    - Configuration Items
    - Baseline Registry
13. PPQA
    - QA Review Checklist
    - Process Audit Plan & Findings
14. Metrics Definition
    - Metric Definitions
    - Data Collection Schedule
15. Project Management
    - Project Plan
    - Tailoring Record
    - Stakeholder Register
16. System Admin
    - User & Role Management
    - Permission Matrix
    - Master Data
    - System Settings
17. Security & Dependencies
    - Access Review
    - Security Review
    - External Dependency Register
18. Configuration Audit
    - Configuration Audit Log
19. Governance & Operations
    - RACI Map
    - Approval Evidence Log
    - Workflow Override Log
    - SLA & Escalation Rules
    - Data Retention Policy
20. Release & Deployment
    - Release Register
    - Deployment Checklist
    - Release Notes
21. Defect & Non-Conformance
    - Defect Log
    - Non-Conformance Log
22. Supplier & Agreement
    - Supplier Register
    - SLA/Contract Evidence
23. Performance Review
    - Metrics Review Log
    - Trend Analysis Report
24. Knowledge Base
    - Lessons Learned
25. Access Recertification
    - Access Recertification Schedule
26. Architecture & Design Governance
    - Architecture Register
    - Design Review
    - Integration Review
27. Security Operations
    - Security Incident Register
    - Vulnerability & Patch Register
    - Secret Rotation Register
    - Privileged Access Log
    - Data Classification Policy
28. Performance & Capacity
    - Performance Baseline
    - Capacity Review
    - Slow Query / API Review
    - Performance Regression Gate
29. Backup, Restore & DR
    - Backup Evidence
    - Restore Verification
    - DR Drill Log
    - Legal Hold Register
30. CAPA & Escalation Execution
    - CAPA Register
    - Notification Queue
    - Escalation History

## 2. Phased Delivery Plan (CMMI L3 + Performance + Security)

## 2.0 Core Operating Flow (CMMI L3-Oriented)

The target operating flow across the system is:

1. Define process context
   - Process Library
   - Project Plan
   - Tailoring Record
   - Stakeholder Register
2. Capture and baseline requirements
   - Requirement Register
   - Requirement Detail
   - Requirement Baseline
   - Traceability Matrix
3. Produce and control governed artifacts
   - Document Register
   - Document Detail
   - Configuration Items
   - Baseline Registry
4. Govern changes and decisions
   - Change Request Register
   - Change Request Detail
   - MOM Register
   - Decision Log
5. Execute delivery and validation
   - Test Plan
   - Test Case & Execution
   - UAT Sign-off
   - Release Register
   - Deployment Checklist
   - Release Notes
6. Measure, review, and correct
   - Metrics Dashboard
   - Quality Gate Status
   - Metrics Review Log
   - CAPA Register
   - Lessons Learned
7. Sustain auditability, security, and resilience
   - Audit Log
   - Evidence Export
   - Security Review
   - Security Incident Register
   - Backup Evidence
   - Restore Verification
   - DR Drill Log

Mandatory cross-cutting control points in the flow:

1. No requirement may enter baseline without approval and traceability owner.
2. No release may proceed without test evidence, UAT result, and quality gate status.
3. No override may occur without reason, approver, timestamp, and audit record.
4. No privileged or security-sensitive action may bypass logging.
5. Every closed finding, defect, or incident must point to corrective action or explicit acceptance.

### Phase 0: Security & Access Foundation
- Scope (Backend)
  - Permission matrix model + enforcement
  - Audit scope + secure defaults
- Scope (Frontend)
  - Route guard + action guard
  - Session hardening UX
- Scope (3rd‑party)
  - Keycloak realm/client config
  - Redis policy for tokens/session cache
- Screens
  - User & Role Management
  - Permission Matrix
  - System Settings

### Phase 1: Process Assets & Governance Baseline
- Scope (Backend)
  - Process library + templates
  - QA checklist + project plan store
- Scope (Frontend)
  - Process library UI
  - Project plan + tailoring forms
- Scope (3rd‑party)
  - None (internal)
- Screens
  - Process Library
  - QA Review Checklist
  - Project Plan
  - Stakeholder Register
  - Tailoring Record

### Phase 2: Document Governance Core
- Scope (Backend)
  - Document types + metadata + versioning
  - Approval workflow states
- Scope (Frontend)
  - Document list/detail + approval UI
- Scope (3rd‑party)
  - MinIO storage integration
  - Redis caching (optional)
- Screens
  - Document Type Setup
  - Document Register
  - Document Detail

### Phase 3: Requirements + Traceability
- Scope (Backend)
  - Requirement model + baseline
  - Traceability links
- Scope (Frontend)
  - Requirement register/detail UI
  - Traceability view (matrix)
- Scope (3rd‑party)
  - None (internal)
- Screens
  - Requirement Register
  - Requirement Detail
  - Requirement Baseline
  - Traceability Matrix

### Phase 4: Change Control + Configuration Management
- Scope (Backend)
  - CR workflow + impact analysis
  - Configuration items + baseline registry
- Scope (Frontend)
  - CR register/detail UI
  - Baseline/CI UI
- Scope (3rd‑party)
  - None (internal)
- Screens
  - Change Request Register
  - Change Request Detail
  - Change Log
  - Configuration Items
  - Baseline Registry

### Phase 5: Risk & Issue Management
- Scope (Backend)
  - Risk + issue entities + workflow
- Scope (Frontend)
  - Risk/issue register + detail
- Scope (3rd‑party)
  - None (internal)
- Screens
  - Risk Register
  - Issue / Action Log

### Phase 6: Meetings & Decisions
- Scope (Backend)
  - MOM + decision entities
- Scope (Frontend)
  - MOM register/detail + decision log
- Scope (3rd‑party)
  - None (internal)
- Screens
  - MOM Register
  - MOM Detail
  - Decision Log

### Phase 7: Verification & Validation
- Scope (Backend)
  - Test plan/case/execution store
  - UAT sign-off workflow
- Scope (Frontend)
  - Test plan UI
  - Test execution UI
  - UAT approval UI
- Scope (3rd‑party)
  - None (internal)
- Screens
  - Test Plan
  - Test Case & Execution
  - UAT Sign-off

### Phase 8: Audit & Compliance
- Scope (Backend)
  - Audit log aggregation
  - Evidence export package
  - Process audit plan + findings
- Scope (Frontend)
  - Audit log view
  - Evidence export UI
  - Audit plan/findings UI
- Scope (3rd‑party)
  - Loki/Tempo (optional log backend)
- Screens
  - Audit Log
  - Evidence Export
  - Process Audit Plan & Findings

### Phase 9: Metrics & Quality Gates (Performance)
- Scope (Backend)
  - Metric definitions + collection schedule
  - Quality gate evaluation
- Scope (Frontend)
  - Metrics dashboard
  - Quality gate view
- Scope (3rd‑party)
  - Prometheus/Grafana (metrics)
- Screens
  - Metric Definitions
  - Data Collection Schedule
  - Metrics Dashboard
  - Quality Gate Status

### Phase 10: Project Governance Hardening
- Scope (Backend)
  - Project roles + team assignment
  - Phase approval enforcement
- Scope (Frontend)
  - Project role UI
  - Team assignment UI
  - Phase approval UI
- Scope (3rd‑party)
  - None (internal)
- Screens
  - Project Roles
  - Team Assignment
  - Project Phase Approval

### Phase 11: Master Data & Operations Support
- Scope (Backend)
  - Master data management
- Scope (Frontend)
  - Master data UI
- Scope (3rd‑party)
  - None (internal)
- Screens
  - Master Data

### Phase 12: Security, Dependency & Configuration Audit
- Scope (Backend)
  - Access review records
  - Security review checklist + evidence
  - External dependency register
  - Configuration audit log
- Scope (Frontend)
  - Access/security review UI
  - Dependency register UI
  - Configuration audit log UI
- Scope (3rd‑party)
  - None (internal)
- Screens
  - Access Review
  - Security Review
  - External Dependency Register
  - Configuration Audit Log

### Phase 13: Governance & Operations
- Scope (Backend)
  - Approval evidence capture
  - Workflow override log
  - SLA/esc rules
  - Retention policy registry
  - RACI mapping store
- Scope (Frontend)
  - RACI map UI
  - Approval evidence UI
  - Override log UI
  - SLA/esc rule UI
  - Retention policy UI
- Scope (3rd‑party)
  - None (internal)
- Screens
  - RACI Map
  - Approval Evidence Log
  - Workflow Override Log
  - SLA & Escalation Rules
  - Data Retention Policy

### Phase 14: Release & Deployment Management
- Scope (Backend)
  - Release registry + deployment checklist
  - Release notes storage
- Scope (Frontend)
  - Release register UI
  - Deployment checklist UI
  - Release notes UI
- Scope (3rd‑party)
  - None (internal)
- Screens
  - Release Register
  - Deployment Checklist
  - Release Notes

### Phase 15: Defect & Non‑Conformance
- Scope (Backend)
  - Defect + NC entities + workflow
- Scope (Frontend)
  - Defect/NC register + detail
- Scope (3rd‑party)
  - None (internal)
- Screens
  - Defect Log
  - Non‑Conformance Log

### Phase 16: Supplier & Agreement
- Scope (Backend)
  - Supplier register + SLA/contract evidence
- Scope (Frontend)
  - Supplier register UI
  - SLA/contract evidence UI
- Scope (3rd‑party)
  - None (internal)
- Screens
  - Supplier Register
  - SLA/Contract Evidence

### Phase 17: Performance Review
- Scope (Backend)
  - Metrics review + trend analysis records
- Scope (Frontend)
  - Metrics review log UI
  - Trend analysis report UI
- Scope (3rd‑party)
  - None (internal)
- Screens
  - Metrics Review Log
  - Trend Analysis Report

### Phase 18: Knowledge Base
- Scope (Backend)
  - Lessons learned repository
- Scope (Frontend)
  - Lessons learned UI
- Scope (3rd‑party)
  - None (internal)
- Screens
  - Lessons Learned

### Phase 19: Access Recertification
- Scope (Backend)
  - Recertification schedule + attestations
- Scope (Frontend)
  - Recertification schedule UI
- Scope (3rd‑party)
  - None (internal)
- Screens
  - Access Recertification Schedule

### Phase 20: Architecture & Design Governance
- Scope (Backend)
  - Architecture/design review records
  - Integration review evidence
- Scope (Frontend)
  - Architecture register UI
  - Design review UI
  - Integration review UI
- Scope (3rd-party)
  - None (internal)
- Screens
  - Architecture Register
  - Design Review
  - Integration Review

### Phase 21: Security Operations
- Scope (Backend)
  - Security incident records
  - Vulnerability/patch records
  - Secret rotation history
  - Privileged access audit log
  - Data classification policy store
- Scope (Frontend)
  - Security ops screens
  - Privileged access review UI
- Scope (3rd-party)
  - Keycloak, Redis, MinIO secret rotation touchpoints
- Screens
  - Security Incident Register
  - Vulnerability & Patch Register
  - Secret Rotation Register
  - Privileged Access Log
  - Data Classification Policy

### Phase 22: Performance & Capacity Control
- Scope (Backend)
  - Performance baseline registry
  - Capacity review records
  - Slow query / API review records
  - Performance regression gate rules
- Scope (Frontend)
  - Performance review screens
  - Gate result UI
- Scope (3rd-party)
  - Prometheus/Grafana/Loki traces and metrics
- Screens
  - Performance Baseline
  - Capacity Review
  - Slow Query / API Review
  - Performance Regression Gate

### Phase 23: Backup, Restore & DR
- Scope (Backend)
  - Backup evidence records
  - Restore verification evidence
  - DR drill records
  - Legal hold registry
- Scope (Frontend)
  - Backup/restore/DR evidence UI
  - Legal hold UI
- Scope (3rd-party)
  - PostgreSQL, MinIO backup/restore touchpoints
- Screens
  - Backup Evidence
  - Restore Verification
  - DR Drill Log
  - Legal Hold Register

### Phase 24: CAPA & Escalation Execution
- Scope (Backend)
  - CAPA workflow
  - Notification queue + delivery history
  - Escalation execution history
- Scope (Frontend)
  - CAPA UI
  - Notification queue UI
  - Escalation history UI
- Scope (3rd-party)
  - Email/chat/webhook integration if used later
- Screens
  - CAPA Register
  - Notification Queue
  - Escalation History

## 2.5 Cross-Cutting Security Requirements

These requirements apply to all phases and all screens:

1. Authentication must be centralized through Keycloak; no local password bypass for protected modules.
2. Authorization must be enforced both at route level and action level.
3. Sensitive actions must be logged with actor, timestamp, target, reason, and outcome.
4. Role changes, access reviews, privileged access, and overrides must be independently auditable.
5. Secrets, keys, and integration credentials must be rotated on schedule and after incidents.
6. Sensitive records and documents must support classification and retention handling.
7. Security findings must feed CAPA, not remain isolated in a review screen.
8. Exported evidence must respect authorization and data classification policy.

## 2.6 Cross-Cutting Performance Requirements

These requirements apply to all phases and all screens:

1. All list screens must define paging, sorting, filtering, and export limits.
2. Traceability, audit, and evidence screens must avoid full-table loading by default.
3. Background packaging jobs such as exports, evidence bundles, and large reports should be asynchronous.
4. Quality gates must include performance regression checks for high-risk modules.
5. Slow query and slow API findings must route into corrective work, not remain passive reporting.
6. Caching must be explicit, measurable, and invalidated by workflow events where needed.
7. Dashboard and overview screens must aggregate data efficiently and avoid fan-out requests per widget.

## 2.7 Cross-Cutting Workflow Integrity Rules

These rules keep the system aligned with CMMI Level 3 process discipline:

1. Create, review, approve, baseline, release, and archive transitions must be state-driven, not ad hoc.
2. Segregation of duties should be enforced for baseline approval, release approval, access review, and major overrides.
3. Required evidence must be attached before a gate can move to approved.
4. Superseded artifacts must remain visible for audit and traceability, never overwritten in place.
5. Findings, defects, non-conformances, risks, and incidents must support closed-loop follow-up through CAPA or equivalent action tracking.

## 2.8 Implementation Detail Standard

Each phase is not implementation-ready until the following are defined for every screen and workflow:

1. Data model
   - Aggregate/root entity
   - Child entities
   - Required fields
   - Foreign keys / ownership
   - Status field and transition rules
2. API contract
   - List endpoint
   - Detail endpoint
   - Create/update endpoint
   - State transition endpoints
   - Export endpoint if required
3. Frontend contract
   - Page route
   - List filters and sorting
   - Detail form fields
   - Action buttons by permission
   - Empty/error/loading states
4. Validation
   - Required fields
   - Referential validation
   - State transition validation
   - Segregation-of-duties validation
5. Audit
   - Logged actions
   - Required metadata in log
   - Evidence attachments if required
6. Security
   - Authorization points
   - Sensitive field handling
   - Export restrictions
7. Performance
   - Query paging
   - Search strategy
   - Cache policy if any
   - Async/background processing needs

## 2.9 Minimum Build Spec for Start Phases

### Phase 0 Implementation Detail
- Data model
  - `users`, `roles`, `permissions`, `role_permissions`, `user_role_assignments`
  - `access_review_records`, `security_events`
- API contract
  - `GET /admin/users`
  - `GET /admin/roles`
  - `PUT /admin/users/{id}/roles`
  - `GET /admin/permissions`
  - `PUT /admin/permissions/matrix`
  - `GET /admin/settings`
  - `PUT /admin/settings`
- Frontend contract
  - `/app/admin/users`
  - `/app/admin/permissions`
  - `/app/admin/settings`
  - Filters: role, status, search
  - Actions: assign role, revoke role, apply matrix, refresh config
- Validation
  - Cannot remove last required admin role without replacement
  - Only authorized admins can change permission matrix
- Audit
  - Log role assignment, role removal, permission matrix change, settings change
- Security
  - All endpoints require authorization
  - Matrix changes require elevated permission and reason
- Performance
  - User list must page
  - Role lookup should be cached in-memory or Redis if needed

### Phase 1 Implementation Detail
- Data model
  - `process_assets`, `process_asset_versions`
  - `project_plans`, `stakeholders`, `tailoring_records`
  - `qa_checklists`
- API contract
  - `GET /process-assets`
  - `POST /process-assets`
  - `PUT /process-assets/{id}`
  - `GET /project-plans`
  - `POST /project-plans`
  - `PUT /tailoring-records/{id}/submit`
  - `PUT /tailoring-records/{id}/approve`
- Frontend contract
  - `/app/process-library`
  - `/app/project-plans`
  - `/app/tailoring`
  - `/app/qa-checklists`
- Validation
  - Tailoring cannot be applied without approver and rationale
  - Project plan requires owner, scope, dates, and lifecycle reference
- Audit
  - Log create, update, submit, approve, supersede
- Security
  - Approval must be segregated from author where required
- Performance
  - Process asset lists must support type/status/version filters

### Phase 2 Implementation Detail
- Data model
  - `document_types`
  - `documents`
  - `document_versions`
  - `document_approvals`
  - `document_links`
- API contract
  - `GET /documents`
  - `POST /documents`
  - `GET /documents/{id}`
  - `POST /documents/{id}/versions`
  - `PUT /documents/{id}/submit`
  - `PUT /documents/{id}/approve`
  - `PUT /documents/{id}/archive`
  - `GET /document-types`
  - `POST /document-types`
- Frontend contract
  - `/app/documents`
  - `/app/documents/{id}`
  - `/app/document-types`
  - Filters: type, project, phase, status, owner, tag, date
  - Actions: upload, create version, submit, approve, reject, archive, export
- Validation
  - Required metadata: type, owner, project, phase, classification, retention class
  - Archive only after allowed terminal states
- Audit
  - Log upload, version create, submit, approve, reject, archive, export
- Security
  - Enforce classification-aware access for document content and exports
- Performance
  - Large file handling must stream
  - Document register must page and avoid content joins by default
  - Evidence/export packaging should run asynchronously for large payloads

### Phase 3 Implementation Detail
- Data model
  - `requirements`
  - `requirement_versions`
  - `requirement_baselines`
  - `traceability_links`
- API contract
  - `GET /requirements`
  - `POST /requirements`
  - `GET /requirements/{id}`
  - `PUT /requirements/{id}`
  - `PUT /requirements/{id}/submit`
  - `PUT /requirements/{id}/approve`
  - `POST /requirement-baselines`
  - `GET /traceability`
- Frontend contract
  - `/app/requirements`
  - `/app/requirements/{id}`
  - `/app/requirements/baselines`
  - `/app/traceability`
  - Filters: project, status, priority, owner, missing-links
- Validation
  - Requirement baseline requires approved state
  - Traceability links must point to valid governed entities
  - Missing mandatory downstream links must block gate progression
- Audit
  - Log create, update, submit, approve, baseline, link create/remove
- Security
  - Only approved roles can baseline or alter traceability after approval
- Performance
  - Traceability matrix must support server-side filtering and pagination
  - Do not load full graph by default

### Phase 4 Implementation Detail
- Data model
  - `change_requests`
  - `change_impacts`
  - `configuration_items`
  - `baseline_registry`
- API contract
  - `GET /change-requests`
  - `POST /change-requests`
  - `GET /change-requests/{id}`
  - `PUT /change-requests/{id}/submit`
  - `PUT /change-requests/{id}/approve`
  - `GET /configuration-items`
  - `POST /configuration-items`
  - `POST /baseline-registry`
- Frontend contract
  - `/app/change-requests`
  - `/app/change-requests/{id}`
  - `/app/configuration-items`
  - `/app/baselines`
- Validation
  - Baseline changes require approved CR
  - Impact section requires schedule, scope, quality, risk, and security impact
- Audit
  - Log CR lifecycle and baseline supersede events
- Security
  - Override and emergency change paths require explicit approval evidence
- Performance
  - Change and baseline history views must be indexed by project, status, date

All later phases should follow the same structure before implementation starts.

## 2.10 Executable Spec Rollout Strategy

Implementation should proceed phase-by-phase. Before coding starts for any phase, create a build package for that phase containing:

1. Entity and table design
   - Final table names
   - Column names and types
   - Required indexes
   - Foreign keys
   - Ownership by module
2. DTO and API schema
   - Request and response payloads
   - Error response codes
   - Paging and filter contracts
   - Export contracts if applicable
3. State transition matrix
   - Current state
   - Allowed action
   - Next state
   - Required role
   - Required evidence
   - Audit event emitted
4. Screen spec
   - Route
   - Columns
   - Filters
   - Form fields
   - Action buttons
   - Empty/loading/error states
5. Non-functional rules
   - Authorization points
   - Audit requirements
   - Data classification handling
   - Performance limits and async jobs
6. Acceptance pack
   - Functional acceptance criteria
   - Permission acceptance criteria
   - Audit acceptance criteria
   - Security acceptance criteria
   - Performance acceptance criteria

No phase should begin implementation until this build package is approved.

## 2.11 Phase 0 Build Package Template

Use this as the canonical template for turning a phase into executable spec.

### Phase 0 Final Data Design
- Core tables
  - `users`
  - `roles`
  - `permissions`
  - `role_permissions`
  - `user_role_assignments`
  - `security_events`
  - `access_review_records`
- Required audit/security columns on mutable records
  - `created_at`
  - `created_by`
  - `updated_at`
  - `updated_by`
  - `deleted_at` when soft delete is needed
- Required indexes
  - `users(status)`
  - `user_role_assignments(user_id, role_id)` unique
  - `role_permissions(role_id, permission_key)` unique
  - `security_events(event_type, occurred_at)`

### Phase 0 API Schema Expectations
- `GET /admin/users`
  - Query: `page`, `pageSize`, `search`, `status`, `role`
  - Response: paged user list with role summary
- `GET /admin/roles`
  - Response: role list with permission counts
- `PUT /admin/users/{id}/roles`
  - Request: `{ roleIds: string[], reason: string }`
  - Errors: `forbidden`, `validation_failed`, `last_admin_removal_blocked`
- `GET /admin/permissions`
  - Response: permission catalog grouped by module
- `PUT /admin/permissions/matrix`
  - Request: `{ entries: [{ roleId, permissionKey, allowed }], reason: string }`
- `GET /admin/settings`
  - Response: security-sensitive operational settings summary
- `PUT /admin/settings`
  - Request: allowed setting patch + `reason`

### Phase 0 Transition Matrix
- `User role assignment`
  - Active assignment → update roles → Active assignment
  - Required role: security admin or equivalent
  - Required evidence: reason
  - Audit event: `user_role_assignment_updated`
- `Permission matrix`
  - Draft edit → apply → Applied
  - Required role: privileged admin
  - Required evidence: reason
  - Audit event: `permission_matrix_applied`
- `Access review`
  - Scheduled → In Review → Approved → Archived
  - Required role: compliance approver for approval step
  - Required evidence: reviewer, decision, rationale
  - Audit event: `access_review_status_changed`

### Phase 0 Screen Spec Minimum
- `/app/admin/users`
  - Columns: name, email, status, roles, lastUpdated
  - Filters: search, status, role
  - Actions: assign roles, disable user, view audit context
- `/app/admin/permissions`
  - Views: role-centric matrix and permission-centric matrix
  - Actions: edit, reset staged change, apply staged change
- `/app/admin/settings`
  - Actions: view effective settings, update allowed settings, trigger safe refresh actions

### Phase 0 Acceptance Minimum
- Functional
  - Admin can assign and revoke roles within policy
  - Permission matrix updates affect UI and API authorization consistently
- Security
  - Unauthorized role or matrix changes are blocked and logged
  - Last-admin removal protection works
- Audit
  - Every sensitive change has actor, time, reason, outcome
- Performance
  - User list loads with paging
  - Matrix screen does not fetch duplicated permission payloads per interaction

## 2.12 Phase-by-Phase Working Rule

For every next phase from Phase 1 onward:

1. Copy the Phase 0 build package structure.
2. Replace example tables/endpoints/states with the target phase objects.
3. Add field-level schema before coding.
4. Add acceptance criteria before coding.
5. Add performance and security thresholds before coding.
6. Only then start backend and frontend implementation.

## 2.13 Detailed Build Packages for Early Phases

These packages extend the minimum build spec and should be treated as the default working shape for Phase 1 to Phase 4.

### Phase 1 Detailed Build Package

#### Phase 1 Core Entities
- `process_assets`
  - Required fields: `id`, `code`, `name`, `category`, `status`, `owner_user_id`, `effective_from`, `effective_to`, `current_version_id`
- `process_asset_versions`
  - Required fields: `id`, `process_asset_id`, `version_number`, `title`, `summary`, `content_ref`, `status`, `change_summary`, `approved_by`, `approved_at`
- `qa_checklists`
  - Required fields: `id`, `code`, `name`, `scope`, `status`, `owner_user_id`
- `project_plans`
  - Required fields: `id`, `project_id`, `name`, `scope_summary`, `lifecycle_model`, `start_date`, `target_end_date`, `owner_user_id`, `status`
- `stakeholders`
  - Required fields: `id`, `project_id`, `name`, `role_name`, `influence_level`, `contact_channel`, `status`
- `tailoring_records`
  - Required fields: `id`, `project_id`, `requester_user_id`, `requested_change`, `reason`, `impact_summary`, `status`, `approver_user_id`, `approved_at`

#### Phase 1 UI Field Groups
- Process Library
  - List fields: `code`, `name`, `category`, `status`, `currentVersion`, `owner`
  - Detail fields: `code`, `name`, `category`, `owner`, `summary`, `effective dates`, `linked templates`
- QA Checklist
  - List fields: `code`, `name`, `scope`, `status`
  - Detail fields: `items[]`, `mandatory`, `applicable phase`, `evidence rule`
- Project Plan
  - Fields: `project`, `scope`, `milestones`, `lifecycle model`, `roles`, `risk approach`, `quality approach`
- Tailoring Record
  - Fields: `requested deviation`, `justification`, `impacted process asset`, `approver`, `approval rationale`

#### Phase 1 Transition Matrix
- Process asset version
  - Draft → Reviewed → Approved → Active → Deprecated
  - Approval evidence required at `Approved`
- QA checklist
  - Draft → Approved → Active → Deprecated
- Project plan
  - Draft → Review → Approved → Baseline → Superseded
- Tailoring record
  - Draft → Submitted → Approved/Rejected → Applied → Archived

#### Phase 1 Acceptance Focus
- Functional
  - Project cannot claim governance readiness without approved project plan and applicable process assets
- Security
  - Only authorized governance roles may approve tailoring or activate process assets
- Audit
  - Every approval and supersede action produces evidence with actor, time, and rationale
- Performance
  - Process library list and project plan list must filter by status and owner without loading version content blobs

### Phase 2 Detailed Build Package

#### Phase 2 Core Entities
- `document_types`
  - Required fields: `id`, `code`, `name`, `module_owner`, `classification_default`, `retention_class_default`, `status`
- `documents`
  - Required fields: `id`, `document_type_id`, `project_id`, `phase_code`, `owner_user_id`, `current_version_id`, `status`, `classification`, `retention_class`
- `document_versions`
  - Required fields: `id`, `document_id`, `version_number`, `storage_key`, `file_name`, `file_size`, `mime_type`, `uploaded_by`, `uploaded_at`, `status`
- `document_approvals`
  - Required fields: `id`, `document_version_id`, `step_name`, `reviewer_user_id`, `decision`, `decision_reason`, `decided_at`
- `document_links`
  - Required fields: `id`, `source_document_id`, `target_entity_type`, `target_entity_id`, `link_type`

#### Phase 2 UI Field Groups
- Document Register
  - List fields: `docCode`, `title`, `type`, `project`, `phase`, `owner`, `status`, `version`, `classification`
  - Filters: `type`, `project`, `phase`, `status`, `owner`, `classification`, `updatedAt`
- Document Detail
  - Header: `title`, `type`, `project`, `phase`, `status`, `owner`
  - Metadata: `classification`, `retention class`, `tags`, `linked entities`
  - Version panel: `version number`, `uploaded by`, `uploaded at`, `approval status`
- Document Type Setup
  - Fields: `code`, `name`, `module owner`, `default classification`, `default retention`, `approval required`

#### Phase 2 Transition Matrix
- Document
  - Draft → Review → Approved/Rejected → Baseline → Archived
- Document version
  - Uploaded → Submitted → Approved/Rejected → Superseded
- Document type
  - Active → Deprecated

#### Phase 2 Acceptance Focus
- Functional
  - Documents support version history, approval chain, and links to governed entities
- Security
  - Classified documents enforce read/export restrictions by role
- Audit
  - Download, export, approval, and archive are auditable
- Performance
  - Binary content is never loaded in list endpoints
  - Large export packaging is asynchronous and status-tracked

### Phase 3 Detailed Build Package

#### Phase 3 Core Entities
- `requirements`
  - Required fields: `id`, `project_id`, `code`, `title`, `description`, `priority`, `owner_user_id`, `status`, `current_version_id`
- `requirement_versions`
  - Required fields: `id`, `requirement_id`, `version_number`, `business_reason`, `acceptance_criteria`, `security_impact`, `performance_impact`, `status`
- `requirement_baselines`
  - Required fields: `id`, `project_id`, `baseline_name`, `approved_by`, `approved_at`, `status`
- `traceability_links`
  - Required fields: `id`, `source_type`, `source_id`, `target_type`, `target_id`, `link_rule`, `created_by`

#### Phase 3 UI Field Groups
- Requirement Register
  - List fields: `code`, `title`, `priority`, `owner`, `status`, `baseline status`, `missing link count`
  - Filters: `project`, `priority`, `status`, `owner`, `baseline`, `missing downstream links`
- Requirement Detail
  - Fields: `title`, `description`, `business reason`, `acceptance criteria`, `risk note`, `security impact`, `performance impact`
  - Panels: `linked documents`, `linked test evidence`, `change history`
- Traceability Matrix
  - Dimensions: `requirement`, `document`, `test`, `change request`, `release`
  - Controls: `filter by missing coverage`, `filter by baseline`, `filter by project`

#### Phase 3 Transition Matrix
- Requirement
  - Draft → Review → Approved → Baselined → Superseded
- Requirement baseline
  - Proposed → Approved → Locked → Superseded
- Traceability link
  - Created → Validated → Broken → Resolved

#### Phase 3 Acceptance Focus
- Functional
  - Every approved requirement can be traced to required downstream artifacts
- Security
  - Only authorized roles can modify approved requirements or baseline content
- Audit
  - Baseline, approval, and traceability changes are fully logged
- Performance
  - Traceability queries use server-side filtering and must not materialize the full graph by default

### Phase 4 Detailed Build Package

#### Phase 4 Core Entities
- `change_requests`
  - Required fields: `id`, `project_id`, `code`, `title`, `requested_by`, `reason`, `status`, `priority`, `target_baseline_id`
- `change_impacts`
  - Required fields: `id`, `change_request_id`, `scope_impact`, `schedule_impact`, `quality_impact`, `security_impact`, `performance_impact`, `risk_impact`
- `configuration_items`
  - Required fields: `id`, `project_id`, `code`, `name`, `item_type`, `owner_module`, `status`, `baseline_ref`
- `baseline_registry`
  - Required fields: `id`, `project_id`, `baseline_name`, `baseline_type`, `source_entity_type`, `source_entity_id`, `status`, `approved_by`

#### Phase 4 UI Field Groups
- Change Request Register
  - List fields: `code`, `title`, `priority`, `requester`, `status`, `target baseline`, `approval status`
- Change Request Detail
  - Fields: `reason`, `business justification`, `impact summary`, `linked requirements`, `linked configuration items`, `decision rationale`
- Configuration Items
  - Fields: `code`, `name`, `type`, `owner module`, `status`, `related baseline`, `last change`
- Baseline Registry
  - Fields: `baseline name`, `type`, `source object`, `approved by`, `approved at`, `superseded by`

#### Phase 4 Transition Matrix
- Change request
  - Draft → Submitted → In Review → Approved/Rejected → Implemented → Closed
- Configuration item
  - Draft → Approved → Baseline → Superseded
- Baseline registry record
  - Proposed → Approved → Locked → Superseded

#### Phase 4 Acceptance Focus
- Functional
  - No governed baseline change can occur without approved CR linkage
- Security
  - Emergency or override change path requires elevated role and explicit evidence
- Audit
  - All change and baseline supersede actions are searchable and exportable
- Performance
  - Change history and baseline history are indexed by project, status, date, and code

### Phase 5 Detailed Build Package

#### Phase 5 Core Entities
- `risks`
  - Required fields: `id`, `project_id`, `code`, `title`, `description`, `probability`, `impact`, `owner_user_id`, `mitigation_plan`, `status`
- `risk_reviews`
  - Required fields: `id`, `risk_id`, `reviewed_by`, `reviewed_at`, `decision`, `notes`
- `issues`
  - Required fields: `id`, `project_id`, `code`, `title`, `description`, `owner_user_id`, `due_date`, `status`, `severity`
- `issue_actions`
  - Required fields: `id`, `issue_id`, `action_description`, `assigned_to`, `due_date`, `status`

#### Phase 5 UI Field Groups
- Risk Register
  - List fields: `code`, `title`, `probability`, `impact`, `owner`, `status`, `next review`
  - Filters: `project`, `status`, `owner`, `risk level`
- Risk Detail
  - Fields: `description`, `cause`, `effect`, `mitigation plan`, `contingency plan`, `owner`, `review history`
- Issue / Action Log
  - List fields: `code`, `title`, `severity`, `owner`, `due date`, `status`, `open actions`
  - Detail fields: `root issue`, `actions[]`, `dependencies`, `resolution summary`

#### Phase 5 Transition Matrix
- Risk
  - Draft → Assessed → Mitigated → Closed
- Issue
  - Open → In Progress → Resolved → Closed
- Issue action
  - Open → In Progress → Completed → Verified

#### Phase 5 Acceptance Focus
- Functional
  - Risks and issues must support ownership, due dates, and review history
- Security
  - Sensitive issue content must respect role-based visibility if linked to incidents or access findings
- Audit
  - Status changes, owner changes, and action closures are logged
- Performance
  - Risk and issue lists must filter by owner, severity, and due date with paging

### Phase 6 Detailed Build Package

#### Phase 6 Core Entities
- `meeting_records`
  - Required fields: `id`, `project_id`, `meeting_type`, `title`, `meeting_at`, `facilitator_user_id`, `status`
- `meeting_minutes`
  - Required fields: `id`, `meeting_record_id`, `summary`, `decisions_summary`, `actions_summary`, `status`
- `meeting_attendees`
  - Required fields: `id`, `meeting_record_id`, `user_id`, `attendance_status`
- `decisions`
  - Required fields: `id`, `project_id`, `code`, `title`, `decision_type`, `rationale`, `approved_by`, `status`

#### Phase 6 UI Field Groups
- MOM Register
  - List fields: `title`, `meeting type`, `meeting date`, `facilitator`, `status`
  - Filters: `project`, `type`, `date`, `status`
- MOM Detail
  - Fields: `agenda`, `discussion summary`, `decisions`, `actions`, `attendees`
- Decision Log
  - List fields: `code`, `title`, `decision type`, `approved by`, `status`, `linked meeting`
  - Detail fields: `rationale`, `alternatives considered`, `impacted artifacts`

#### Phase 6 Transition Matrix
- Meeting record
  - Draft → Approved → Archived
- Meeting minutes
  - Draft → Reviewed → Approved → Archived
- Decision
  - Proposed → Approved → Applied → Archived

#### Phase 6 Acceptance Focus
- Functional
  - Meetings produce decisions and actions that can link back to requirements and change items
- Security
  - Restricted meetings or decisions can be hidden by permission and classification
- Audit
  - Decision approval, edit history, and attendee confirmation are logged when applicable
- Performance
  - Meeting and decision searches must filter by project, type, and date without loading full text content in list queries

### Phase 7 Detailed Build Package

#### Phase 7 Core Entities
- `test_plans`
  - Required fields: `id`, `project_id`, `code`, `title`, `scope_summary`, `owner_user_id`, `status`
- `test_cases`
  - Required fields: `id`, `test_plan_id`, `code`, `title`, `preconditions`, `expected_result`, `status`
- `test_executions`
  - Required fields: `id`, `test_case_id`, `executed_by`, `executed_at`, `result`, `evidence_ref`, `notes`
- `uat_signoffs`
  - Required fields: `id`, `project_id`, `release_id`, `submitted_by`, `approved_by`, `status`, `decision_reason`

#### Phase 7 UI Field Groups
- Test Plan
  - List fields: `code`, `title`, `owner`, `status`, `coverage status`
  - Detail fields: `scope`, `linked requirements`, `entry criteria`, `exit criteria`
- Test Case & Execution
  - List fields: `code`, `title`, `status`, `latest result`, `linked requirement`
  - Detail fields: `steps`, `expected results`, `execution history`, `evidence`
- UAT Sign-off
  - Fields: `release`, `scope`, `decision`, `decision reason`, `linked evidence`

#### Phase 7 Transition Matrix
- Test plan
  - Draft → Review → Approved → Baseline
- Test case
  - Draft → Ready → Active → Retired
- Test execution
  - Planned → Executed → Passed/Failed → Retest
- UAT sign-off
  - Draft → Submitted → Approved/Rejected

#### Phase 7 Acceptance Focus
- Functional
  - Requirements must be traceable to test coverage and execution evidence
- Security
  - Test evidence containing sensitive content follows document classification and access rules
- Audit
  - Execution results, evidence references, and UAT decisions are logged
- Performance
  - Test execution history must page and filter by result, executor, and date

### Phase 8 Detailed Build Package

#### Phase 8 Core Entities
- `audit_events`
  - Required fields: `id`, `occurred_at`, `actor_user_id`, `entity_type`, `entity_id`, `action`, `outcome`, `reason`, `metadata`
- `audit_findings`
  - Required fields: `id`, `audit_plan_id`, `code`, `title`, `description`, `severity`, `status`, `owner_user_id`
- `audit_plans`
  - Required fields: `id`, `project_id`, `title`, `scope`, `planned_at`, `status`, `owner_user_id`
- `evidence_exports`
  - Required fields: `id`, `requested_by`, `scope_type`, `scope_ref`, `requested_at`, `status`, `output_ref`

#### Phase 8 UI Field Groups
- Audit Log
  - List fields: `occurred at`, `actor`, `entity type`, `action`, `outcome`, `reason`
  - Filters: `project`, `entity type`, `action`, `actor`, `date`, `outcome`
- Process Audit Plan & Findings
  - Plan fields: `scope`, `criteria`, `schedule`, `owner`
  - Finding fields: `severity`, `description`, `owner`, `due date`, `resolution`
- Evidence Export
  - Fields: `scope`, `date range`, `included artifact types`, `request status`, `output`

#### Phase 8 Transition Matrix
- Audit plan
  - Planned → In Review → Findings Issued → Closed
- Audit finding
  - Open → Action Planned → In Progress → Verified → Closed
- Evidence export
  - Requested → Generated → Downloaded/Expired

#### Phase 8 Acceptance Focus
- Functional
  - Audit evidence can be searched, filtered, and exported by project, release, and date window
- Security
  - Exported evidence must honor data classification and requester permissions
- Audit
  - Audit events are immutable and queryable
- Performance
  - Audit queries and evidence packaging must use paging and asynchronous jobs for large result sets

### Phase 9 Detailed Build Package

#### Phase 9 Core Entities
- `metric_definitions`
  - Required fields: `id`, `code`, `name`, `metric_type`, `owner_user_id`, `target_value`, `threshold_value`, `status`
- `metric_collection_schedules`
  - Required fields: `id`, `metric_definition_id`, `collection_frequency`, `collector_type`, `status`
- `metric_results`
  - Required fields: `id`, `metric_definition_id`, `measured_at`, `measured_value`, `status`, `source_ref`
- `quality_gate_results`
  - Required fields: `id`, `project_id`, `gate_type`, `evaluated_at`, `result`, `reason`, `override_reason`

#### Phase 9 UI Field Groups
- Metric Definitions
  - List fields: `code`, `name`, `owner`, `target`, `threshold`, `status`
- Data Collection Schedule
  - Fields: `metric`, `frequency`, `collector`, `next run`, `status`
- Metrics Dashboard
  - Widgets: `trend`, `current vs target`, `breach count`, `open actions`
- Quality Gate Status
  - Fields: `gate type`, `project`, `result`, `evaluated at`, `blocking reason`, `override`

#### Phase 9 Transition Matrix
- Metric definition
  - Draft → Approved → Active → Deprecated
- Collection schedule
  - Draft → Active → Archived
- Quality gate result
  - Pending → Passed/Failed → Overridden

#### Phase 9 Acceptance Focus
- Functional
  - Quality gates must evaluate measurable data and block progression when required
- Security
  - Only authorized users can override quality gates
- Audit
  - Metric threshold changes and gate overrides are auditable
- Performance
  - Dashboard aggregation must avoid per-widget fan-out

### Phase 10 Detailed Build Package

#### Phase 10 Core Entities
- `project_role_definitions`
  - Required fields: `id`, `project_id`, `role_code`, `role_name`, `status`
- `project_team_assignments`
  - Required fields: `id`, `project_id`, `user_id`, `project_role_id`, `start_date`, `end_date`, `status`
- `phase_approval_requests`
  - Required fields: `id`, `project_id`, `phase_code`, `submitted_by`, `submitted_at`, `status`, `decision_reason`

#### Phase 10 UI Field Groups
- Project Roles
  - List fields: `role code`, `role name`, `status`, `assigned count`
- Team Assignment
  - List fields: `user`, `role`, `start`, `end`, `status`
- Project Phase Approval
  - Fields: `phase`, `entry criteria`, `required evidence`, `decision`, `decision reason`

#### Phase 10 Transition Matrix
- Project role
  - Active → Archived
- Team assignment
  - Active → Removed
- Phase approval request
  - Draft → Submitted → Approved/Rejected → Baseline

#### Phase 10 Acceptance Focus
- Functional
  - Projects must declare accountable roles and attach evidence before phase approval
- Security
  - Approval role must be separate where segregation-of-duties is required
- Audit
  - Assignment changes and phase approvals are auditable
- Performance
  - Team and role queries must support project-scoped paging and filtering

### Phase 11 Detailed Build Package

#### Phase 11 Core Entities
- `master_data_items`
  - Required fields: `id`, `domain`, `code`, `name`, `status`, `display_order`
- `master_data_changes`
  - Required fields: `id`, `master_data_item_id`, `change_type`, `changed_by`, `changed_at`, `reason`

#### Phase 11 UI Field Groups
- Master Data
  - List fields: `domain`, `code`, `name`, `status`, `last changed`
  - Detail fields: `name`, `code`, `display order`, `status`, `change reason`

#### Phase 11 Transition Matrix
- Master data item
  - Active → Archived

#### Phase 11 Acceptance Focus
- Functional
  - Master data supports controlled changes and is reusable across modules
- Security
  - Only authorized admins can mutate master data
- Audit
  - Every master data change records actor and reason
- Performance
  - Lookup endpoints should be optimized for option-loading use cases

### Phase 12 Detailed Build Package

#### Phase 12 Core Entities
- `security_reviews`
  - Required fields: `id`, `scope_type`, `scope_ref`, `planned_at`, `reviewed_by`, `status`, `summary`
- `access_reviews`
  - Required fields: `id`, `scope_type`, `scope_ref`, `review_cycle`, `reviewed_by`, `status`
- `external_dependencies`
  - Required fields: `id`, `name`, `dependency_type`, `owner_user_id`, `criticality`, `status`, `review_due_at`
- `configuration_audits`
  - Required fields: `id`, `scope_ref`, `planned_at`, `status`, `finding_count`

#### Phase 12 UI Field Groups
- Access Review
  - Fields: `scope`, `review cycle`, `assigned reviewer`, `decision`, `rationale`
- Security Review
  - Fields: `scope`, `controls reviewed`, `findings`, `status`
- External Dependency Register
  - Fields: `name`, `type`, `owner`, `criticality`, `review due`, `status`
- Configuration Audit Log
  - Fields: `scope`, `plan date`, `status`, `finding count`

#### Phase 12 Transition Matrix
- Access review
  - Scheduled → In Review → Approved → Archived
- Security review
  - Planned → In Review → Findings Issued → Closed
- External dependency
  - Active → Review Due → Updated → Archived
- Configuration audit
  - Planned → In Review → Findings Issued → Closed

#### Phase 12 Acceptance Focus
- Functional
  - Security, dependency, and configuration reviews must produce trackable outcomes
- Security
  - Access review outcomes must influence access recertification or corrective actions
- Audit
  - Review decisions and findings are exportable
- Performance
  - Review logs must be filterable by due date, status, and scope

### Phase 13 Detailed Build Package

#### Phase 13 Core Entities
- `raci_maps`
  - Required fields: `id`, `process_code`, `role_name`, `responsibility_type`, `status`
- `approval_evidence_logs`
  - Required fields: `id`, `entity_type`, `entity_id`, `approver_user_id`, `approved_at`, `reason`, `outcome`
- `workflow_override_logs`
  - Required fields: `id`, `entity_type`, `entity_id`, `requested_by`, `approved_by`, `reason`, `occurred_at`
- `sla_rules`
  - Required fields: `id`, `scope_type`, `scope_ref`, `target_duration_hours`, `escalation_policy_id`, `status`
- `retention_policies`
  - Required fields: `id`, `policy_code`, `applies_to`, `retention_period_days`, `status`

#### Phase 13 UI Field Groups
- RACI Map
  - Fields: `process`, `role`, `R/A/C/I`, `status`
- Approval Evidence Log
  - Fields: `entity`, `approver`, `decision`, `reason`, `timestamp`
- Workflow Override Log
  - Fields: `entity`, `requester`, `approver`, `reason`, `timestamp`
- SLA & Escalation Rules
  - Fields: `scope`, `target duration`, `breach action`, `status`
- Data Retention Policy
  - Fields: `policy code`, `scope`, `retention`, `archive rule`, `status`

#### Phase 13 Transition Matrix
- RACI map
  - Draft → Approved → Active → Archived
- SLA rule
  - Draft → Approved → Active → Archived
- Retention policy
  - Draft → Approved → Active → Archived

#### Phase 13 Acceptance Focus
- Functional
  - Governance rules must be visible, controlled, and referenced by workflows
- Security
  - Override evidence and SLA breach actions must be protected from tampering
- Audit
  - Approval and override evidence must be immutable
- Performance
  - Governance logs must remain searchable at scale

### Phase 14 Detailed Build Package

#### Phase 14 Core Entities
- `releases`
  - Required fields: `id`, `project_id`, `release_code`, `title`, `planned_at`, `released_at`, `status`
- `deployment_checklists`
  - Required fields: `id`, `release_id`, `checklist_item`, `owner_user_id`, `status`, `completed_at`
- `release_notes`
  - Required fields: `id`, `release_id`, `summary`, `included_changes`, `known_issues`, `status`

#### Phase 14 UI Field Groups
- Release Register
  - List fields: `release code`, `title`, `planned date`, `released date`, `status`
- Deployment Checklist
  - Fields: `item`, `owner`, `status`, `completed at`, `evidence`
- Release Notes
  - Fields: `summary`, `included changes`, `known issues`, `publish status`

#### Phase 14 Transition Matrix
- Release
  - Draft → Approved → Released → Archived
- Deployment checklist item
  - Draft → Reviewed → Approved → Executed
- Release notes
  - Draft → Approved → Published → Archived

#### Phase 14 Acceptance Focus
- Functional
  - Release cannot move to released without required checklist and gate evidence
- Security
  - Release approvals require authorized approvers
- Audit
  - Release publication and deployment execution are logged
- Performance
  - Release history queries support project/date filtering and paging

### Phase 15 Detailed Build Package

#### Phase 15 Core Entities
- `defects`
  - Required fields: `id`, `project_id`, `code`, `title`, `description`, `severity`, `owner_user_id`, `status`, `detected_in_phase`
- `non_conformances`
  - Required fields: `id`, `project_id`, `code`, `title`, `description`, `source_type`, `owner_user_id`, `status`, `corrective_action_ref`

#### Phase 15 UI Field Groups
- Defect Log
  - List fields: `code`, `title`, `severity`, `owner`, `status`, `phase found`
  - Filters: `project`, `severity`, `status`, `owner`
- Non-Conformance Log
  - List fields: `code`, `title`, `source`, `owner`, `status`, `corrective action`
  - Detail fields: `description`, `root cause`, `resolution`, `linked findings`

#### Phase 15 Transition Matrix
- Defect
  - Open → In Progress → Resolved → Closed
- Non-conformance
  - Open → In Review → Corrective Action → Closed

#### Phase 15 Acceptance Focus
- Functional
  - Defects and non-conformances must link to corrective actions and affected artifacts
- Security
  - Sensitive defect details must honor permission and classification boundaries
- Audit
  - State transitions and ownership changes are logged
- Performance
  - Defect and NC logs must support project/severity/date paging filters

### Phase 16 Detailed Build Package

#### Phase 16 Core Entities
- `suppliers`
  - Required fields: `id`, `name`, `supplier_type`, `owner_user_id`, `status`, `criticality`
- `supplier_agreements`
  - Required fields: `id`, `supplier_id`, `agreement_type`, `effective_from`, `effective_to`, `status`, `evidence_ref`

#### Phase 16 UI Field Groups
- Supplier Register
  - List fields: `name`, `type`, `owner`, `criticality`, `status`
- SLA/Contract Evidence
  - Fields: `supplier`, `agreement type`, `effective dates`, `SLA terms`, `evidence`, `status`

#### Phase 16 Transition Matrix
- Supplier
  - Active → Review Due → Updated → Archived
- Supplier agreement
  - Draft → Approved → Active → Archived

#### Phase 16 Acceptance Focus
- Functional
  - External dependencies must be traceable to supplier ownership and governing agreements
- Security
  - Agreement evidence access must be restricted to authorized roles
- Audit
  - Agreement approval and supplier review changes are logged
- Performance
  - Supplier and agreement lists support filtering by criticality, owner, and due review date

### Phase 17 Detailed Build Package

#### Phase 17 Core Entities
- `metric_reviews`
  - Required fields: `id`, `project_id`, `review_period`, `reviewed_by`, `status`, `summary`
- `trend_reports`
  - Required fields: `id`, `project_id`, `metric_definition_id`, `period_from`, `period_to`, `status`, `report_ref`

#### Phase 17 UI Field Groups
- Metrics Review Log
  - List fields: `project`, `period`, `reviewer`, `status`, `actions tracked`
- Trend Analysis Report
  - Fields: `metric`, `period`, `trend direction`, `variance`, `recommended action`

#### Phase 17 Transition Matrix
- Metric review
  - Planned → Reviewed → Actions Tracked → Closed
- Trend report
  - Draft → Approved → Archived

#### Phase 17 Acceptance Focus
- Functional
  - Management reviews must connect measured trends to decisions and follow-up actions
- Security
  - Review data access follows metrics ownership and project visibility
- Audit
  - Review closure and approved trend conclusions are logged
- Performance
  - Trend calculation should use pre-aggregated or indexed data paths for large periods

### Phase 18 Detailed Build Package

#### Phase 18 Core Entities
- `lessons_learned`
  - Required fields: `id`, `project_id`, `title`, `summary`, `lesson_type`, `owner_user_id`, `status`, `source_ref`

#### Phase 18 UI Field Groups
- Lessons Learned
  - List fields: `title`, `type`, `project`, `owner`, `status`, `published at`
  - Detail fields: `context`, `what happened`, `what to repeat`, `what to avoid`, `linked evidence`

#### Phase 18 Transition Matrix
- Lesson
  - Draft → Reviewed → Published → Archived

#### Phase 18 Acceptance Focus
- Functional
  - Lessons can be linked back to actual project evidence or findings
- Security
  - Published visibility follows classification and project access
- Audit
  - Publication and archive events are logged
- Performance
  - Knowledge search must support keyword, type, project, and date filters

### Phase 19 Detailed Build Package

#### Phase 19 Core Entities
- `access_recertification_schedules`
  - Required fields: `id`, `scope_type`, `scope_ref`, `planned_at`, `review_owner_user_id`, `status`
- `access_recertification_decisions`
  - Required fields: `id`, `schedule_id`, `subject_user_id`, `decision`, `reason`, `decided_by`, `decided_at`

#### Phase 19 UI Field Groups
- Access Recertification Schedule
  - List fields: `scope`, `planned date`, `review owner`, `status`, `completed count`
  - Detail fields: `subjects`, `decisions`, `rationale`, `exceptions`

#### Phase 19 Transition Matrix
- Recertification schedule
  - Planned → In Review → Approved → Completed
- Recertification decision
  - Pending → Kept/Revoked/Adjusted

#### Phase 19 Acceptance Focus
- Functional
  - Access recertification decisions must feed actual access governance workflows
- Security
  - Revocation and adjustment decisions are protected and traceable
- Audit
  - Decision rationale and actor are always logged
- Performance
  - Recertification lists support scoping by role, system area, and due date

### Phase 20 Detailed Build Package

#### Phase 20 Core Entities
- `architecture_records`
  - Required fields: `id`, `project_id`, `title`, `architecture_type`, `owner_user_id`, `status`, `current_version_id`
- `design_reviews`
  - Required fields: `id`, `architecture_record_id`, `review_type`, `reviewed_by`, `status`, `decision_reason`
- `integration_reviews`
  - Required fields: `id`, `scope_ref`, `integration_type`, `reviewed_by`, `status`, `decision_reason`

#### Phase 20 UI Field Groups
- Architecture Register
  - List fields: `title`, `type`, `owner`, `status`, `current version`
- Design Review
  - Fields: `design summary`, `concerns`, `decision`, `decision reason`, `evidence`
- Integration Review
  - Fields: `integration scope`, `risks`, `dependency impact`, `decision`, `evidence`

#### Phase 20 Transition Matrix
- Architecture record
  - Draft → Reviewed → Approved → Active → Superseded
- Design review
  - Draft → In Review → Approved/Rejected → Baseline
- Integration review
  - Draft → In Review → Approved/Rejected → Applied

#### Phase 20 Acceptance Focus
- Functional
  - Architecture and integration decisions must be reviewable before downstream implementation gates
- Security
  - Security-sensitive architecture changes require explicit review evidence
- Audit
  - Review decisions and superseded architecture versions are retained
- Performance
  - Design and architecture lists must not load large document content into list queries

### Phase 21 Detailed Build Package

#### Phase 21 Core Entities
- `security_incidents`
  - Required fields: `id`, `project_id`, `code`, `title`, `severity`, `reported_at`, `owner_user_id`, `status`
- `vulnerability_records`
  - Required fields: `id`, `asset_ref`, `title`, `severity`, `identified_at`, `owner_user_id`, `status`
- `secret_rotations`
  - Required fields: `id`, `secret_scope`, `planned_at`, `rotated_at`, `verified_by`, `status`
- `privileged_access_events`
  - Required fields: `id`, `requested_by`, `approved_by`, `used_by`, `requested_at`, `status`, `reason`
- `data_classification_policies`
  - Required fields: `id`, `policy_code`, `classification_level`, `scope`, `status`

#### Phase 21 UI Field Groups
- Security Incident Register
  - List fields: `code`, `title`, `severity`, `owner`, `status`, `reported at`
- Vulnerability & Patch Register
  - Fields: `asset`, `severity`, `identified at`, `patch due`, `status`
- Secret Rotation Register
  - Fields: `scope`, `planned`, `rotated`, `verified by`, `status`
- Privileged Access Log
  - Fields: `requester`, `approver`, `user`, `reason`, `status`, `used at`
- Data Classification Policy
  - Fields: `policy code`, `level`, `scope`, `status`

#### Phase 21 Transition Matrix
- Security incident
  - Reported → Assessed → Contained → Resolved → Closed
- Vulnerability
  - Open → Assessed → Scheduled → Patched → Verified → Closed
- Secret rotation
  - Planned → Rotated → Verified → Archived
- Privileged access
  - Requested → Approved → Used → Reviewed → Closed
- Classification policy
  - Draft → Approved → Active → Archived

#### Phase 21 Acceptance Focus
- Functional
  - Security operational events must connect to CAPA, risk, and audit evidence where relevant
- Security
  - Privileged access and secret handling are fully controlled and reviewable
- Audit
  - Incident lifecycle and privileged access lifecycle are immutable and searchable
- Performance
  - Security logs must support time-range and severity filtering without full scan defaults

### Phase 22 Detailed Build Package

#### Phase 22 Core Entities
- `performance_baselines`
  - Required fields: `id`, `scope_type`, `scope_ref`, `metric_name`, `target_value`, `threshold_value`, `status`
- `capacity_reviews`
  - Required fields: `id`, `scope_ref`, `review_period`, `reviewed_by`, `status`, `summary`
- `slow_operation_reviews`
  - Required fields: `id`, `operation_type`, `operation_key`, `observed_latency_ms`, `status`, `owner_user_id`
- `performance_gate_results`
  - Required fields: `id`, `scope_ref`, `evaluated_at`, `result`, `reason`, `override_reason`

#### Phase 22 UI Field Groups
- Performance Baseline
  - Fields: `scope`, `metric`, `target`, `threshold`, `status`
- Capacity Review
  - Fields: `period`, `scope`, `summary`, `actions`
- Slow Query / API Review
  - Fields: `operation`, `latency`, `frequency`, `owner`, `status`
- Performance Regression Gate
  - Fields: `scope`, `result`, `reason`, `override`, `evidence`

#### Phase 22 Transition Matrix
- Performance baseline
  - Draft → Approved → Active → Superseded
- Capacity review
  - Planned → Reviewed → Actioned → Closed
- Slow operation review
  - Open → Investigating → Optimized → Verified → Closed
- Performance gate
  - Pending → Passed/Failed → Overridden

#### Phase 22 Acceptance Focus
- Functional
  - Performance findings must feed optimization or corrective work before closure
- Security
  - Operational telemetry access should remain role-restricted where needed
- Audit
  - Gate overrides and performance threshold changes are logged
- Performance
  - Review screens must aggregate from pre-filtered or indexed sources where possible

### Phase 23 Detailed Build Package

#### Phase 23 Core Entities
- `backup_evidence`
  - Required fields: `id`, `backup_scope`, `executed_at`, `executed_by`, `status`, `evidence_ref`
- `restore_verifications`
  - Required fields: `id`, `backup_evidence_id`, `executed_at`, `executed_by`, `status`, `result_summary`
- `dr_drills`
  - Required fields: `id`, `scope_ref`, `planned_at`, `executed_at`, `status`, `finding_count`
- `legal_holds`
  - Required fields: `id`, `scope_type`, `scope_ref`, `placed_at`, `placed_by`, `status`, `reason`

#### Phase 23 UI Field Groups
- Backup Evidence
  - Fields: `scope`, `executed at`, `operator`, `status`, `evidence`
- Restore Verification
  - Fields: `backup`, `executed at`, `operator`, `result`, `notes`
- DR Drill Log
  - Fields: `scope`, `planned`, `executed`, `status`, `finding count`
- Legal Hold Register
  - Fields: `scope`, `placed at`, `placed by`, `status`, `reason`

#### Phase 23 Transition Matrix
- Backup evidence
  - Planned → Completed → Verified → Archived
- Restore verification
  - Planned → Executed → Verified → Closed
- DR drill
  - Planned → Executed → Findings Issued → Closed
- Legal hold
  - Active → Released → Archived

#### Phase 23 Acceptance Focus
- Functional
  - Backup and restore evidence must be reviewable by audit/compliance
- Security
  - Legal hold and recovery records must be restricted to authorized users
- Audit
  - Backup, restore, and DR execution are time-stamped and immutable
- Performance
  - Evidence lists must filter by scope and date with paging

### Phase 24 Detailed Build Package

#### Phase 24 Core Entities
- `capa_records`
  - Required fields: `id`, `source_type`, `source_ref`, `title`, `owner_user_id`, `root_cause_summary`, `status`
- `capa_actions`
  - Required fields: `id`, `capa_record_id`, `action_description`, `assigned_to`, `due_date`, `status`
- `notification_queue`
  - Required fields: `id`, `channel`, `target_ref`, `payload_ref`, `queued_at`, `status`, `retry_count`
- `escalation_events`
  - Required fields: `id`, `scope_type`, `scope_ref`, `triggered_at`, `trigger_reason`, `escalated_to`, `status`

#### Phase 24 UI Field Groups
- CAPA Register
  - List fields: `source`, `title`, `owner`, `status`, `due date`, `open actions`
- Notification Queue
  - Fields: `channel`, `target`, `queued at`, `status`, `retry count`, `last error`
- Escalation History
  - Fields: `scope`, `triggered at`, `reason`, `escalated to`, `status`

#### Phase 24 Transition Matrix
- CAPA
  - Open → Root Cause Analysis → Action Planned → Action In Progress → Verified → Closed
- CAPA action
  - Open → In Progress → Completed → Verified
- Notification queue item
  - Queued → Sent/Failed → Retried → Closed
- Escalation event
  - Triggered → Acknowledged → Resolved → Closed

#### Phase 24 Acceptance Focus
- Functional
  - Findings, incidents, risks, and defects must be able to drive CAPA to closure
- Security
  - Notification and escalation records must not expose sensitive payload details to unauthorized users
- Audit
  - CAPA progression, retries, and escalations are logged and reviewable
- Performance
  - Queue views and escalation history must support paging and operational filtering

## 2.1 Recommended Delivery Order

The full phase list is broad. For practical implementation, start in this order:

1. Phase 0: Security & Access Foundation
2. Phase 1: Process Assets & Governance Baseline
3. Phase 2: Document Governance Core
4. Phase 3: Requirements + Traceability
5. Phase 4: Change Control + Configuration Management
6. Phase 7: Verification & Validation
7. Phase 8: Audit & Compliance
8. Phase 9: Metrics & Quality Gates
9. Phase 10: Project Governance Hardening
10. Phase 5 and Phase 6: Risk, Issue, Meetings, Decisions
11. Phase 14 and Phase 15: Release, Deployment, Defect, Non-Conformance
12. Remaining phases after core flow is stable

## 2.2 Implementation Backlog by Phase

### Phase 0 Backlog
- Backend
  - Add role/permission catalog and enforce action-level authorization
  - Add audit event model for login, logout, permission denied, settings change
  - Add Keycloak integration settings validation
  - Add Redis key strategy and expiration policy for session/cache use
- Frontend
  - Add route-level authorization guard
  - Add action-level UI guard for buttons, menus, and pages
  - Add unauthorized and expired-session states
- 3rd-party
  - Finalize Keycloak client/realm roles
  - Finalize Redis environment and retention policy
- Done criteria
  - No protected screen or action is accessible without permission
  - Permission failures are logged
  - Keycloak and Redis settings are validated at startup

### Phase 1 Backlog
- Backend
  - Add entities/contracts for process library, QA checklist, project plan, tailoring, stakeholders
  - Add approval evidence storage for tailoring
- Frontend
  - Build list/detail/edit pages for process assets and project planning
  - Add approval UI for tailoring
- Done criteria
  - Teams can define the operating process and project plan inside the system
  - Tailoring approval stores approver, time, and rationale

### Phase 2 Backlog
- Backend
  - Add document type, document metadata, versioning, and approval workflow
  - Add MinIO-backed file storage abstraction
  - Add document search/filter query model
- Frontend
  - Build document type setup, register, and detail screens
  - Add upload, version history, approval, and archive actions
- 3rd-party
  - Integrate MinIO
- Done criteria
  - Documents can be uploaded, versioned, approved, and archived with metadata
  - File storage is externalized through storage abstraction

### Phase 3 Backlog
- Backend
  - Add requirement model, baseline model, and traceability link model
  - Add validation for mandatory links at lifecycle checkpoints
- Frontend
  - Build requirement register/detail/baseline screens
  - Build traceability matrix screen
- Done criteria
  - Requirements can be baselined and linked to downstream artifacts
  - Traceability gaps are visible before gate approvals

### Phase 4 Backlog
- Backend
  - Add CR workflow, impact analysis, configuration items, baseline registry
  - Add baseline lock rules and supersede rules
- Frontend
  - Build CR register/detail, configuration item, and baseline registry screens
- Done criteria
  - Controlled changes require CR approval
  - Baselines are immutable once approved except through governed change

### Phase 7 Backlog
- Backend
  - Add test plan, test case, execution result, and UAT sign-off models
  - Add requirement-to-test coverage checks
- Frontend
  - Build test plan, execution, and UAT screens
- Done criteria
  - Test evidence is attached to requirements and releases
  - UAT sign-off is traceable and approval-based

### Phase 8 Backlog
- Backend
  - Add audit log query model, evidence export packaging, audit findings model
  - Ensure critical business actions produce auditable events
- Frontend
  - Build audit log, export, and audit findings screens
- 3rd-party
  - Integrate log backend if needed
- Done criteria
  - Audit evidence can be exported per project/release/audit window
  - Critical workflow actions are fully traceable

### Phase 9 Backlog
- Backend
  - Add metric definition model, collection schedule, quality gate engine
  - Add performance baseline and threshold evaluation
- Frontend
  - Build metrics definition, dashboard, and quality gate screens
- 3rd-party
  - Connect Prometheus/Grafana views where needed
- Done criteria
  - Quality gates can block progression based on objective data
  - Performance thresholds are visible and enforced

## 2.3 Phase Definition of Done

Every phase is complete only when all of the following are true:

1. Backend contracts, entities, workflow transitions, and authorization rules are implemented.
2. Frontend screens support the intended user actions and permission boundaries.
3. Audit events are emitted for create, update, approve, reject, archive, override, and delete actions where applicable.
4. Validation rules enforce required metadata, traceability, and state transitions.
5. Tests exist for critical workflows and permission boundaries.
6. Performance-sensitive queries, lists, and exports are reviewed for scale risks.
7. Security-sensitive actions are logged and protected.

## 2.4 Prompt Template for Future Build Rounds

Use prompts in this shape when starting implementation work:

- `Implement Phase 0 from docs/cmmi-l3/PHASE_MENU_ROLE_WORKFLOW_SPEC.md. Include backend, frontend, and 3rd-party integration changes. Keep permission enforcement, audit logging, performance, and security in scope.`
- `Implement Phase 2 from docs/cmmi-l3/PHASE_MENU_ROLE_WORKFLOW_SPEC.md. Build the screens, API contracts, workflow rules, storage integration, and quality checks needed for document governance.`
- `Implement Phase 3 and update docs/cmmi-l3/PHASE_MENU_ROLE_WORKFLOW_SPEC.md if you refine scope during implementation.`

## 3. Screen + Role + Permission + Workflow State

### Overview
- Roles/Permissions
  - PM/BA/QA/Approver/ComplianceAdmin: R
  - SystemAdmin: R
- Workflow State
  - Read-only
- Notes
  - Must show project health, pending approvals, risk summary, SLA breaches, quality gate status, and recent audit activity

### Project Register
- Roles/Permissions
  - PM: C/R/U
  - BA: R
  - Approver: R
  - Dev/QA/DocController/Auditor: R
  - SystemAdmin/ComplianceAdmin: R/U
- Workflow State
  - Draft → Active → On Hold → Closed → Archived

### Project Detail
- Roles/Permissions
  - PM: R/U
  - BA/Dev/QA: R
  - Approver: R
  - DocController: R
  - ComplianceAdmin: R
- Workflow State
  - Active → On Hold → Closed

### Project Roles
- Roles/Permissions
  - PM: C/R/U
  - SystemAdmin: R/U
- Workflow State
  - Active → Archived

### Team Assignment
- Roles/Permissions
  - PM: C/R/U
  - SystemAdmin: R
- Workflow State
  - Active → Removed

### Project Phase Approval
- Roles/Permissions
  - PM: Submit
  - Approver: A/R
  - ComplianceAdmin: A/R
- Workflow State
  - Draft → Submitted → Approved/Rejected → Baseline

### Requirement Register
- Roles/Permissions
  - BA: C/R/U
  - PM: R/U
  - Dev/QA: R
  - Approver: R
- Workflow State
  - Draft → Review → Approved → Baseline → Changed

### Requirement Detail
- Roles/Permissions
  - BA: C/R/U
  - PM: R/U
  - Dev/QA: R
  - Approver: R
- Workflow State
  - Draft → Review → Approved → Baseline → Superseded

### Requirement Baseline
- Roles/Permissions
  - PM: Create
  - Approver: Approve
- Workflow State
  - Proposed → Approved → Locked

### Traceability Matrix
- Roles/Permissions
  - BA: R
  - PM: R
  - Dev/QA: R
  - ComplianceAdmin: R
- Workflow State
  - Read-only
- Notes
  - Links must be created from source modules and validated against required lifecycle checkpoints

### Document Type Setup
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - SystemAdmin: C/R/U
  - DocController: R
- Workflow State
  - Active → Deprecated

### Document Register
- Roles/Permissions
  - DocController: C/R/U
  - BA/PM/Dev/QA: C/R (by doc type)
  - Approver: R
  - Auditor: R
- Workflow State
  - Draft → Review → Approved → Baseline → Archived

### Document Detail
- Roles/Permissions
  - DocController: C/R/U
  - Approver: A
  - BA/PM/Dev/QA: R
- Workflow State
  - Draft → Review → Approved/Rejected → Baseline → Archived

### Change Request Register
- Roles/Permissions
  - PM: C/R/U
  - BA: C/R
  - Approver: R
- Workflow State
  - Draft → Submitted → In Review → Approved/Rejected → Implemented → Closed

### Change Request Detail
- Roles/Permissions
  - PM: C/R/U
  - BA: C/R/U
  - Approver: A
  - Dev/QA: R
- Workflow State
  - Draft → Submitted → Approved/Rejected → Implemented → Closed

### Change Log
- Roles/Permissions
  - PM/BA/Dev/QA/Auditor: R
- Workflow State
  - Read-only

### CAPA Register
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - PM/QA/Approver: R/U
  - Auditor: R
- Workflow State
  - Open → Root Cause Analysis → Action Planned → Action In Progress → Verified → Closed

### MOM Register
- Roles/Permissions
  - BA/PM: C/R/U
  - DocController: R
  - Approver: R
- Workflow State
  - Draft → Approved → Archived

### MOM Detail
- Roles/Permissions
  - BA/PM: C/R/U
  - DocController: R
  - Approver: R
- Workflow State
  - Draft → Approved → Archived

### Decision Log
- Roles/Permissions
  - BA/PM: C/R
  - Approver: A
  - Auditor: R
- Workflow State
  - Proposed → Approved → Applied → Archived

### Test Plan
- Roles/Permissions
  - QA: C/R/U
  - PM/Dev: R
  - Approver: R
- Workflow State
  - Draft → Review → Approved → Baseline

### Test Case & Execution
- Roles/Permissions
  - QA: C/R/U/X
  - Dev: R
- Workflow State
  - Draft → Ready → Executed → Passed/Failed → Retest

### UAT Sign-off
- Roles/Permissions
  - PM: Submit
  - Approver: A
  - BA: R
- Workflow State
  - Draft → Submitted → Approved/Rejected

### Audit Log
- Roles/Permissions
  - Auditor: R
  - PM: R
  - ComplianceAdmin/Support: R
- Workflow State
  - Read-only

### Evidence Export
- Roles/Permissions
  - Auditor: E
  - PM: E
  - ComplianceAdmin: E
- Workflow State
  - Requested → Generated → Downloaded

### Metrics Dashboard
- Roles/Permissions
  - PM/BA/QA: R
  - ComplianceAdmin: R
- Workflow State
  - Read-only

### Quality Gate Status
- Roles/Permissions
  - PM: R
  - Approver/ComplianceAdmin: A/R
- Workflow State
  - Open → Blocked → Approved → Overridden

### Performance Baseline
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - PM/QA/SystemAdmin: R
- Workflow State
  - Draft → Approved → Active → Superseded

### Capacity Review
- Roles/Permissions
  - SystemAdmin: C/R/U
  - ComplianceAdmin/PM: R
- Workflow State
  - Planned → Reviewed → Actioned → Closed

### Slow Query / API Review
- Roles/Permissions
  - SystemAdmin: C/R/U
  - Dev/QA/PM: R
  - ComplianceAdmin: R
- Workflow State
  - Open → Investigating → Optimized → Verified → Closed

### Performance Regression Gate
- Roles/Permissions
  - QA: C/R/U
  - Approver/ComplianceAdmin: A/R
  - PM/Dev: R
- Workflow State
  - Pending → Passed/Failed → Overridden

### User & Role Management
- Roles/Permissions
  - SystemAdmin: C/R/U
- Workflow State
  - Active → Disabled

### Permission Matrix
- Roles/Permissions
  - SystemAdmin: C/R/U
- Workflow State
  - Draft → Applied

### Master Data
- Roles/Permissions
  - SystemAdmin: C/R/U
- Workflow State
  - Active → Archived

### System Settings
- Roles/Permissions
  - SystemAdmin/Support: X
- Workflow State
  - Action-based (no state)

### Process Library
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - SystemAdmin: R/U
  - PM/BA: R
- Workflow State
  - Draft → Reviewed → Approved → Active → Deprecated

### Training & Competency
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - SystemAdmin: R/U
  - PM: R
- Workflow State
  - Planned → In Progress → Completed → Archived

### Risk Register
- Roles/Permissions
  - PM/BA: C/R/U
  - Approver: R
  - ComplianceAdmin: R
- Workflow State
  - Draft → Assessed → Mitigated → Closed

### Issue / Action Log
- Roles/Permissions
  - PM/BA/Dev/QA: C/R/U
  - Approver: R
- Workflow State
  - Open → In Progress → Resolved → Closed

### Configuration Items
- Roles/Permissions
  - DocController: C/R/U
  - PM/BA: R
  - ComplianceAdmin: R
- Workflow State
  - Draft → Approved → Baseline → Superseded

### Baseline Registry
- Roles/Permissions
  - PM: C/R
  - Approver: A
  - ComplianceAdmin: R
- Workflow State
  - Proposed → Approved → Locked → Superseded

### QA Review Checklist
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - Approver: R
  - PM/BA/QA: R
- Workflow State
  - Draft → Approved → Active → Deprecated

### Process Audit Plan & Findings
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - Auditor: C/R/U
  - PM: R
- Workflow State
  - Planned → In Review → Findings Issued → Closed

### Metric Definitions
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - PM/QA: R
- Workflow State
  - Draft → Approved → Active → Deprecated

### Data Collection Schedule
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - PM/QA: R
- Workflow State
  - Draft → Active → Archived

### Project Plan
- Roles/Permissions
  - PM: C/R/U
  - Approver: R
  - ComplianceAdmin: R
- Workflow State
  - Draft → Review → Approved → Baseline → Superseded

### Tailoring Record
- Roles/Permissions
  - PM: C/R/U
  - ComplianceAdmin: A/R
- Workflow State
  - Draft → Submitted → Approved → Applied → Archived
  - Evidence: Approver + rationale required

### Stakeholder Register
- Roles/Permissions
  - PM: C/R/U
  - BA: R
  - ComplianceAdmin: R
- Workflow State
  - Active → Archived

### Access Review
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - SystemAdmin: R/U
  - Auditor: R
- Workflow State
  - Scheduled → In Review → Approved → Archived

### Security Review
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - SystemAdmin: R
  - Auditor: R
- Workflow State
  - Planned → In Review → Findings Issued → Closed

### External Dependency Register
- Roles/Permissions
  - SystemAdmin: C/R/U
  - ComplianceAdmin: R
  - PM: R
- Workflow State
  - Active → Review Due → Updated → Archived

### Configuration Audit Log
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - Auditor: C/R/U
  - PM: R
- Workflow State
  - Planned → In Review → Findings Issued → Closed

### RACI Map
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - SystemAdmin: R
  - PM/BA: R
- Workflow State
  - Draft → Approved → Active → Archived

### Approval Evidence Log
- Roles/Permissions
  - ComplianceAdmin: R
  - Auditor: R
  - PM/Approver: R
- Workflow State
  - Read-only

### Workflow Override Log
- Roles/Permissions
  - ComplianceAdmin: R
  - Auditor: R
  - PM/Approver: R
- Workflow State
  - Read-only

### SLA & Escalation Rules
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - PM: R
  - SystemAdmin: R
- Workflow State
  - Draft → Approved → Active → Archived

### Data Retention Policy
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - SystemAdmin: R/U
  - Auditor: R
- Workflow State
  - Draft → Approved → Active → Archived

### Release Register
- Roles/Permissions
  - PM: C/R/U
  - Approver: R
  - ComplianceAdmin: R
- Workflow State
  - Draft → Approved → Released → Archived

### Deployment Checklist
- Roles/Permissions
  - PM/Dev/QA: C/R/U
  - Approver: R
- Workflow State
  - Draft → Reviewed → Approved → Executed

### Release Notes
- Roles/Permissions
  - PM/BA: C/R/U
  - ComplianceAdmin: R
- Workflow State
  - Draft → Approved → Published → Archived

### Defect Log
- Roles/Permissions
  - QA: C/R/U
  - Dev/PM: R
  - Approver: R
- Workflow State
  - Open → In Progress → Resolved → Closed

### Non‑Conformance Log
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - Auditor: R
  - PM: R
- Workflow State
  - Open → In Review → Corrective Action → Closed

### Supplier Register
- Roles/Permissions
  - SystemAdmin: C/R/U
  - ComplianceAdmin: R
  - PM: R
- Workflow State
  - Active → Review Due → Updated → Archived

### SLA/Contract Evidence
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - SystemAdmin: R
  - Auditor: R
- Workflow State
  - Draft → Approved → Active → Archived

### Metrics Review Log
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - PM/QA: R
- Workflow State
  - Planned → Reviewed → Actions Tracked → Closed

### Trend Analysis Report
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - PM/QA: R
- Workflow State
  - Draft → Approved → Archived

### Lessons Learned
- Roles/Permissions
  - PM/BA/QA: C/R/U
  - ComplianceAdmin: R
- Workflow State
  - Draft → Reviewed → Published → Archived

### Access Recertification Schedule
- Roles/Permissions
  - SystemAdmin: C/R/U
  - ComplianceAdmin: R
  - Auditor: R
- Workflow State
  - Planned → In Review → Approved → Completed

### Architecture Register
- Roles/Permissions
  - PM/BA/Dev: C/R/U
  - ComplianceAdmin: R
  - Approver: R
- Workflow State
  - Draft → Reviewed → Approved → Active → Superseded

### Design Review
- Roles/Permissions
  - Dev/BA: C/R/U
  - Approver/ComplianceAdmin: A/R
  - PM/QA: R
- Workflow State
  - Draft → In Review → Approved/Rejected → Baseline

### Integration Review
- Roles/Permissions
  - Dev/SystemAdmin: C/R/U
  - Approver/ComplianceAdmin: A/R
  - PM: R
- Workflow State
  - Draft → In Review → Approved/Rejected → Applied

### Security Incident Register
- Roles/Permissions
  - ComplianceAdmin/SystemAdmin: C/R/U
  - Auditor: R
  - PM: R
- Workflow State
  - Reported → Assessed → Contained → Resolved → Closed

### Vulnerability & Patch Register
- Roles/Permissions
  - SystemAdmin: C/R/U
  - ComplianceAdmin: R
  - Auditor: R
- Workflow State
  - Open → Assessed → Scheduled → Patched → Verified → Closed

### Secret Rotation Register
- Roles/Permissions
  - SystemAdmin: C/R/U
  - ComplianceAdmin: R
  - Auditor: R
- Workflow State
  - Planned → Rotated → Verified → Archived

### Privileged Access Log
- Roles/Permissions
  - SystemAdmin: C/R/U
  - ComplianceAdmin/Auditor: R
- Workflow State
  - Requested → Approved → Used → Reviewed → Closed

### Data Classification Policy
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - SystemAdmin: R/U
  - PM/BA/DocController: R
- Workflow State
  - Draft → Approved → Active → Archived

### Backup Evidence
- Roles/Permissions
  - SystemAdmin: C/R/U
  - ComplianceAdmin/Auditor: R
- Workflow State
  - Planned → Completed → Verified → Archived

### Restore Verification
- Roles/Permissions
  - SystemAdmin: C/R/U
  - ComplianceAdmin/Auditor: R
- Workflow State
  - Planned → Executed → Verified → Closed

### DR Drill Log
- Roles/Permissions
  - SystemAdmin/ComplianceAdmin: C/R/U
  - Auditor: R
- Workflow State
  - Planned → Executed → Findings Issued → Closed

### Legal Hold Register
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - SystemAdmin: R
  - Auditor: R
- Workflow State
  - Active → Released → Archived

### Notification Queue
- Roles/Permissions
  - SystemAdmin: C/R/U
  - ComplianceAdmin/PM: R
- Workflow State
  - Queued → Sent/Failed → Retried → Closed

### Escalation History
- Roles/Permissions
  - ComplianceAdmin: R
  - PM/Approver/SystemAdmin: R
  - Auditor: R
- Workflow State
  - Read-only
