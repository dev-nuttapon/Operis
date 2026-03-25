# CMMI L3 Phase + Menu + Role + Workflow Spec

This document combines:
- Menu structure and delivery phases
- Detailed screen roles/permissions
- Workflow states for each screen

Legend:
- C: Create
- R: Read
- U: Update
- A: Approve/Review
- X: Execute/Run
- E: Export

## 1. Menu Structure (Order + Screen Names)

1. Overview
2. Projects
   - Project Register
   - Project Detail
   - Project Roles
   - Team Assignment
   - Project Phase Approval
3. Requirements
   - Requirement Register
   - Requirement Detail
   - Requirement Baseline
4. Documents
   - Document Type Setup
   - Document Register
   - Document Detail
5. Change Control
   - Change Request Register
   - Change Request Detail
   - Change Log
6. Meetings & Decisions
   - MOM Register
   - MOM Detail
   - Decision Log
7. Test & Validation
   - Test Plan
   - Test Case & Execution
   - UAT Sign-off
8. Audit & Evidence
   - Audit Log
   - Evidence Export
9. Metrics & Quality
   - Metrics Dashboard
   - Quality Gate Status
10. Process & Organization
    - Process Library
    - Training & Competency
11. Risk & Issue
    - Risk Register
    - Issue / Action Log
12. Configuration & Baseline
    - Configuration Items
    - Baseline Registry
13. PPQA
    - QA Review Checklist
    - Process Audit Plan & Findings
14. Metrics Definition
    - Metric Definitions
    - Data Collection Schedule
15. Project Management
    - Project Plan
    - Tailoring Record
    - Stakeholder Register
16. System Admin
    - User & Role Management
    - Permission Matrix
    - Master Data
    - System Settings

## 2. Phased Delivery Plan (CMMI L3 + Performance + Security)

### Phase 0: Security & Access Foundation
- User & Role Management
- Permission Matrix
- System Settings
- Baseline security policy (authentication, authorization, audit scope)

### Phase 1: Process Assets & Governance Baseline
- Process Library
- QA Review Checklist
- Project Plan
- Stakeholder Register
- Tailoring Record

### Phase 2: Document Governance Core
- Document Type Setup
- Document Register
- Document Detail
- Basic metadata and approval workflow

### Phase 3: Requirements + Traceability
- Requirement Register
- Requirement Detail
- Requirement Baseline
- Link requirements to documents/CR/test

### Phase 4: Change Control + Configuration Management
- Change Request Register
- Change Request Detail
- Change Log
- Configuration Items
- Baseline Registry

### Phase 5: Risk & Issue Management
- Risk Register
- Issue / Action Log

### Phase 6: Meetings & Decisions
- MOM Register
- MOM Detail
- Decision Log

### Phase 7: Verification & Validation
- Test Plan
- Test Case & Execution
- UAT Sign-off

### Phase 8: Audit & Compliance
- Audit Log
- Evidence Export
- Process Audit Plan & Findings

### Phase 9: Metrics & Quality Gates (Performance)
- Metric Definitions
- Data Collection Schedule
- Metrics Dashboard
- Quality Gate Status

### Phase 10: Project Governance Hardening
- Project Roles
- Team Assignment
- Project Phase Approval
- Gate enforcement across phases

### Phase 11: Master Data & Operations Support
- Master Data

## 3. Screen + Role + Permission + Workflow State

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

### Project Phase Approval
- Roles/Permissions
  - PM: Submit
  - Approver: A/R
  - ComplianceAdmin: A/R
- Workflow State
  - Draft → Submitted → Approved/Rejected → Baseline

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

### MOM Register
- Roles/Permissions
  - BA/PM: C/R/U
  - DocController: R
  - Approver: R
- Workflow State
  - Draft → Approved → Archived

### MOM Detail
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

### Process Library
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - SystemAdmin: R/U
  - PM/BA: R
- Workflow State
  - Draft → Reviewed → Approved → Active → Deprecated

### Training & Competency
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - SystemAdmin: R/U
  - PM: R
- Workflow State
  - Planned → In Progress → Completed → Archived

### Risk Register
- Roles/Permissions
  - PM/BA: C/R/U
  - Approver: R
  - ComplianceAdmin: R
- Workflow State
  - Draft → Assessed → Mitigated → Closed

### Issue / Action Log
- Roles/Permissions
  - PM/BA/Dev/QA: C/R/U
  - Approver: R
- Workflow State
  - Open → In Progress → Resolved → Closed

### Configuration Items
- Roles/Permissions
  - DocController: C/R/U
  - PM/BA: R
  - ComplianceAdmin: R
- Workflow State
  - Draft → Approved → Baseline → Superseded

### Baseline Registry
- Roles/Permissions
  - PM: C/R
  - Approver: A
  - ComplianceAdmin: R
- Workflow State
  - Proposed → Approved → Locked → Superseded

### QA Review Checklist
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - Approver: R
  - PM/BA/QA: R
- Workflow State
  - Draft → Approved → Active → Deprecated

### Process Audit Plan & Findings
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - Auditor: C/R/U
  - PM: R
- Workflow State
  - Planned → In Review → Findings Issued → Closed

### Metric Definitions
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - PM/QA: R
- Workflow State
  - Draft → Approved → Active → Deprecated

### Data Collection Schedule
- Roles/Permissions
  - ComplianceAdmin: C/R/U
  - PM/QA: R
- Workflow State
  - Draft → Active → Archived

### Project Plan
- Roles/Permissions
  - PM: C/R/U
  - Approver: R
  - ComplianceAdmin: R
- Workflow State
  - Draft → Review → Approved → Baseline → Superseded

### Tailoring Record
- Roles/Permissions
  - PM: C/R/U
  - ComplianceAdmin: A/R
- Workflow State
  - Draft → Submitted → Approved → Applied → Archived

### Stakeholder Register
- Roles/Permissions
  - PM: C/R/U
  - BA: R
  - ComplianceAdmin: R
- Workflow State
  - Active → Archived
