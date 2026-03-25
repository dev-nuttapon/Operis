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

## 2.12A Phase Execution Contract

When a future implementation round is requested as `Implement Phase N`, that phase is considered complete only if all items below are delivered together in the same round unless explicitly deferred:

1. Workflow alignment
   - The phase workflow matches the intended CMMI Level 3 operating flow.
   - State transitions, approvals, evidence requirements, and traceability rules are implemented.
   - Cross-module links required by the phase are working end-to-end.
2. Backend delivery
   - Data model, domain rules, API contracts, and authorization are implemented.
   - Audit events are emitted for all critical state changes and sensitive actions.
   - Validation rules prevent invalid transitions and missing evidence.
3. Frontend delivery
   - Required list/detail/form/action screens for the phase are implemented.
   - Permission-aware UI behavior is enforced.
   - Empty/loading/error states are included.
4. Security delivery
   - Route-level and action-level protection are enforced.
   - Sensitive data handling, export restrictions, and approval evidence rules are applied.
   - Security-relevant actions are logged and reviewable.
5. Performance delivery
   - Lists page correctly.
   - Filters and search are server-driven where needed.
   - Heavy exports or evidence packaging run asynchronously when appropriate.
   - Queries and endpoints introduced by the phase avoid obvious scale risks.
6. Verification delivery
   - Tests are added for critical workflow paths, permissions, and validation.
   - Any required smoke or integration verification for the phase is run when feasible.
7. Documentation update
   - This file is updated if implementation decisions refine the phase scope, routes, fields, thresholds, or transitions.

If any of these are intentionally excluded for a round, the round is not a full phase implementation and must be requested as a partial phase.

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

#### Phase 5 API Contract
- `GET /risks`
- `POST /risks`
- `GET /risks/{id}`
- `PUT /risks/{id}`
- `PUT /risks/{id}/assess`
- `PUT /risks/{id}/mitigate`
- `PUT /risks/{id}/close`
- `GET /issues`
- `POST /issues`
- `GET /issues/{id}`
- `PUT /issues/{id}`
- `POST /issues/{id}/actions`
- `PUT /issues/{id}/resolve`
- `PUT /issues/{id}/close`

#### Phase 5 Validation and Error Contract
- Risk cannot move to `Mitigated` without owner and mitigation plan
- Issue cannot move to `Resolved` while open actions remain incomplete
- Errors
  - `risk_owner_required`
  - `risk_mitigation_required`
  - `issue_open_actions_exist`
  - `invalid_workflow_transition`

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
- Thresholds
  - Default page size: `25`
  - Max page size: `100`
  - Risk/issue list target response: `< 500 ms` for normal filters

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

#### Phase 6 API Contract
- `GET /meetings`
- `POST /meetings`
- `GET /meetings/{id}`
- `PUT /meetings/{id}`
- `PUT /meetings/{id}/approve`
- `GET /meetings/{id}/minutes`
- `PUT /meetings/{id}/minutes`
- `GET /decisions`
- `POST /decisions`
- `GET /decisions/{id}`
- `PUT /decisions/{id}/approve`
- `PUT /decisions/{id}/apply`

#### Phase 6 Validation and Error Contract
- Meeting minutes cannot be approved without attendees and summary
- Decision cannot move to `Applied` without approved rationale
- Errors
  - `meeting_attendees_required`
  - `meeting_summary_required`
  - `decision_rationale_required`
  - `invalid_workflow_transition`

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
- Thresholds
  - Default page size: `25`
  - Max page size: `100`
  - Full minute content is excluded from list endpoints by default

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

#### Phase 7 API Contract
- `GET /test-plans`
- `POST /test-plans`
- `GET /test-plans/{id}`
- `PUT /test-plans/{id}`
- `PUT /test-plans/{id}/approve`
- `GET /test-cases`
- `POST /test-cases`
- `GET /test-cases/{id}`
- `POST /test-executions`
- `GET /test-executions`
- `POST /uat-signoffs`
- `PUT /uat-signoffs/{id}/submit`
- `PUT /uat-signoffs/{id}/approve`
- `PUT /uat-signoffs/{id}/reject`

#### Phase 7 Validation and Error Contract
- Test plan cannot be approved without linked scope and entry/exit criteria
- UAT cannot be approved without release reference and evidence
- Errors
  - `test_plan_scope_required`
  - `test_plan_criteria_required`
  - `uat_release_required`
  - `uat_evidence_required`
  - `invalid_workflow_transition`

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
- Thresholds
  - Default page size: `25`
  - Max page size: `100`
  - Evidence-rich execution exports should run asynchronously above configured size threshold

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

#### Phase 8 API Contract
- `GET /audit-events`
- `GET /audit-plans`
- `POST /audit-plans`
- `GET /audit-plans/{id}`
- `PUT /audit-plans/{id}`
- `POST /audit-findings`
- `PUT /audit-findings/{id}`
- `PUT /audit-findings/{id}/close`
- `POST /evidence-exports`
- `GET /evidence-exports`
- `GET /evidence-exports/{id}`

#### Phase 8 Validation and Error Contract
- Evidence export requires valid scope and date range
- Audit finding cannot close without resolution summary
- Errors
  - `export_scope_required`
  - `export_date_range_required`
  - `audit_finding_resolution_required`
  - `invalid_workflow_transition`

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
- Thresholds
  - Audit list default page size: `50`
  - Evidence export above synchronous limit must queue background job
  - Export retention must follow retention policy and classification rules

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

#### Phase 9 API Contract
- `GET /metric-definitions`
- `POST /metric-definitions`
- `PUT /metric-definitions/{id}`
- `GET /metric-collection-schedules`
- `POST /metric-collection-schedules`
- `GET /metric-results`
- `GET /quality-gates`
- `POST /quality-gates/evaluate`
- `PUT /quality-gates/{id}/override`

#### Phase 9 Validation and Error Contract
- Gate override requires reason and elevated permission
- Metric definition requires target and threshold
- Errors
  - `metric_target_required`
  - `metric_threshold_required`
  - `quality_gate_override_reason_required`
  - `invalid_workflow_transition`

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
- Thresholds
  - Dashboard should load summary widgets without N+1 API fan-out
  - Gate evaluation must complete within configured SLA for synchronous checks or switch to queued evaluation

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

#### Phase 10 API Contract
- `GET /project-roles`
- `POST /project-roles`
- `GET /team-assignments`
- `POST /team-assignments`
- `PUT /team-assignments/{id}`
- `POST /phase-approvals`
- `PUT /phase-approvals/{id}/submit`
- `PUT /phase-approvals/{id}/approve`
- `PUT /phase-approvals/{id}/reject`

#### Phase 10 Validation and Error Contract
- Phase approval requires entry criteria and required evidence links
- Team assignment requires valid role and active date range
- Errors
  - `phase_entry_criteria_required`
  - `phase_evidence_required`
  - `project_role_required`
  - `invalid_assignment_period`
  - `invalid_workflow_transition`

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
- Thresholds
  - Team queries default page size: `25`
  - Phase approval detail should pre-load only required evidence summaries, not full artifacts

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

#### Phase 11 API Contract
- `GET /master-data`
- `POST /master-data`
- `GET /master-data/{id}`
- `PUT /master-data/{id}`
- `PUT /master-data/{id}/archive`

#### Phase 11 Validation and Error Contract
- Code must be unique within domain
- Archive is blocked when active references exist unless governed exception path exists
- Errors
  - `master_data_code_duplicate`
  - `master_data_in_use`
  - `invalid_workflow_transition`

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
- Thresholds
  - Lookup endpoints should support small payload projections for dropdown use
  - Domain lists should sort by display order then name

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

#### Phase 12 API Contract
- `GET /access-reviews`
- `POST /access-reviews`
- `PUT /access-reviews/{id}`
- `PUT /access-reviews/{id}/approve`
- `GET /security-reviews`
- `POST /security-reviews`
- `PUT /security-reviews/{id}`
- `GET /external-dependencies`
- `POST /external-dependencies`
- `PUT /external-dependencies/{id}`
- `GET /configuration-audits`
- `POST /configuration-audits`

#### Phase 12 Validation and Error Contract
- Access review approval requires reviewer decision and rationale
- External dependency requires owner and criticality
- Errors
  - `access_review_decision_required`
  - `access_review_rationale_required`
  - `dependency_owner_required`
  - `dependency_criticality_required`
  - `invalid_workflow_transition`

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
- Thresholds
  - Review lists default page size: `25`
  - Dependency register must support due-date sorting and review-due filtering

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

#### Phase 13 API Contract
- `GET /raci-maps`
- `POST /raci-maps`
- `PUT /raci-maps/{id}`
- `GET /approval-evidence`
- `GET /workflow-overrides`
- `GET /sla-rules`
- `POST /sla-rules`
- `PUT /sla-rules/{id}`
- `GET /retention-policies`
- `POST /retention-policies`
- `PUT /retention-policies/{id}`

#### Phase 13 Validation and Error Contract
- SLA rule requires target duration and escalation policy
- Override log is read-only and must only be written by governed workflow actions
- Errors
  - `sla_target_required`
  - `sla_escalation_policy_required`
  - `override_log_mutation_forbidden`
  - `invalid_workflow_transition`

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
- Thresholds
  - Governance logs default page size: `50`
  - Approval evidence search must support date, actor, entity type, and outcome filters

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

