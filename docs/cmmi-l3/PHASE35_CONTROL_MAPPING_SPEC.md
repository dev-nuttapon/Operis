# Phase 35: Control Mapping

Goal:

* add a governed control catalog, evidence mappings, and coverage snapshots for assessor workflows

In Scope:

* control catalog CRUD within the `Assessment` module
* control mapping creation and workflow transitions
* coverage snapshot query and review screen
* route/menu/authz wiring for assessment control surfaces

Out of Scope:

* automated import from external control libraries
* background recalculation jobs beyond query-time snapshot generation

Owning Module:

* `Assessment`

Owned Tables:

* `control_catalog`
* `control_mappings`
* `control_coverage_snapshots`

Routes:

* `/app/assessment/control-mapping`
* `/app/assessment/control-coverage`

Permissions:

* `assessment.controls.read`
* `assessment.controls.manage`

API Contracts:

* `GET /api/v1/assessment/control-catalog`
* `GET /api/v1/assessment/control-catalog/{controlId}`
* `POST /api/v1/assessment/control-catalog`
* `PUT /api/v1/assessment/control-catalog/{controlId}`
* `GET /api/v1/assessment/control-mappings`
* `GET /api/v1/assessment/control-mappings/{mappingId}`
* `POST /api/v1/assessment/control-mappings`
* `POST /api/v1/assessment/control-mappings/{mappingId}/transition`
* `GET /api/v1/assessment/control-coverage`

Validation/Error Codes:

* `control_code_required`
* `control_mapping_target_required`
* `project_not_found`
* `invalid_workflow_transition`

Workflow States:

* control catalog item: `draft`, `active`, `retired`
* control mapping: `draft -> active -> retired`

Tests Required:

* command validation for control code and mapping target
* endpoint guard coverage for catalog and mapping mutations

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

* users with `assessment.controls.read` can review control catalog and coverage
* users with `assessment.controls.manage` can create and transition control mappings
* control coverage shows `sufficient`, `partial`, and `gap` snapshots derived from active mappings
