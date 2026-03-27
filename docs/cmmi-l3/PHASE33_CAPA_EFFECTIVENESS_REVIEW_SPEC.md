# Phase 33: CAPA Effectiveness Review

Goal:

* verify whether completed CAPA actions remained effective after closure and allow ineffective closure to reopen through a traceable approval path

In Scope:

* `capa_effectiveness_reviews`
* post-closure review register
* effectiveness result and evidence capture
* reopen path for ineffective CAPA

Out of Scope:

* redesign of the original CAPA workflow
* changes to escalation ownership

Owning Module:

* backend: `Operations`
* frontend: `operations`

Owned Tables:

* `capa_effectiveness_reviews`

Routes:

* `/app/operations/capa-effectiveness`

Permissions:

* existing `operations.read`
* existing `operations.manage`
* existing `operations.approve`

API Contracts:

* `GET /api/v1/operations/capa-effectiveness`
* `POST /api/v1/operations/capa-effectiveness`
* `POST /api/v1/capa/{id}/reopen`

Validation / Error Codes:

* `capa_effectiveness_result_required`
* `capa_effectiveness_evidence_required`
* `capa_reopen_reason_required`

Workflow States:

* review states: `draft -> submitted -> accepted/ineffective`
* implementation shortcut for this phase: submitted review is recorded directly as `accepted` or `ineffective` based on `effectiveness_result`

Backend Notes:

* effectiveness review creation is allowed only for `closed` CAPA records
* reopen is allowed only when the latest effectiveness review is `ineffective`
* reopen moves CAPA back to `action_planned` and clears closure metadata while preserving review history

Frontend Notes:

* page stays thin and follows `Page -> Hook -> API -> HTTP client`
* closed CAPA options are loaded from existing CAPA list API
* ineffective reviews expose reopen action only to approve-capable users

Tests Required:

* effectiveness evidence validation
* reopen validation
* endpoint permission guard for reopen

Quality Gates:

* `dotnet build apps/backend/Operis_API/Operis_API.csproj`
* `dotnet test apps/backend/Operis_API.Tests/Operis_API.Tests.csproj`
* `dotnet tool run dotnet-ef migrations has-pending-model-changes --project Operis_API.csproj`
* `node scripts/check-backend-architecture.mjs`
* `node scripts/check-module-contracts.mjs`
* `npm run check:architecture`
* `npm test`
* `npm run build:local`
* `npm run perf:bundle-report`
* `npm run perf:bundle-budget`

Acceptance Criteria:

* ineffective CAPA can be reopened with traceable reason
* effective versus ineffective closure is reportable from the UI