#### Phase 14 API Contract
- `GET /releases`
- `POST /releases`
- `GET /releases/{id}`
- `PUT /releases/{id}`
- `PUT /releases/{id}/approve`
- `PUT /releases/{id}/release`
- `GET /deployment-checklists`
- `POST /deployment-checklists`
- `PUT /deployment-checklists/{id}`
- `GET /release-notes`
- `POST /release-notes`
- `PUT /release-notes/{id}/publish`

#### Phase 14 Validation and Error Contract
- Release requires checklist completion and quality gate pass unless override path is used
- Release notes cannot publish without approved release
- Errors
  - `release_checklist_incomplete`
  - `release_quality_gate_failed`
  - `release_notes_release_required`
  - `invalid_workflow_transition`

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
- Thresholds
  - Release list default page size: `25`
  - Deployment checklist should batch-update status changes where practical

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

#### Phase 15 API Contract
- `GET /defects`
- `POST /defects`
- `GET /defects/{id}`
- `PUT /defects/{id}`
- `PUT /defects/{id}/resolve`
- `PUT /defects/{id}/close`
- `GET /non-conformances`
- `POST /non-conformances`
- `GET /non-conformances/{id}`
- `PUT /non-conformances/{id}`
- `PUT /non-conformances/{id}/close`

#### Phase 15 Validation and Error Contract
- Defect closure requires resolution summary
- Non-conformance closure requires corrective action reference or accepted disposition
- Errors
  - `defect_resolution_required`
  - `non_conformance_corrective_action_required`
  - `invalid_workflow_transition`

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
- Thresholds
  - Default page size: `25`
  - Defect and NC searches must support project + status + severity composite filtering

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

#### Phase 16 API Contract
- `GET /suppliers`
- `POST /suppliers`
- `GET /suppliers/{id}`
- `PUT /suppliers/{id}`
- `GET /supplier-agreements`
- `POST /supplier-agreements`
- `PUT /supplier-agreements/{id}`

#### Phase 16 Validation and Error Contract
- Agreement requires evidence and effective dates
- Supplier archive blocked when active agreement exists unless governed closure path exists
- Errors
  - `supplier_agreement_evidence_required`
  - `supplier_agreement_effective_dates_required`
  - `supplier_active_agreement_exists`
  - `invalid_workflow_transition`

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
- Thresholds
  - Default page size: `25`
  - Agreement evidence metadata should be listed without downloading the evidence binary

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

#### Phase 17 API Contract
- `GET /metric-reviews`
- `POST /metric-reviews`
- `PUT /metric-reviews/{id}`
- `GET /trend-reports`
- `POST /trend-reports`
- `GET /trend-reports/{id}`

#### Phase 17 Validation and Error Contract
- Review cannot close while tracked actions remain open
- Trend report approval requires defined metric and period
- Errors
  - `metric_review_open_actions_exist`
  - `trend_metric_required`
  - `trend_period_required`
  - `invalid_workflow_transition`

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
- Thresholds
  - Heavy trend generation jobs should queue in background above configured data volume

### Phase 18 Detailed Build Package

#### Phase 18 Core Entities
- `lessons_learned`
  - Required fields: `id`, `project_id`, `title`, `summary`, `lesson_type`, `owner_user_id`, `status`, `source_ref`

#### Phase 18 UI Field Groups
- Lessons Learned
  - List fields: `title`, `type`, `project`, `owner`, `status`, `published at`
  - Detail fields: `context`, `what happened`, `what to repeat`, `what to avoid`, `linked evidence`

#### Phase 18 API Contract
- `GET /lessons-learned`
- `POST /lessons-learned`
- `GET /lessons-learned/{id}`
- `PUT /lessons-learned/{id}`
- `PUT /lessons-learned/{id}/publish`

#### Phase 18 Validation and Error Contract
- Publish requires context, lesson summary, and linked evidence or source reference
- Errors
  - `lesson_context_required`
  - `lesson_summary_required`
  - `lesson_source_required`
  - `invalid_workflow_transition`

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
- Thresholds
  - Search must support paged keyword search with stable sort order

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

#### Phase 19 API Contract
- `GET /access-recertifications`
- `POST /access-recertifications`
- `GET /access-recertifications/{id}`
- `PUT /access-recertifications/{id}`
- `POST /access-recertifications/{id}/decisions`
- `PUT /access-recertifications/{id}/complete`

#### Phase 19 Validation and Error Contract
- Schedule cannot complete while pending decisions remain
- Revocation or adjustment decision requires rationale
- Errors
  - `access_recertification_pending_decisions`
  - `access_recertification_decision_rationale_required`
  - `invalid_workflow_transition`

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
- Thresholds
  - Default page size: `25`
  - Subject decision lists should page on large scope reviews

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

#### Phase 20 API Contract
- `GET /architecture-records`
- `POST /architecture-records`
- `GET /architecture-records/{id}`
- `PUT /architecture-records/{id}`
- `GET /design-reviews`
- `POST /design-reviews`
- `PUT /design-reviews/{id}`
- `GET /integration-reviews`
- `POST /integration-reviews`
- `PUT /integration-reviews/{id}`

#### Phase 20 Validation and Error Contract
- Design review approval requires decision reason
- Integration review cannot apply without approved decision
- Errors
  - `design_review_decision_reason_required`
  - `integration_review_approval_required`
  - `invalid_workflow_transition`

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
- Thresholds
  - Architecture lists default page size: `25`
  - Linked design artifacts should load lazily in detail screens

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

#### Phase 21 API Contract
- `GET /security-incidents`
- `POST /security-incidents`
- `GET /security-incidents/{id}`
- `PUT /security-incidents/{id}`
- `GET /vulnerabilities`
- `POST /vulnerabilities`
- `PUT /vulnerabilities/{id}`
- `GET /secret-rotations`
- `POST /secret-rotations`
- `PUT /secret-rotations/{id}`
- `GET /privileged-access-events`
- `POST /privileged-access-events`
- `PUT /privileged-access-events/{id}`
- `GET /classification-policies`
- `POST /classification-policies`
- `PUT /classification-policies/{id}`

#### Phase 21 Validation and Error Contract
- Privileged access use requires approved request
- Secret rotation verification requires verifier and verification time
- Security incident closure requires resolution summary
- Errors
  - `privileged_access_approval_required`
  - `secret_rotation_verification_required`
  - `security_incident_resolution_required`
  - `invalid_workflow_transition`

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
- Thresholds
  - Security event searches default page size: `50`
  - Incident lists support severity/date filtering and descending time order by default

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

#### Phase 22 API Contract
- `GET /performance-baselines`
- `POST /performance-baselines`
- `PUT /performance-baselines/{id}`
- `GET /capacity-reviews`
- `POST /capacity-reviews`
- `PUT /capacity-reviews/{id}`
- `GET /slow-operations`
- `POST /slow-operations`
- `PUT /slow-operations/{id}`
- `GET /performance-gates`
- `POST /performance-gates/evaluate`
- `PUT /performance-gates/{id}/override`

#### Phase 22 Validation and Error Contract
- Performance gate override requires reason and elevated role
- Slow operation cannot close without optimization verification
- Errors
  - `performance_gate_override_reason_required`
  - `slow_operation_verification_required`
  - `invalid_workflow_transition`

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
- Thresholds
  - Slow operation lists default page size: `50`
  - Gate evaluation above configured data volume should queue or down-scope evaluation

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

#### Phase 23 API Contract
- `GET /backup-evidence`
- `POST /backup-evidence`
- `GET /restore-verifications`
- `POST /restore-verifications`
- `GET /dr-drills`
- `POST /dr-drills`
- `PUT /dr-drills/{id}`
- `GET /legal-holds`
- `POST /legal-holds`
- `PUT /legal-holds/{id}/release`

#### Phase 23 Validation and Error Contract
- Restore verification requires backup reference
- Legal hold release requires rationale and authorized role
- Errors
  - `restore_backup_reference_required`
  - `legal_hold_release_reason_required`
  - `invalid_workflow_transition`

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
- Thresholds
  - DR and backup evidence lists default page size: `25`
  - Evidence binary retrieval remains out of list query paths

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

#### Phase 24 API Contract
- `GET /capa`
- `POST /capa`
- `GET /capa/{id}`
- `PUT /capa/{id}`
- `POST /capa/{id}/actions`
- `PUT /capa/{id}/verify`
- `PUT /capa/{id}/close`
- `GET /notification-queue`
- `POST /notification-queue`
- `PUT /notification-queue/{id}/retry`
- `GET /escalations`
- `POST /escalations`

#### Phase 24 Validation and Error Contract
- CAPA cannot close while open actions remain
- Retry requires previous failed notification state
- Errors
  - `capa_open_actions_exist`
  - `notification_retry_invalid_state`
  - `invalid_workflow_transition`

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
- Thresholds
  - Notification queue default page size: `50`
  - Retry and escalation operations must be idempotent where possible

## 2.14 Command Templates for Future Rounds

Use these prompts when you want implementation rounds to complete a full phase, including workflow, security, and performance.

### Full phase command

`Implement Phase N from docs/cmmi-l3/PHASE_MENU_ROLE_WORKFLOW_SPEC.md as a full phase. Complete backend, frontend, workflow alignment to CMMI Level 3, security controls, performance controls, tests, and update the spec if implementation refines it.`

