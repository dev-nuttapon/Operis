# audits backend module

Purpose:

* owns audit log querying and audit-facing contracts

Public surface:

* `AuditsModule.cs`
* `Application/AuditLogQueries.cs`
* `Contracts/`

Owned data:

* audit logs

Notes:

* keep audit filtering, paging, and projection inside the application service
