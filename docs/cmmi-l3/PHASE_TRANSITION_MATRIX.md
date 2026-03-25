# CMMI L3 Phase Transition Matrix

This document defines machine-readable style transition rules for the first-version implementation.
Use together with `PHASE_MENU_ROLE_WORKFLOW_SPEC.md`.

## Rule Format

Each transition should define:

- Entity
- Current state
- Action
- Allowed roles
- Required evidence
- Next state
- Audit event
- Blocking conditions

## Phase 0

### User Role Assignment
- Entity: `user_role_assignments`
- Current state: `active`
- Action: `update_roles`
- Allowed roles: `SystemAdmin`
- Required evidence: `reason`
- Next state: `active`
- Audit event: `user_role_assignment_updated`
- Blocking conditions:
  - removing last effective admin assignment
  - requester lacks permission

### Permission Matrix
- Entity: `permission_matrix`
- Current state: `draft`
- Action: `apply`
- Allowed roles: `SystemAdmin`
- Required evidence: `reason`
- Next state: `applied`
- Audit event: `permission_matrix_applied`
- Blocking conditions:
  - requester lacks permission
  - invalid permission key

### Access Review
- Entity: `access_reviews`
- Current state: `scheduled`
- Action: `start_review`
- Allowed roles: `ComplianceAdmin`
- Required evidence: none
- Next state: `in_review`
- Audit event: `access_review_started`

- Entity: `access_reviews`
- Current state: `in_review`
- Action: `approve`
- Allowed roles: `ComplianceAdmin`
- Required evidence: `decision`, `rationale`
- Next state: `approved`
- Audit event: `access_review_approved`
- Blocking conditions:
  - missing rationale

## Phase 1

### Process Asset Version
- Entity: `process_asset_versions`
- Current state: `draft`
- Action: `submit_review`
- Allowed roles: `ComplianceAdmin`
- Required evidence: none
- Next state: `reviewed`
- Audit event: `process_asset_version_reviewed`

- Entity: `process_asset_versions`
- Current state: `reviewed`
- Action: `approve`
- Allowed roles: `ComplianceAdmin`
- Required evidence: `approver`, `change_summary`
- Next state: `approved`
- Audit event: `process_asset_version_approved`

- Entity: `process_asset_versions`
- Current state: `approved`
- Action: `activate`
- Allowed roles: `ComplianceAdmin`
- Required evidence: none
- Next state: `active`
- Audit event: `process_asset_version_activated`

### Tailoring Record
- Entity: `tailoring_records`
- Current state: `draft`
- Action: `submit`
- Allowed roles: `PM`
- Required evidence: `reason`
- Next state: `submitted`
- Audit event: `tailoring_submitted`

- Entity: `tailoring_records`
- Current state: `submitted`
- Action: `approve`
- Allowed roles: `ComplianceAdmin`
- Required evidence: `reason`
- Next state: `approved`
- Audit event: `tailoring_approved`

- Entity: `tailoring_records`
- Current state: `approved`
- Action: `apply`
- Allowed roles: `PM`
- Required evidence: none
- Next state: `applied`
- Audit event: `tailoring_applied`

## Phase 2

### Document
- Entity: `documents`
- Current state: `draft`
- Action: `submit`
- Allowed roles: `DocController`, `PM`, `BA`, `Dev`, `QA`
- Required evidence: required metadata completed
- Next state: `review`
- Audit event: `document_submitted`

- Entity: `documents`
- Current state: `review`
- Action: `approve`
- Allowed roles: `Approver`
- Required evidence: `decision_reason`
- Next state: `approved`
- Audit event: `document_approved`

- Entity: `documents`
- Current state: `review`
- Action: `reject`
- Allowed roles: `Approver`
- Required evidence: `decision_reason`
- Next state: `rejected`
- Audit event: `document_rejected`

- Entity: `documents`
- Current state: `approved`
- Action: `baseline`
- Allowed roles: `Approver`, `DocController`
- Required evidence: approved version exists
- Next state: `baseline`
- Audit event: `document_baselined`

- Entity: `documents`
- Current state: `baseline`
- Action: `archive`
- Allowed roles: `DocController`
- Required evidence: archive reason when policy requires
- Next state: `archived`
- Audit event: `document_archived`

## Phase 3

### Requirement
- Entity: `requirements`
- Current state: `draft`
- Action: `submit`
- Allowed roles: `BA`, `PM`
- Required evidence: acceptance criteria present
- Next state: `review`
- Audit event: `requirement_submitted`

- Entity: `requirements`
- Current state: `review`
- Action: `approve`
- Allowed roles: `Approver`
- Required evidence: approval reason
- Next state: `approved`
- Audit event: `requirement_approved`

- Entity: `requirements`
- Current state: `approved`
- Action: `baseline`
- Allowed roles: `PM`, `Approver`
- Required evidence: required traceability links complete
- Next state: `baselined`
- Audit event: `requirement_baselined`
- Blocking conditions:
  - missing required downstream links

## Phase 4

### Change Request
- Entity: `change_requests`
- Current state: `draft`
- Action: `submit`
- Allowed roles: `PM`, `BA`
- Required evidence: full impact section
- Next state: `submitted`
- Audit event: `change_request_submitted`

- Entity: `change_requests`
- Current state: `submitted`
- Action: `approve`
- Allowed roles: `Approver`
- Required evidence: decision reason
- Next state: `approved`
- Audit event: `change_request_approved`

- Entity: `change_requests`
- Current state: `approved`
- Action: `implement`
- Allowed roles: `PM`
- Required evidence: linked baseline or CI scope
- Next state: `implemented`
- Audit event: `change_request_implemented`

- Entity: `change_requests`
- Current state: `implemented`
- Action: `close`
- Allowed roles: `PM`
- Required evidence: implementation summary
- Next state: `closed`
- Audit event: `change_request_closed`

## Phases 5-24

For first-version implementation, apply the same transition pattern from the main spec:

- all transitions must map to declared workflow states only
- all approval/reject/override actions require evidence where defined
- all close/archive/release actions emit audit events
- all denied transitions must return stable business errors