### Full phase command with strict completion rule

`Implement Phase N from docs/cmmi-l3/PHASE_MENU_ROLE_WORKFLOW_SPEC.md and finish the phase in full. Do not treat it as complete unless workflow, permissions, audit logging, security handling, performance safeguards, and required screens/APIs for the phase are all done together. Update the spec if anything changes.`

### Sequential build command

`Implement the next unfinished phase from docs/cmmi-l3/PHASE_MENU_ROLE_WORKFLOW_SPEC.md as a full phase. Keep CMMI Level 3 workflow integrity, system security, and performance requirements in scope. Finish the implementation and update the spec.`

### Partial phase command

`Implement only the backend executable spec for Phase N from docs/cmmi-l3/PHASE_MENU_ROLE_WORKFLOW_SPEC.md. Do not mark the phase complete. Update the spec with any implementation decisions.`

### Review command

`Review Phase N implementation against docs/cmmi-l3/PHASE_MENU_ROLE_WORKFLOW_SPEC.md with focus on CMMI Level 3 workflow integrity, security controls, performance behavior, and missing acceptance coverage.`

## 2.15 Definition of a Finished Phase

A phase is finished only when all conditions below are true:

1. Required screens for the phase are implemented and wired to real APIs.
2. Required backend models, validation, workflow transitions, and audit events are implemented.
3. Required role and permission checks are enforced in UI and API.
4. Security requirements relevant to the phase are implemented, not deferred as notes.
5. Performance requirements relevant to the phase are implemented, not deferred as notes.
6. Tests or verification evidence exist for the phase's critical paths.
7. Any changed scope or implementation decision has been reflected back into this spec.

## 2.16 Database Column and Index Standard

Every phase database design must define the following at table level before implementation:

1. Column contract
   - Name
   - Type
   - Nullability
   - Default value
   - Max length for string columns
   - Enum/value set where applicable
2. Required common columns on mutable records
   - `id`
   - `created_at`
   - `created_by`
   - `updated_at`
   - `updated_by`
   - `deleted_at` when soft delete applies
3. Index contract
   - Primary key
   - Unique constraints
   - Foreign key indexes
   - Primary list/query indexes
   - Time-range indexes for audit and history tables
4. State and ownership columns
   - `status`
   - owning project or module reference
   - approval fields when approval exists
5. Retention and classification columns where required
   - `classification`
   - `retention_class`
   - legal-hold or archive flags where applicable

## 2.17 Request and Response Schema Standard

Every API endpoint in a phase must define:

1. Request schema
   - Field name
   - Type
   - Required/optional
   - Validation rule
   - Allowed values
2. Response schema
   - Field name
   - Type
   - Nullability
   - Expansion rules for related objects
3. Error schema
   - HTTP status
   - Stable business error code
   - User-safe message
   - Retryable or non-retryable classification
4. Query schema for list endpoints
   - `page`
   - `pageSize`
   - `sortBy`
   - `sortOrder`
   - `search`
   - module-specific filters
5. Operational behavior
   - idempotency rule if applicable
   - timeout expectation
   - async execution rule if export or heavy processing is involved

## 2.17A Response Envelope Standard

Unless a phase explicitly requires a different shape, use these response envelopes:

1. List response
   - `items`: array
   - `page`: integer
   - `pageSize`: integer
   - `total`: integer
2. Detail response
   - flat object plus expanded related summaries only when requested
3. Mutation response
   - minimal changed object summary
   - `id`
   - `status`
   - `updatedAt` or `createdAt` as applicable
4. Error response
   - `code`: stable business error code
   - `message`: user-safe message
   - `traceId`: request correlation id
   - `details`: optional field-level validation payload

## 2.17B Operational Endpoint Policy

Every endpoint must define:

1. Timeout class
   - `interactive_read`: target timeout `3s`
   - `interactive_write`: target timeout `5s`
   - `heavy_read_or_export`: queue asynchronously if expected to exceed `5s`
2. Idempotency
   - all approval, submit, archive, close, retry, and release actions must define whether duplicate requests are safe
   - mutation endpoints with financial, security, release, or escalation impact should support idempotency keys when replays are plausible
3. Retry policy
   - no blind retry on validation or authorization failures
   - bounded retry for transient infrastructure or queue delivery failure
4. Async policy
   - export, evidence packaging, trend generation, large audit retrieval, and bulk notification dispatch should be asynchronous above configured thresholds

## 2.17C Performance Threshold Defaults

Use these defaults unless a phase overrides them with stricter values:

1. Default list page size: `25`
2. Maximum list page size: `100`
3. Default audit/queue page size: `50`
4. Synchronous export/evidence packaging limit: `5s` target wall time
5. Above sync limit: enqueue background job and return job status handle
6. Heavy detail endpoints should lazy-load secondary panels rather than expand everything in the first response

## 2.17D Security Response and Logging Rules

1. Authorization failure returns stable code and no sensitive existence leak unless policy allows it.
2. Sensitive exports require actor, scope, reason when applicable, and audit event.
3. Privileged actions must emit audit records even on denial.
4. Security-sensitive endpoints must attach a correlation id to responses and logs.

## 2.17E Given/When/Then Scenario Standard

Every phase should define scenarios in this form:

1. Functional scenario
   - Given system state
   - When user performs action
   - Then data, workflow state, and UI result are correct
2. Permission scenario
   - Given role without permission
   - When protected action is attempted
   - Then action is blocked and logged if required
3. Audit scenario
   - Given auditable action
   - When state changes
   - Then expected audit event is emitted with required metadata
4. Security scenario
   - Given classified or sensitive data
   - When unauthorized or lower-scope actor requests it
   - Then access is denied or redacted according to policy
5. Performance scenario
   - Given representative dataset volume
   - When list/detail/export endpoint is invoked
   - Then paging, latency, and async behavior remain within target thresholds

## 2.17F Database Engine and Index Notes

Assume PostgreSQL unless an implementation phase explicitly states otherwise.

1. Add composite indexes that match dominant list filters and sort order together.
2. Add descending time indexes for high-volume audit, queue, event, and history tables where recent-first access dominates.
3. Avoid indexing large text columns directly unless using specialized search strategy.
4. For uniqueness scoped to project or domain, prefer composite unique indexes over application-only checks.
5. Revisit indexes after each high-volume phase based on actual query plans.

## 2.18 Phase 0 Field-Level Executable Spec

### Table: `users`
- Columns
  - `id`: string, required, PK
  - `email`: string, required, max 320, unique
  - `display_name`: string, required, max 256
  - `status`: enum(`active`,`disabled`), required, indexed
  - `created_at`: datetime, required
  - `created_by`: string, required
  - `updated_at`: datetime, required
  - `updated_by`: string, required
- Indexes
  - unique(`email`)
  - index(`status`)
  - index(`updated_at`)

### Table: `roles`
- Columns
  - `id`: string, required, PK
  - `code`: string, required, max 128, unique
  - `name`: string, required, max 256
  - `status`: enum(`active`,`archived`), required
  - `created_at`: datetime, required
  - `updated_at`: datetime, required
- Indexes
  - unique(`code`)
  - index(`status`)

### Table: `permissions`
- Columns
  - `id`: string, required, PK
  - `module_code`: string, required, max 128, indexed
  - `permission_key`: string, required, max 256, unique
  - `description`: string, optional, max 512
- Indexes
  - unique(`permission_key`)
  - index(`module_code`)

### Table: `role_permissions`
- Columns
  - `id`: string, required, PK
  - `role_id`: string, required, FK(`roles.id`)
  - `permission_id`: string, required, FK(`permissions.id`)
  - `allowed`: boolean, required, default `true`
- Indexes
  - unique(`role_id`,`permission_id`)

### Table: `user_role_assignments`
- Columns
  - `id`: string, required, PK
  - `user_id`: string, required, FK(`users.id`)
  - `role_id`: string, required, FK(`roles.id`)
  - `assigned_at`: datetime, required
  - `assigned_by`: string, required
  - `reason`: string, required, max 1000
- Indexes
  - unique(`user_id`,`role_id`)
  - index(`assigned_at`)

### Table: `security_events`
- Columns
  - `id`: string, required, PK
  - `event_type`: string, required, max 128, indexed
  - `actor_user_id`: string, optional, indexed
  - `target_type`: string, optional, max 128
  - `target_id`: string, optional
  - `reason`: string, optional, max 1000
  - `outcome`: enum(`success`,`failure`,`denied`), required
  - `occurred_at`: datetime, required, indexed
  - `metadata_json`: json, optional
- Indexes
  - index(`event_type`,`occurred_at`)
  - index(`actor_user_id`,`occurred_at`)

### `GET /admin/users` response schema
- Query
  - `page`: integer, optional, min 1, default 1
  - `pageSize`: integer, optional, min 5, max 100, default 25
  - `search`: string, optional, max 200
  - `status`: enum(`active`,`disabled`), optional
  - `role`: string, optional
- Response
  - `items[]`
    - `id`: string
    - `email`: string
    - `displayName`: string
    - `status`: string
    - `roles[]`: string
    - `updatedAt`: string(datetime)
  - `page`: integer
  - `pageSize`: integer
  - `total`: integer
- Errors
  - `401 unauthorized`
  - `403 forbidden`

