# PROJECT_SNAPSHOT.md

One-page snapshot of the current repository state.

Use this document as the default starting point before deciding whether to refactor more or return to feature delivery.

---

# Current State

Snapshot date:

* `2026-03-13`

Current position:

* `standardization`: complete for the current module set
* `migration-ready baseline`: complete for the current module set
* `performance-readiness baseline`: complete

Meaning:

* the codebase is now structured strongly enough for future module extraction without requiring repository, service, or database splits today
* further architecture work should be justified by features, regressions, or measured performance pain

---

# Module Snapshot

| Module | Frontend | Backend | Overall | Summary |
| --- | --- | --- | --- | --- |
| `users` | `8/10` | `8.5/10` | `8.5/10` | Strongest module in the repo, with separated application services and meaningful tests. |
| `auth` | `7/10` | `n/a` | `7/10` | Public surface is clearer, but it remains a cross-cutting module. |
| `audits` | `6.5/10` | `7.5/10` | `7/10` | Bounded module with query and handler-level coverage in backend. |
| `documents` | `6/10` | `7/10` | `6.5/10` | Small module that now follows the standard backend/frontend pattern more closely. |

---

# Baseline In Place

## structure

* frontend modules use public entries
* backend modules use `Module -> Application -> Contracts`
* module manifests live beside module code

## checks

* frontend boundary check
* backend architecture check
* module contracts check
* frontend bundle report
* frontend bundle budget check

## CI

* frontend quality workflow
* backend quality workflow

## docs

* architecture
* baseline status
* module readiness
* module contracts
* data ownership
* performance hotspots
* performance standards
* module template
* new module checklist

---

# Current Performance Baseline

Frontend snapshot:

* `antd-core-vendor`: about `973 kB`
* `react-vendor`: about `224 kB`
* `index`: about `81 kB`
* `AdminUsersPage`: about `23 kB`
* `AuditLogsPage`: about `7.35 kB`
* `DocumentDashboardPage`: about `3.78 kB`

Performance rule:

* optimize only with measured before/after evidence

---

# What Is Done

* module boundaries are enforced
* public contracts are documented and checked
* module manifests are required
* backend composition layers are thin
* key backend flows have query/command/handler-level tests
* frontend has budget-aware performance checks

---

# What Is Backlog

Only continue with these when justified:

* feature-driven tests and hardening as modules grow
* targeted performance tuning when hotspots are measured
* deeper extraction work only if ownership or deployment needs change

---

# Default Next Step

Default decision:

* use the current baseline for feature delivery

Before creating a new module:

* start with `docs/MODULE_TEMPLATE.md`
* finish with `docs/NEW_MODULE_CHECKLIST.md`

If architecture questions come up:

* check `docs/BASELINE_STATUS.md`
* check `docs/MODULE_READINESS.md`
* check `docs/PERFORMANCE_HOTSPOTS.md`
* check `docs/PERFORMANCE_STANDARDS.md`
