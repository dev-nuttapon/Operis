# Phase 30: Tailoring Governance Hardening

## Goal

Govern project tailoring deviations against standard process more strictly with explicit criteria, review cycles, due dates, and approval attention flags.

## In Scope

* tailoring criteria library
* tailoring review cycles
* deviation metadata on tailoring records
* expiry and approval-attention visibility

## Out Of Scope

* rewrite of existing tailoring record workflow

## Owning Module

* `Governance`

## Owned Tables

* `tailoring_criteria`
* `tailoring_review_cycles`
* `tailoring_records`

## Routes

* `/app/governance/tailoring-criteria`
* `/app/projects/tailoring-reviews`
* existing `/app/tailoring-records`

## API Contracts

* `GET /api/v1/governance/tailoring-criteria`
* `POST /api/v1/governance/tailoring-criteria`
* `PUT /api/v1/governance/tailoring-criteria/{id}`
* `GET /api/v1/governance/tailoring-reviews`
* `GET /api/v1/governance/tailoring-reviews/{id}`
* `POST /api/v1/governance/tailoring-reviews`
* `PUT /api/v1/governance/tailoring-reviews/{id}`
* `POST /api/v1/governance/tailoring-reviews/{id}/transition`

## Validation / Error Codes

* `tailoring_standard_reference_required`
* `tailoring_deviation_reason_required`
* `tailoring_review_due_date_required`
* `tailoring_criteria_not_found`
* `tailoring_review_not_found`

## Workflow States

Review cycle:

* `draft -> submitted -> approved/rejected -> expired`

## Acceptance Criteria

* tailoring deviations capture standard reference and deviation reason
* review cycles are visible by project
* overdue review cycles and deviations needing approval attention are visible in the UI