### `PUT /admin/users/{id}/roles` request schema
- Request
  - `roleIds`: array<string>, required, min 1
  - `reason`: string, required, max 1000
- Response
  - `id`: string
  - `roles[]`: string
  - `updatedAt`: string(datetime)
- Errors
  - `403 forbidden`
  - `404 user_not_found`
  - `409 last_admin_removal_blocked`
  - `400 validation_failed`

### `PUT /admin/permissions/matrix` request schema
- Request
  - `entries[]`: required
    - `roleId`: string, required
    - `permissionKey`: string, required
    - `allowed`: boolean, required
  - `reason`: string, required, max 1000
- Response
  - `updatedCount`: integer
  - `updatedAt`: string(datetime)
- Errors
  - `403 forbidden`
  - `400 validation_failed`

## 2.19 Phase 1 Field-Level Executable Spec

### Table: `process_assets`
- Columns
  - `id`: string, required, PK
  - `code`: string, required, max 128, unique
  - `name`: string, required, max 256
  - `category`: string, required, max 128, indexed
  - `status`: enum(`draft`,`reviewed`,`approved`,`active`,`deprecated`), required, indexed
  - `owner_user_id`: string, required
  - `effective_from`: datetime, optional
  - `effective_to`: datetime, optional
  - `current_version_id`: string, optional
  - `created_at`: datetime, required
  - `updated_at`: datetime, required
- Indexes
  - unique(`code`)
  - index(`category`,`status`)

### Table: `project_plans`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `name`: string, required, max 256
  - `scope_summary`: string, required, max 4000
  - `lifecycle_model`: string, required, max 128
  - `start_date`: date, required
  - `target_end_date`: date, required
  - `owner_user_id`: string, required
  - `status`: enum(`draft`,`review`,`approved`,`baseline`,`superseded`), required
- Indexes
  - index(`project_id`,`status`)

### `POST /project-plans` request schema
- Request
  - `projectId`: string, required
  - `name`: string, required, max 256
  - `scopeSummary`: string, required, max 4000
  - `lifecycleModel`: string, required
  - `startDate`: string(date), required
  - `targetEndDate`: string(date), required
  - `ownerUserId`: string, required
- Response
  - `id`: string
  - `status`: string
- Errors
  - `400 validation_failed`
  - `404 project_not_found`

### `PUT /tailoring-records/{id}/approve` request schema
- Request
  - `decision`: enum(`approved`,`rejected`), required
  - `reason`: string, required, max 2000
- Response
  - `id`: string
  - `status`: string
  - `approvedAt`: string(datetime)
- Errors
  - `403 forbidden`
  - `404 tailoring_record_not_found`
  - `400 tailoring_reason_required`

## 2.20 Phase 2 Field-Level Executable Spec

### Table: `document_types`
- Columns
  - `id`: string, required, PK
  - `code`: string, required, max 128, unique
  - `name`: string, required, max 256
  - `module_owner`: string, required, max 128
  - `classification_default`: string, required, max 64
  - `retention_class_default`: string, required, max 64
  - `status`: enum(`active`,`deprecated`), required
- Indexes
  - unique(`code`)
  - index(`status`)

### Table: `documents`
- Columns
  - `id`: string, required, PK
  - `document_type_id`: string, required, indexed
  - `project_id`: string, required, indexed
  - `phase_code`: string, required, max 64, indexed
  - `owner_user_id`: string, required, indexed
  - `current_version_id`: string, optional
  - `status`: enum(`draft`,`review`,`approved`,`rejected`,`baseline`,`archived`), required, indexed
  - `classification`: string, required, max 64, indexed
  - `retention_class`: string, required, max 64
  - `title`: string, required, max 512
  - `created_at`: datetime, required
  - `updated_at`: datetime, required
- Indexes
  - index(`project_id`,`status`,`phase_code`)
  - index(`document_type_id`,`status`)
  - index(`owner_user_id`,`updated_at`)

### Table: `document_versions`
- Columns
  - `id`: string, required, PK
  - `document_id`: string, required, indexed
  - `version_number`: integer, required
  - `storage_key`: string, required, max 512, unique
  - `file_name`: string, required, max 512
  - `file_size`: bigint, required
  - `mime_type`: string, required, max 128
  - `uploaded_by`: string, required
  - `uploaded_at`: datetime, required
  - `status`: enum(`uploaded`,`submitted`,`approved`,`rejected`,`superseded`), required
- Indexes
  - unique(`document_id`,`version_number`)
  - index(`uploaded_at`)

### `POST /documents` request schema
- Request
  - `documentTypeId`: string, required
  - `projectId`: string, required
  - `phaseCode`: string, required
  - `ownerUserId`: string, required
  - `classification`: string, required
  - `retentionClass`: string, required
  - `title`: string, required, max 512
  - `tags[]`: array<string>, optional
- Response
  - `id`: string
  - `status`: string
  - `title`: string
- Errors
  - `400 validation_failed`
  - `404 document_type_not_found`
  - `404 project_not_found`

### `POST /documents/{id}/versions` request schema
- Request
  - `file`: binary, required
  - `fileName`: string, required, max 512
  - `mimeType`: string, required
- Response
  - `id`: string
  - `versionNumber`: integer
  - `status`: string
- Errors
  - `400 file_required`
  - `400 file_too_large`
  - `400 mime_type_not_allowed`

## 2.21 Phase 3 Field-Level Executable Spec

### Table: `requirements`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `code`: string, required, max 128, unique within project
  - `title`: string, required, max 512
  - `description`: string, required, max 8000
  - `priority`: enum(`low`,`medium`,`high`,`critical`), required, indexed
  - `owner_user_id`: string, required, indexed
  - `status`: enum(`draft`,`review`,`approved`,`baselined`,`superseded`), required, indexed
  - `current_version_id`: string, optional
  - `created_at`: datetime, required
  - `updated_at`: datetime, required
- Indexes
  - unique(`project_id`,`code`)
  - index(`project_id`,`status`,`priority`)

### Table: `requirement_versions`
- Columns
  - `id`: string, required, PK
  - `requirement_id`: string, required, indexed
  - `version_number`: integer, required
  - `business_reason`: string, required, max 4000
  - `acceptance_criteria`: string, required, max 8000
  - `security_impact`: string, optional, max 4000
  - `performance_impact`: string, optional, max 4000
  - `status`: enum(`draft`,`submitted`,`approved`,`rejected`,`superseded`), required
  - `created_at`: datetime, required
- Indexes
  - unique(`requirement_id`,`version_number`)

### Table: `requirement_baselines`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `baseline_name`: string, required, max 256
  - `approved_by`: string, required
  - `approved_at`: datetime, required
  - `status`: enum(`proposed`,`approved`,`locked`,`superseded`), required, indexed
- Indexes
  - index(`project_id`,`status`)

### Table: `traceability_links`
- Columns
  - `id`: string, required, PK
  - `source_type`: string, required, max 64, indexed
  - `source_id`: string, required, indexed
  - `target_type`: string, required, max 64, indexed
  - `target_id`: string, required, indexed
  - `link_rule`: string, required, max 128
  - `status`: enum(`created`,`validated`,`broken`,`resolved`), required, indexed
  - `created_by`: string, required
  - `created_at`: datetime, required
- Indexes
  - unique(`source_type`,`source_id`,`target_type`,`target_id`,`link_rule`)
  - index(`status`,`source_type`,`target_type`)

### `POST /requirements` request schema
- Request
  - `projectId`: string, required
  - `code`: string, required, max 128
  - `title`: string, required, max 512
  - `description`: string, required, max 8000
  - `priority`: enum(`low`,`medium`,`high`,`critical`), required
  - `ownerUserId`: string, required
  - `businessReason`: string, required, max 4000
  - `acceptanceCriteria`: string, required, max 8000
  - `securityImpact`: string, optional, max 4000
  - `performanceImpact`: string, optional, max 4000
- Response
  - `id`: string
  - `code`: string
  - `status`: string
  - `currentVersionId`: string
- Errors
  - `400 validation_failed`
  - `404 project_not_found`
  - `409 requirement_code_duplicate`

### `POST /requirement-baselines` request schema
- Request
  - `projectId`: string, required
  - `baselineName`: string, required, max 256
  - `requirementIds[]`: array<string>, required, min 1
  - `reason`: string, required, max 2000
- Response
  - `id`: string
  - `status`: string
  - `approvedAt`: string(datetime)
- Errors
  - `400 requirement_not_approved`
  - `400 baseline_name_required`
  - `400 validation_failed`

### `GET /traceability` response schema
- Query
  - `projectId`: string, required
  - `status`: enum(`created`,`validated`,`broken`,`resolved`), optional
  - `sourceType`: string, optional
  - `targetType`: string, optional
  - `missingOnly`: boolean, optional, default `false`
  - `page`: integer, optional, default `1`
  - `pageSize`: integer, optional, default `25`, max `100`
- Response
  - `items[]`
    - `sourceType`: string
    - `sourceId`: string
    - `sourceCode`: string
    - `targetType`: string
    - `targetId`: string | null
    - `targetCode`: string | null
    - `status`: string
    - `linkRule`: string
  - `page`, `pageSize`, `total`

## 2.22 Phase 4 Field-Level Executable Spec

