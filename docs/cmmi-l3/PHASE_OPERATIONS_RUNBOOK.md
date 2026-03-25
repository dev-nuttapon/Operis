# CMMI L3 Phase Operations Runbook

This document defines first-version operational guidance for the phased system.
Use together with `PHASE_MENU_ROLE_WORKFLOW_SPEC.md`.

## Operating Principles

1. No security-sensitive degradation path may bypass audit logging.
2. No export or packaging failure may silently discard evidence.
3. No queue/retry flow may retry indefinitely.
4. No third-party outage may cause uncontrolled workflow transition.

## Third-Party Dependencies

### Keycloak
- Used for centralized authentication and identity claims.
- Failure mode:
  - login or token validation unavailable
- Required handling:
  - block protected actions
  - surface degraded auth message
  - emit security/availability event

### Redis
- Used for cache/session/supporting performance where configured.
- Failure mode:
  - cache unavailable or eviction anomaly
- Required handling:
  - fall back to source-of-truth reads where safe
  - do not fail authorization decisions open
  - emit operational event

### MinIO
- Used for governed file and evidence storage.
- Failure mode:
  - upload unavailable
  - object fetch unavailable
- Required handling:
  - fail document upload explicitly
  - do not mark version upload successful until storage commit completes
  - emit storage failure event

## Operational Scenarios

### Export Job Stuck
- Detect:
  - export remains `requested` or `running` beyond threshold
- Respond:
  - mark as failed with reason
  - allow bounded retry
  - preserve request metadata and audit trail

### Notification Queue Backlog
- Detect:
  - retry count or queue age exceeds threshold
- Respond:
  - pause repeated retries after bounded attempts
  - escalate to operational owner
  - preserve escalation history

### Privileged Access Usage
- Before use:
  - approval required
  - reason required
- After use:
  - review required
  - closure required

### Backup / Restore Failure
- Backup failure:
  - record failed evidence
  - do not mark backup verified
  - trigger follow-up action
- Restore failure:
  - record verification result
  - create corrective work item

## Minimum Runbook Checks by Phase

### Phase 0
- Verify Keycloak configuration and role mapping
- Verify permission matrix auditability

### Phase 2
- Verify document upload failure handling
- Verify export queue failure handling

### Phase 8
- Verify audit export packaging and failure path

### Phase 14
- Verify release checklist blocks release when dependencies fail

### Phase 21
- Verify secret rotation and privileged access review handling

### Phase 23
- Verify backup, restore, and DR drill evidence capture
