# PERFORMANCE_HOTSPOTS.md

Performance hotspots and optimization notes for the current modular monolith.

This document exists so future changes improve performance deliberately instead of incidentally.

---

# Principles

Rules:

* optimize only when a query path or bundle path is identifiable
* prefer single-owner optimization inside the owning module
* avoid speculative caching or splitting
* record why a performance choice exists, not just what changed

---

# Backend Hotspots

## users list with identity

Owner:

* `users`

Code path:

* [UserQueries.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Users/Application/UserQueries.cs)

Why it matters:

* this path loads local users and may also call Keycloak for profile and role data
* latency can grow with page size if remote identity calls are uncontrolled

Current strategy:

* local DB query is paged
* department/job title/app role metadata comes from cache
* Keycloak profile and role calls run with controlled concurrency
* role mapping is pre-indexed in memory by `KeycloakRoleName`

Do not regress:

* do not move Keycloak calls back into `*Module.cs`
* do not remove concurrency limits without evidence
* do not add per-user DB queries inside the identity mapping loop

Future options:

* add Keycloak batch API support if the client later exposes it
* add response caching only if user list traffic proves it is necessary

## registration and invitation search

Owner:

* `users`

Code paths:

* [UserRegistrationQueries.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Users/Application/UserRegistrationQueries.cs)
* [UserInvitationQueries.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Users/Application/UserInvitationQueries.cs)

Why it matters:

* list screens filter by text and date ranges
* these queries will grow with operational usage

Current strategy:

* PostgreSQL-friendly `ILIKE` is used for case-insensitive search
* paging and sorting stay inside the query path

Do not regress:

* do not reintroduce `ToLower().Contains()` on database columns
* do not sort in memory after loading unbounded data

Future options:

* add trigram indexes if text search becomes a measured hotspot

## audit log search

Owner:

* `audits`

Code path:

* [AuditLogQueries.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Audits/Application/AuditLogQueries.cs)

Why it matters:

* audit logs are the most likely table to grow fastest
* filter combinations can become expensive under compliance-heavy usage

Current strategy:

* filtering, sorting, count, and projection are kept in one query service
* actor search uses `ILIKE`
* indexes already exist for `OccurredAt`, `Module`, `Action`, `Status`, `ActorUserId`, `ActorEmail`, and `DepartmentId`

Do not regress:

* do not add broad in-memory filtering over audit rows
* do not move audit query logic into endpoint layers

Future options:

* add retention partitioning or archive strategy if volume grows significantly
* consider trigram indexes for actor or entity-id search if measured

## documents list

Owner:

* `documents`

Code path:

* [DocumentQueries.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Documents/Application/DocumentQueries.cs)

Why it matters:

* this path is simple now but may become a dashboard feed later

Current strategy:

* reads only latest 50 documents
* uses `AsNoTracking`
* sorts by `UploadedAt`

Do not regress:

* do not turn this into an unbounded list query
* keep storage access behind backend APIs

---

# Frontend Hotspots

## Ant Design vendor bundle

Owner:

* frontend platform shell

Why it matters:

* this is the largest frontend chunk today

Current state:

* vendor splitting exists
* low-risk reductions have already removed some unnecessary Ant Design usage
* CI now enforces bundle budgets for the main shell and key route chunks

Do not regress:

* do not add heavy UI dependencies without checking bundle impact
* do not perform risky manual chunk splits without before/after evidence

Future options:

* replace high-cost decorative components in public paths first
* continue route-, section-, and modal-level lazy loading where it reduces initial load

Current bundle budgets:

* `antd-core-vendor`: `<= 1050 kB`
* `react-vendor`: `<= 240 kB`
* `index`: `<= 90 kB`
* `AdminUsersPage`: `<= 26 kB`
* `AuditLogsPage`: `<= 10 kB`
* `DocumentDashboardPage`: `<= 6 kB`
* `PublicRegistrationPage`: `<= 8 kB`
* `InvitationAcceptPage`: `<= 8 kB`
* `RegistrationPasswordSetupPage`: `<= 7 kB`

## admin users screen

Owner:

* `users`

Why it matters:

* this is the densest frontend workflow in the repo

Current strategy:

* sections are split
* modals are lazy-loaded
* screen orchestration is moved into hooks

Do not regress:

* do not move heavy workflow logic back into the page
* do not eagerly import modal-heavy components into initial screen render

## documents dashboard

Owner:

* `documents`

Why it matters:

* this module is still small, so it should stay structurally clean while it grows

Current strategy:

* page uses hooks and API layer
* latest document list is loaded through TanStack Query

Do not regress:

* do not bypass the hook/API path from the page

---

# Index Notes

Current indexed paths already in the model:

* users: deleted/created, department, job title
* registration requests: email/status, status/requested-at, password-setup token
* invitations: email/status, status/invited-at, invitation token
* audit logs: occurred-at, module/occurred-at, action/occurred-at, status/occurred-at, actor/occurred-at, department/occurred-at

When to add new indexes:

1. a specific query path is used often
2. the filter and sort pattern is stable
3. the change is justified by measured slowness or obvious volume growth

Avoid:

* adding indexes generically “just in case”
* indexing every searchable text column without a query plan reason

---

# Validation

Use these checks after performance-sensitive changes:

Frontend:

* `npm run build:local`
* `npm run perf:bundle-report`
* `npm run perf:bundle-budget`

Backend:

* `dotnet build apps/backend/Operis_API/Operis_API.csproj`

Architecture:

* `npm run check:architecture`
* `node scripts/check-backend-architecture.mjs`
