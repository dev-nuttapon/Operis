# MODULE_CONTRACTS.md

Public contract map for current modules.

Use this document to keep extraction boundaries explicit while the codebase is still small.

---

# Frontend Public Surfaces

## auth

Public entry:

* [apps/frontend/src/modules/auth/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/auth/index.ts)
* [apps/frontend/src/modules/auth/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/auth/README.md)

Notes:

* app shell should import auth capabilities only through this surface
* auth is cross-cutting and should stay thin at the composition layer

## users

Public entries:

* [apps/frontend/src/modules/users/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/users/index.ts)
* [apps/frontend/src/modules/users/public.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/users/public.ts)
* [apps/frontend/src/modules/users/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/users/README.md)

Notes:

* route-level composition should prefer `public.ts` where bundle boundaries matter
* pages should keep using `Page -> Hook -> API -> HTTP client`

## documents

Public entry:

* [apps/frontend/src/modules/documents/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/documents/index.ts)
* [apps/frontend/src/modules/documents/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/documents/README.md)

## audits

Public entry:

* [apps/frontend/src/modules/audits/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/audits/index.ts)
* [apps/frontend/src/modules/audits/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/audits/README.md)

## activities

Public entry:

* [apps/frontend/src/modules/activities/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/activities/index.ts)
* [apps/frontend/src/modules/activities/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/activities/README.md)

## workflows

Public entry:

* [apps/frontend/src/modules/workflows/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/workflows/index.ts)
* [apps/frontend/src/modules/workflows/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/workflows/README.md)

## notifications

Public entry:

* [apps/frontend/src/modules/notifications/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/notifications/index.ts)
* [apps/frontend/src/modules/notifications/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/notifications/README.md)

## governance

Public entry:

* [apps/frontend/src/modules/governance/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/governance/index.ts)
* [apps/frontend/src/modules/governance/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/governance/README.md)

## requirements

Public entry:

* [apps/frontend/src/modules/requirements/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/requirements/index.ts)
* [apps/frontend/src/modules/requirements/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/requirements/README.md)

## change-control

Public entry:

* [apps/frontend/src/modules/change-control/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/change-control/index.ts)
* [apps/frontend/src/modules/change-control/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/change-control/README.md)

## risks

Public entry:

* [apps/frontend/src/modules/risks/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/risks/index.ts)
* [apps/frontend/src/modules/risks/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/risks/README.md)

## meetings

Public entry:

* [apps/frontend/src/modules/meetings/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/meetings/index.ts)
* [apps/frontend/src/modules/meetings/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/meetings/README.md)

## verification

Public entry:

* [apps/frontend/src/modules/verification/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/verification/index.ts)
* [apps/frontend/src/modules/verification/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/verification/README.md)

## metrics

Public entry:

* [apps/frontend/src/modules/metrics/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/metrics/index.ts)
* [apps/frontend/src/modules/metrics/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/metrics/README.md)

## releases

Public entry:

* [apps/frontend/src/modules/releases/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/releases/index.ts)
* [apps/frontend/src/modules/releases/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/releases/README.md)

## defects

Public entry:

* [apps/frontend/src/modules/defects/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/defects/index.ts)
* [apps/frontend/src/modules/defects/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/defects/README.md)

## operations

Public entry:

* [apps/frontend/src/modules/operations/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/operations/index.ts)
* [apps/frontend/src/modules/operations/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/operations/README.md)

## knowledge

Public entry:

* [apps/frontend/src/modules/knowledge/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/knowledge/index.ts)
* [apps/frontend/src/modules/knowledge/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/knowledge/README.md)

## learning

Public entry:

* [apps/frontend/src/modules/learning/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/learning/index.ts)
* [apps/frontend/src/modules/learning/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/learning/README.md)

---

# Backend Public Surfaces

## users

Module entry:

* [apps/backend/Operis_API/Modules/Users/UsersModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Users/UsersModule.cs)
* [apps/backend/Operis_API/Modules/Users/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Users/README.md)

Application surfaces:

* user queries and commands
* registration queries and commands
* invitation queries and commands
* reference data queries and commands

Rule:

* endpoints delegate to `Application/`

## audits

Module entry:

* [apps/backend/Operis_API/Modules/Audits/AuditsModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Audits/AuditsModule.cs)
* [apps/backend/Operis_API/Modules/Audits/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Audits/README.md)

Application surfaces:

* audit log queries
* audit compliance queries
* audit compliance commands

Notes:

* `GET /audit-events` projects from immutable `audit_logs`
* audit plans, findings, and evidence export workflows remain owned by the `Audits` module
* evidence completeness rules, evaluator runs, and missing evidence registers are also owned by `Audits`

## activities

Module entry:

* [apps/backend/Operis_API/Modules/Activities/ActivitiesModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Activities/ActivitiesModule.cs)
* [apps/backend/Operis_API/Modules/Activities/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Activities/README.md)

Application surfaces:

* activity log queries

## documents

Module entry:

* [apps/backend/Operis_API/Modules/Documents/DocumentsModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Documents/DocumentsModule.cs)
* [apps/backend/Operis_API/Modules/Documents/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Documents/README.md)

Application surfaces:

* document queries