### Table: `change_requests`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `code`: string, required, max 128, unique within project
  - `title`: string, required, max 512
  - `requested_by`: string, required
  - `reason`: string, required, max 4000
  - `status`: enum(`draft`,`submitted`,`in_review`,`approved`,`rejected`,`implemented`,`closed`), required, indexed
  - `priority`: enum(`low`,`medium`,`high`,`critical`), required, indexed
  - `target_baseline_id`: string, optional, indexed
  - `created_at`: datetime, required
  - `updated_at`: datetime, required
- Indexes
  - unique(`project_id`,`code`)
  - index(`project_id`,`status`,`priority`)

### Table: `change_impacts`
- Columns
  - `id`: string, required, PK
  - `change_request_id`: string, required, unique
  - `scope_impact`: string, required, max 4000
  - `schedule_impact`: string, required, max 4000
  - `quality_impact`: string, required, max 4000
  - `security_impact`: string, required, max 4000
  - `performance_impact`: string, required, max 4000
  - `risk_impact`: string, required, max 4000

### Table: `configuration_items`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `code`: string, required, max 128
  - `name`: string, required, max 256
  - `item_type`: string, required, max 128, indexed
  - `owner_module`: string, required, max 128
  - `status`: enum(`draft`,`approved`,`baseline`,`superseded`), required, indexed
  - `baseline_ref`: string, optional
- Indexes
  - unique(`project_id`,`code`)
  - index(`project_id`,`item_type`,`status`)

### Table: `baseline_registry`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `baseline_name`: string, required, max 256
  - `baseline_type`: string, required, max 64
  - `source_entity_type`: string, required, max 64
  - `source_entity_id`: string, required
  - `status`: enum(`proposed`,`approved`,`locked`,`superseded`), required, indexed
  - `approved_by`: string, optional
  - `approved_at`: datetime, optional
- Indexes
  - index(`project_id`,`baseline_type`,`status`)

### `POST /change-requests` request schema
- Request
  - `projectId`: string, required
  - `code`: string, required, max 128
  - `title`: string, required, max 512
  - `reason`: string, required, max 4000
  - `priority`: enum(`low`,`medium`,`high`,`critical`), required
  - `targetBaselineId`: string, optional
  - `impact`: object, required
    - `scopeImpact`: string, required
    - `scheduleImpact`: string, required
    - `qualityImpact`: string, required
    - `securityImpact`: string, required
    - `performanceImpact`: string, required
    - `riskImpact`: string, required
- Response
  - `id`, `code`, `status`
- Errors
  - `400 validation_failed`
  - `409 change_request_code_duplicate`
  - `404 baseline_not_found`

### `POST /baseline-registry` request schema
- Request
  - `projectId`: string, required
  - `baselineName`: string, required, max 256
  - `baselineType`: string, required
  - `sourceEntityType`: string, required
  - `sourceEntityId`: string, required
  - `changeRequestId`: string, required
- Response
  - `id`, `status`
- Errors
  - `400 approved_change_request_required`
  - `400 validation_failed`

## 2.23 Phase 5 Field-Level Executable Spec

### Table: `risks`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `code`: string, required, max 128
  - `title`: string, required, max 512
  - `description`: string, required, max 4000
  - `probability`: integer, required, range `1..5`
  - `impact`: integer, required, range `1..5`
  - `owner_user_id`: string, required, indexed
  - `mitigation_plan`: string, optional, max 4000
  - `status`: enum(`draft`,`assessed`,`mitigated`,`closed`), required, indexed
  - `next_review_at`: datetime, optional, indexed
- Indexes
  - unique(`project_id`,`code`)
  - index(`project_id`,`status`,`next_review_at`)

### Table: `issues`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `code`: string, required, max 128
  - `title`: string, required, max 512
  - `description`: string, required, max 4000
  - `owner_user_id`: string, required, indexed
  - `due_date`: date, optional, indexed
  - `status`: enum(`open`,`in_progress`,`resolved`,`closed`), required, indexed
  - `severity`: enum(`low`,`medium`,`high`,`critical`), required, indexed
- Indexes
  - unique(`project_id`,`code`)
  - index(`project_id`,`status`,`severity`)

### `POST /risks` request schema
- Request
  - `projectId`, `code`, `title`, `description`, `probability`, `impact`, `ownerUserId`: required
  - `mitigationPlan`: optional
  - `nextReviewAt`: optional datetime
- Response
  - `id`, `code`, `status`
- Errors
  - `400 validation_failed`
  - `409 risk_code_duplicate`

### `POST /issues` request schema
- Request
  - `projectId`, `code`, `title`, `description`, `ownerUserId`, `severity`: required
  - `dueDate`: optional
- Response
  - `id`, `code`, `status`
- Errors
  - `400 validation_failed`
  - `409 issue_code_duplicate`

## 2.24 Phase 6 Field-Level Executable Spec

### Table: `meeting_records`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `meeting_type`: string, required, max 128, indexed
  - `title`: string, required, max 512
  - `meeting_at`: datetime, required, indexed
  - `facilitator_user_id`: string, required
  - `status`: enum(`draft`,`approved`,`archived`), required, indexed
- Indexes
  - index(`project_id`,`meeting_type`,`meeting_at`)

### Table: `decisions`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `code`: string, required, max 128
  - `title`: string, required, max 512
  - `decision_type`: string, required, max 128
  - `rationale`: string, required, max 4000
  - `approved_by`: string, optional
  - `status`: enum(`proposed`,`approved`,`applied`,`archived`), required, indexed
- Indexes
  - unique(`project_id`,`code`)
  - index(`project_id`,`status`,`decision_type`)

### `POST /meetings` request schema
- Request
  - `projectId`, `meetingType`, `title`, `meetingAt`, `facilitatorUserId`: required
  - `attendeeUserIds[]`: optional
- Response
  - `id`, `status`
- Errors
  - `400 validation_failed`
  - `404 project_not_found`

### `POST /decisions` request schema
- Request
  - `projectId`, `code`, `title`, `decisionType`, `rationale`: required
  - `meetingId`: optional
- Response
  - `id`, `code`, `status`
- Errors
  - `400 validation_failed`
  - `409 decision_code_duplicate`

## 2.25 Phase 7 Field-Level Executable Spec

### Table: `test_plans`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `code`: string, required, max 128
  - `title`: string, required, max 512
  - `scope_summary`: string, required, max 4000
  - `owner_user_id`: string, required, indexed
  - `status`: enum(`draft`,`review`,`approved`,`baseline`), required, indexed
- Indexes
  - unique(`project_id`,`code`)
  - index(`project_id`,`status`)

### Table: `test_cases`
- Columns
  - `id`: string, required, PK
  - `test_plan_id`: string, required, indexed
  - `code`: string, required, max 128
  - `title`: string, required, max 512
  - `preconditions`: string, optional, max 4000
  - `expected_result`: string, required, max 4000
  - `status`: enum(`draft`,`ready`,`active`,`retired`), required, indexed
- Indexes
  - unique(`test_plan_id`,`code`)
  - index(`test_plan_id`,`status`)

### Table: `test_executions`
- Columns
  - `id`: string, required, PK
  - `test_case_id`: string, required, indexed
  - `executed_by`: string, required
  - `executed_at`: datetime, required, indexed
  - `result`: enum(`passed`,`failed`,`retest`), required, indexed
  - `evidence_ref`: string, optional
  - `notes`: string, optional, max 4000
- Indexes
  - index(`test_case_id`,`executed_at`)
  - index(`result`,`executed_at`)

### `POST /test-plans` request schema
- Request
  - `projectId`, `code`, `title`, `scopeSummary`, `ownerUserId`: required
- Response
  - `id`, `status`
- Errors
  - `400 validation_failed`
  - `409 test_plan_code_duplicate`

### `POST /test-executions` request schema
- Request
  - `testCaseId`: string, required
  - `result`: enum(`passed`,`failed`,`retest`), required
  - `evidenceRef`: string, optional
  - `notes`: string, optional, max 4000
- Response
  - `id`, `executedAt`, `result`
- Errors
  - `400 validation_failed`
  - `404 test_case_not_found`

## 2.26 Phase 8 Field-Level Executable Spec

### Table: `audit_events`
- Columns
  - `id`: string, required, PK
  - `occurred_at`: datetime, required, indexed
  - `actor_user_id`: string, optional, indexed
  - `entity_type`: string, required, max 128, indexed
  - `entity_id`: string, required, indexed
  - `action`: string, required, max 128, indexed
  - `outcome`: enum(`success`,`failure`,`denied`), required, indexed
  - `reason`: string, optional, max 2000
  - `metadata_json`: json, optional
- Indexes
  - index(`occurred_at`)
  - index(`entity_type`,`action`,`occurred_at`)
  - index(`actor_user_id`,`occurred_at`)

### Table: `evidence_exports`
- Columns
  - `id`: string, required, PK
  - `requested_by`: string, required, indexed
  - `scope_type`: string, required, max 64
  - `scope_ref`: string, required
  - `requested_at`: datetime, required, indexed
  - `status`: enum(`requested`,`generated`,`downloaded`,`expired`), required, indexed
  - `output_ref`: string, optional
- Indexes
  - index(`requested_by`,`requested_at`)
  - index(`status`,`requested_at`)

### `GET /audit-events` response schema
- Query
  - `projectId`: string, optional
  - `entityType`: string, optional
  - `action`: string, optional
  - `actorUserId`: string, optional
  - `from`: datetime, optional
  - `to`: datetime, optional
  - `page`: integer, default `1`
  - `pageSize`: integer, default `50`, max `200`
