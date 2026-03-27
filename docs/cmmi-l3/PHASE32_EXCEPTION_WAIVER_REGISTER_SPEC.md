# PHASE32_EXCEPTION_WAIVER_REGISTER_SPEC

Goal

* govern temporary process waivers and their compensating controls with explicit approval and expiry handling

In Scope

* waiver register
* waiver detail and transition flow
* compensating controls
* waiver review history

Out of Scope

* permanent process redesign
* dashboard aggregation beyond waiver list filters

Owning Module

* `Exceptions`

Owned Tables

* `waivers`
* `compensating_controls`
* `waiver_reviews`

Routes

* `GET /api/v1/exceptions/waivers`
* `GET /api/v1/exceptions/waivers/{waiverId}`
* `POST /api/v1/exceptions/waivers`
* `PUT /api/v1/exceptions/waivers/{waiverId}`
* `POST /api/v1/exceptions/waivers/{waiverId}/transition`
* `/app/exceptions/waivers`
* `/app/exceptions/waivers/:waiverId`

Permissions

* `exceptions.waivers.read`
* `exceptions.waivers.manage`
* `exceptions.waivers.approve`

Validation / Error Codes

* `waiver_scope_required`
* `waiver_expiry_required`
* `waiver_compensating_control_required`
* `waiver_not_found`
* `waiver_code_duplicate`

Workflow States

* `draft -> submitted -> approved/rejected -> expired -> closed`

Acceptance Criteria

* waivers cannot be approved without compensating controls
* expired waivers can be filtered from the register
* waiver detail shows controls and review history from owned tables
