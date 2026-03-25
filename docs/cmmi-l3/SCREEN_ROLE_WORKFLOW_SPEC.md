# CMMI L3 Screen + Role + Permission + Workflow Spec

This document captures the detailed screen list, role permissions, and workflow states
for a CMMI Level 3-ready delivery system. It is intended as a baseline for phased
implementation and permission matrix design.

Legend:
- C: Create
- R: Read
- U: Update
- A: Approve/Review
- X: Execute/Run
- E: Export

## 1. Project Setup & Governance

### Project Register
- Roles/Permissions
  - PM: C/R/U
  - BA: R
  - Approver: R
  - Dev/QA/DocController/Auditor: R
  - SystemAdmin/ComplianceAdmin: R/U
- Workflow State
  - Draft → Active → On Hold → Closed → Archived

### Project Detail
- Roles/Permissions
  - PM: R/U
  - BA/Dev/QA: R
  - Approver: R
  - DocController: R
  - ComplianceAdmin: R
- Workflow State
  - Active → On Hold → Closed

### Project Roles
- Roles/Permissions
  - PM: C/R/U
  - SystemAdmin: R/U
- Workflow State
  - Active → Archived

### Team Assignment
- Roles/Permissions
  - PM: C/R/U
  - SystemAdmin: R
- Workflow State
  - Active → Removed

### Stage Gate / Phase Approval
- Roles/Permissions
  - PM: Submit
  - Approver: A/R
  - ComplianceAdmin: A/R
- Workflow State
  - Draft → Submitted → Approved/Rejected → Baseline

## 2. Requirements

### Requirement Register
- Roles/Permissions
  - BA: C/R/U
  - PM: R/U
  - Dev/QA: R
  - Approver: R
- Workflow State
  - Draft → Review → Approved → Baseline → Changed

### Requirement Detail
- Roles/Permissions
  - BA: C/R/U
  - PM: R/U
  - Dev/QA: R
  - Approver: R
- Workflow State
  - Draft → Review → Approved → Baseline → Superseded

### Requirement Baseline
- Roles/Permissions
  - PM: Create
  - Approver: Approve
- Workflow State
  - Proposed → Approved → Locked

## 3. Document Management

### Document Type Setup
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - SystemAdmin: C/R/U
  - DocController: R
- Workflow State
  - Active → Deprecated

### Document Register
- Roles/Permissions
  - DocController: C/R/U
  - BA/PM/Dev/QA: C/R (by doc type)
  - Approver: R
  - Auditor: R
- Workflow State
  - Draft → Review → Approved → Baseline → Archived

### Document Detail
- Roles/Permissions
  - DocController: C/R/U
  - Approver: A
  - BA/PM/Dev/QA: R
- Workflow State
  - Draft → Review → Approved/Rejected → Baseline → Archived

## 4. Change Control

### Change Request Register
- Roles/Permissions
  - PM: C/R/U
  - BA: C/R
  - Approver: R
- Workflow State
  - Draft → Submitted → In Review → Approved/Rejected → Implemented → Closed

### Change Request Detail
- Roles/Permissions
  - PM: C/R/U
  - BA: C/R/U
  - Approver: A
  - Dev/QA: R
- Workflow State
  - Draft → Submitted → Approved/Rejected → Implemented → Closed

### Change Log
- Roles/Permissions
  - PM/BA/Dev/QA/Auditor: R
- Workflow State
  - Read-only

## 5. Meetings & Decisions

### MOM Register
- Roles/Permissions
  - BA/PM: C/R/U
  - DocController: R
  - Approver: R
- Workflow State
  - Draft → Approved → Archived

### Decision Log
- Roles/Permissions
  - BA/PM: C/R
  - Approver: A
  - Auditor: R
- Workflow State
  - Proposed → Approved → Applied → Archived

## 6. Verification & Validation

### Test Plan
- Roles/Permissions
  - QA: C/R/U
  - PM/Dev: R
  - Approver: R
- Workflow State
  - Draft → Review → Approved → Baseline

### Test Case & Execution
- Roles/Permissions
  - QA: C/R/U/X
  - Dev: R
- Workflow State
  - Draft → Ready → Executed → Passed/Failed → Retest

### UAT Sign-off
- Roles/Permissions
  - PM: Submit
  - Approver: A
  - BA: R
- Workflow State
  - Draft → Submitted → Approved/Rejected

## 7. Audit & Compliance

### Audit Log
- Roles/Permissions
  - Auditor: R
  - PM: R
  - ComplianceAdmin/Support: R
- Workflow State
  - Read-only

### Evidence Export
- Roles/Permissions
  - Auditor: E
  - PM: E
  - ComplianceAdmin: E
- Workflow State
  - Requested → Generated → Downloaded

## 8. Metrics & Quality Gate

### Metrics Dashboard
- Roles/Permissions
  - PM/BA/QA: R
  - ComplianceAdmin: R
- Workflow State
  - Read-only

### Quality Gate Status
- Roles/Permissions
  - PM: R
  - Approver/ComplianceAdmin: A/R
- Workflow State
  - Open → Blocked → Approved → Overridden

## 9. System Admin (Website Roles)

### User & Role Management
- Roles/Permissions
  - SystemAdmin: C/R/U
- Workflow State
  - Active → Disabled

### Permission Matrix
- Roles/Permissions
  - SystemAdmin: C/R/U
- Workflow State
  - Draft → Applied

### Master Data
- Roles/Permissions
  - SystemAdmin: C/R/U
- Workflow State
  - Active → Archived

### System Settings
- Roles/Permissions
  - SystemAdmin/Support: X
- Workflow State
  - Action-based (no state)
