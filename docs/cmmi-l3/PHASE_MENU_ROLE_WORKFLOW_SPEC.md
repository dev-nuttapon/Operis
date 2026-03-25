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