- Response
  - `items[]`
    - `id`, `occurredAt`, `actorUserId`, `entityType`, `entityId`, `action`, `outcome`, `reason`
  - `page`, `pageSize`, `total`

### `POST /evidence-exports` request schema
- Request
  - `scopeType`: string, required
  - `scopeRef`: string, required
  - `from`: datetime, optional
  - `to`: datetime, optional
  - `includedArtifactTypes[]`: array<string>, optional
- Response
  - `id`, `status`
- Errors
  - `400 export_scope_required`
  - `400 export_date_range_required`
  - `403 forbidden`

## 2.27 Phase 9 Field-Level Executable Spec

### Table: `metric_definitions`
- Columns
  - `id`: string, required, PK
  - `code`: string, required, max 128, unique
  - `name`: string, required, max 256
  - `metric_type`: string, required, max 128, indexed
  - `owner_user_id`: string, required, indexed
  - `target_value`: decimal, required
  - `threshold_value`: decimal, required
  - `status`: enum(`draft`,`approved`,`active`,`deprecated`), required, indexed
- Indexes
  - unique(`code`)
  - index(`metric_type`,`status`)

### Table: `quality_gate_results`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `gate_type`: string, required, max 128, indexed
  - `evaluated_at`: datetime, required, indexed
  - `result`: enum(`pending`,`passed`,`failed`,`overridden`), required, indexed
  - `reason`: string, optional, max 2000
  - `override_reason`: string, optional, max 2000
- Indexes
  - index(`project_id`,`gate_type`,`evaluated_at`)
  - index(`result`,`evaluated_at`)

### `POST /metric-definitions` request schema
- Request
  - `code`: string, required, max 128
  - `name`: string, required, max 256
  - `metricType`: string, required
  - `ownerUserId`: string, required
  - `targetValue`: decimal, required
  - `thresholdValue`: decimal, required
- Response
  - `id`, `code`, `status`
- Errors
  - `400 metric_target_required`
  - `400 metric_threshold_required`
  - `409 metric_code_duplicate`

### `PUT /quality-gates/{id}/override` request schema
- Request
  - `reason`: string, required, max 2000
- Response
  - `id`, `result`, `overrideReason`
- Errors
  - `403 forbidden`
  - `400 quality_gate_override_reason_required`

## 2.28 Phase 10 Field-Level Executable Spec

### Table: `project_role_definitions`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `role_code`: string, required, max 128
  - `role_name`: string, required, max 256
  - `status`: enum(`active`,`archived`), required, indexed
- Indexes
  - unique(`project_id`,`role_code`)

### Table: `project_team_assignments`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `user_id`: string, required, indexed
  - `project_role_id`: string, required, indexed
  - `start_date`: date, required
  - `end_date`: date, optional
  - `status`: enum(`active`,`removed`), required, indexed
- Indexes
  - index(`project_id`,`status`)
  - index(`user_id`,`status`)

### `POST /team-assignments` request schema
- Request
  - `projectId`: string, required
  - `userId`: string, required
  - `projectRoleId`: string, required
  - `startDate`: string(date), required
  - `endDate`: string(date), optional
- Response
  - `id`, `status`
- Errors
  - `400 project_role_required`
  - `400 invalid_assignment_period`
  - `404 project_not_found`

### `POST /phase-approvals` request schema
- Request
  - `projectId`: string, required
  - `phaseCode`: string, required
  - `entryCriteriaSummary`: string, required, max 4000
  - `requiredEvidenceRefs[]`: array<string>, required, min 1
- Response
  - `id`, `status`
- Errors
  - `400 phase_entry_criteria_required`
  - `400 phase_evidence_required`

## 2.29 Phase 11 Field-Level Executable Spec

### Table: `master_data_items`
- Columns
  - `id`: string, required, PK
  - `domain`: string, required, max 128, indexed
  - `code`: string, required, max 128
  - `name`: string, required, max 256
  - `status`: enum(`active`,`archived`), required, indexed
  - `display_order`: integer, required, default `0`
  - `updated_at`: datetime, required, indexed
- Indexes
  - unique(`domain`,`code`)
  - index(`domain`,`status`,`display_order`)

### `POST /master-data` request schema
- Request
  - `domain`: string, required
  - `code`: string, required, max 128
  - `name`: string, required, max 256
  - `displayOrder`: integer, optional, default `0`
- Response
  - `id`, `domain`, `code`, `status`
- Errors
  - `400 validation_failed`
  - `409 master_data_code_duplicate`

## 2.30 Phase 12 Field-Level Executable Spec

### Table: `access_reviews`
- Columns
  - `id`: string, required, PK
  - `scope_type`: string, required, max 64
  - `scope_ref`: string, required
  - `review_cycle`: string, required, max 64
  - `reviewed_by`: string, optional
  - `status`: enum(`scheduled`,`in_review`,`approved`,`archived`), required, indexed
  - `decision_rationale`: string, optional, max 2000
- Indexes
  - index(`scope_type`,`status`)

### Table: `external_dependencies`
- Columns
  - `id`: string, required, PK
  - `name`: string, required, max 256
  - `dependency_type`: string, required, max 128, indexed
  - `owner_user_id`: string, required, indexed
  - `criticality`: enum(`low`,`medium`,`high`,`critical`), required, indexed
  - `status`: enum(`active`,`review_due`,`updated`,`archived`), required, indexed
  - `review_due_at`: datetime, optional, indexed
- Indexes
  - index(`criticality`,`status`,`review_due_at`)

### `POST /access-reviews` request schema
- Request
  - `scopeType`: string, required
  - `scopeRef`: string, required
  - `reviewCycle`: string, required
- Response
  - `id`, `status`
- Errors
  - `400 validation_failed`

### `POST /external-dependencies` request schema
- Request
  - `name`: string, required, max 256
  - `dependencyType`: string, required
  - `ownerUserId`: string, required
  - `criticality`: enum(`low`,`medium`,`high`,`critical`), required
  - `reviewDueAt`: string(datetime), optional
- Response
  - `id`, `status`
- Errors
  - `400 dependency_owner_required`
  - `400 dependency_criticality_required`

## 2.31 Phase 13 Field-Level Executable Spec

### Table: `raci_maps`
- Columns
  - `id`: string, required, PK
  - `process_code`: string, required, max 128, indexed
  - `role_name`: string, required, max 256
  - `responsibility_type`: enum(`R`,`A`,`C`,`I`), required, indexed
  - `status`: enum(`draft`,`approved`,`active`,`archived`), required, indexed
- Indexes
  - unique(`process_code`,`role_name`,`responsibility_type`)

### Table: `sla_rules`
- Columns
  - `id`: string, required, PK
  - `scope_type`: string, required, max 64, indexed
  - `scope_ref`: string, required
  - `target_duration_hours`: integer, required
  - `escalation_policy_id`: string, required
  - `status`: enum(`draft`,`approved`,`active`,`archived`), required, indexed
- Indexes
  - index(`scope_type`,`status`)

### `POST /sla-rules` request schema
- Request
  - `scopeType`: string, required
  - `scopeRef`: string, required
  - `targetDurationHours`: integer, required, min `1`
  - `escalationPolicyId`: string, required
- Response
  - `id`, `status`
- Errors
  - `400 sla_target_required`
  - `400 sla_escalation_policy_required`

## 2.32 Phase 14 Field-Level Executable Spec

### Table: `releases`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `release_code`: string, required, max 128
  - `title`: string, required, max 512
  - `planned_at`: datetime, optional, indexed
  - `released_at`: datetime, optional, indexed
  - `status`: enum(`draft`,`approved`,`released`,`archived`), required, indexed
- Indexes
  - unique(`project_id`,`release_code`)
  - index(`project_id`,`status`,`planned_at`)

### Table: `deployment_checklists`
- Columns
  - `id`: string, required, PK
  - `release_id`: string, required, indexed
  - `checklist_item`: string, required, max 512
  - `owner_user_id`: string, required
  - `status`: enum(`draft`,`reviewed`,`approved`,`executed`), required, indexed
  - `completed_at`: datetime, optional
- Indexes
  - index(`release_id`,`status`)

### `POST /releases` request schema
- Request
  - `projectId`: string, required
  - `releaseCode`: string, required, max 128
  - `title`: string, required, max 512
  - `plannedAt`: string(datetime), optional
- Response
  - `id`, `releaseCode`, `status`
- Errors
  - `400 validation_failed`
  - `409 release_code_duplicate`

## 2.33 Phase 15 Field-Level Executable Spec

### Table: `defects`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `code`: string, required, max 128
  - `title`: string, required, max 512
  - `description`: string, required, max 4000
  - `severity`: enum(`low`,`medium`,`high`,`critical`), required, indexed
  - `owner_user_id`: string, required, indexed
  - `status`: enum(`open`,`in_progress`,`resolved`,`closed`), required, indexed
  - `detected_in_phase`: string, optional, max 64
- Indexes
  - unique(`project_id`,`code`)
  - index(`project_id`,`severity`,`status`)

### Table: `non_conformances`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `code`: string, required, max 128
  - `title`: string, required, max 512
  - `description`: string, required, max 4000
  - `source_type`: string, required, max 128
  - `owner_user_id`: string, required
  - `status`: enum(`open`,`in_review`,`corrective_action`,`closed`), required, indexed
  - `corrective_action_ref`: string, optional
