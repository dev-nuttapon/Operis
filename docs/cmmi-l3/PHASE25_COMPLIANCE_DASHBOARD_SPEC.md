# Phase 25: Compliance Dashboard Core Spec

This document defines the implementation-ready scope for Phase 25 after Phase 24 completion.

Use together with:

- `docs/cmmi-l3/POST_PHASE_CMMI_L3_ROADMAP.md`
- `docs/cmmi-l3/PHASE_MENU_ROLE_WORKFLOW_SPEC.md`
- `docs/cmmi-l3/PHASE_TEST_SPEC.md`
- `docs/MODULE_CONTRACTS.md`
- `docs/DATA_OWNERSHIP.md`

## 1. Goal

Provide a single compliance readiness view for projects and process areas using governed, read-only aggregation from existing modules.

This phase is intended to answer:

- Which projects are missing governed artifacts?
- Which approvals or baselines are overdue or stale?
- Which CAPA, audit, or security items are still open?
- Which process areas are currently weakest for a project?

## 2. In Scope

- compliance dashboard summary endpoint
- project and process area readiness summary
- overdue approval counters
- stale baseline counters
- open CAPA counters
- open audit finding counters
- open security review and incident counters
- drilldown payloads that point back to owning module routes
- dashboard filter preferences per user
- permission-gated dashboard screen

## 3. Out Of Scope

- evidence rule authoring
- generalized policy engine
- assessor package generation
- cross-project benchmarking beyond summary counts
- predictive scoring or trend forecasting
- external BI exports

## 4. Owning Module

Phase 25 is owned by `Governance`.

Rationale:

- the feature is cross-process and read-mostly
- it should not move ownership of underlying records away from their existing modules
- governance already owns cross-cutting oversight surfaces

## 5. Ownership Model

### 5.1 Tables Owned By Governance

- `compliance_snapshots`
- `compliance_dashboard_preferences`

### 5.2 Read-Only Upstream Sources

The dashboard may read from these modules only through application/query contracts or stable read access patterns approved by ownership:

- `Users`
- `Governance`
- `Requirements`
- `Documents`
- `ChangeControl`
- `Verification`
- `Audits`
- `Operations`
- `Metrics`

Phase 25 must not write into upstream tables owned by those modules.

## 6. Core Readiness Model

Readiness is displayed at two levels:

1. Project readiness
2. Process area readiness

The dashboard uses weighted counters and status buckets instead of opaque scoring only.

Required top-level summary cards:

- projects in good standing
- projects with missing governed artifacts
- overdue approvals
- stale baselines
- open CAPA
- open audit findings
- open security items

Required process area slices:

- process assets and planning
- requirements and traceability
- document governance
- change and configuration control
- verification and release readiness
- audit and CAPA
- security and operational resilience

## 7. Readiness Rules For First Version

### 7.1 Project In Good Standing

A project is in good standing when all of these are true:

- no overdue approvals
- no stale baselines
- no open critical CAPA
- no open critical audit findings
- no open critical security incidents

### 7.2 Missing Governed Artifacts

A project is counted as missing governed artifacts when one or more of these conditions is true:

- no active project plan
- no approved tailoring record where required
- no approved requirement baseline where requirements exist
- no approved document baseline where governed document types exist
- no active test plan for projects in verification/release stages

### 7.3 Overdue Approvals

Count records whose required approval date is before today and status is still pending/in-review/submitted, limited to approved source workflows.

### 7.4 Stale Baselines

Count baselines or baseline-eligible records that exceed their expected review cadence without refresh or supersession.

### 7.5 Open CAPA

Count CAPA records in non-terminal states.

### 7.6 Open Audit Findings

Count audit findings in non-terminal states.

### 7.7 Open Security Items

Count:

- security reviews that remain open
- security incidents not closed
- critical vulnerabilities not remediated

## 8. Data Model

## 8.1 `compliance_snapshots`

Purpose:

