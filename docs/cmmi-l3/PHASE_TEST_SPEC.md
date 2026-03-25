# CMMI L3 Phase Test Spec

This document defines the first-version acceptance scenarios for phased implementation.
Use together with `PHASE_MENU_ROLE_WORKFLOW_SPEC.md`.

## Test Pack Structure

Each phase should include:

- Functional tests
- Permission tests
- Audit tests
- Security tests
- Performance tests

## Phase 0

### Functional
- Given an admin user, when roles are assigned to a target user, then the target user role summary is updated correctly.

### Permission
- Given a non-admin user, when permission matrix update is attempted, then the API returns forbidden and no change is persisted.

### Audit
- Given a role change, when assignment succeeds, then an audit event exists with actor, reason, and outcome.

### Security
- Given the last effective admin, when removal is attempted, then the system blocks the action.

### Performance
- Given more than 100 users, when user list is loaded, then paging is applied and response stays within target threshold.

## Phase 1

### Functional
- Given a project plan draft, when all required fields are completed and approved, then the plan reaches baseline-ready state.

### Permission
- Given a non-approver, when tailoring approval is attempted, then the action is rejected.

### Audit
- Given a tailoring approval, when completed, then approver and rationale are stored in audit trail.

## Phase 2

### Functional
- Given a document with valid metadata, when uploaded and submitted, then it enters review successfully.
- Given an approved document, when baseline is created, then the baseline state is visible in the register.

### Permission
- Given a user without document approval permission, when approve is attempted, then the action is denied.

### Audit
- Given document upload, submit, approve, reject, archive, or export, when action succeeds or is denied, then the action is logged appropriately.

### Security
- Given a classified document, when an unauthorized user requests content or export, then access is blocked or redacted according to policy.

### Performance
- Given large document volume, when document list is loaded, then binary content is not returned and paging is enforced.

## Phase 3

### Functional
- Given an approved requirement, when baseline is created, then it is locked into governed baseline.
- Given a requirement missing mandatory traceability, when baseline is attempted, then the action is blocked.

### Permission
- Given a user without baseline permission, when baseline is attempted, then the system denies it.

### Audit
- Given traceability link creation or removal, when performed, then the event is logged.

### Performance
- Given many linked artifacts, when traceability matrix loads, then server-side filtering and paging are used.

## Phase 4

### Functional
- Given a CR with complete impact analysis, when approved, then baseline change becomes eligible.
- Given no approved CR, when baseline change is requested, then the request is rejected.

### Security
- Given emergency override path, when missing elevated role or rationale, then override is denied.

## Phase 5-24 Minimum Pack

For first-version implementation, each remaining phase must define at least:

- one successful end-to-end functional path
- one permission denial path
- one audit verification path
- one security-sensitive handling path if the phase touches sensitive data or privileged actions
- one performance-sensitive path if the phase has list, export, report, queue, or dashboard behavior
