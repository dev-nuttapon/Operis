# Post-Phase 24 CMMI L3 Roadmap

This document defines the post-delivery roadmap after Phase 24 so future work can be executed one phase at a time without scope drift.

Use this document together with:

- `docs/cmmi-l3/PHASE_MENU_ROLE_WORKFLOW_SPEC.md`
- `docs/cmmi-l3/PHASE_TRANSITION_MATRIX.md`
- `docs/cmmi-l3/PHASE_TEST_SPEC.md`
- `docs/cmmi-l3/PHASE_OPERATIONS_RUNBOOK.md`
- `docs/cmmi-l3/PHASE25_COMPLIANCE_DASHBOARD_SPEC.md`
- `docs/MODULE_CONTRACTS.md`
- `docs/DATA_OWNERSHIP.md`

## 1. How To Use This Roadmap With Codex

Rules for prompting:

1. Implement one phase per prompt.
2. Update spec/docs before or alongside implementation.
3. Preserve module ownership. Do not create a new module unless the phase explicitly requires one.
4. Keep endpoint and page layers thin.
5. Cross-module reads must use public query/application contracts.
6. Every phase must finish with tests, quality gates, and a residual-risk summary.

Required completion checklist for each phase:

1. Goal and scope are restated.
2. Backend ownership is explicit.
3. Frontend routes and pages are explicit.
4. Permissions are explicit.
5. Workflow states are explicit.
6. Validation and error codes are explicit.
7. Migration approach is explicit.
8. Required tests are implemented and run.
9. Quality gates are run.

Required Codex delivery summary for each phase:

- files changed
- migrations added or updated
- tests run
- quality gates run
- remaining risks or deferred gaps

## 2. Phase Spec Template

Every new phase spec should follow this shape:

```md
## Phase XX: <Name>
- Goal
- In Scope
- Out of Scope
- Owning Module
- Owned Tables / Records
- Screens / Routes
- Permissions
- API Contracts
- Validation / Error Codes
- Workflow States
- Integrations / Touchpoints
- Tests Required
- Quality Gates
- Acceptance Criteria
```

## 3. Global Guardrails

### 3.1 Backend

- `*Module.cs` must remain composition only.
- Endpoints may validate HTTP concerns, but orchestration belongs in `Application/`.
- New tables must have one owning module.
- Non-owning modules must not write another module's tables directly.
- Cross-module queries should prefer application/query contracts over direct `OperisDbContext` access.

### 3.2 Frontend

- `Page -> Hook -> API -> HTTP Client` is mandatory.
- New pages must live inside a module.
- New routes must use public module exports.
- Permission checks must exist both at route level and action level for sensitive actions.

### 3.3 Quality Gates

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

## 4. Post-Phase Roadmap

## Phase 25: Compliance Dashboard Core

Detailed implementation spec:

- `docs/cmmi-l3/PHASE25_COMPLIANCE_DASHBOARD_SPEC.md`

- Goal
  - Provide a single readiness view for projects, process areas, and overdue compliance work.
- In Scope
  - dashboard summary widgets
  - project readiness score
  - missing artifact counters
  - overdue approvals, stale baselines, open CAPA summary
  - drilldown links into existing modules
- Out of Scope
  - rule engine for detailed evidence evaluation
  - assessor package export
- Owning Module
  - `Governance`
- Owned Tables / Records
  - `compliance_snapshots`
  - `compliance_dashboard_preferences`
- Screens / Routes
  - `/app/governance/compliance-dashboard`
- Permissions
  - `governance.compliance.read`
  - `governance.compliance.manage`
- API Contracts
  - `GET /api/v1/governance/compliance-dashboard`
  - `GET /api/v1/governance/compliance-dashboard/drilldown`
  - `PUT /api/v1/governance/compliance-dashboard/preferences`
- Validation / Error Codes
  - `compliance_dashboard_scope_required`
  - `compliance_dashboard_period_invalid`
- Workflow States
  - snapshot states: `draft -> published -> superseded`
- Integrations / Touchpoints
  - read-only aggregation from `Requirements`, `Documents`, `Verification`, `Operations`, `Metrics`
- Tests Required
  - aggregation query tests
  - permission tests
  - UI filter and drilldown tests
- Quality Gates
  - all global gates
- Acceptance Criteria
  - readiness dashboard loads from real module data
  - users can filter by project, process area, period
  - each widget links to an owned module screen with retained filters

## Phase 26: Evidence Completeness Rules

Companion spec:

* [PHASE26_EVIDENCE_COMPLETENESS_SPEC.md](/Users/nuttapon/Github-dev/Operis/docs/cmmi-l3/PHASE26_EVIDENCE_COMPLETENESS_SPEC.md)

- Goal
  - Detect missing required evidence for each project and process area.
- In Scope
  - evidence rule definitions
  - evaluation engine
  - missing evidence register
  - drilldown by artifact type
- Out of Scope
  - assessor-facing package workflows
- Owning Module
  - `Audits`
- Owned Tables / Records
  - `evidence_rules`
  - `evidence_rule_results`
  - `evidence_missing_items`
- Screens / Routes
  - `/app/audits/evidence-completeness`
  - `/app/audits/evidence-completeness/:resultId`
- Permissions
  - `audits.evidence.read`
  - `audits.evidence.manage`
- API Contracts
  - `GET /api/v1/audits/evidence-rules`
  - `POST /api/v1/audits/evidence-rules/evaluate`
  - `GET /api/v1/audits/evidence-results`
- Validation / Error Codes
  - `evidence_rule_target_required`
  - `evidence_rule_expression_invalid`
  - `evidence_missing_approval`
  - `evidence_missing_traceability`
- Workflow States
  - rule states: `draft -> active -> retired`
  - evaluation states: `queued -> completed -> superseded`
- Integrations / Touchpoints
  - reads from all evidence-producing modules
- Tests Required
  - evaluator tests
  - rule lifecycle tests
  - page rendering tests for missing evidence states
- Quality Gates
  - all global gates
- Acceptance Criteria
  - users can define and activate evidence rules
  - evaluations identify missing evidence records by project and process area
  - dashboard drilldowns open exact missing artifacts

## Phase 27: Management Review Cadence

Companion spec:

* [PHASE27_MANAGEMENT_REVIEW_CADENCE_SPEC.md](/Users/nuttapon/Github-dev/Operis/docs/cmmi-l3/PHASE27_MANAGEMENT_REVIEW_CADENCE_SPEC.md)

- Goal
  - Institutionalize management review as a governed recurring process.
- In Scope
  - review schedule
  - agenda and minutes
  - decisions and follow-up actions
  - escalation linkage
- Out of Scope
  - broad meeting module refactor beyond management reviews
- Owning Module
  - `Governance`
- Owned Tables / Records
  - `management_reviews`
  - `management_review_items`
  - `management_review_actions`
- Screens / Routes
  - `/app/governance/management-reviews`
  - `/app/governance/management-reviews/:reviewId`
- Permissions
  - `governance.management_reviews.read`
  - `governance.management_reviews.manage`
  - `governance.management_reviews.approve`
- API Contracts
  - `GET /api/v1/governance/management-reviews`
  - `POST /api/v1/governance/management-reviews`
  - `POST /api/v1/governance/management-reviews/{id}/transition`
- Validation / Error Codes
  - `management_review_schedule_required`
  - `management_review_minutes_required`
  - `management_review_open_actions_block_close`
- Workflow States
  - `draft -> scheduled -> in_review -> closed -> archived`
- Integrations / Touchpoints
  - actions may link to `CAPA`, `Escalation`, `Risks`
- Tests Required
  - state transition tests
  - open-action close-block tests
  - UI schedule/detail tests
- Quality Gates
  - all global gates
- Acceptance Criteria
  - review cannot close while mandatory actions remain open
  - decisions and actions are retained with audit history

## Phase 28: Training & Competency

- Companion Spec
  - [PHASE28_TRAINING_COMPETENCY_SPEC.md](/Users/nuttapon/Github-dev/Operis/docs/cmmi-l3/PHASE28_TRAINING_COMPETENCY_SPEC.md)

- Goal
  - Track required training and competency readiness by role and user.
- In Scope
  - training catalog
  - role training requirements
  - training completion tracking
  - overdue training visibility
- Out of Scope
  - LMS integration unless added later as explicit touchpoint work
- Owning Module
  - `Learning`
- Owned Tables / Records
  - `training_courses`
  - `role_training_requirements`
  - `training_completions`
  - `competency_reviews`
- Screens / Routes
  - `/app/learning/training-catalog`
  - `/app/learning/role-training-matrix`
  - `/app/learning/completions`
- Permissions
  - `learning.training.read`
  - `learning.training.manage`
  - `learning.training.approve`
- API Contracts
  - `GET /api/v1/learning/courses`
  - `POST /api/v1/learning/completions`
  - `GET /api/v1/learning/role-matrix`