- retain dashboard outputs for auditability and performance
- allow superseded snapshot history

Minimum fields:

- `id`
- `project_id`
- `process_area`
- `period_start`
- `period_end`
- `readiness_score`
- `status`
- `missing_artifact_count`
- `overdue_approval_count`
- `stale_baseline_count`
- `open_capa_count`
- `open_audit_finding_count`
- `open_security_item_count`
- `details_json`
- `generated_at`
- `generated_by`
- `superseded_by_snapshot_id`
- audit columns consistent with existing conventions

Indexes:

- `(project_id, process_area, status)`
- `(period_start, period_end)`
- `(generated_at)`

## 8.2 `compliance_dashboard_preferences`

Purpose:

- persist user dashboard filters and view defaults

Minimum fields:

- `id`
- `user_id`
- `default_project_id`
- `default_process_area`
- `default_period_days`
- `default_show_only_at_risk`
- `updated_at`

Indexes:

- unique `(user_id)`

## 9. Backend Contracts

## 9.1 Queries

### `GET /api/v1/governance/compliance-dashboard`

Query params:

- `projectId`
- `processArea`
- `periodDays`
- `showOnlyAtRisk`

Response must include:

- `summary`
- `projects`
- `processAreas`
- `generatedAt`
- `filters`

### `GET /api/v1/governance/compliance-dashboard/drilldown`

Query params:

- `projectId`
- `processArea`
- `issueType`

Allowed `issueType` values:

- `missing-artifact`
- `overdue-approval`
- `stale-baseline`
- `open-capa`
- `open-audit-finding`
- `open-security-item`

Response must include:

- list of drilldown rows
- owning module
- target route
- target id
- target status
- overdue or stale metadata when applicable

### `PUT /api/v1/governance/compliance-dashboard/preferences`

Request:

- `defaultProjectId`
- `defaultProcessArea`
- `defaultPeriodDays`
- `defaultShowOnlyAtRisk`

Response:

- saved preference payload

## 9.2 Application Layer Responsibilities

`GovernanceModule.cs`:

- endpoint composition only

`Application/`:

- dashboard aggregation query service
- snapshot persistence service
- preference query and command services
- drilldown mapper service

Inline endpoint lambdas are not allowed.

## 10. Frontend Scope

## 10.1 Routes

- `/app/governance/compliance-dashboard`

## 10.2 Pages

- `ComplianceDashboardPage.tsx`

## 10.3 Hook/API Pattern

Required structure:

- `modules/governance/pages/ComplianceDashboardPage.tsx`
- `modules/governance/hooks/useGovernance.ts` or dedicated `useComplianceDashboard.ts`
- `modules/governance/api/governanceApi.ts`
- module public export update

The page must remain thin and delegate all API work through hooks.

## 10.4 Required UI Sections

- filter bar
- summary cards
- per-project readiness table
- per-process-area readiness table
- drilldown drawer or panel
- last-generated timestamp

## 10.5 Required Filters

- project
- process area
- period
- at-risk only toggle

## 10.6 Required Actions

- load dashboard
- open drilldown
- save default filters
- navigate to owning module target

## 11. Permissions

Backend and frontend must both recognize:

- `governance.compliance.read`
- `governance.compliance.manage`

Expected access model:

- `read` may load dashboard and drilldowns
- `manage` may save dashboard preferences and trigger snapshot refresh if added in implementation

Permission matrix defaults must be updated for relevant admin and governance roles.

## 12. Workflow States

### 12.1 Snapshot Lifecycle

- `draft`
- `published`
- `superseded`

Rules:

- new generation creates a `published` snapshot
- previous published snapshot for the same project/process window becomes `superseded`
- drilldowns must reflect current published snapshot by default

### 12.2 Dashboard Preference Lifecycle

- preferences do not need a workflow
- last write wins, with audit trail

## 13. Validation And Error Codes

Minimum required error codes:

