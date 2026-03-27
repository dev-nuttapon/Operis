# Phase 34: Assessor Workspace

Goal

* provide an assessor-facing workspace to package and review evidence by project and process area

Owning module

* `Assessment`

Owned tables

* `assessment_packages`
* `assessment_findings`
* `assessment_notes`

Routes

* `/app/assessment/workspace`
* `/app/assessment/findings`

Permissions

* `assessment.workspace.read`
* `assessment.workspace.manage`
* `assessment.workspace.review`

Backend contracts

* `GET /api/v1/assessment/packages`
* `GET /api/v1/assessment/packages/{packageId}`
* `POST /api/v1/assessment/packages`
* `POST /api/v1/assessment/packages/{packageId}/transition`
* `POST /api/v1/assessment/packages/{packageId}/notes`
* `GET /api/v1/assessment/findings`
* `GET /api/v1/assessment/findings/{findingId}`
* `POST /api/v1/assessment/findings`
* `POST /api/v1/assessment/findings/{findingId}/transition`

Workflow

* package states: `draft -> prepared -> shared -> archived`
* finding states: `open -> accepted -> closed`

Validation / error codes

* `assessment_package_scope_required`
* `assessment_package_not_found`
* `assessment_package_sharing_requires_finding`
* `assessment_finding_title_required`
* `assessment_finding_not_found`
* `assessment_finding_evidence_reference_required`

Notes

* package creation snapshots read-only evidence references from owner modules
* findings must point to evidence already included in the package snapshot