- Validation / Error Codes
  - `training_course_title_required`
  - `training_requirement_role_required`
  - `training_completion_date_required`
- Workflow States
  - course states: `draft -> active -> retired`
  - completion states: `assigned -> completed -> expired`
- Integrations / Touchpoints
  - role mapping from `Users`
- Tests Required
  - completion expiry tests
  - role requirement tests
  - UI matrix tests
- Quality Gates
  - all global gates
- Acceptance Criteria
  - role-to-training requirements are visible
  - overdue and expired training is visible by user and project

## Phase 29: Policy Acknowledgement

Companion spec:

* [PHASE29_POLICY_ACKNOWLEDGEMENT_SPEC.md](/Users/nuttapon/Github-dev/Operis/docs/cmmi-l3/PHASE29_POLICY_ACKNOWLEDGEMENT_SPEC.md)

- Goal
  - Govern policy publication, acknowledgement, and overdue attestation.
- In Scope
  - policy register
  - acknowledgement campaigns
  - attestation records
  - overdue acknowledgement reporting
- Out of Scope
  - external policy distribution integrations
- Owning Module
  - `Governance`
- Owned Tables / Records
  - `policies`
  - `policy_campaigns`
  - `policy_acknowledgements`
- Screens / Routes
  - `/app/governance/policies`
  - `/app/governance/policy-acknowledgements`
- Permissions
  - `governance.policies.read`
  - `governance.policies.manage`
  - `governance.policies.approve`
- API Contracts
  - `GET /api/v1/governance/policies`
  - `POST /api/v1/governance/policy-campaigns`
  - `POST /api/v1/governance/policy-acknowledgements`
- Validation / Error Codes
  - `policy_title_required`
  - `policy_effective_date_required`
  - `policy_campaign_scope_required`
- Workflow States
  - policy states: `draft -> approved -> published -> retired`
  - campaign states: `draft -> launched -> closed`
- Integrations / Touchpoints
  - user targeting from `Users`
- Tests Required
  - campaign launch tests
  - overdue acknowledgement tests
  - UI acknowledgement flow tests
- Quality Gates
  - all global gates
- Acceptance Criteria
  - published policies can be acknowledged by target users
  - overdue acknowledgements are visible and exportable

## Phase 30: Tailoring Governance Hardening

- Goal
  - Govern project tailoring deviations against standard process more strictly.
- In Scope
  - tailoring criteria library
  - deviation reasons
  - approval chain
  - periodic tailoring review
- Out of Scope
  - full project planning rewrite
- Owning Module
  - `Governance`
- Owned Tables / Records
  - `tailoring_criteria`
  - `tailoring_review_cycles`
  - `tailoring_deviations`
- Screens / Routes
  - `/app/governance/tailoring-criteria`
  - `/app/projects/tailoring-reviews`
- Permissions
  - `governance.tailoring.read`
  - `governance.tailoring.manage`
  - `governance.tailoring.approve`
- API Contracts
  - `GET /api/v1/governance/tailoring-criteria`
  - `POST /api/v1/governance/tailoring-reviews`
  - `POST /api/v1/governance/tailoring-reviews/{id}/transition`
- Validation / Error Codes
  - `tailoring_standard_reference_required`
  - `tailoring_deviation_reason_required`
  - `tailoring_review_due_date_required`
- Workflow States
  - review states: `draft -> submitted -> approved/rejected -> expired`
- Integrations / Touchpoints
  - project context from `Users`
- Tests Required
  - deviation approval tests
  - expiry tests
  - UI review tests
- Quality Gates
  - all global gates
- Acceptance Criteria
  - deviations without approval are highlighted
  - expired tailoring reviews are visible in governance reporting

## Phase 31: Adoption Scoring

- Goal
  - Measure actual platform process adoption by project.
- In Scope
  - adoption rules
  - project scorecards
  - anomaly flags for underused workflows
- Out of Scope
  - predictive analytics
- Owning Module
  - `Metrics`
- Owned Tables / Records
  - `adoption_rules`
  - `adoption_scores`
  - `adoption_anomalies`
- Screens / Routes
  - `/app/metrics/adoption-scorecards`
- Permissions
  - `metrics.adoption.read`
  - `metrics.adoption.manage`
- API Contracts
  - `GET /api/v1/metrics/adoption-scorecards`
  - `POST /api/v1/metrics/adoption-rules/evaluate`
- Validation / Error Codes
  - `adoption_rule_scope_required`
  - `adoption_rule_threshold_invalid`
