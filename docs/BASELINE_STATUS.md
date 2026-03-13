# BASELINE_STATUS.md

Status of the current repository baseline after the standardization and migration-readiness phase.

Use this document to decide whether to keep investing in architecture hardening or move back to feature delivery.

---

# Phase Result

Current status on `2026-03-13`:

* `standardization`: done for the current module set
* `migration-ready baseline`: done for the current module set
* `performance-readiness baseline`: done

Interpretation:

* the repository now has enough structure, checks, manifests, and tests to support future module extraction with low friction
* additional work should be justified by a real feature, real growth, or measured performance pressure

---

# Done

## module structure

* frontend modules use public entries and required directories
* backend modules use `Module -> Application -> Contracts`
* module manifests now exist beside code in both frontend and backend

## guardrails

* frontend boundary check exists and is enforced
* backend architecture check exists and is enforced
* module contracts check exists and is enforced
* CI runs these checks for frontend and backend changes

## readiness docs

* architecture, module readiness, module contracts, data ownership, module template, new module checklist, and performance hotspots are documented
* module-level `README.md` manifests are now part of the baseline

## backend safety net

* `users` has query, command, and handler-level tests for key flows
* `audits` and `documents` have application and handler-level baseline tests

## frontend readiness

* `users`, `audits`, and `documents` follow module boundaries more consistently
* public routes and admin-heavy flows use lazy boundaries where useful
* bundle reporting and bundle budget checks exist

---

# Not In Scope For This Phase

These are intentionally not required to call the baseline complete:

* separate services
* separate repositories
* separate databases
* aggressive frontend vendor splitting
* speculative backend indexing without measured need
* exhaustive tests for every path in every module

---

# Backlog

Only continue with these when justified by real work.

## feature-driven hardening

* add module tests when a module gains new write flows
* extend manifests and contracts when a module surface grows
* keep new modules aligned with `docs/MODULE_TEMPLATE.md`
* use `docs/NEW_MODULE_CHECKLIST.md` before merging any new module

## performance-driven work

* reduce `antd-core-vendor` only when a change has measurable value
* add backend indexes only when a concrete query path becomes a measured hotspot
* add runtime profiling if real latency data becomes available

## extraction-driven work

* strengthen `auth` boundaries if auth is prepared for separate ownership
* deepen frontend maturity of `audits` and `documents` only when their feature surface expands
* revisit module readiness scores after meaningful feature growth

---

# Decision Rule

Use this rule before doing more architecture work:

1. if the task is needed for a real feature, do it
2. if the task prevents a clear regression, do it
3. if the task is backed by measured performance pain, do it
4. otherwise, prefer feature delivery over more baseline refactoring

Related operating docs:

* `docs/PROJECT_SNAPSHOT.md`
* `docs/MODULE_TEMPLATE.md`
* `docs/NEW_MODULE_CHECKLIST.md`
* `docs/MODULE_CONTRACTS.md`
* `docs/PERFORMANCE_HOTSPOTS.md`

---

# Exit Criteria

This phase should be considered closed unless one of these changes:

* a new module is introduced
* a current module gains a materially larger workflow surface
* measured performance regresses
* ownership boundaries become unclear again