- Indexes
  - unique(`project_id`,`code`)
  - index(`project_id`,`status`)

### `POST /defects` request schema
- Request
  - `projectId`, `code`, `title`, `description`, `severity`, `ownerUserId`: required
  - `detectedInPhase`: optional
- Response
  - `id`, `status`
- Errors
  - `400 validation_failed`
  - `409 defect_code_duplicate`

## 2.34 Phase 16 Field-Level Executable Spec

### Table: `suppliers`
- Columns
  - `id`: string, required, PK
  - `name`: string, required, max 256
  - `supplier_type`: string, required, max 128, indexed
  - `owner_user_id`: string, required, indexed
  - `status`: enum(`active`,`review_due`,`updated`,`archived`), required, indexed
  - `criticality`: enum(`low`,`medium`,`high`,`critical`), required, indexed
- Indexes
  - unique(`name`)
  - index(`criticality`,`status`)

### Table: `supplier_agreements`
- Columns
  - `id`: string, required, PK
  - `supplier_id`: string, required, indexed
  - `agreement_type`: string, required, max 128
  - `effective_from`: date, required
  - `effective_to`: date, optional
  - `status`: enum(`draft`,`approved`,`active`,`archived`), required, indexed
  - `evidence_ref`: string, required
- Indexes
  - index(`supplier_id`,`status`)

### `POST /supplier-agreements` request schema
- Request
  - `supplierId`: string, required
  - `agreementType`: string, required
  - `effectiveFrom`: string(date), required
  - `effectiveTo`: string(date), optional
  - `evidenceRef`: string, required
- Response
  - `id`, `status`
- Errors
  - `400 supplier_agreement_effective_dates_required`
  - `400 supplier_agreement_evidence_required`

## 2.35 Phase 17 Field-Level Executable Spec

### Table: `metric_reviews`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `review_period`: string, required, max 64
  - `reviewed_by`: string, required
  - `status`: enum(`planned`,`reviewed`,`actions_tracked`,`closed`), required, indexed
  - `summary`: string, optional, max 4000
- Indexes
  - index(`project_id`,`status`)

### Table: `trend_reports`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `metric_definition_id`: string, required, indexed
  - `period_from`: date, required
  - `period_to`: date, required
  - `status`: enum(`draft`,`approved`,`archived`), required, indexed
  - `report_ref`: string, optional
- Indexes
  - index(`project_id`,`metric_definition_id`)

### `POST /metric-reviews` request schema
- Request
  - `projectId`: string, required
  - `reviewPeriod`: string, required
  - `reviewedBy`: string, required
  - `summary`: string, optional, max 4000
- Response
  - `id`, `status`
- Errors
  - `400 validation_failed`

## 2.36 Phase 18 Field-Level Executable Spec

### Table: `lessons_learned`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `title`: string, required, max 512
  - `summary`: string, required, max 4000
  - `lesson_type`: string, required, max 128, indexed
  - `owner_user_id`: string, required, indexed
  - `status`: enum(`draft`,`reviewed`,`published`,`archived`), required, indexed
  - `source_ref`: string, optional
- Indexes
  - index(`project_id`,`lesson_type`,`status`)

### `POST /lessons-learned` request schema
- Request
  - `projectId`, `title`, `summary`, `lessonType`, `ownerUserId`: required
  - `sourceRef`: optional
- Response
  - `id`, `status`
- Errors
  - `400 lesson_summary_required`
  - `400 validation_failed`

## 2.37 Phase 19 Field-Level Executable Spec

### Table: `access_recertification_schedules`
- Columns
  - `id`: string, required, PK
  - `scope_type`: string, required, max 64
  - `scope_ref`: string, required
  - `planned_at`: datetime, required, indexed
  - `review_owner_user_id`: string, required, indexed
  - `status`: enum(`planned`,`in_review`,`approved`,`completed`), required, indexed
- Indexes
  - index(`status`,`planned_at`)

### Table: `access_recertification_decisions`
- Columns
  - `id`: string, required, PK
  - `schedule_id`: string, required, indexed
  - `subject_user_id`: string, required, indexed
  - `decision`: enum(`kept`,`revoked`,`adjusted`), required, indexed
  - `reason`: string, required, max 2000
  - `decided_by`: string, required
  - `decided_at`: datetime, required
- Indexes
  - index(`schedule_id`,`decision`)

### `POST /access-recertifications` request schema
- Request
  - `scopeType`: string, required
  - `scopeRef`: string, required
  - `plannedAt`: string(datetime), required
  - `reviewOwnerUserId`: string, required
- Response
  - `id`, `status`
- Errors
  - `400 validation_failed`

## 2.38 Phase 20 Field-Level Executable Spec

### Table: `architecture_records`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, required, indexed
  - `title`: string, required, max 512
  - `architecture_type`: string, required, max 128, indexed
  - `owner_user_id`: string, required, indexed
  - `status`: enum(`draft`,`reviewed`,`approved`,`active`,`superseded`), required, indexed
  - `current_version_id`: string, optional
- Indexes
  - index(`project_id`,`architecture_type`,`status`)

### Table: `design_reviews`
- Columns
  - `id`: string, required, PK
  - `architecture_record_id`: string, required, indexed
  - `review_type`: string, required, max 128
  - `reviewed_by`: string, optional
  - `status`: enum(`draft`,`in_review`,`approved`,`rejected`,`baseline`), required, indexed
  - `decision_reason`: string, optional, max 2000
- Indexes
  - index(`architecture_record_id`,`status`)

### `POST /architecture-records` request schema
- Request
  - `projectId`, `title`, `architectureType`, `ownerUserId`: required
- Response
  - `id`, `status`
- Errors
  - `400 validation_failed`

## 2.39 Phase 21 Field-Level Executable Spec

### Table: `security_incidents`
- Columns
  - `id`: string, required, PK
  - `project_id`: string, optional, indexed
  - `code`: string, required, max 128
  - `title`: string, required, max 512
  - `severity`: enum(`low`,`medium`,`high`,`critical`), required, indexed
  - `reported_at`: datetime, required, indexed
  - `owner_user_id`: string, required, indexed
  - `status`: enum(`reported`,`assessed`,`contained`,`resolved`,`closed`), required, indexed
- Indexes
  - unique(`code`)
  - index(`severity`,`status`,`reported_at`)

### Table: `vulnerability_records`
- Columns
  - `id`: string, required, PK
  - `asset_ref`: string, required, indexed
  - `title`: string, required, max 512
  - `severity`: enum(`low`,`medium`,`high`,`critical`), required, indexed
  - `identified_at`: datetime, required, indexed
  - `owner_user_id`: string, required, indexed
  - `status`: enum(`open`,`assessed`,`scheduled`,`patched`,`verified`,`closed`), required, indexed
- Indexes
  - index(`asset_ref`,`status`)
  - index(`severity`,`status`)

### `POST /security-incidents` request schema
- Request
  - `projectId`: string, optional
  - `code`: string, required, max 128
  - `title`: string, required, max 512
  - `severity`: enum(`low`,`medium`,`high`,`critical`), required
  - `ownerUserId`: string, required
- Response
  - `id`, `status`
- Errors
  - `400 validation_failed`
  - `409 security_incident_code_duplicate`

## 2.40 Phase 22 Field-Level Executable Spec

### Table: `performance_baselines`
- Columns
  - `id`: string, required, PK
  - `scope_type`: string, required, max 64
  - `scope_ref`: string, required
  - `metric_name`: string, required, max 128, indexed
  - `target_value`: decimal, required
  - `threshold_value`: decimal, required
  - `status`: enum(`draft`,`approved`,`active`,`superseded`), required, indexed
- Indexes
  - index(`scope_type`,`metric_name`,`status`)

### Table: `slow_operation_reviews`
- Columns
  - `id`: string, required, PK
  - `operation_type`: string, required, max 64, indexed
  - `operation_key`: string, required, max 256, indexed
  - `observed_latency_ms`: integer, required
  - `status`: enum(`open`,`investigating`,`optimized`,`verified`,`closed`), required, indexed
  - `owner_user_id`: string, required, indexed
- Indexes
  - index(`operation_type`,`status`)
  - index(`owner_user_id`,`status`)

### `POST /performance-baselines` request schema
- Request
  - `scopeType`, `scopeRef`, `metricName`, `targetValue`, `thresholdValue`: required
- Response
  - `id`, `status`
- Errors
  - `400 validation_failed`

## 2.41 Phase 23 Field-Level Executable Spec

### Table: `backup_evidence`
- Columns
  - `id`: string, required, PK
  - `backup_scope`: string, required, max 128, indexed
  - `executed_at`: datetime, required, indexed
  - `executed_by`: string, required
  - `status`: enum(`planned`,`completed`,`verified`,`archived`), required, indexed
  - `evidence_ref`: string, optional
- Indexes
  - index(`backup_scope`,`executed_at`)

### Table: `legal_holds`
- Columns
  - `id`: string, required, PK
  - `scope_type`: string, required, max 64
  - `scope_ref`: string, required
  - `placed_at`: datetime, required, indexed
  - `placed_by`: string, required
  - `status`: enum(`active`,`released`,`archived`), required, indexed
  - `reason`: string, required, max 2000
