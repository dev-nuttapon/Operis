# DATA_OWNERSHIP.md

Data ownership map for the modular monolith.

This document exists to make future extraction easier without splitting repositories, services, or databases today.

---

# Rules

Ownership rules:

* every table has a single owning module
* non-owning modules must not treat another module's tables as their write model
* cross-module reads should prefer application/query contracts over direct persistence coupling
* schema is still shared today, but ownership must be explicit now

---

# Current Ownership

| Table | Owning Module | Purpose | Notes |
| --- | --- | --- | --- |
| `documents` | `documents` | document metadata | file binaries still belong to MinIO flow, not direct frontend storage writes |
| `users` | `users` | provisioned local user state | Keycloak remains identity source, local table is app-owned user state |
| `departments` | `users` | user master data | shared reference data but owned by `users` |
| `job_titles` | `users` | user master data | shared reference data but owned by `users` |
| `app_roles` | `users` | app-role to Keycloak-role mapping | local authorization mapping |
| `user_registration_requests` | `users` | self-registration workflow state | includes password setup token state |
| `user_invitations` | `users` | invitation workflow state | includes invitation token lifecycle |
| `audit_logs` | `audits` | immutable audit trail | cross-cutting write target, but storage ownership belongs to `audits` |

---

# Extraction Notes

## users

Extraction boundary:

* owns operational user state, invitations, registration requests, and reference data
* depends on Keycloak for identity lifecycle
* should expose contracts for reads instead of sharing persistence access

Migration caution:

* `users` is the highest-coupling backend module because it coordinates identity and local state
* if extracted later, Keycloak orchestration and local write model should move together

## documents

Extraction boundary:

* owns document metadata and listing/query behavior
* file storage integration should stay behind backend APIs

Migration caution:

* document binaries are not in PostgreSQL ownership scope
* extraction should preserve `Frontend -> Backend -> MinIO` flow

## audits

Extraction boundary:

* owns audit log persistence, retention concerns, and audit query behavior
* other modules may write audit entries, but they do not own the store

Migration caution:

* this is the most natural candidate for separate retention and compliance policies later

---

# Read Patterns

Allowed patterns:

* module reads its own tables directly
* composition layer calls module application/query service
* cross-cutting reporting is built from module contracts

Avoid:

* one module updating another module's tables directly
* endpoint layers composing raw persistence reads across multiple module stores
* shared helpers that hide cross-module persistence coupling

---

# Future Changes

Before adding a new table:

1. assign an owning module
2. document whether the table is write-owned or reference-owned
3. decide whether other modules may read it directly or only through contracts
4. add indexes based on actual query paths, not generic assumptions