## workflows

Module entry:

* [apps/backend/Operis_API/Modules/Workflows/WorkflowsModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Workflows/WorkflowsModule.cs)
* [apps/backend/Operis_API/Modules/Workflows/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Workflows/README.md)

Application surfaces:

* workflow definition queries
* workflow definition commands

## notifications

Module entry:

* [apps/backend/Operis_API/Modules/Notifications/NotificationsModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Notifications/NotificationsModule.cs)
* [apps/backend/Operis_API/Modules/Notifications/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Notifications/README.md)

Application surfaces:

* notification queries
* notification commands

## governance

Module entry:

* [apps/backend/Operis_API/Modules/Governance/GovernanceModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Governance/GovernanceModule.cs)
* [apps/backend/Operis_API/Modules/Governance/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Governance/README.md)

## learning

Module entry:

* [apps/backend/Operis_API/Modules/Learning/LearningModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Learning/LearningModule.cs)
* [apps/backend/Operis_API/Modules/Learning/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Learning/README.md)

Application surfaces:

* learning queries
* learning commands

Notes:

* training catalog, role-training requirements, completions, and competency reviews are owned by `Learning`
* project, role, and assignment reads still come from `Users`-owned data through persistence reads inside `Learning` queries

Application surfaces:

* governance queries
* governance commands
* governance operations queries
* governance operations commands

Notes:

* governance owns compliance dashboard snapshot generation and saved dashboard preferences
* governance owns management review cadence records, agenda items, and follow-up actions
* governance owns policy registers, policy acknowledgement campaigns, and policy attestation records
* cross-module compliance reads stay query-only; governance does not take ownership of upstream project, requirements, verification, change control, audit, or operations tables

## requirements

Module entry:

* [apps/backend/Operis_API/Modules/Requirements/RequirementsModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Requirements/RequirementsModule.cs)
* [apps/backend/Operis_API/Modules/Requirements/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Requirements/README.md)

Application surfaces:

* requirement queries
* requirement commands

## changecontrol

Module entry:

* [apps/backend/Operis_API/Modules/ChangeControl/ChangeControlModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/ChangeControl/ChangeControlModule.cs)
* [apps/backend/Operis_API/Modules/ChangeControl/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/ChangeControl/README.md)

## metrics

Module entry:

* [apps/backend/Operis_API/Modules/Metrics/MetricsModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Metrics/MetricsModule.cs)
* [apps/backend/Operis_API/Modules/Metrics/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Metrics/README.md)

Application surfaces:

* metric queries
* metric commands

## releases

Module entry:

* [apps/backend/Operis_API/Modules/Releases/ReleasesModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Releases/ReleasesModule.cs)
* [apps/backend/Operis_API/Modules/Releases/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Releases/README.md)

Application surfaces:

* release queries
* release commands

## defects

Module entry:

* [apps/backend/Operis_API/Modules/Defects/DefectsModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Defects/DefectsModule.cs)
* [apps/backend/Operis_API/Modules/Defects/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Defects/README.md)

Application surfaces:

* defect queries
* defect commands

## operations

Module entry:

* [apps/backend/Operis_API/Modules/Operations/OperationsModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Operations/OperationsModule.cs)
* [apps/backend/Operis_API/Modules/Operations/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Operations/README.md)

Application surfaces:

* operations queries
* operations commands

## knowledge

Module entry:

* [apps/backend/Operis_API/Modules/Knowledge/KnowledgeModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Knowledge/KnowledgeModule.cs)
* [apps/backend/Operis_API/Modules/Knowledge/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Knowledge/README.md)

Application surfaces:

* operations queries
* operations commands

Notes:

* access review approval stays on a dedicated endpoint because it enforces decision and rationale requirements
* dependency and audit registers are owned by this module and not shared with project admin persistence

Application surfaces:

* change control queries
* change control commands

## risks

Module entry:

* [apps/backend/Operis_API/Modules/Risks/RisksModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Risks/RisksModule.cs)
* [apps/backend/Operis_API/Modules/Risks/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Risks/README.md)

Application surfaces:

* risk queries
* risk commands

## meetings

Module entry:

* [apps/backend/Operis_API/Modules/Meetings/MeetingsModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Meetings/MeetingsModule.cs)
* [apps/backend/Operis_API/Modules/Meetings/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Meetings/README.md)

Application surfaces:

* meeting queries
* meeting commands

## verification

Module entry:

* [apps/backend/Operis_API/Modules/Verification/VerificationModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Verification/VerificationModule.cs)
* [apps/backend/Operis_API/Modules/Verification/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Verification/README.md)

Application surfaces:

* verification queries
* verification commands

---

# Enforcement

Frontend:

* `npm run check:architecture`
* `node scripts/check-module-contracts.mjs`

Backend:

* `node scripts/check-backend-architecture.mjs`
* `node scripts/check-module-contracts.mjs`

CI:

* [/.github/workflows/frontend-quality.yml](/Users/nuttapon/Github-dev/Operis/.github/workflows/frontend-quality.yml)
* [/.github/workflows/backend-quality.yml](/Users/nuttapon/Github-dev/Operis/.github/workflows/backend-quality.yml)