- Workflow States
  - rule states: `draft -> active -> retired`
  - score states: `calculated -> published -> superseded`
- Integrations / Touchpoints
  - read-only metrics from all major modules
- Tests Required
  - scoring engine tests
  - anomaly detection tests
  - UI scorecard tests
- Quality Gates
  - all global gates
- Acceptance Criteria
  - adoption score is visible per project and process area
  - anomaly flags identify missing or weak operational usage

## Phase 32: Exception & Waiver Register

- Goal
  - Govern temporary process waivers and compensating controls.
- In Scope
  - waiver request
  - compensating controls
  - approval and expiry
  - recurring review
- Out of Scope
  - permanent policy redesign
- Owning Module
  - `Exceptions`
- Owned Tables / Records
  - `waivers`
  - `compensating_controls`
  - `waiver_reviews`
- Screens / Routes
  - `/app/exceptions/waivers`
  - `/app/exceptions/waivers/:waiverId`
- Permissions
  - `exceptions.waivers.read`
  - `exceptions.waivers.manage`
  - `exceptions.waivers.approve`
- API Contracts
  - `GET /api/v1/exceptions/waivers`
  - `POST /api/v1/exceptions/waivers`
  - `POST /api/v1/exceptions/waivers/{id}/transition`
- Validation / Error Codes
  - `waiver_scope_required`
  - `waiver_expiry_required`
  - `waiver_compensating_control_required`
- Workflow States
  - `draft -> submitted -> approved/rejected -> expired -> closed`
- Integrations / Touchpoints
  - references process areas from `Governance`
- Tests Required
  - expiry tests
  - approval tests
  - UI register/detail tests
- Quality Gates
  - all global gates
- Acceptance Criteria
  - waivers cannot be approved without compensating controls
  - expired waivers appear in dashboard and review queues

## Phase 33: CAPA Effectiveness Review

- Goal
  - Verify whether completed CAPA actions were effective after closure.
- In Scope
  - post-closure review
  - effectiveness result
  - reopen path
- Out of Scope
  - redesign of original CAPA workflow
- Owning Module
  - `Operations`
- Owned Tables / Records
  - `capa_effectiveness_reviews`
- Screens / Routes
  - `/app/operations/capa-effectiveness`
- Permissions
  - `operations.capa.read`
  - `operations.capa.manage`
  - `operations.capa.approve`
- API Contracts
  - `GET /api/v1/operations/capa-effectiveness`
  - `POST /api/v1/operations/capa-effectiveness`
  - `POST /api/v1/operations/capa/{id}/reopen`
- Validation / Error Codes
  - `capa_effectiveness_result_required`
  - `capa_effectiveness_evidence_required`
- Workflow States
  - review states: `draft -> submitted -> accepted/ineffective`
- Integrations / Touchpoints
  - existing `CAPA` records
- Tests Required
  - reopen tests
  - effectiveness evidence tests
  - UI review tests
- Quality Gates
  - all global gates
- Acceptance Criteria
  - ineffective CAPA can be reopened with traceable reason
  - effective versus ineffective closure is reportable

## Phase 34: Assessor Workspace

- Goal
  - Provide an assessor-facing workspace to package and review evidence.
- In Scope
  - evidence package builder
  - package scope by project and process area
  - findings tracker
  - assessor notes
- Out of Scope
  - external assessor portal authentication hardening beyond local app roles
- Owning Module
  - `Assessment`
- Owned Tables / Records
  - `assessment_packages`
  - `assessment_findings`
  - `assessment_notes`
- Screens / Routes
  - `/app/assessment/workspace`
  - `/app/assessment/findings`
- Permissions
  - `assessment.workspace.read`
  - `assessment.workspace.manage`
  - `assessment.workspace.review`
- API Contracts
  - `GET /api/v1/assessment/packages`
  - `POST /api/v1/assessment/packages`
  - `POST /api/v1/assessment/findings`
- Validation / Error Codes
  - `assessment_package_scope_required`
  - `assessment_finding_title_required`
- Workflow States
  - package states: `draft -> prepared -> shared -> archived`
  - finding states: `open -> accepted -> closed`
- Integrations / Touchpoints
  - read-only evidence references across modules
- Tests Required
  - package build tests
  - findings lifecycle tests
  - workspace UI tests
- Quality Gates
  - all global gates
- Acceptance Criteria
  - packages can be created from project and process filters
  - findings can reference exact evidence and follow closure lifecycle

## Phase 35: Control Mapping