- `compliance_dashboard_scope_required`
- `compliance_dashboard_period_invalid`
- `compliance_dashboard_issue_type_invalid`
- `compliance_dashboard_project_not_found`
- `compliance_dashboard_process_area_invalid`

Validation rules:

- `periodDays` must be within an allowed bounded range
- `issueType` must be one of the supported values
- `processArea` must be one of the governed slices defined in this spec
- `projectId` must reference a visible project

## 14. Cross-Module Read Contracts

Phase 25 should prefer existing query services where already available.

If a needed aggregate is not currently available, add read-only query contracts in owning modules rather than reading foreign tables ad hoc from endpoint composition.

Required read concerns:

- project status and lifecycle
- project plan existence and approval state
- tailoring status
- requirement baseline readiness
- document baseline readiness
- test plan/UAT readiness
- open CAPA count
- open audit finding count
- open security item count

## 15. Audit Requirements

The system must log:

- preference updates
- snapshot generation
- permission-denied dashboard actions for sensitive drilldowns if current audit policy already covers denied operations

Audit events must capture:

- actor
- scope
- action
- outcome
- timestamp

## 16. Performance Requirements

The dashboard is a report surface and must be deliberate about query shape.

Minimum requirements:

- use server-side filtering
- avoid N+1 cross-module queries where aggregation can be batched
- support paging on project-level readiness rows if volume grows
- store snapshots to reduce repeated heavy recomputation when appropriate
- avoid transferring unrelated detail payloads until drilldown is opened

## 17. Tests Required

## 17.1 Functional

- Given a project with missing requirements baseline, when dashboard loads, then missing artifact count includes that project.
- Given a project with open CAPA and overdue approvals, when dashboard loads, then both counts appear in summary and project rows.
- Given a saved preference, when the user returns to the dashboard, then defaults are restored.

## 17.2 Permission

- Given a user without `governance.compliance.read`, when dashboard is requested, then access is forbidden.
- Given a user without `governance.compliance.manage`, when preference update is attempted, then access is forbidden.

## 17.3 Audit

- Given a preference update, when save succeeds, then an audit event exists with actor and updated scope.
- Given snapshot generation, when completed, then an audit event exists with project/process scope and outcome.

## 17.4 Performance

- Given many projects, when dashboard loads, then aggregation remains bounded and detail payloads are deferred.

## 17.5 Frontend

- dashboard filter changes update query state
- drilldown opens expected row set
- at-risk-only filter reduces rendered rows correctly

## 18. Quality Gates

Frontend:

- `npm run check:architecture`
- `npm test`
- `npm run build:local`
- `npm run perf:bundle-report`
- `npm run perf:bundle-budget`

Backend:

- `node scripts/check-backend-architecture.mjs`
- `node scripts/check-module-contracts.mjs`
- `dotnet build apps/backend/Operis_API/Operis_API.csproj`
- `dotnet test apps/backend/Operis_API.Tests/Operis_API.Tests.csproj`

## 19. Migration Approach

Preferred:

- standard EF migration

Fallback when local tooling requires it:

- manual migration consistent with current repository practice

Migration must add:

- `compliance_snapshots`
- `compliance_dashboard_preferences`
- required indexes

## 20. Acceptance Criteria

Phase 25 is complete when:

1. compliance dashboard route exists and is permission-guarded
2. dashboard loads summary and readiness data from real module state
3. drilldowns navigate users to owning module screens
4. preferences can be saved and restored
5. snapshot history is persisted
6. tests are added for aggregation, permissions, and UI behavior
7. all quality gates pass

## 21. Recommended Codex Prompt

```md
Implement Phase 25 from docs/cmmi-l3/PHASE25_COMPLIANCE_DASHBOARD_SPEC.md.
Use the companion CMMI docs as required.
Keep module ownership intact.
Finish the phase in full:
- backend
- frontend
- permissions
- migration
- tests
- docs
- quality gates
Then report:
- files changed
- tests run
- quality gates run
- residual risks
```
