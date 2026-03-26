# audits backend module

Purpose:

* owns immutable audit event projection, process audit plans, findings, and evidence export orchestration

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

Notes:

* `GET /audit-events` is backed by the immutable `audit_logs` store
* keep audit filtering, paging, export packaging, and workflow transitions inside application services