- Goal
  - Map controls or practices to system artifacts and process evidence.
- In Scope
  - control catalog
  - mapping rules
  - coverage reporting
- Out of Scope
  - automated standards import unless explicitly added later
- Owning Module
  - `Assessment`
- Owned Tables / Records
  - `control_catalog`
  - `control_mappings`
  - `control_coverage_snapshots`
- Screens / Routes
  - `/app/assessment/control-mapping`
  - `/app/assessment/control-coverage`
- Permissions
  - `assessment.controls.read`
  - `assessment.controls.manage`
- API Contracts
  - `GET /api/v1/assessment/control-catalog`
  - `POST /api/v1/assessment/control-mappings`
  - `GET /api/v1/assessment/control-coverage`
- Validation / Error Codes
  - `control_code_required`
  - `control_mapping_target_required`
- Workflow States
  - mapping states: `draft -> active -> retired`
- Integrations / Touchpoints
  - evidence references from all modules
- Tests Required
  - mapping tests
  - coverage aggregation tests
  - UI coverage tests
- Quality Gates
  - all global gates
- Acceptance Criteria
  - users can see which controls have sufficient evidence coverage
  - gaps are reportable by control set and project

## Phase 36: Operational Automation

- Goal
  - Record and govern recurring operational automation work as first-class controls.
- In Scope
  - automation job definitions
  - job run history
  - evidence linkage
  - failed-job visibility
- Out of Scope
  - full external orchestrator replacement
- Owning Module
  - `Operations`
- Owned Tables / Records
  - `automation_jobs`
  - `automation_job_runs`
  - `automation_job_evidence_refs`
- Screens / Routes
  - `/app/operations/automation`
  - `/app/operations/automation/runs`
- Permissions
  - `operations.automation.read`
  - `operations.automation.manage`
  - `operations.automation.execute`
- API Contracts
  - `GET /api/v1/operations/automation-jobs`
  - `POST /api/v1/operations/automation-jobs`
  - `GET /api/v1/operations/automation-job-runs`
- Validation / Error Codes
  - `automation_job_name_required`
  - `automation_job_type_required`
  - `automation_job_evidence_required`
- Workflow States
  - job states: `draft -> active -> paused -> retired`
  - run states: `queued -> running -> succeeded/failed`
- Integrations / Touchpoints
  - backup, retention, export, secret rotation, alert jobs
- Tests Required
  - job state tests
  - run history tests
  - UI execution history tests
- Quality Gates
  - all global gates
- Acceptance Criteria
  - operational jobs and runs are traceable
  - failed runs are visible with linked evidence or remediation path

## Phase 37: Production Readiness & Rollout

- Goal
  - Make rollout and production readiness measurable and auditable.
- In Scope
  - environment readiness checklist
  - runbook attestation
  - rollout gate view
  - go-live status summary
- Out of Scope
  - infrastructure-as-code redesign
- Owning Module
  - `Operations`
- Owned Tables / Records
  - `environment_readiness`
  - `rollout_checklists`
  - `runbook_attestations`
- Screens / Routes
  - `/app/operations/production-readiness`
- Permissions
  - `operations.production_readiness.read`
  - `operations.production_readiness.manage`
  - `operations.production_readiness.approve`
- API Contracts
  - `GET /api/v1/operations/production-readiness`
  - `POST /api/v1/operations/rollout-checklists`
  - `POST /api/v1/operations/runbook-attestations`
- Validation / Error Codes
  - `production_environment_required`
  - `rollout_gate_result_required`
  - `runbook_attestation_required`
- Workflow States
  - readiness states: `draft -> assessed -> approved -> blocked`
- Integrations / Touchpoints
  - `Release`, `Operations`, and runbook evidence from existing modules
- Tests Required
  - readiness gate tests
  - attestation tests
  - UI readiness dashboard tests
- Quality Gates
  - all global gates
- Acceptance Criteria
  - users can assess production readiness per environment
  - blocked rollout reasons are visible and auditable

## 5. Recommended Execution Order

Highest value first:

1. Phase 25: Compliance Dashboard Core
2. Phase 26: Evidence Completeness Rules
3. Phase 27: Management Review Cadence
4. Phase 28: Training & Competency
5. Phase 32: Exception & Waiver Register
6. Phase 34: Assessor Workspace

## 6. Prompting Pattern For Future Implementation

Recommended prompt shape:

```md
Implement Phase XX from docs/cmmi-l3/POST_PHASE_CMMI_L3_ROADMAP.md.
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