- Indexes
  - index(`status`,`placed_at`)

### `POST /legal-holds` request schema
- Request
  - `scopeType`, `scopeRef`, `reason`: required
- Response
  - `id`, `status`
- Errors
  - `400 validation_failed`

## 2.42 Phase 24 Field-Level Executable Spec

### Table: `capa_records`
- Columns
  - `id`: string, required, PK
  - `source_type`: string, required, max 64, indexed
  - `source_ref`: string, required, indexed
  - `title`: string, required, max 512
  - `owner_user_id`: string, required, indexed
  - `root_cause_summary`: string, optional, max 4000
  - `status`: enum(`open`,`root_cause_analysis`,`action_planned`,`action_in_progress`,`verified`,`closed`), required, indexed
- Indexes
  - index(`source_type`,`status`)
  - index(`owner_user_id`,`status`)

### Table: `notification_queue`
- Columns
  - `id`: string, required, PK
  - `channel`: string, required, max 64, indexed
  - `target_ref`: string, required, max 256
  - `payload_ref`: string, optional
  - `queued_at`: datetime, required, indexed
  - `status`: enum(`queued`,`sent`,`failed`,`retried`,`closed`), required, indexed
  - `retry_count`: integer, required, default `0`
  - `last_error`: string, optional, max 2000
- Indexes
  - index(`status`,`queued_at`)
  - index(`channel`,`status`)

### `POST /capa` request schema
- Request
  - `sourceType`, `sourceRef`, `title`, `ownerUserId`: required
  - `rootCauseSummary`: optional
- Response
  - `id`, `status`
- Errors
  - `400 validation_failed`

### `PUT /notification-queue/{id}/retry` request schema
- Request
  - `reason`: string, optional, max 1000
- Response
  - `id`, `status`, `retryCount`
- Errors
  - `400 notification_retry_invalid_state`
  - `404 notification_not_found`

## 2.43 Remaining Gap to Reach Near-Final Executable Spec

After this file update, the remaining document-level gap is limited to production-informed tuning rather than missing specification structure:

1. Query-plan-driven index refinements once real implementation and dataset characteristics are known.
2. Production threshold tuning for timeout, retry, paging, and async cutover based on real load.
3. Optional expansion of non-core endpoint field coverage where a phase introduces more screens than currently planned.

At this point, the document is suitable as a near-final executable specification for phased delivery.

## 2.44 Phase-Specific Given/When/Then Scenario Packs

### Phase 0
- Given an admin with permission to manage roles, when the admin assigns a role to a user, then the API persists the assignment, the UI refreshes role state, and an audit event is recorded with actor, reason, and outcome.
- Given a user without permission to edit the permission matrix, when that user attempts to apply a matrix change, then the action is blocked, no mutation occurs, and a denial event is logged.
- Given the last active admin assignment, when removal is attempted, then the system rejects the request with `last_admin_removal_blocked`.

### Phase 1
- Given a project plan in draft, when required fields are complete and it is submitted for review, then the plan transitions to review and is visible in governance queues.
- Given a tailoring record without approval reason, when an approver attempts approval, then the system rejects the transition and stores no approval.

### Phase 2
- Given a document upload with required metadata, when the owner submits it for review, then the document transitions to review and its version is traceable.
- Given a classified document, when an unauthorized user requests export, then export is denied and the denial is logged.
- Given a large evidence package request, when the package exceeds synchronous threshold, then the system creates an async job instead of blocking the request.

### Phase 3
- Given an approved requirement, when a baseline is created, then the requirement enters governed baseline and appears in traceability views.
- Given a requirement missing mandatory downstream links, when a gate evaluation occurs, then progression is blocked and the missing links are shown.

### Phase 4
- Given a requested baseline change, when no approved CR exists, then the baseline registry rejects the change.
- Given an emergency override path, when the user lacks elevated permission or reason, then the override is denied and logged.

### Phase 5
- Given a risk without mitigation plan, when the owner tries to mark it mitigated, then the transition fails with `risk_mitigation_required`.
- Given an issue with open actions, when the owner tries to resolve it, then the system blocks resolution until actions are complete.

### Phase 6
- Given a meeting without attendees or summary, when minutes are approved, then approval is rejected.
- Given a decision without approved rationale, when a user tries to apply it, then the transition is rejected.

### Phase 7
- Given a test plan without entry and exit criteria, when approval is attempted, then the system blocks approval.
- Given a UAT submission without evidence, when approval is attempted, then the system returns `uat_evidence_required`.

### Phase 8
- Given an evidence export with invalid scope or date range, when export is requested, then the system rejects the request with a stable error code.
- Given an audit finding without resolution summary, when closure is attempted, then closure is denied.

### Phase 9
- Given a failed quality gate, when an unauthorized user tries to override it, then override is blocked and logged.
- Given a dashboard request, when multiple widgets are loaded, then the backend returns aggregated data without N+1 fan-out behavior.

### Phase 10
- Given a phase approval without required evidence, when submission occurs, then the system blocks the request with `phase_evidence_required`.
- Given a project role assignment with invalid dates, when save is attempted, then the system returns `invalid_assignment_period`.

### Phase 11
- Given active references to a master data item, when archive is attempted, then the system blocks archive with `master_data_in_use`.

### Phase 12
- Given an access review without rationale, when approval is attempted, then the system rejects the transition.
- Given an external dependency without owner or criticality, when save is attempted, then validation fails.

### Phase 13
- Given an SLA rule without escalation policy, when approval is attempted, then save fails with `sla_escalation_policy_required`.
- Given a workflow override log endpoint, when a mutation request is sent directly, then the system rejects it as read-only.

### Phase 14
- Given a release with incomplete checklist items, when release is attempted, then the system rejects the action with `release_checklist_incomplete`.
- Given release notes without approved release linkage, when publish is attempted, then publication is blocked.

### Phase 15
- Given a defect without resolution summary, when close is attempted, then the system rejects closure.
- Given a non-conformance without corrective action or accepted disposition, when close is attempted, then the system blocks closure.

### Phase 16
- Given a supplier agreement without effective dates or evidence, when save is attempted, then validation fails.
- Given a supplier with active agreement, when archive is attempted, then the system blocks archive unless governed closure path is used.

### Phase 17
- Given a metrics review with open follow-up actions, when close is attempted, then the system rejects closure.
- Given a trend report without metric or period, when approval is attempted, then validation fails.

### Phase 18
- Given a lesson without summary or source reference, when publish is attempted, then the system blocks publication.

### Phase 19
- Given pending recertification decisions, when the schedule is completed, then the system rejects completion.
- Given a revoke or adjust decision without rationale, when save is attempted, then validation fails.

### Phase 20
- Given a design review without decision reason, when approval is attempted, then the system rejects approval.
- Given an integration review without approved decision, when apply is attempted, then apply is blocked.

### Phase 21
- Given a privileged access request without approval, when use is attempted, then the event is rejected and logged.
- Given a security incident without resolution summary, when close is attempted, then closure is blocked.

### Phase 22
- Given a performance gate override without reason, when override is attempted, then the system rejects the request.
- Given a slow-operation review without verification, when closure is attempted, then the system blocks closure.

### Phase 23
- Given a restore verification without backup reference, when execution is recorded, then validation fails.
- Given a legal hold release without rationale, when release is attempted, then the system rejects the action.

### Phase 24
- Given a CAPA with open actions, when close is attempted, then the system rejects closure.
- Given a notification not in failed state, when retry is attempted, then the system rejects the retry with `notification_retry_invalid_state`.

## 2.45 Endpoint-Level Override Matrix

Use these overrides in addition to the default operational endpoint policy:

1. `GET /audit-events`
   - Page size max: `200`
   - Default sort: `occurred_at desc`
   - Timeout class: `interactive_read`
2. `POST /evidence-exports`
   - Timeout class: `heavy_read_or_export`
   - Async required above sync threshold
   - Idempotency recommended for duplicate requests over same scope/date window
3. `GET /traceability`
   - Page size max: `100`
   - Must remain server-filtered
   - Full graph expansion forbidden by default
4. `POST /documents/{id}/versions`
   - Timeout class: `interactive_write` for metadata; file upload may stream longer under upload policy
   - Async packaging only for downstream export, not upload itself
5. `PUT /quality-gates/{id}/override`
   - Idempotency recommended
   - Requires elevated permission, reason, audit event
6. `PUT /releases/{id}/release`
   - Idempotency required
   - Timeout class: `interactive_write`
   - Must fail fast on missing checklist or failed gate
7. `PUT /notification-queue/{id}/retry`
   - Idempotency required
   - Retry only from failed state
   - Bounded retry policy only
8. `POST /performance-gates/evaluate`
   - Queue execution if evaluation scope exceeds configured synchronous limit
9. `GET /documents`
   - Never include binary content, large evidence bodies, or full version payloads in list response
10. `GET /meetings`
   - Never include full minutes body in list response

## 2.46 Handoff Readiness Statement

This document is ready to be used for phase-by-phase implementation handoff under these assumptions:

1. The team will implement phases sequentially.
2. The team will update this file if implementation narrows or expands actual routes, entities, or states.
3. Production tuning of indexes and thresholds will happen after representative workload is available.
4. Fine-grain non-core endpoint schemas may still be added incrementally, but core phase execution can start without blocking on them.

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
