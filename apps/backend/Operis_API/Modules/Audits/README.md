# audits backend module

Purpose:

* owns immutable audit event projection, process audit plans, findings, and evidence export orchestration
* owns Phase 26 evidence completeness rules, evaluator runs, and missing evidence register

Public surface:

* `AuditsModule.cs`
* `Application/AuditLogQueries.cs`
* `Application/AuditComplianceQueries.cs`
* `Application/AuditComplianceCommands.cs`
* `Contracts/`

Owned data:

* `audit_logs`
* `business_audit_events`
* `audit_plans`
* `audit_findings`
* `evidence_exports`
* `evidence_rules`
* `evidence_rule_results`
* `evidence_missing_items`

Notes:

* `GET /audit-events` is backed by the immutable `audit_logs` store
* keep audit filtering, paging, export packaging, and workflow transitions inside application services
* evidence completeness reads upstream module data but persists evaluator state only inside `Audits`
