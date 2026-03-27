# Phase 27: Management Review Cadence

Goal:

* institutionalize recurring management review as a governed process with retained actions and decisions

Owning module:

* `Governance`

Owned tables:

* `management_reviews`
* `management_review_items`
* `management_review_actions`

Screens and routes:

* `/app/governance/management-reviews`
* `/app/governance/management-reviews/:reviewId`

API contracts:

* `GET /api/v1/governance/management-reviews`
* `GET /api/v1/governance/management-reviews/{id}`
* `POST /api/v1/governance/management-reviews`
* `PUT /api/v1/governance/management-reviews/{id}`
* `POST /api/v1/governance/management-reviews/{id}/transition`

Permissions:

* `governance.management_reviews.read`
* `governance.management_reviews.manage`
* `governance.management_reviews.approve`

Workflow:

* `draft -> scheduled -> in_review -> closed -> archived`

Validation and error codes:

* `management_review_schedule_required`
* `management_review_minutes_required`
* `management_review_open_actions_block_close`
* `management_review_not_found`
* `management_review_code_duplicate`

Implementation notes:

* close requires `minutes_summary` and `decision_summary`
* close is blocked when any mandatory action remains open
* actions may link to `capa`, `escalation`, or `risk` records by type and id
* review history is exposed through retained business audit events for the management review entity

Tests required:

* state transition tests
* open-action close-block tests
* permission handler tests
* UI register rendering tests
