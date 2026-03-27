# Phase 36: Operational Automation

Goal:

* record and govern recurring operational automation work as first-class controls

In Scope:

* automation job definitions in the `Operations` module
* automation job execution run log with evidence references
* operational automation and job-run review pages
* route, menu, and permission wiring for automation control surfaces

Out of Scope:

* background schedulers or worker-host execution
* direct provider-side orchestration for backup, retention, or secret rotation jobs

Owning Module:

* `Operations`

Owned Tables:

* `automation_jobs`
* `automation_job_runs`
* `automation_job_evidence_refs`

Routes:

* `/app/operations/automation`
* `/app/operations/automation-runs`

Permissions:

* `operations.automation.read`
* `operations.automation.manage`
* `operations.automation.execute`

API Contracts:

* `GET /api/v1/operations/automation-jobs`
* `GET /api/v1/operations/automation-jobs/{jobId}`
* `POST /api/v1/operations/automation-jobs`
* `PUT /api/v1/operations/automation-jobs/{jobId}`
* `POST /api/v1/operations/automation-jobs/{jobId}/transition`
* `POST /api/v1/operations/automation-jobs/{jobId}/execute`
* `GET /api/v1/operations/automation-job-runs`

Validation/Error Codes:

* `automation_job_name_required`
* `automation_job_type_required`
* `automation_job_evidence_required`
* `invalid_workflow_transition`

Workflow States:

* automation job: `draft -> active -> paused -> retired`
* automation run: `queued -> running -> succeeded | failed`

Tests Required:

* command validation for missing job name and job type
* command validation for succeeded or failed runs without evidence
* endpoint guard coverage for manage and execute permissions
* frontend render coverage for automation run history

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

* users with `operations.automation.read` can inspect job definitions and run history
* users with `operations.automation.manage` can create, update, and transition automation jobs
* users with `operations.automation.execute` can record run outcomes with evidence linkage
* successful and failed runs retain evidence references for downstream audit and compliance review
